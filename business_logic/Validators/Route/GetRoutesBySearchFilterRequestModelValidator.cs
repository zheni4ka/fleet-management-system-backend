using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Route
{
    public class GetRoutesBySearchFilterRequestModelValidator : AbstractValidator<GetRoutesBySearchFilterRequestModel>
    {
        public GetRoutesBySearchFilterRequestModelValidator()
        {
            RuleFor(x => x.DepartureToDate)
                .GreaterThanOrEqualTo(x => x.DepartureFromDate)
                .When(x => x.DepartureFromDate.HasValue && x.DepartureToDate.HasValue)
                .WithMessage("Кінцева дата відправлення не може бути раніше за початкову.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .When(x => x.Status.HasValue)
                .WithMessage("Статус маршруту має бути коректним значенням.");

            RuleFor(x => x.DriverId)
                .GreaterThan(0)
                .When(x => x.DriverId.HasValue)
                .WithMessage("Ідентифікатор водія має бути більшим за 0.");

            RuleFor(x => x.AutoId)
                .GreaterThan(0)
                .When(x => x.AutoId.HasValue)
                .WithMessage("Ідентифікатор автомобіля має бути більшим за 0.");
        }
    }
}
