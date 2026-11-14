using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators
{
    public class GetLocationsBySearchFilterRequestModelValidator : AbstractValidator<GetLocationsBySearchFilterRequestModel>
    {
        public GetLocationsBySearchFilterRequestModelValidator()
        {
            RuleFor(x => x.Country)
                .MaximumLength(PagedListConstraints.SearchMaxLength)
                .WithMessage($"Країна у фільтрі не може перевищувати {PagedListConstraints.SearchMaxLength} символів.");
        }
    }
}
