namespace SmartEduPro.Domain.Entities;

public class Activity_Log
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Target_Type { get; set; } = string.Empty;
    public Guid? Target_Id { get; set; }
    public string? Ip_Address { get; set; }
    public Guid? Institute_Id { get; set; }
    public DateTime Created_At { get; set; }
}
