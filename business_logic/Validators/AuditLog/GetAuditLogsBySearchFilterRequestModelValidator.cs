using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators
{
    public class GetAuditLogsBySearchFilterRequestModelValidator : AbstractValidator<GetAuditLogsBySearchFilterRequestModel>
    {
        public GetAuditLogsBySearchFilterRequestModelValidator()
        {
            RuleFor(x => x.Action)
                .IsInEnum()
                .When(x => x.Action.HasValue)
                .WithMessage(x => $"Дія журналу '{x.Action}' не підтримується.");

            RuleFor(x => x.TimeToDate)
                .GreaterThanOrEqualTo(x => x.TimeFromDate)
                .When(x => x.TimeFromDate.HasValue && x.TimeToDate.HasValue)
                .WithMessage(x => $"Кінцева дата ({x.TimeToDate:dd.MM.yyyy}) не може бути раніше за початкову ({x.TimeFromDate:dd.MM.yyyy}).");
        }
    }
}
