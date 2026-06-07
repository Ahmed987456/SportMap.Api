using FluentValidation;
using SportMap.Application.DTOs.Reviews;

namespace SportMap.Application.Validators;

public class ReviewRequestValidator : AbstractValidator<ReviewRequest>
{
    public ReviewRequestValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0).WithMessage("BookingId is required");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required")
            .MaximumLength(500).WithMessage("Comment must not exceed 500 characters");
    }
}