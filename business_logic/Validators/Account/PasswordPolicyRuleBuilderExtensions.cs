using business_logic.Constants;
using FluentValidation;

namespace business_logic.Validators.Account
{
    public static class PasswordPolicyRuleBuilderExtensions
    {
        public static IRuleBuilderOptions<TModel, string> MustSatisfyPasswordPolicy<TModel>(this IRuleBuilder<TModel, string> passwordRuleBuilder)
        {
            return passwordRuleBuilder
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .MinimumLength(AccountConstraints.PasswordMinLength).WithMessage(ValidationMessages.MinimumLengthRequired)
                .MaximumLength(AccountConstraints.PasswordMaxLength).WithMessage(ValidationMessages.MaximumLengthExceeded)
                .Matches(AccountConstraints.PasswordDigitPattern).WithMessage(ValidationMessages.PasswordDigitRequired)
                .Matches(AccountConstraints.PasswordUppercaseLetterPattern).WithMessage(ValidationMessages.PasswordUppercaseLetterRequired)
                .Matches(AccountConstraints.PasswordLowercaseLetterPattern).WithMessage(ValidationMessages.PasswordLowercaseLetterRequired);
        }
    }
}
