using business_logic.Constants;
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
                .WithMessage(_ => ValidationMessages.DateRangeEndBeforeStart);

            RuleFor(x => x.Status)
                .IsInEnum()
                .When(x => x.Status.HasValue)
                .WithMessage(_ => ValidationMessages.UnsupportedValue)
                .WithName(_ => FieldDisplayNames.RouteStatus);

            RuleFor(x => x.DriverId)
                .GreaterThan(0)
                .When(x => x.DriverId.HasValue)
                .WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.Driver);

            RuleFor(x => x.AutoId)
                .GreaterThan(0)
                .When(x => x.AutoId.HasValue)
                .WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.Auto);
        }
    }
}
