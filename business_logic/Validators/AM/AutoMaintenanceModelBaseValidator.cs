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
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName("Автомобіль");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .MaximumLength(AutoMaintenanceConstraints.NameMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName("Тип робіт");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .MaximumLength(AutoMaintenanceConstraints.DescriptionMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName("Опис проблеми");

            RuleFor(x => x.PlannedEndDate)
                .GreaterThan(x => x.ServiceDate).WithMessage(_ => ValidationMessages.MaintenancePlannedEndMustFollowStart)
                .When(x => x.PlannedEndDate.HasValue);
        }
    }
}
