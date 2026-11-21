using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Account
{
    public class RefreshSessionRequestModelValidator : AbstractValidator<RefreshSessionRequestModel>
    {
        public RefreshSessionRequestModelValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .WithName(_ => FieldDisplayNames.RefreshToken);
        }
    }
}
