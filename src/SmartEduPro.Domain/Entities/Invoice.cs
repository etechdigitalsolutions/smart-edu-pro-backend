namespace SmartEduPro.Domain.Entities;
public class Invoice
{
    [Key]
    public Guid Id { get; set; }
    public string Invoice_No { get; set; } = string.Empty;
    [ForeignKey("Students")]
    public Guid Student_Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    [ForeignKey("Fee_Records")]
    public Guid Fee_Record_Id { get; set; }
    public decimal Amount_Lkr { get; set; }
    public DateOnly Issued_Date { get; set; }
    public DateOnly Due_Date { get; set; }
    public string Pdf_Url { get; set; } = string.Empty;
    public string Sent_Via { get; set; } = string.Empty;
    public DateTime Created_At { get; set; }
    public Student Students { get; set; }
    public Institute Institutes { get; set; }
    public Student_Fee_Record Fee_Records { get; set; }
}