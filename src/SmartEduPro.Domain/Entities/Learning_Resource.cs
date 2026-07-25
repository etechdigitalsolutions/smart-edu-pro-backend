namespace SmartEduPro.Domain.Entities;

public class Learning_Resource
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey("Institutes")]
    public Guid Institute_Id { get; set; }

    [ForeignKey("Classes")]
    public Guid? Class_Id { get; set; }

    [ForeignKey("Subjects")]
    public Guid? Subject_Id { get; set; }

    [ForeignKey("Users")]
    public Guid Uploaded_By { get; set; }

    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [StringLength(20)]
    public string? File_Type { get; set; }

    [StringLength(500)]
    public string File_Url { get; set; } = string.Empty;

    [Column(TypeName = "decimal(8,2)")]
    public decimal? File_Size_Mb { get; set; }

    public int Download_Count { get; set; }
    public bool Is_Published { get; set; } = true;
    public DateTime Created_At { get; set; }

    public Institute? Institutes { get; set; }
    public Class? Classes { get; set; }
    public Subject? Subjects { get; set; }
    public User? Users { get; set; }
}
