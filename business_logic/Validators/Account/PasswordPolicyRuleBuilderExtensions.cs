using business_logic.Constants;
using FluentValidation;

namespace business_logic.Validators.Account
{
    public static class PasswordPolicyRuleBuilderExtensions
    {
        public static IRuleBuilderOptions<TModel, string> MustSatisfyPasswordPolicy<TModel>(this IRuleBuilder<TModel, string> passwordRuleBuilder)
        {
            return passwordRuleBuilder
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .MinimumLength(AccountConstraints.PasswordMinLength).WithMessage(_ => ValidationMessages.MinimumLengthRequired)
                .MaximumLength(AccountConstraints.PasswordMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .Matches(AccountConstraints.PasswordDigitPattern).WithMessage(_ => ValidationMessages.PasswordDigitRequired)
                .Matches(AccountConstraints.PasswordUppercaseLetterPattern).WithMessage(_ => ValidationMessages.PasswordUppercaseLetterRequired)
                .Matches(AccountConstraints.PasswordLowercaseLetterPattern).WithMessage(_ => ValidationMessages.PasswordLowercaseLetterRequired);
        }
    }
}
