using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators
{
    public class GetCityGeocodingCandidatesRequestModelValidator : AbstractValidator<GetCityGeocodingCandidatesRequestModel>
    {
        public GetCityGeocodingCandidatesRequestModelValidator()
        {
            RuleFor(x => x.SearchText)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .MinimumLength(CityGeocodingConstraints.SearchTextMinLength).WithMessage(_ => ValidationMessages.MinimumLengthRequired)
                .MaximumLength(CityGeocodingConstraints.SearchTextMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName("Місто");
        }
    }
}
