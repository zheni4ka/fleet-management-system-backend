using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators
{
    public class UpdateDriverRequestModelValidator : AbstractValidator<UpdateDriverRequestModel>
    {
        public UpdateDriverRequestModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Ідентифікатор");

            Include(new DriverRequestModelBaseValidator());
        }
    }
}
