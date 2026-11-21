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
                .WithName(_ => FieldDisplayNames.Username);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .EmailAddress().WithMessage(_ => ValidationMessages.EmailAddressInvalid)
                .MaximumLength(AccountConstraints.EmailMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName(_ => FieldDisplayNames.Email);

            RuleFor(x => x.Password)
                .MustSatisfyPasswordPolicy()
                .WithName(_ => FieldDisplayNames.Password);

            RuleFor(x => x.DepotIds)
                .NotEmpty().When(x => x.Role == UserRoles.Dispatcher).WithMessage(_ => ValidationMessages.DispatcherDepotsRequired)
                .Empty().When(x => x.Role == UserRoles.Admin).WithMessage(_ => ValidationMessages.AdminDepotsForbidden)
                .WithName(_ => FieldDisplayNames.Depot);

            RuleForEach(x => x.DepotIds)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.Depot);

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .Must(role => UserRoles.All.Contains(role))
                .WithMessage(_ => string.Format(ValidationMessages.UnsupportedRoleTemplate, string.Join(", ", UserRoles.All)))
                .WithName(_ => FieldDisplayNames.Role);
        }
    }
}
