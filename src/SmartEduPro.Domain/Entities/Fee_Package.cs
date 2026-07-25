namespace SmartEduPro.Domain.Entities;
public class Fee_Package
{
    [Key]
    public Guid Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [ForeignKey("Classes")]
    public Guid Class_Id { get; set; }
    public decimal Amount_Lkr { get; set; }
    public string Billing_Cycle { get; set; } = string.Empty;
    public int Due_Day { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal Late_Fee_Lkr { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Is_Active { get; set; }
    public DateTime Created_At { get; set; }
    public Institute Institutes { get; set; }
    public Class Classes { get; set; }
}