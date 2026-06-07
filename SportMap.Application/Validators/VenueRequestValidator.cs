using FluentValidation;
using SportMap.Application.DTOs.Venues;

namespace SportMap.Application.Validators;

public class VenueRequestValidator : AbstractValidator<VenueRequest>
{
    public VenueRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Venue name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Invalid latitude");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Invalid longitude");

        RuleFor(x => x.PricePerHour)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0");
    }
}