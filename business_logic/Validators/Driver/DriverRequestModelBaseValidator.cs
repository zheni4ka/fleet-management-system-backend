using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators
{
    internal sealed class DriverRequestModelBaseValidator : AbstractValidator<DriverRequestModelBase>
    {
        public DriverRequestModelBaseValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .Matches(DriverNameFormat.Pattern).WithMessage(DriverNameFormat.InvalidFormatMessage)
                .MaximumLength(DriverConstraints.NameMaxLength).WithMessage(ValidationMessages.MaximumLengthExceeded)
                .WithName("Ім'я");

            RuleFor(x => x.Surname)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .Matches(DriverNameFormat.Pattern).WithMessage(DriverNameFormat.InvalidFormatMessage)
                .MaximumLength(DriverConstraints.SurnameMaxLength).WithMessage(ValidationMessages.MaximumLengthExceeded)
                .WithName("Прізвище");

            RuleFor(x => x.Patronymic)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .Matches(DriverNameFormat.Pattern).WithMessage(DriverNameFormat.InvalidFormatMessage)
                .MaximumLength(DriverConstraints.PatronymicMaxLength).WithMessage(ValidationMessages.MaximumLengthExceeded)
                .WithName("По батькові");

            RuleFor(x => x.DepotId)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Депо");

            RuleFor(x => x.LicenseNumber)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .Matches(DriverLicenseNumberFormat.Pattern).WithMessage(DriverLicenseNumberFormat.InvalidFormatMessage)
                .WithName("Посвідчення водія");
        }
    }
}
