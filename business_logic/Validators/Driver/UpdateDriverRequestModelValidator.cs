using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators
{
    public class UpdateDriverRequestModelValidator : AbstractValidator<UpdateDriverRequestModel>
    {
        public UpdateDriverRequestModelValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be greater than 0");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .Matches(DriverNameFormat.Pattern).WithMessage(DriverNameFormat.InvalidFormatMessage)
                .MaximumLength(DriverConstraints.NameMaxLength).WithMessage($"Name cannot exceed {DriverConstraints.NameMaxLength} characters.");

            RuleFor(x => x.Surname)
                .NotEmpty().WithMessage("Surname is required.")
                .Matches(DriverNameFormat.Pattern).WithMessage(DriverNameFormat.InvalidFormatMessage)
                .MaximumLength(DriverConstraints.SurnameMaxLength).WithMessage($"Surname cannot exceed {DriverConstraints.SurnameMaxLength} characters.");

            RuleFor(x => x.Patronymic)
                .NotEmpty().WithMessage("Patronymic is required.")
                .Matches(DriverNameFormat.Pattern).WithMessage(DriverNameFormat.InvalidFormatMessage)
                .MaximumLength(DriverConstraints.PatronymicMaxLength).WithMessage($"Patronymic cannot exceed {DriverConstraints.PatronymicMaxLength} characters.");

            RuleFor(x => x.LicenseNumber)
                .NotEmpty().WithMessage("License number is required.")
                .Matches(@"^[A-Z]{3}\s\d{6}$")
                .WithMessage("Format must be correct (ABC 123456)");
        }
    }
}