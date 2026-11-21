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
                .Matches(DriverNameFormat.Pattern).WithMessage(_ => ValidationMessages.DriverNameFormatInvalid)
                .MaximumLength(DriverConstraints.NameMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName(_ => FieldDisplayNames.Name);

            RuleFor(x => x.Surname)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .Matches(DriverNameFormat.Pattern).WithMessage(_ => ValidationMessages.DriverNameFormatInvalid)
                .MaximumLength(DriverConstraints.SurnameMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName(_ => FieldDisplayNames.Surname);

            RuleFor(x => x.Patronymic)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .Matches(DriverNameFormat.Pattern).WithMessage(_ => ValidationMessages.DriverNameFormatInvalid)
                .MaximumLength(DriverConstraints.PatronymicMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName(_ => FieldDisplayNames.Patronymic);

            RuleFor(x => x.DepotId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.Depot);

            RuleFor(x => x.LicenseNumber)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .Matches(DriverLicenseNumberFormat.Pattern).WithMessage(_ => ValidationMessages.DriverLicenseNumberFormatInvalid)
                .WithName(_ => FieldDisplayNames.DriverLicenseNumber);
        }
    }
}
