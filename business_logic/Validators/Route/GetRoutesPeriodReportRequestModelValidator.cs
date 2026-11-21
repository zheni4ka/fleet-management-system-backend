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
                .WithMessage(_ => ValidationMessages.PeriodEndBeforeStart);

            RuleFor(x => x.PeriodEndDate)
                .Must((requestModel, periodEndDate) =>
                    periodEndDate.DayNumber - requestModel.PeriodStartDate.DayNumber < RouteReportConstraints.PeriodReportMaxDaysCount)
                .WithMessage(_ => string.Format(ValidationMessages.PeriodLengthExceededTemplate, RouteReportConstraints.PeriodReportMaxDaysCount));
        }
    }
}
