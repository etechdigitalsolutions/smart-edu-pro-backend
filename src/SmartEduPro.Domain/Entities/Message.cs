namespace SmartEduPro.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid Institute_Id { get; set; }
    public Guid Sender_Id { get; set; }
    public Guid Recipient_Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool Is_Read { get; set; }
    public DateTime? Read_At { get; set; }
    public string? Attachment_Url { get; set; }
    public DateTime Created_At { get; set; }
}
