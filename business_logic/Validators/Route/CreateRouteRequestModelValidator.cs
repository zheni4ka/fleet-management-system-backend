using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators
{
    public class CreateRouteRequestModelValidator : AbstractValidator<CreateRouteRequestModel>
    {
        public CreateRouteRequestModelValidator()
        {
            RuleFor(x => x.StartLocationId)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Початкова локація");

            RuleFor(x => x.DestinationLocationId)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .NotEqual(x => x.StartLocationId).WithMessage("Початкова та кінцева локації мають відрізнятися.")
                .WithName("Кінцева локація");

            RuleFor(x => x.AutoId)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Автомобіль");

            RuleFor(x => x.DriverId)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Водій");

            RuleFor(x => x.DepartureTimeUtc)
                .Must(departureTimeUtc => departureTimeUtc.Kind == DateTimeKind.Utc).WithMessage(ValidationMessages.UtcTimeRequired)
                .LessThan(x => x.ArrivalTimeUtc).WithMessage("Час відправлення має бути раніше за час прибуття.")
                .WithName("Час відправлення");

            RuleFor(x => x.ArrivalTimeUtc)
                .Must(arrivalTimeUtc => arrivalTimeUtc.Kind == DateTimeKind.Utc).WithMessage(ValidationMessages.UtcTimeRequired)
                .WithName("Час прибуття");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage(ValidationMessages.UnsupportedValue)
                .WithName("Статус");
        }
    }
}
