using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Route
{
    public class UpdateRouteRequestModelValidator : AbstractValidator<UpdateRouteRequestModel>
    {
        public UpdateRouteRequestModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
            RuleFor(x => x.StartLocationId)
                .GreaterThan(0).WithMessage("Start location ID is required.");
            RuleFor(x => x.DestinationLocationId)
                .GreaterThan(0).WithMessage("Destination location ID is required.");
            RuleFor(x => x.AutoId)
                .GreaterThan(0).WithMessage("AutoId must be greater than 0.");
            RuleFor(x => x.DriverId)
                .GreaterThan(0).WithMessage("DriverId must be greater than 0.");
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Status must be a valid value.");
        }
    }
}
