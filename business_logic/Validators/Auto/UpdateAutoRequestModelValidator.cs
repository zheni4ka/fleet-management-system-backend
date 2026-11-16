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
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Ідентифікатор");

            Include(new AutoRequestModelBaseValidator());
        }
    }
}
