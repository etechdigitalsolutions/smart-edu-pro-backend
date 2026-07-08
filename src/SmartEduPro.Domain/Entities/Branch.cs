namespace SmartEduPro.Domain.Entities;
public class Branch
{
    public Guid Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    [ForeignKey("Managers")]
    public Guid Manager_Id { get; set; }
    public bool Is_Active { get; set; } = true;
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
    public Institute Institutes { get; set; }
}