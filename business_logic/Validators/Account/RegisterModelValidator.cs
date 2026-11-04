using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Account
{
    public class RegisterModelValidator : AbstractValidator<RegisterModel>
    {
        public RegisterModelValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(AccountConstraints.UsernameMaxLength)
                .WithMessage($"Name must not exceed {AccountConstraints.UsernameMaxLength} characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid Email format.")
                .MaximumLength(AccountConstraints.EmailMaxLength)
                .WithMessage($"Email must not exceed {AccountConstraints.EmailMaxLength} characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(AccountConstraints.PasswordMinLength)
                .WithMessage($"Password must be at least {AccountConstraints.PasswordMinLength} characters long.")
                .MaximumLength(AccountConstraints.PasswordMaxLength)
                .WithMessage($"Password must not exceed {AccountConstraints.PasswordMaxLength} characters.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(role => UserRoles.All.Contains(role))
                .WithMessage($"Role must be one of: {string.Join(", ", UserRoles.All)}.");
        }
    }
}
