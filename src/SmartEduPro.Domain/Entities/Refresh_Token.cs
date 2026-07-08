namespace SmartEduPro.Domain.Entities;
public class Refresh_Token
{
    public Guid Id { get; set; }
    [ForeignKey("Users")]
    public Guid User_Id { get; set; }
    public string Token_Hash { get; set; } = string.Empty;
    public string Device_Info {  get; set; } = string.Empty;
    public required IPAddress Ip_Address {  get; set; }
    public DateTime Expire_At { get; set; }
    public DateTime Revoked_At { get; set; }
    public DateTime Created_At { get; set; }
    public User Users { get; set; }
}