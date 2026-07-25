namespace SmartEduPro.Domain.Entities;
public class Student_Parent
{
    public Guid Id { get; set; }
    [ForeignKey("Students")]
    public Guid Student_Id { get; set; }
    [ForeignKey("Parents")]
    public Guid Parent_Id { get; set; }
    [StringLength(50)]
    public string Relation { get; set; } = string.Empty;
    public bool Is_Primary { get; set; }
    public DateTime Created_At { get; set; }
    public Student Students { get; set; }
    public Parent Parents { get; set; }
}