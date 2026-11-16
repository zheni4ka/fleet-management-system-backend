using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Account
{
    public class UpdateUserDepotsRequestModelValidator : AbstractValidator<UpdateUserDepotsRequestModel>
    {
        public UpdateUserDepotsRequestModelValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .WithName("Ідентифікатор користувача");

            RuleFor(x => x.DepotIds)
                .NotEmpty().WithMessage(ValidationMessages.DispatcherDepotsRequired)
                .WithName("Депо");

            RuleForEach(x => x.DepotIds)
                .GreaterThan(0).WithMessage(ValidationMessages.PositiveNumberRequired)
                .WithName("Депо");
        }
    }
}
