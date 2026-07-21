using System;
using System.Collections.Generic;

namespace BIProxy.Models;

public class AdmissionsPayload
{
    public Candidate? Candidate { get; set; }
    public ParentDetails? ParentDetails { get; set; }
    public AdditionalInfo? AdditionalInfo { get; set; }
    public List<SchoolAttended>? SchoolsAttended { get; set; }
    public List<Sibling>? Siblings { get; set; }
}

public class Candidate
{
    public required string FullName { get; set; }
    public required string Dob { get; set; }
    public required string Religion { get; set; }
    public string? Denomination { get; set; }
    public required string BirthOrder { get; set; }
    public string? MedicalInfo { get; set; }
}

public class ParentDetails
{
    public string? Residency { get; set; }
    public string? FatherName { get; set; }
    public string? FatherPhone { get; set; }
    public string? FatherProfession { get; set; }
    public string? FatherWork { get; set; }
    public string? FatherEmail { get; set; }
    public string? MotherName { get; set; }
    public string? MotherPhone { get; set; }
    public string? MotherProfession { get; set; }
    public string? MotherWork { get; set; }
    public string? MotherEmail { get; set; }
}

public class AdditionalInfo
{
    public string? Source { get; set; }
    public bool HasAppliedBefore { get; set; }
    public List<int>? PreviousApplicationYears { get; set; }
}

public class SchoolAttended
{
    public string? SchoolName { get; set; }
    public string? YearsRange { get; set; }
}

public class Sibling
{
    public string? Name { get; set; }
    public string? Relationship { get; set; }
    public string? SchoolName { get; set; }
    public string? Dob { get; set; }
}
