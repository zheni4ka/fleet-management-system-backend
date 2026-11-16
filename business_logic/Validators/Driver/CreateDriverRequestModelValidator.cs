using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators
{
    public class CreateDriverRequestModelValidator : AbstractValidator<CreateDriverRequestModel>
    {
        public CreateDriverRequestModelValidator()
        {
            RuleFor(x => x.Name)
                .Matches(DriverNameFormat.Pattern).WithMessage(DriverNameFormat.InvalidFormatMessage)
                .MaximumLength(DriverConstraints.NameMaxLength).WithMessage($"Name cannot exceed {DriverConstraints.NameMaxLength} characters.");
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(x => x.Surname)
                .Matches(DriverNameFormat.Pattern).WithMessage(DriverNameFormat.InvalidFormatMessage)
                .MaximumLength(DriverConstraints.SurnameMaxLength).WithMessage($"Surname cannot exceed {DriverConstraints.SurnameMaxLength} characters.");

            RuleFor(x => x.Surname)
                .NotEmpty().WithMessage("Surname is required.");

            RuleFor(x => x.Patronymic)
                .Matches(DriverNameFormat.Pattern).WithMessage(DriverNameFormat.InvalidFormatMessage)
                .MaximumLength(DriverConstraints.PatronymicMaxLength).WithMessage($"Patronymic cannot exceed {DriverConstraints.PatronymicMaxLength} characters.");

            RuleFor(x => x.Patronymic)
                .NotEmpty().WithMessage("Patronymic is required.");

            RuleFor(x => x.LicenseNumber)
                .Matches(@"^[A-Z]{3}\s\d{6}$")
                .WithMessage("Format must be correct (ABC 123456)");

            RuleFor(x => x.LicenseNumber)
                .NotEmpty().WithMessage("License number is required.");
        }
    }
}