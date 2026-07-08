namespace SmartEduPro.Domain.Entities;
public class Otp_Code
{
    public Guid Id { get; set; }
    [StringLength(255)]
    public string Identifier { get; set; } = string.Empty;
    [StringLength(6)]
    public string Code { get; set; } = string.Empty;
    [StringLength(50)]
    public string Purpose {  get; set; } = string.Empty;
    public int Attempts { get; set; } = 0;
    public bool Verified { get; set; } = false;
    public DateTime Expires_At { get; set; }
    public DateTime Created_At { get; set; } = DateTime.UtcNow;
}