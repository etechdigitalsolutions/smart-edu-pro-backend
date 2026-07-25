namespace SmartEduPro.Domain.Entities;
public class Recording
{
    [Key]
    public Guid Id { get; set; }
    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }
    [ForeignKey("Sessions")]
    public Guid Session_Id { get; set; }
    [ForeignKey("Classes")]
    public Guid Class_Id { get; set; }
    [ForeignKey("Subjects")]
    public Guid Subject_Id { get; set; }
    [ForeignKey("Teachers")]
    public Guid Teacher_Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Duration_Sec { get; set; }
    public string File_Url { get; set; } = string.Empty;
    public string Thumbnail_Url { get; set; } = string.Empty;
    [Column(TypeName="decimal(8,2)")]
    public decimal File_Size_Mb { get; set; }
    public int View_Count { get; set; }
    public bool Is_Published { get; set; }
    public DateTime Recorded_At { get; set; }
    public DateTime Created_At { get; set; }
    public Institute Institutes { get; set; }
    public Live_Session Sessions { get; set; }
    public Class Classes { get; set; }
    public Subject Subjects { get; set; }
    public Teacher Teachers { get; set; }
}