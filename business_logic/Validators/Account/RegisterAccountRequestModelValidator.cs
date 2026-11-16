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
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .MaximumLength(AccountConstraints.UsernameMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName("Логін");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .EmailAddress().WithMessage("Електронна пошта має некоректний формат.")
                .MaximumLength(AccountConstraints.EmailMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName("Електронна пошта");

            RuleFor(x => x.Password)
                .MustSatisfyPasswordPolicy()
                .WithName("Пароль");

            RuleFor(x => x.DepotIds)
                .NotEmpty().When(x => x.Role == UserRoles.Dispatcher).WithMessage(_ => ValidationMessages.DispatcherDepotsRequired)
                .Empty().When(x => x.Role == UserRoles.Admin).WithMessage(_ => ValidationMessages.AdminDepotsForbidden)
                .WithName("Депо");

            RuleForEach(x => x.DepotIds)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName("Депо");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .Must(role => UserRoles.All.Contains(role))
                .WithMessage($"Роль має бути однією з: {string.Join(", ", UserRoles.All)}.")
                .WithName("Роль");
        }
    }
}
