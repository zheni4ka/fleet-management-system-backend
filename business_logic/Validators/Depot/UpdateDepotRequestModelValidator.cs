using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Depot
{
    public class UpdateDepotRequestModelValidator : AbstractValidator<UpdateDepotRequestModel>
    {
        public UpdateDepotRequestModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Ідентифікатор");

            Include(new DepotRequestModelBaseValidator());
        }
    }
}
