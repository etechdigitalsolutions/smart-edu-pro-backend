namespace SmartEduPro.Domain.Entities;
public class Live_Session
{
    [Key]
    public Guid Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    [ForeignKey("Classes")]
    public Guid Class_Id { get; set; }
    [ForeignKey("Subjects")]
    public Guid Subject_Id { get; set; }
    [ForeignKey("Teachers")]
    public Guid Teacher_Id { get; set; }
    public string Title { get; set; } = string.Empty;
    [ForeignKey("TimeTable_Slots")]
    public Guid TimeTable_Slot_Id { get; set; }
    public DateTime Scheduled_Start { get; set; }
    public DateTime Scheduled_End { get; set; }
    public TimeOnly Actual_Start { get; set; }
    public TimeOnly Actual_End { get; set; }
    public Session_Status Status { get; set; }
    public string Room_Id { get; set; } = string.Empty;
    public string Room_Token { get; set; } = string.Empty;
    public string Join_Url { get; set; } = string.Empty;
    public string Recording_Url { get; set; } = string.Empty;
    public int Participant_Count { get; set; }
    public int Peak_Viewers { get; set; }
    public bool Chat_Enabled { get; set; } = true;
    public bool Recording_Enabled { get; set; } = false;
    public string WhiteBoard_Data { get; set; } = string.Empty;
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
}