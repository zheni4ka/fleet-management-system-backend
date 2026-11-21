using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Route
{
    internal sealed class RouteAddressesRequestModelValidator : AbstractValidator<IRouteAddressesRequestModel>
    {
        public RouteAddressesRequestModelValidator(bool areAddressesRequired)
        {
            When(x => areAddressesRequired || IsAddressSent(x.StartAddressLine, x.StartAddressLatitude, x.StartAddressLongitude), () =>
            {
                RuleFor(x => x.StartAddressLine)
                    .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                    .MaximumLength(AddressGeocodingConstraints.AddressLineMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                    .WithName(_ => FieldDisplayNames.StartAddress);

                RuleFor(x => x.StartAddressLatitude)
                    .NotNull().WithMessage(_ => ValidationMessages.RequiredField)
                    .InclusiveBetween(AddressGeocodingConstraints.MinimumLatitude, AddressGeocodingConstraints.MaximumLatitude).WithMessage(_ => ValidationMessages.NumberOutOfRange)
                    .WithName(_ => FieldDisplayNames.Latitude);

                RuleFor(x => x.StartAddressLongitude)
                    .NotNull().WithMessage(_ => ValidationMessages.RequiredField)
                    .InclusiveBetween(AddressGeocodingConstraints.MinimumLongitude, AddressGeocodingConstraints.MaximumLongitude).WithMessage(_ => ValidationMessages.NumberOutOfRange)
                    .WithName(_ => FieldDisplayNames.Longitude);
            });

            When(x => areAddressesRequired || IsAddressSent(x.DestinationAddressLine, x.DestinationAddressLatitude, x.DestinationAddressLongitude), () =>
            {
                RuleFor(x => x.DestinationAddressLine)
                    .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                    .MaximumLength(AddressGeocodingConstraints.AddressLineMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                    .WithName(_ => FieldDisplayNames.DestinationAddress);

                RuleFor(x => x.DestinationAddressLatitude)
                    .NotNull().WithMessage(_ => ValidationMessages.RequiredField)
                    .InclusiveBetween(AddressGeocodingConstraints.MinimumLatitude, AddressGeocodingConstraints.MaximumLatitude).WithMessage(_ => ValidationMessages.NumberOutOfRange)
                    .WithName(_ => FieldDisplayNames.Latitude);

                RuleFor(x => x.DestinationAddressLongitude)
                    .NotNull().WithMessage(_ => ValidationMessages.RequiredField)
                    .InclusiveBetween(AddressGeocodingConstraints.MinimumLongitude, AddressGeocodingConstraints.MaximumLongitude).WithMessage(_ => ValidationMessages.NumberOutOfRange)
                    .WithName(_ => FieldDisplayNames.Longitude);
            });
        }

        private static bool IsAddressSent(string? addressLine, double? addressLatitude, double? addressLongitude)
        {
            return !string.IsNullOrWhiteSpace(addressLine) || addressLatitude.HasValue || addressLongitude.HasValue;
        }
    }
}
