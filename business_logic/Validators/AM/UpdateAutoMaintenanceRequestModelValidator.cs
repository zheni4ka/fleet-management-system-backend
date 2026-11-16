using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.AM
{
    public class UpdateAutoMaintenanceRequestModelValidator: AbstractValidator<UpdateAutoMaintenanceRequestModel>
    {
        public UpdateAutoMaintenanceRequestModelValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.AutoId).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().WithMessage("Maintenance name is required.").MaximumLength(AutoMaintenanceConstraints.NameMaxLength);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(AutoMaintenanceConstraints.DescriptionMaxLength);
        }
    }
}
