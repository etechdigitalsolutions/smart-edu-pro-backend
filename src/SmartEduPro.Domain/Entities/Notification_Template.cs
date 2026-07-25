namespace SmartEduPro.Domain.Entities;

public class Notification_Template
{
    public Guid Id { get; set; }
    public Guid? Institute_Id { get; set; }
    public Notification_Type Type { get; set; }
    public Notification_Channel Channel { get; set; }
    public string Title_Template { get; set; } = string.Empty;
    public string Body_Template { get; set; } = string.Empty;
    public bool Is_Active { get; set; } = true;
    public DateTime Created_At { get; set; }
}
