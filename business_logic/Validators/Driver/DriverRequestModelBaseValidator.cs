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
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .Matches(DriverNameFormat.Pattern).WithMessage(DriverNameFormat.InvalidFormatMessage)
                .MaximumLength(DriverConstraints.NameMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName("Ім'я");

            RuleFor(x => x.Surname)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .Matches(DriverNameFormat.Pattern).WithMessage(DriverNameFormat.InvalidFormatMessage)
                .MaximumLength(DriverConstraints.SurnameMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName("Прізвище");

            RuleFor(x => x.Patronymic)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .Matches(DriverNameFormat.Pattern).WithMessage(DriverNameFormat.InvalidFormatMessage)
                .MaximumLength(DriverConstraints.PatronymicMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName("По батькові");

            RuleFor(x => x.DepotId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName("Депо");

            RuleFor(x => x.LicenseNumber)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .Matches(DriverLicenseNumberFormat.Pattern).WithMessage(DriverLicenseNumberFormat.InvalidFormatMessage)
                .WithName("Посвідчення водія");
        }
    }
}
