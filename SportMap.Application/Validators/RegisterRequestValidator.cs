using FluentValidation;
using SportMap.Application.DTOs.Auth;

namespace SportMap.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Matches(@"^01[0125][0-9]{8}$")
            .WithMessage("Invalid Egyptian phone number");
    }
}