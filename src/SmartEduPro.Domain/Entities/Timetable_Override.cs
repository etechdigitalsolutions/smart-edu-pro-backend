namespace SmartEduPro.Domain.Entities;
public class Timetable_Override
{
    [Key]
    public Guid Id { get; set; }
    [ForeignKey("TimeTable_Slots")]
    public Guid Time_Table_Slot_Id { get; set; }
    public DateOnly Override_Date { get; set; }
    public string Action { get; set; } = string.Empty;
    public TimeOnly New_Start_Time { get; set; }
    public TimeOnly New_End_Time { get; set; }
    public string New_Room { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public bool Notify_Student { get; set; }
    [ForeignKey("Users")]
    public Guid Created_By { get; set; }
    public DateTime Created_At { get; set; }
    public Timetable_Slot TimeTable_Slots { get; set; }
    public User Users { get; set; }
}