using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Account
{
    public class ChangePasswordRequestModelValidator : AbstractValidator<ChangePasswordRequestModel>
    {
        public ChangePasswordRequestModelValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .WithName("Логін або email");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .WithName("Поточний пароль");

            RuleFor(x => x.NewPassword)
                .MustSatisfyPasswordPolicy()
                .NotEqual(x => x.CurrentPassword).WithMessage(ValidationMessages.NewPasswordMustDifferFromCurrent)
                .WithName("Новий пароль");
        }
    }
}
