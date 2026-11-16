using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Depot
{
    public class CreateDepotRequestModelValidator : AbstractValidator<CreateDepotRequestModel>
    {
        public CreateDepotRequestModelValidator()
        {
            Include(new DepotRequestModelBaseValidator());
        }
    }
}
