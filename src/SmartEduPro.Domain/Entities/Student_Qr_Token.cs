namespace SmartEduPro.Domain.Entities;
public class Student_Qr_Token
{
    [Key]
    public Guid Id { get; set; }
    [ForeignKey("Students")]
    public Guid Student_Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Qr_Data { get; set; } = string.Empty;
    public DateTime Expires_At { get; set; }
    public DateTime Last_Used_At { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
    public Student Students { get; set; }
}