using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Account
{
    public class RegisterAccountRequestModelValidator : AbstractValidator<RegisterAccountRequestModel>
    {
        public RegisterAccountRequestModelValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .MaximumLength(AccountConstraints.UsernameMaxLength).WithMessage(ValidationMessages.MaximumLengthExceeded)
                .WithName("Логін");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .EmailAddress().WithMessage("Електронна пошта має некоректний формат.")
                .MaximumLength(AccountConstraints.EmailMaxLength).WithMessage(ValidationMessages.MaximumLengthExceeded)
                .WithName("Електронна пошта");

            RuleFor(x => x.Password)
                .MustSatisfyPasswordPolicy()
                .WithName("Пароль");

            RuleFor(x => x.DepotIds)
                .NotEmpty().When(x => x.Role == UserRoles.Dispatcher).WithMessage(ValidationMessages.DispatcherDepotsRequired)
                .Empty().When(x => x.Role == UserRoles.Admin).WithMessage(ValidationMessages.AdminDepotsForbidden)
                .WithName("Депо");

            RuleForEach(x => x.DepotIds)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Депо");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .Must(role => UserRoles.All.Contains(role))
                .WithMessage($"Роль має бути однією з: {string.Join(", ", UserRoles.All)}.")
                .WithName("Роль");
        }
    }
}
