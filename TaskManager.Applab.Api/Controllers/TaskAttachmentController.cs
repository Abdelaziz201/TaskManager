using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Applab.Application.DTOs;
using TaskManager.Applab.Application.Services;

namespace TaskManager.Applab.Api.Controllers;

[ApiController]
[Route("api/task/{taskId}/attachments")]
public class TaskAttachmentController : ControllerBase
{
    private readonly TaskAttachmentService _attachmentService;
    private readonly IValidator<UploadAttachmentDto> _validator;

    public TaskAttachmentController(TaskAttachmentService attachmentService, IValidator<UploadAttachmentDto> validator)
    {
        _attachmentService = attachmentService;
        _validator = validator;
    }


    // POST /api/task/5/attachments
    [HttpPost]
    public async Task<IActionResult> Upload(int taskId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file provided" });

        var dto = new UploadAttachmentDto
        {
            TaskItemId = taskId,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length
        };

        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, message = string.Join("; ", errors) });
        }

        using var stream = file.OpenReadStream();
        var result = await _attachmentService.UploadAsync(dto, stream);   // ⬅ now passes dto + stream

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET /api/task/5/attachments
    [HttpGet]
    public async Task<IActionResult> GetAll(int taskId)
    {
        var result = await _attachmentService.GetByTaskIdAsync(taskId);
        return Ok(result);
    }

    // GET /api/task/5/attachments/12/download
    [HttpGet("{attachmentId}/download")]
    public async Task<IActionResult> Download(int taskId, int attachmentId)
    {
        var result = await _attachmentService.DownloadAsync(attachmentId);
        if (result == null) return NotFound();

        Response.Headers["Content-Disposition"] = $"inline; filename=\"{result.Value.fileName}\"";
        return File(result.Value.stream, result.Value.contentType);
    }

    // DELETE /api/task/5/attachments/12
    [HttpDelete("{attachmentId}")]
    public async Task<IActionResult> Delete(int taskId, int attachmentId)
    {
        var result = await _attachmentService.DeleteAsync(attachmentId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}