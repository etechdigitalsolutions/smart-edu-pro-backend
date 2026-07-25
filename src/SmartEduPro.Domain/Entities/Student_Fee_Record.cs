namespace SmartEduPro.Domain.Entities;
public class Student_Fee_Record
{
    public Guid Id { get; set; }
    [ForeignKey("Students")]
    public Guid Student_Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    [ForeignKey("Fee_Packages")]
    public Guid Fee_Package_Id { get; set; }
    public DateOnly Billing_Month { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount_Due { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal Late_Fee { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal Discount { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal Balance { get; set; }
    public Fee_Status Fee_Status { get; set; }
    public DateOnly Due_Date { get; set; }
    public int Reminder_Count { get; set; } = 0;
    public DateTime Last_Reminder_At { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
    public Student Students { get; set; }
    public Institute Institutes { get; set; }
    public Fee_Package Fee_Packages { get; set; }
}