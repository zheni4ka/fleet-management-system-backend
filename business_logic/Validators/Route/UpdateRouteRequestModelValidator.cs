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
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Ідентифікатор");

            RuleFor(x => x.StartLocationId)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Початкова локація");

            RuleFor(x => x.DestinationLocationId)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Кінцева локація");

            RuleFor(x => x.AutoId)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Автомобіль");

            RuleFor(x => x.DriverId)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Водій");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage(ValidationMessages.UnsupportedValue)
                .WithName("Статус");
        }
    }
}
