namespace SmartEduPro.Domain.Entities;
public class Institute
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Registration_No { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Logo_Url { get; set; } = string.Empty;
    public string Address_Line1 { get; set; } = string.Empty;
    public string Address_Line2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string Postal_Code { get; set; } = string.Empty;
    public string Owner_Name { get; set; } = string.Empty;
    public string Owner_Phone { get; set; } = string.Empty;
    public string Owner_Email { get; set; } = string.Empty;
    public Subscription_Plans Subscription_Plan { get; set; } = Subscription_Plans.TRIAL;
    public Subscription_Status Subscription_Status { get; set; }
    public DateOnly Subscription_Start { get; set; }
    public DateOnly Subscription_End { get; set; }
    public DateOnly Trial_End { get; set; }
    public int Max_student {  get; set; }
    public int Max_Teacher { get; set; }
    public string Setting {  get; set; } = string.Empty;
    public bool Is_Active { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
    public DateTime Deleted_At { get; set; }
}