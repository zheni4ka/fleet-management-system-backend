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
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .WithName(_ => FieldDisplayNames.LoginOrEmail);

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .WithName(_ => FieldDisplayNames.CurrentPassword);

            RuleFor(x => x.NewPassword)
                .MustSatisfyPasswordPolicy()
                .NotEqual(x => x.CurrentPassword).WithMessage(_ => ValidationMessages.NewPasswordMustDifferFromCurrent)
                .WithName(_ => FieldDisplayNames.NewPassword);
        }
    }
}
