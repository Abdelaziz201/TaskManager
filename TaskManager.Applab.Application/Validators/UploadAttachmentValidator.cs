using FluentValidation;
using TaskManager.Applab.Application.DTOs;

namespace TaskManager.Applab.Application.Validators
{
    public class UploadAttachmentValidator : AbstractValidator<UploadAttachmentDto>
    {
        private static readonly string[] BlockedExtensions = { ".exe", ".bat", ".cmd", ".sh", ".dll", ".msi" };

        public UploadAttachmentValidator()
        {
            RuleFor(x => x.TaskItemId)
                .GreaterThan(0).WithMessage("A valid task must be specified");

            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required")
                .MaximumLength(255).WithMessage("File name cannot exceed 255 characters")
                .Must(name => !BlockedExtensions.Contains(Path.GetExtension(name).ToLowerInvariant()))
                .WithMessage("This file type is not allowed for security reasons");

            RuleFor(x => x.FileSizeBytes)
                .GreaterThan(0).WithMessage("File appears to be empty")
                .LessThanOrEqualTo(5 * 1024 * 1024).WithMessage("File cannot exceed 5MB");

            RuleFor(x => x.ContentType)
                .NotEmpty().WithMessage("File content type could not be determined");
        }
    }
}