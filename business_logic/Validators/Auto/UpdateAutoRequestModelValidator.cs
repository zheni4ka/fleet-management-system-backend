using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Auto
{
    public class UpdateAutoRequestModelValidator : AbstractValidator<UpdateAutoRequestModel>
    {
        public UpdateAutoRequestModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName("Ідентифікатор");

            RuleFor(x => x.RowVersion)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .WithName("Версія запису");

            Include(new AutoRequestModelBaseValidator());
        }
    }
}
