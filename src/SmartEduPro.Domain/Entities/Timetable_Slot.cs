namespace SmartEduPro.Domain.Entities;
public class Timetable_Slot
{
    [Key]
    public Guid Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    [ForeignKey("Branches")]
    public Guid Branch_Id { get; set; }
    [ForeignKey("Classes")]
    public Guid Class_Id { get; set; }
    [ForeignKey("Subjects")]
    public Guid Subject_Id { get; set; }
    [ForeignKey("Teachers")]
    public Guid Teacher_Id { get; set; }
    public DayOfWeek Day_Of_Week { get; set; }
    public TimeOnly Start_Time { get; set; }
    public TimeOnly End_Time { get; set; }
    public string Room { get; set; } = string.Empty;
    public Class_Type Class_Type { get; set; }
    public DateOnly Effective_From { get; set; }
    public DateOnly Effective_To { get; set; }
    public bool Is_Active { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
    public Institute Institutes { get; set; }
    public Branch Branches { get; set; }
    public Class Classes { get; set; }
    public Subject Subjects { get; set; }
}