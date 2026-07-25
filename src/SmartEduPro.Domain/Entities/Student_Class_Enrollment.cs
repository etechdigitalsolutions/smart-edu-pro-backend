namespace SmartEduPro.Domain.Entities;
public class Student_Class_Enrollment
{
    public Guid Id { get; set; }
    [ForeignKey("Students")]
    public Guid Student_Id { get; set; }
    [ForeignKey("Classes")]
    public Guid Class_Id { get; set; }
    public DateTime Enrolled_Date { get; set; }
    public DateOnly Left_Date { get; set; }
    [ForeignKey("Fee_Packages")]
    public Guid Fee_Package_Id { get; set; }
    public bool Is_Active { get; set; }
    public DateTime Created_At { get; set; }
}