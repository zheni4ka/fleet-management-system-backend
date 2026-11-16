using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Helpers;
using FluentValidation;

namespace business_logic.Validators
{
    public class CreateAutoMaintenanceRequestModelValidator : AbstractValidator<CreateAutoMaintenanceRequestModel>
    {
        public CreateAutoMaintenanceRequestModelValidator()
        {
            RuleFor(x => x.AutoId)
                .GreaterThan(0)
                .WithMessage("Auto ID must be greater than 0.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Maintenance name is required.")
                .MaximumLength(AutoMaintenanceConstraints.NameMaxLength);

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Maintenance description is required.")
                .MaximumLength(AutoMaintenanceConstraints.DescriptionMaxLength);

            RuleFor(x => x.ServiceDate)
                .GreaterThanOrEqualTo(_ => BusinessTimeZoneConverter.GetBusinessNowDateTime())
                .WithMessage("Service date cannot be in the past.");
        }
    }
}
