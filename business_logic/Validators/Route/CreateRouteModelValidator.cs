using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators
{
    public class CreateRouteModelValidator : AbstractValidator<CreateRouteModel>
    {
        public CreateRouteModelValidator()
        {
            RuleFor(x => x.StartLocationId)
                .GreaterThan(0)
                .WithMessage("Start location id must be greater than 0.");

            RuleFor(x => x.DestinationLocationId)
                .GreaterThan(0)
                .WithMessage("Destination location id must be greater than 0.");

            RuleFor(x => x.DestinationLocationId)
                .NotEqual(x => x.StartLocationId)
                .WithMessage("Destination must differ from the start location.");

            RuleFor(x => x.AutoId)
                .GreaterThan(0)
                .WithMessage("Auto ID must be greater than 0.");

            RuleFor(x => x.DriverId)
                .GreaterThan(0)
                .WithMessage("Driver ID must be greater than 0.");

            RuleFor(x => x.DepartureTime)
                .LessThan(x => x.ArrivalTime)
                .WithMessage("Departure time must be before arrival time.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Status must be a valid value.");
        }
    }
}
