using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Helpers;
using business_logic.Validators.AM;
using FluentValidation;

namespace business_logic.Validators
{
    public class CreateAutoMaintenanceRequestModelValidator : AbstractValidator<CreateAutoMaintenanceRequestModel>
    {
        public CreateAutoMaintenanceRequestModelValidator()
        {
            Include(new AutoMaintenanceModelBaseValidator());

            RuleFor(x => x.ServiceDate)
                .GreaterThanOrEqualTo(_ => BusinessTimeZoneConverter.GetBusinessNowDateTime())
                .WithMessage(_ => ValidationMessages.MaintenanceStartMustNotBeInPast);
        }
    }
}
