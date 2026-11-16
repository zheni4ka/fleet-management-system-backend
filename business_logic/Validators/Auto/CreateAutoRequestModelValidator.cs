using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Auto
{
    public class CreateAutoRequestModelValidator : AbstractValidator<CreateAutoRequestModel>
    {
        public CreateAutoRequestModelValidator()
        {
            Include(new AutoRequestModelBaseValidator());
        }
    }
}
