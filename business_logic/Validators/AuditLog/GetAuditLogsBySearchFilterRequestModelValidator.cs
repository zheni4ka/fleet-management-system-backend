using business_logic.Constants;
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
                .WithMessage(_ => ValidationMessages.UnsupportedValue)
                .WithName(_ => FieldDisplayNames.AuditAction);

            RuleFor(x => x.TimeToDate)
                .GreaterThanOrEqualTo(x => x.TimeFromDate)
                .When(x => x.TimeFromDate.HasValue && x.TimeToDate.HasValue)
                .WithMessage(_ => ValidationMessages.DateRangeEndBeforeStart);
        }
    }
}
