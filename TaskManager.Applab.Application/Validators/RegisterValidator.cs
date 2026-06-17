using FluentValidation;
using TaskManager.Applab.Application.DTOs;

namespace TaskManager.Applab.Application.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
       public RegisterValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
                .MaximumLength(20).WithMessage("Username must be at most 20 characters long");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
                .Matches("[A-Z]").WithMessage("password must contain at least one uppercase letter")
                .Matches("[0-9]").WithMessage("password must contain at least one number");

        }
    }

}
