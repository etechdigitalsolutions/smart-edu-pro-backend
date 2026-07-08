using System.ComponentModel;

namespace SmartEduPro.Domain.Entities;
public class Class
{
    public Guid Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    [ForeignKey("Branches")]
    public Guid Branch_Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code {  get; set; } = string.Empty;
    public Study_Level Study_Level { get; set; }
    public Al_Stream Al_Stream { get; set; }
    public int Academic_year { get; set; }
    public Class_Type Class_Type { get; set; }
    public int Max_Student { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Is_Active { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
    public Institute Institutes { get; set; }
    public Branch Branches { get; set; }
}