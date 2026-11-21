using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators
{
    public class GetAddressCandidatesRequestModelValidator : AbstractValidator<GetAddressCandidatesRequestModel>
    {
        public GetAddressCandidatesRequestModelValidator()
        {
            RuleFor(x => x.SearchText)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .MinimumLength(AddressGeocodingConstraints.SearchTextMinLength).WithMessage(_ => ValidationMessages.MinimumLengthRequired)
                .MaximumLength(AddressGeocodingConstraints.SearchTextMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName(_ => FieldDisplayNames.Address);

            RuleFor(x => x.Latitude)
                .NotNull().WithMessage(_ => ValidationMessages.RequiredField)
                .InclusiveBetween(AddressGeocodingConstraints.MinimumLatitude, AddressGeocodingConstraints.MaximumLatitude).WithMessage(_ => ValidationMessages.NumberOutOfRange)
                .WithName(_ => FieldDisplayNames.Latitude);

            RuleFor(x => x.Longitude)
                .NotNull().WithMessage(_ => ValidationMessages.RequiredField)
                .InclusiveBetween(AddressGeocodingConstraints.MinimumLongitude, AddressGeocodingConstraints.MaximumLongitude).WithMessage(_ => ValidationMessages.NumberOutOfRange)
                .WithName(_ => FieldDisplayNames.Longitude);
        }
    }
}
