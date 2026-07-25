namespace SmartEduPro.Domain.Entities;
public class Teacher
{
    public Guid Id { get; set; }
    [ForeignKey("Users")]
    public Guid User_Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    [StringLength(20)]
    public string Teacher_No { get; set; } = string.Empty;
    [StringLength(15)]
    public string Nic { get; set; } = string.Empty;
    public DateOnly Date_Of_Birth { get; set; }
    public Gender Gender { get; set; }
    public string Qualification { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int Experience_Yrs { get; set; }
    public string Photo_Url { get; set; } = string.Empty;
    public string Bank_Name { get; set; } = string.Empty;
    public string Bank_Account { get; set; } = string.Empty;
    [Column(TypeName = "decimal(10,2)")]
    public decimal Salary_Lkr { get; set; }
    public DateOnly Joined_Date { get; set; }
    public bool Is_Active { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
}