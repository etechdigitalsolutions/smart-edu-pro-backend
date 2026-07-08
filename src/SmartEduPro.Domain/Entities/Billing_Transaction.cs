namespace SmartEduPro.Domain.Entities;
public class Billing_Transaction
{
    public Guid Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    public Subscription_Plans Plan { get; set; }
    public decimal Amount_Lkr { get; set; }
    public string Currency { get; set; } = "LKR";
    public string Payment_Method { get; set; } = string.Empty;
    public string Payment_Ref { get; set; } = string.Empty;
    public Payment_Status Status { get; set; }
    public NpgsqlRange<DateTime> BillingPeriod { get; set; }
    public string Invoice_Url { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime Created_At { get; set; }
}