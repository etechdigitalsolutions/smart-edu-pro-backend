namespace SmartEduPro.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid? Institute_Id { get; set; }
    public Guid Recipient_Id { get; set; }
    public Notification_Type Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Data { get; set; } = "{}";
    public Notification_Channel Channel { get; set; }
    public bool Is_Read { get; set; }
    public DateTime? Read_At { get; set; }
    public bool Is_Delivered { get; set; }
    public DateTime? Delivered_At { get; set; }
    public string? Delivery_Error { get; set; }
    public DateTime Created_At { get; set; }
}
