using Microsoft.AspNetCore.Mvc;
using BIProxy.Models;
using BIProxy.Services;

namespace BIProxy.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdmissionsController : ControllerBase
{
    private readonly BCProxyService _proxyService;
    private readonly ILogger<AdmissionsController> _logger;

    public AdmissionsController(
        BCProxyService proxyService,
        ILogger<AdmissionsController> logger)
    {
        _proxyService = proxyService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> PostAdmission([FromBody] AdmissionsPayload payload)
    {
        // If Admission_No is already provided, this is a RETRY — the main record exists.
        // We only insert sub-tables (Parents, Schools, Siblings) on the first creation
        // to prevent duplicates when a sync is retried after a partial failure.
        bool isNewAdmission = string.IsNullOrEmpty(payload.Admission_No);
        string? currentAdmissionNo = payload.Admission_No;

        try
        {
            _logger.LogInformation("Received admission payload for {CandidateName}", payload.Candidate?.FullName);

            if (isNewAdmission)
            {
                // 1. Map to BCAdmission and post
                var admission = new BCAdmission
                {
                    How_you_knew_about_Kianda = payload.AdditionalInfo?.Source,
                    Have_you_ever_applied_before = payload.AdditionalInfo?.HasAppliedBefore ?? false,
                    If_yes_x002C__which_year = payload.AdditionalInfo?.PreviousApplicationYears != null && payload.AdditionalInfo.PreviousApplicationYears.Any() ? string.Join(",", payload.AdditionalInfo.PreviousApplicationYears) : "",
                    Student_Full_Name = payload.Candidate?.FullName,
                    Date_of_Birth = payload.Candidate?.Dob,
                    Religion = payload.Candidate?.Religion,
                    Denomination = payload.Candidate?.Denomination,
                    Place_Among_Siblings = payload.Candidate?.BirthOrder,
                    Relevant_Condition = payload.Candidate?.MedicalInfo,
                    Estate_of_Residence = payload.ParentDetails?.Residency,
                    Programme = payload.Programme,
                    Applying_for_Grade = payload.ApplyingGrade,
                    Transaction_Code = payload.TransactionCode,
                    Intake_Year = payload.AdmissionCycleYear?.ToString(),
                    Disclaimer = true
                };

                currentAdmissionNo = await _proxyService.PostAdmissionAsync(admission);
                _logger.LogInformation("Successfully created Admission: {AdmissionNo}", currentAdmissionNo);

                // 2. Map and post Parents (only on new admission)
                if (payload.ParentDetails != null)
                {
                    if (!string.IsNullOrEmpty(payload.ParentDetails.FatherName))
                    {
                        await _proxyService.PostAdmissionParentAsync(new BCAdmissionParent
                        {
                            Admission_No = currentAdmissionNo,
                            Name = payload.ParentDetails.FatherName,
                            Mobile_Number = payload.ParentDetails.FatherPhone,
                            Profession = payload.ParentDetails.FatherProfession,
                            Place_of_Work = payload.ParentDetails.FatherWork,
                            Email = payload.ParentDetails.FatherEmail,
                            Parent = "Father"
                        });
                    }
                    
                    if (!string.IsNullOrEmpty(payload.ParentDetails.MotherName))
                    {
                        await _proxyService.PostAdmissionParentAsync(new BCAdmissionParent
                        {
                            Admission_No = currentAdmissionNo,
                            Name = payload.ParentDetails.MotherName,
                            Mobile_Number = payload.ParentDetails.MotherPhone,
                            Profession = payload.ParentDetails.MotherProfession,
                            Place_of_Work = payload.ParentDetails.MotherWork,
                            Email = payload.ParentDetails.MotherEmail,
                            Parent = "Mother"
                        });
                    }
                }

                // 3. Map and post AppSchools (only on new admission)
                if (payload.SchoolsAttended != null)
                {
                    foreach (var school in payload.SchoolsAttended)
                    {
                        string yearsEnrolled = "";
                        if (!string.IsNullOrEmpty(school.YearsRange))
                        {
                            var matches = System.Text.RegularExpressions.Regex.Matches(school.YearsRange, @"\d+");
                            if (matches.Count > 0)
                            {
                                yearsEnrolled = string.Join(",", matches.Select(m => m.Value));
                            }
                        }

                        var appSchool = new BCAppSchool
                        {
                            Admission_No = currentAdmissionNo,
                            School_Name = school.SchoolName,
                            Years_Enrolled = yearsEnrolled,
                            Attending = "Day",
                            Other_Reason = ""
                        };
                        await _proxyService.PostAppSchoolAsync(appSchool);
                    }
                }

                // 4. Map and post Siblings (only on new admission)
                if (payload.Siblings != null)
                {
                    foreach (var sibling in payload.Siblings)
                    {
                        if (sibling.Relationship?.Equals("brother", StringComparison.OrdinalIgnoreCase) == true ||
                            sibling.Relationship?.Equals("sister", StringComparison.OrdinalIgnoreCase) == true ||
                            sibling.Relationship?.Equals("sibling", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            var appRelative = new BCAppRelative
                            {
                                Admission_No = currentAdmissionNo,
                                Name_of_Sibling = sibling.Name,
                                Sibling_Type = sibling.SiblingType,
                                School_Attending_Attended = sibling.SchoolName,
                                Order = sibling.Order
                            };
                            await _proxyService.PostAppRelativeAsync(appRelative);
                        }
                        else
                        {
                            var appRelation = new BCAppRelation
                            {
                                Admission_No = currentAdmissionNo,
                                Name = sibling.Name,
                                Relationship = sibling.Relationship,
                                Class_of_Current_Student = "" 
                            };
                            await _proxyService.PostAppRelationAsync(appRelation);
                        }
                    }
                }
            }
            else
            {
                _logger.LogInformation("Resuming sync for existing Admission: {AdmissionNo} — skipping sub-table inserts to prevent duplicates.", currentAdmissionNo);
            }

            return Ok(new { success = true, admissionNo = currentAdmissionNo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing admission payload");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message, admissionNo = currentAdmissionNo });
        }
    }
}
