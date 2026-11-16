using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Account
{
    public class LoginWithExternalIdentityRequestModelValidator : AbstractValidator<LoginWithExternalIdentityRequestModel>
    {
        public LoginWithExternalIdentityRequestModelValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .WithName("Токен ідентифікації");

            RuleFor(x => x.Nonce)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .WithName("Одноразове значення");
        }
    }
}
