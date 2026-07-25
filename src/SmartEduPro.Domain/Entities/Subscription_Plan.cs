namespace SmartEduPro.Domain.Entities;
public class Subscription_Plan
{
    public Guid Id { get; set; }
    public Subscription_Plans Name { get; set; }
    public string Display_Name { get; set; } = string.Empty;
    public decimal Price_Lkr { get; set; }
    public int Max_Student { get; set; }
    public int Max_Teacher { get; set; }
    public int Max_Branch { get; set; } = 1;
    public string Feature { get; set; } = string.Empty;
    public bool Is_Active { get; set; }
    public DateTime Created_at { get; set; }
}