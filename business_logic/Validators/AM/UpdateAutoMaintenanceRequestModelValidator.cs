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
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.Id);

            RuleFor(x => x.RowVersion)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .WithName(_ => FieldDisplayNames.RowVersion);

            Include(new AutoMaintenanceModelBaseValidator());
        }
    }
}
