using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Route
{
    public class GetRoutesPeriodReportRequestModelValidator : AbstractValidator<GetRoutesPeriodReportRequestModel>
    {
        public GetRoutesPeriodReportRequestModelValidator()
        {
            RuleFor(x => x.PeriodEndDate)
                .GreaterThanOrEqualTo(x => x.PeriodStartDate)
                .WithMessage("Кінець періоду не може бути раніше за його початок.");

            RuleFor(x => x.PeriodEndDate)
                .Must((requestModel, periodEndDate) =>
                    periodEndDate.DayNumber - requestModel.PeriodStartDate.DayNumber < RouteReportConstraints.PeriodReportMaxDaysCount)
                .WithMessage($"Період звіту не може перевищувати {RouteReportConstraints.PeriodReportMaxDaysCount} днів.");
        }
    }
}
