namespace SmartEduPro.Domain.Entities;
public class Institute_Admin
{
    public Guid Id { get; set; }
    [ForeignKey("Users")]
    public Guid User_Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    [ForeignKey("Branches")]
    public Guid? Branch_Id { get; set; }
    public string Permission { get; set; } = string.Empty;
    public DateTime Created_At { get; set; }
}
