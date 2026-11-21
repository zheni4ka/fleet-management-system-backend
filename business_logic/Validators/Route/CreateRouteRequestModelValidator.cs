using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;
using business_logic.Validators.Route;

namespace business_logic.Validators
{
    public class CreateRouteRequestModelValidator : AbstractValidator<CreateRouteRequestModel>
    {
        public CreateRouteRequestModelValidator()
        {
            Include(new RouteAddressesRequestModelValidator(areAddressesRequired: true));

            RuleFor(x => x.StartLocationId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.StartLocation);

            RuleFor(x => x.DestinationLocationId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .NotEqual(x => x.StartLocationId).WithMessage(_ => ValidationMessages.StartAndDestinationLocationsMustDiffer)
                .WithName(_ => FieldDisplayNames.DestinationLocation);

            RuleFor(x => x.AutoId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.Auto);

            RuleFor(x => x.DriverId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.Driver);

            RuleFor(x => x.DepartureTimeUtc)
                .Must(departureTimeUtc => departureTimeUtc.Kind == DateTimeKind.Utc).WithMessage(_ => ValidationMessages.UtcTimeRequired)
                .LessThan(x => x.ArrivalTimeUtc).WithMessage(_ => ValidationMessages.DepartureMustPrecedeArrival)
                .WithName(_ => FieldDisplayNames.DepartureTime);

            RuleFor(x => x.ArrivalTimeUtc)
                .Must(arrivalTimeUtc => arrivalTimeUtc.Kind == DateTimeKind.Utc).WithMessage(_ => ValidationMessages.UtcTimeRequired)
                .WithName(_ => FieldDisplayNames.ArrivalTime);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage(_ => ValidationMessages.UnsupportedValue)
                .WithName(_ => FieldDisplayNames.Status);
        }
    }
}
