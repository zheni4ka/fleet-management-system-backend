using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Account
{
    public class UpdatePreferredLanguageRequestModelValidator : AbstractValidator<UpdatePreferredLanguageRequestModel>
    {
        public UpdatePreferredLanguageRequestModelValidator()
        {
            RuleFor(x => x.Language)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .Must(language => SupportedLanguages.All.Contains(language!)).WithMessage(_ => ValidationMessages.UnsupportedValue)
                .WithName(_ => FieldDisplayNames.Language);
        }
    }
}
