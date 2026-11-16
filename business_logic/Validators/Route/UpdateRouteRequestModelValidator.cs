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
        }
    }
}
