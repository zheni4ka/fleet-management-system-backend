using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.DriverTrip
{
    public class GetDriverTripsRequestModelValidator : AbstractValidator<GetDriverTripsRequestModel>
    {
        public GetDriverTripsRequestModelValidator()
        {
            RuleFor(x => x.DriverId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.Driver);
        }
    }
}
