namespace SmartEduPro.Domain.Entities;
public class Exam_Result
{
    [Key]
    public Guid Id { get; set; }
    [ForeignKey("Exams")]
    public Guid Exam_Id { get; set; }
    [ForeignKey("Students")]
    public Guid Student_Id { get; set; }
    public decimal Marks_Obtained { get; set; }
    public Sl_Grade Grade { get; set; }
    public int Rank_Int_Class { get; set; }
    public bool Is_Absent { get; set; }
    public string Teacher_Notes { get; set; } = string.Empty;
    public bool Parent_Notified { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime Updated_At { get; set; }
    public Exam Exams { get; set; }
    public Student Students { get; set; }
}