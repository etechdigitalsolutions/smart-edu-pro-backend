using SmartEduPro.Domain.Enums;
using System;

namespace SmartEduPro.Domain.Entities;
public class Fee_Payment
{
    [Key]
    public Guid Id { get; set; }
    [ForeignKey("Fee_Records")]
    public Guid Fee_Record_Id { get; set; }
    [ForeignKey("Students")]
    public Guid Student_Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    public decimal Amount_Lkr { get; set; }
    public Payment_Method Payment_Method { get; set; }
    public string Payment_Ref { get; set; }
    public DateOnly Payment_Date { get; set; }
    [ForeignKey("Users")]
    public Guid Received_By { get; set; }
    public string Receipt_No { get; set; } = string.Empty;
    public string Receipt_Url { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTime Created_At { get; set; }
    public Student_Fee_Record Fee_Records { get; set; }
    public Student Students { get; set; }
    public Institute Institutes { get; set; }
    public User Users { get; set; }
}