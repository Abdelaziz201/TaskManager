using FluentValidation;
using TaskManager.Applab.Application.DTOs;


namespace TaskManager.Applab.Application.Validators
{
    public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
    {
        public CreateTaskValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is requierd")
                .MinimumLength(3).WithMessage("Title must be at least 3 characters")
                .MaximumLength(30).WithMessage("Title cannot exceed 30 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.DueDate)
                .NotEmpty().WithMessage("Due Date is required")
                .GreaterThan(DateTime.UtcNow).WithMessage("Due Date must be in the future");

        }
    }
}
