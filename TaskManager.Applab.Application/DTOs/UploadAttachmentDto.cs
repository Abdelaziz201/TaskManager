using Microsoft.AspNetCore.Http;

namespace TaskManager.Applab.Application.DTOs;

public class UploadAttachmentDto
{
    public int TaskItemId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;

    public IFormFile UploadedFile { get; set; }

    public long FileSizeBytes { get; set; }
}