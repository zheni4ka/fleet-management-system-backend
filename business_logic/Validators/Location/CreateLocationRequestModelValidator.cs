using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators
{
    public class CreateLocationRequestModelValidator : AbstractValidator<CreateLocationRequestModel>
    {
        public CreateLocationRequestModelValidator()
        {
            RuleFor(x => x.City).NotEmpty().WithMessage("City is required").MaximumLength(200);
            RuleFor(x => x.Country).NotEmpty().WithMessage("Country is required").MaximumLength(200);
        }
    }
}
