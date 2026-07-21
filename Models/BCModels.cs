using System;
using System.Text.Json.Serialization;

namespace BIProxy.Models;

public class BCAdmission
{
    [JsonPropertyName("Admission_No")]
    public string? Admission_No { get; set; }

    [JsonPropertyName("How_you_knew_about_Kianda")]
    public string? How_you_knew_about_Kianda { get; set; }

    [JsonPropertyName("Have_you_ever_applied_before")]
    public bool Have_you_ever_applied_before { get; set; }

    [JsonPropertyName("If_yes_x002C__which_year")]
    public int If_yes_x002C__which_year { get; set; }

    [JsonPropertyName("Student_Full_Name")]
    public string? Student_Full_Name { get; set; }

    [JsonPropertyName("Date_of_Birth")]
    public string? Date_of_Birth { get; set; }

    [JsonPropertyName("Religion")]
    public string? Religion { get; set; }

    [JsonPropertyName("Denomination")]
    public string? Denomination { get; set; }

    [JsonPropertyName("Place_Among_Siblings")]
    public string? Place_Among_Siblings { get; set; }

    [JsonPropertyName("Relevant_Condition")]
    public string? Relevant_Condition { get; set; }

    [JsonPropertyName("Estate_of_Residence")]
    public string? Estate_of_Residence { get; set; }

    [JsonPropertyName("Disclaimer")]
    public bool Disclaimer { get; set; }
}

public class BCAppSchool
{
    [JsonPropertyName("Admission_No")]
    public string? Admission_No { get; set; }

    [JsonPropertyName("School_Name")]
    public string? School_Name { get; set; }

    [JsonPropertyName("Years_Enrolled")]
    public int Years_Enrolled { get; set; }

    [JsonPropertyName("Attending")]
    public string? Attending { get; set; }

    [JsonPropertyName("Other_Reason")]
    public string? Other_Reason { get; set; }
}

public class BCAppRelative
{
    [JsonPropertyName("Admission_No")]
    public string? Admission_No { get; set; }

    [JsonPropertyName("Name_of_Sibling")]
    public string? Name_of_Sibling { get; set; }

    [JsonPropertyName("Date_of_Birth")]
    public string? Date_of_Birth { get; set; }

    [JsonPropertyName("School_Attending_Attended")]
    public string? School_Attending_Attended { get; set; }
}

public class BCAppRelation
{
    [JsonPropertyName("Admission_No")]
    public string? Admission_No { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Relationship")]
    public string? Relationship { get; set; }

    [JsonPropertyName("Class_of_Current_Student")]
    public string? Class_of_Current_Student { get; set; }
}

public class BCAdmissionParent
{
    [JsonPropertyName("Admission_No")]
    public string? Admission_No { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Mobile_Number")]
    public string? Mobile_Number { get; set; }

    [JsonPropertyName("Profession")]
    public string? Profession { get; set; }

    [JsonPropertyName("Place_of_Work")]
    public string? Place_of_Work { get; set; }

    [JsonPropertyName("Email")]
    public string? Email { get; set; }

    [JsonPropertyName("Parent")]
    public string? Parent { get; set; }
}
