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
                .WithName(_ => FieldDisplayNames.Auto);

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .MaximumLength(AutoMaintenanceConstraints.NameMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName(_ => FieldDisplayNames.MaintenanceWorkType);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .MaximumLength(AutoMaintenanceConstraints.DescriptionMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName(_ => FieldDisplayNames.MaintenanceProblemDescription);

            RuleFor(x => x.PlannedEndDate)
                .GreaterThan(x => x.ServiceDate).WithMessage(_ => ValidationMessages.MaintenancePlannedEndMustFollowStart)
                .When(x => x.PlannedEndDate.HasValue);
        }
    }
}
