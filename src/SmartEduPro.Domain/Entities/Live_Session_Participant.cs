namespace SmartEduPro.Domain.Entities;
public class Live_Session_Participant
{
    [Key]
    public Guid Id { get; set; }
    [ForeignKey("Live_Sessions")]
    public Guid Session_Id { get; set; }
    [ForeignKey("Users")]
    public Guid User_Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime Joined_At { get; set; }
    public DateTime Left_At { get; set; }
    public int Duration_Secs { get; set; }
    public Live_Session Live_Sessions { get; set; }
    public User Users { get; set; }
}
