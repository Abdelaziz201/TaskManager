namespace TaskManager.Applab.Web.Models;

public class AttachmentViewModel
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}