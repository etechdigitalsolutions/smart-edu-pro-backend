namespace SmartEduPro.Domain.Entities;
public class Teacher_Class_Assignment
{
    public Guid Id { get; set; }
    [ForeignKey("Teachers")]
    public Guid Teacher_Id { get; set; }
    [ForeignKey("Classes")]
    public Guid Class_Id { get; set; }
    [ForeignKey("Subjects")]
    public Guid Subject_Id { get; set; }
    public bool Is_Primary { get; set; }
    public DateOnly From_Date { get; set; }
    public DateTime To_Date { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
}