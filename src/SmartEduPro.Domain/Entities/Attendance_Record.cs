namespace SmartEduPro.Domain.Entities;
public class Attendance_Record
{
    [Key]
    public Guid Id { get; set; }
    [ForeignKey("Attendance_Sessions")]
    public Guid Session_Id { get; set; }
    [ForeignKey("Students")]
    public Guid Student_Id { get; set; }
    public Attendance_Status Status { get; set; }
    public DateTime Check_In_Time { get; set; }
    public DateTime Check_Out_Time { get; set; }
    [StringLength(50)]
    public string Marked_Via { get; set; } = string.Empty;
    public string Qr_Scan_Log { get; set; } = string.Empty;
    public int Late_Minutes { get; set; }
    public bool Parent_Notified { get; set; }
    public DateTime Notified_At { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
    public Attendance_Session Attendance_Sessions { get; set; }
    public Student Students { get; set; }
}