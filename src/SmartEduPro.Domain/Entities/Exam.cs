namespace SmartEduPro.Domain.Entities;
public class Exam
{
    [Key]
    public Guid Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    [ForeignKey("Classes")]
    public Guid Class_Id { get; set; }
    [ForeignKey("Subjects")]
    public Guid Subject_Id { get; set; }
    [ForeignKey("Teachers")]
    public Guid Teacher_Id { get; set; }
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;
    public Exam_Type Exam_Type { get; set; }
    public DateOnly Exam_Date { get; set; }
    public TimeOnly Start_Time { get; set; }
    public int Duration_Min { get; set; }
    public decimal Total_Mark { get; set; }
    public decimal Pass_Mark { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Instruction { get; set; } = string.Empty;
    public bool Is_Published { get; set; }
    public DateTime Published_At { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
    public Institute Institutes { get; set; }
    public Class Classes { get; set; }
    public Subject Subjects { get; set; }
    public Teacher Teachers { get; set; }
}