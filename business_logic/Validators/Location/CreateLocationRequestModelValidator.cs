using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators
{
    public class CreateLocationRequestModelValidator : AbstractValidator<CreateLocationRequestModel>
    {
        public CreateLocationRequestModelValidator()
        {
            RuleFor(x => x.GeoNamesId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.City);
        }
    }
}
