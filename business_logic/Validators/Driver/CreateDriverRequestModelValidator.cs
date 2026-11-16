using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators
{
    public class CreateDriverRequestModelValidator : AbstractValidator<CreateDriverRequestModel>
    {
        public CreateDriverRequestModelValidator()
        {
            Include(new DriverRequestModelBaseValidator());
        }
    }
}
