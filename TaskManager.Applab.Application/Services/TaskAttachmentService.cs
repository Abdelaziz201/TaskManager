using TaskManager.Applab.Application.Common;
using TaskManager.Applab.Application.DTOs;
using TaskManager.Applab.Application.Interfaces;
using TaskManager.Applab.Application.Settings;
using TaskManager.Applab.Domain.Entities;
using Microsoft.Extensions.Options;

namespace TaskManager.Applab.Application.Services;

public class TaskAttachmentService
{
    private readonly ITaskAttachmentRepository _attachmentRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly FileStorageSettings _settings;

    public TaskAttachmentService(
        ITaskAttachmentRepository attachmentRepository,
        ITaskRepository taskRepository,
        IFileStorageService fileStorage,
        IOptions<FileStorageSettings> settings)
    {
        _attachmentRepository = attachmentRepository;
        _taskRepository = taskRepository;
        _fileStorage = fileStorage;
        _settings = settings.Value;
    }

    public async Task<ApiResponse<AttachmentDto>> UploadAsync(UploadAttachmentDto dto, Stream fileStream)
    {
        var task = await _taskRepository.GetByIdAsync(dto.TaskItemId);
        if (task == null)
            return ApiResponse<AttachmentDto>.Fail($"Task with id {dto.TaskItemId} not found");

        if (dto.FileSizeBytes > _settings.MaxFileSizeBytes)
            return ApiResponse<AttachmentDto>.Fail($"File exceeds the {_settings.MaxFileSizeBytes / 1024 / 1024}MB limit");

        var (storedFileName, sizeBytes) = await _fileStorage.SaveFileAsync(fileStream, dto.FileName);

        var attachment = new TaskAttachment
        {
            TaskItemId = dto.TaskItemId,
            FileName = dto.FileName,
            StoredFileName = storedFileName,
            ContentType = dto.ContentType,
            FileSizeBytes = sizeBytes
        };

        await _attachmentRepository.AddAsync(attachment);

        var result = MapToDto(attachment); // ← map before returning
        return ApiResponse<AttachmentDto>.Ok(result, "File uploaded successfully");
    }

    public async Task<ApiResponse<List<AttachmentDto>>> GetByTaskIdAsync(int taskId)
    {
        var attachments = await _attachmentRepository.GetByTaskIdAsync(taskId);
        var result = attachments.Select(MapToDto).ToList();
        return ApiResponse<List<AttachmentDto>>.Ok(result);
    }

    public async Task<ApiResponse> DeleteAsync(int attachmentId)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
        if (attachment == null)
            return ApiResponse.Fail("Attachment not found");

        await _fileStorage.DeleteFileAsync(attachment.StoredFileName);
        await _attachmentRepository.DeleteAsync(attachment);

        return ApiResponse.Ok("Attachment deleted");
    }

    public async Task<(Stream stream, string fileName, string contentType)?> DownloadAsync(int attachmentId)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
        if (attachment == null) return null;

        var stream = _fileStorage.GetFileStream(attachment.StoredFileName);
        return (stream, attachment.FileName, attachment.ContentType);
    }

    private static AttachmentDto MapToDto(TaskAttachment a) => new()
    {
        Id = a.Id,
        TaskItemId = a.TaskItemId,
        FileName = a.FileName,
        ContentType = a.ContentType,
        FileSizeBytes = a.FileSizeBytes,
        UploadedAt = a.UploadedAt
    };
}
