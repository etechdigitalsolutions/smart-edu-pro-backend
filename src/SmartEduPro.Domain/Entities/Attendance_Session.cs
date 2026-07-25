namespace SmartEduPro.Domain.Entities;
public class Attendance_Session
{
    public Guid Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    [ForeignKey("Classes")]
    public Guid Class_Id { get; set; }
    [ForeignKey("Subjects")]
    public Guid Subject_Id { get; set; }
    [ForeignKey("Teachers")]
    public Guid Teacher_Id { get; set; }
    public DateOnly Session_Date { get; set; }
    public TimeOnly Start_Time { get; set; }
    public TimeOnly End_Time { get; set; }
    public bool Is_Completed { get; set; }
    public int Total_Present { get; set; }
    public int Total_Absent { get; set; }
    public int Total_Late { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
    public Institute Institutes { get; set; }
    public Class Classes { get; set; }
    public Subject Subjects { get; set; }
    public Teacher Teachers { get; set; }
}