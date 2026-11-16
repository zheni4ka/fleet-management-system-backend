using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Account
{
    public class LogoutRequestModelValidator : AbstractValidator<LogoutRequestModel>
    {
        public LogoutRequestModelValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .WithName("Токен оновлення");
        }
    }
}
