namespace SmartEduPro.Domain.Entities;
public class Student
{
    public Guid Id { get; set; }
    public Guid Institute_Id { get; set; }
    [ForeignKey("Users")]
    public Guid User_Id { get; set; }
    [StringLength(20)]
    public string Student_No { get; set; } = string.Empty;
    [StringLength(100)]
    public string Nic_Or_Birth_Cert { get; set; } = string.Empty;
    public DateOnly Date_Of_Birth { get; set; }
    public Gender Gender { get; set; }
    [StringLength(200)]
    public string School_Name { get; set; } = string.Empty;
    [StringLength(500)]
    public string Home_Address { get; set; } = string.Empty;
    [StringLength(100)]
    public string City { get; set; } = string.Empty;
    [StringLength(100)]
    public string District { get; set; } = string.Empty;
    [StringLength(200)]
    public string Guardian_Name { get; set; } = string.Empty;
    [StringLength(20)]
    public string Guardian_Phone { get; set; } = string.Empty;
    [StringLength(50)]
    public string Guardian_Relation { get; set; } = string.Empty;
    public string Photo_Url { get; set; } = string.Empty;
    public string Qr_Code_Url { get; set; } = string.Empty;
    public string Qr_Secret { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateOnly Enrolled_At { get; set; }
    public bool Is_Active { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
    public User Users { get; set; }
}