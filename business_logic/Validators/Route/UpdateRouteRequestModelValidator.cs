using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Route
{
    public class UpdateRouteRequestModelValidator : AbstractValidator<UpdateRouteRequestModel>
    {
        public UpdateRouteRequestModelValidator()
        {
            Include(new RouteAddressesRequestModelValidator(areAddressesRequired: false));

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.Id);

            RuleFor(x => x.RowVersion)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .WithName(_ => FieldDisplayNames.RowVersion);

            RuleFor(x => x.StartLocationId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.StartLocation);

            RuleFor(x => x.DestinationLocationId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.DestinationLocation);

            RuleFor(x => x.AutoId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.Auto);

            RuleFor(x => x.DriverId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.Driver);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage(_ => ValidationMessages.UnsupportedValue)
                .WithName(_ => FieldDisplayNames.Status);

            RuleFor(x => x)
                .Must(model => model.DepartureTimeUtc.HasValue == model.ArrivalTimeUtc.HasValue)
                .WithMessage(_ => ValidationMessages.RouteTimesMustBeProvidedTogether);

            When(x => x.DepartureTimeUtc.HasValue && x.ArrivalTimeUtc.HasValue, () =>
            {
                RuleFor(x => x.DepartureTimeUtc!.Value)
                    .Must(departureTimeUtc => departureTimeUtc.Kind == DateTimeKind.Utc).WithMessage(_ => ValidationMessages.UtcTimeRequired)
                    .LessThan(x => x.ArrivalTimeUtc!.Value).WithMessage(_ => ValidationMessages.DepartureMustPrecedeArrival)
                    .OverridePropertyName(nameof(UpdateRouteRequestModel.DepartureTimeUtc))
                    .WithName(_ => FieldDisplayNames.DepartureTime);

                RuleFor(x => x.ArrivalTimeUtc!.Value)
                    .Must(arrivalTimeUtc => arrivalTimeUtc.Kind == DateTimeKind.Utc).WithMessage(_ => ValidationMessages.UtcTimeRequired)
                    .OverridePropertyName(nameof(UpdateRouteRequestModel.ArrivalTimeUtc))
                    .WithName(_ => FieldDisplayNames.ArrivalTime);
            });
        }
    }
}
