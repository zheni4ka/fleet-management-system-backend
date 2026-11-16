using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators
{
    public class CreateLocationRequestModelValidator : AbstractValidator<CreateLocationRequestModel>
    {
        public CreateLocationRequestModelValidator()
        {
            RuleFor(x => x.City)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .MaximumLength(LocationConstraints.CityMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName("Місто");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .MaximumLength(LocationConstraints.CountryMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName("Країна");

            RuleFor(x => x.TimeZoneId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .MaximumLength(LocationConstraints.TimeZoneIdMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .Must(timeZoneId => TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId!, out _)).WithMessage(_ => ValidationMessages.UnknownTimeZoneId)
                .WithName("Часовий пояс");
        }
    }
}
