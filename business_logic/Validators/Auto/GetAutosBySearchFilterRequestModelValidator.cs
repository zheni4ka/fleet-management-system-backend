using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Auto
{
    public class GetAutosBySearchFilterRequestModelValidator : AbstractValidator<GetAutosBySearchFilterRequestModel>
    {
        public GetAutosBySearchFilterRequestModelValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum()
                .When(x => x.Status.HasValue)
                .WithMessage(_ => ValidationMessages.UnsupportedValue)
                .WithName(_ => FieldDisplayNames.AutoStatus);

            RuleFor(x => x.Color)
                .MaximumLength(PagedListConstraints.SearchMaxLength)
                .WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName(_ => FieldDisplayNames.Color);
        }
    }
}
