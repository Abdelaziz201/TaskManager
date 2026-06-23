namespace TaskManager.Applab.Application.DTOs;

public class UploadAttachmentDto
{
    public int TaskItemId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}