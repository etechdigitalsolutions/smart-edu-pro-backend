namespace SmartEduPro.Domain.Entities;
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password_Hash { get; set; } = string.Empty;
    public User_Role Role { get; set; }
    public string First_Name { get; set; } = string.Empty;
    public string Last_Name { get; set;} = string.Empty;
    public string Avatar_Url {  get; set; } = string.Empty;
    public bool Is_Active { get; set; }
    public bool Is_Verified { get; set; }
    public DateTime Last_Login_At { get; set; }
    public string Fcm_Token { get; set; } = string.Empty;
    public string Apns_Token {  get; set; } = string.Empty;
    public string Preferred_Lan { get; set; } = "en";
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
    public DateTime Deleted_At { get; set; }
}