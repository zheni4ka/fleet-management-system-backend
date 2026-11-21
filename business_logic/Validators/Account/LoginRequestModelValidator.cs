using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Account
{
    public class LoginRequestModelValidator : AbstractValidator<LoginRequestModel>
    {
        public LoginRequestModelValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .WithName(_ => FieldDisplayNames.LoginOrEmail);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .WithName(_ => FieldDisplayNames.Password);
        }
    }
}
