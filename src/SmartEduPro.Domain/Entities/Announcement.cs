namespace SmartEduPro.Domain.Entities;

public class Announcement
{
    public Guid Id { get; set; }
    public Guid Institute_Id { get; set; }
    public Guid Author_Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Guid? Target_Class_Id { get; set; }
    public User_Role? Target_Role { get; set; }
    public bool Is_Pinned { get; set; }
    public bool Send_Push { get; set; } = true;
    public bool Send_Sms { get; set; }
    public bool Send_Whatsapp { get; set; }
    public DateTime Published_At { get; set; }
    public DateTime? Expires_At { get; set; }
    public DateTime Created_At { get; set; }
}
