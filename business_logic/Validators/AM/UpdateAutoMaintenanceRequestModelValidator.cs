using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.AM
{
    public class UpdateAutoMaintenanceRequestModelValidator : AbstractValidator<UpdateAutoMaintenanceRequestModel>
    {
        public UpdateAutoMaintenanceRequestModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Ідентифікатор");

            RuleFor(x => x.RowVersion)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .WithName("Версія запису");

            Include(new AutoMaintenanceModelBaseValidator());
        }
    }
}
