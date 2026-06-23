using Microsoft.AspNetCore.Http;

namespace TaskManager.Applab.Application.DTOs;

public class AttachmentDto
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}