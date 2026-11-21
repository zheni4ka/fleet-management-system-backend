using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Route
{
    public class UpdateRouteRequestModelValidator : AbstractValidator<UpdateRouteRequestModel>
    {
        public UpdateRouteRequestModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName("Ідентифікатор");

            RuleFor(x => x.RowVersion)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .WithName("Версія запису");

            RuleFor(x => x.StartLocationId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName("Початкова локація");

            RuleFor(x => x.DestinationLocationId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName("Кінцева локація");

            RuleFor(x => x.AutoId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName("Автомобіль");

            RuleFor(x => x.DriverId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName("Водій");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage(_ => ValidationMessages.UnsupportedValue)
                .WithName("Статус");

            RuleFor(x => x)
                .Must(model => model.DepartureTimeUtc.HasValue == model.ArrivalTimeUtc.HasValue)
                .WithMessage(_ => ValidationMessages.RouteTimesMustBeProvidedTogether);

            When(x => x.DepartureTimeUtc.HasValue && x.ArrivalTimeUtc.HasValue, () =>
            {
                RuleFor(x => x.DepartureTimeUtc!.Value)
                    .Must(departureTimeUtc => departureTimeUtc.Kind == DateTimeKind.Utc).WithMessage(_ => ValidationMessages.UtcTimeRequired)
                    .LessThan(x => x.ArrivalTimeUtc!.Value).WithMessage(_ => ValidationMessages.DepartureMustPrecedeArrival)
                    .OverridePropertyName(nameof(UpdateRouteRequestModel.DepartureTimeUtc));

                RuleFor(x => x.ArrivalTimeUtc!.Value)
                    .Must(arrivalTimeUtc => arrivalTimeUtc.Kind == DateTimeKind.Utc).WithMessage(_ => ValidationMessages.UtcTimeRequired)
                    .OverridePropertyName(nameof(UpdateRouteRequestModel.ArrivalTimeUtc));
            });
        }
    }
}
