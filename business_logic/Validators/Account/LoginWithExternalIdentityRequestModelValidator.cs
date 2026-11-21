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
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .WithName(_ => FieldDisplayNames.IdentityToken);

            RuleFor(x => x.Nonce)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .WithName(_ => FieldDisplayNames.Nonce);
        }
    }
}
