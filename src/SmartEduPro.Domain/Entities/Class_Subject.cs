namespace SmartEduPro.Domain.Entities;
public class Class_Subject
{
    public Guid Id { get; set; }
    [ForeignKey("Classes")]
    public Guid Class_Id { get; set; }
    [ForeignKey("Subjects")]
    public Guid Subject_Id { get; set; }
    [ForeignKey("Teachers")]
    public Guid Teacher_Id { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal Monthly_Fee { get; set; }
    public DateTime Created_At { get; set; }
}