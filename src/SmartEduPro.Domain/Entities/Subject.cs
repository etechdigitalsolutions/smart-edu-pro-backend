namespace SmartEduPro.Domain.Entities;
public class Subject
{
    public Guid Id { get; set; }
    [ForeignKey("Insitutes")]
    public Guid Insitute_Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code {  get; set; } = string.Empty;
    public Study_Level Study_Level { get; set; }
    public Al_Stream Al_Stream { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Is_Active { get; set; }
    public DateTime Created_At { get; set; }
    public Institute Institutes { get; set; }
}