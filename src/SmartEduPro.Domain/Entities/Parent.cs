namespace SmartEduPro.Domain.Entities;
public class Parent
{
    public Guid Id { get; set; }
    [ForeignKey("Users")]
    public Guid User_Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    [StringLength(200)]
    public string Occupation { get; set; } = string.Empty;
    [StringLength(20)]
    public string Whatsapp_No { get; set; } = string.Empty;
    [StringLength(20)]
    public string Alt_Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Notification_Channel Preffered_Channel { get; set; }
    public DateTime Created_At { get; set; }
}