namespace TaskManager.Applab.Domain.Entities;

public class TaskAttachment
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;


    public string FileName { get; set; } = string.Empty;  // original name shown to user
    public string StoredFileName { get; set; } = string.Empty;  // actual name on disk (GUID-based)
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}