using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.AM
{
    internal sealed class AutoMaintenanceModelBaseValidator : AbstractValidator<AutoMaintenanceModelBase>
    {
        public AutoMaintenanceModelBaseValidator()
        {
            RuleFor(x => x.AutoId)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Автомобіль");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .MaximumLength(AutoMaintenanceConstraints.NameMaxLength).WithMessage(ValidationMessages.MaximumLengthExceeded)
                .WithName("Тип робіт");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .MaximumLength(AutoMaintenanceConstraints.DescriptionMaxLength).WithMessage(ValidationMessages.MaximumLengthExceeded)
                .WithName("Опис проблеми");
        }
    }
}
