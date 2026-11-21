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
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .WithName(_ => FieldDisplayNames.UserId);

            RuleFor(x => x.DepotIds)
                .NotEmpty().WithMessage(_ => ValidationMessages.DispatcherDepotsRequired)
                .WithName(_ => FieldDisplayNames.Depot);

            RuleForEach(x => x.DepotIds)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.Depot);
        }
    }
}
