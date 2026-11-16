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
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .MaximumLength(LocationConstraints.CityMaxLength).WithMessage(ValidationMessages.MaximumLengthExceeded)
                .WithName("Місто");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .MaximumLength(LocationConstraints.CountryMaxLength).WithMessage(ValidationMessages.MaximumLengthExceeded)
                .WithName("Країна");

            RuleFor(x => x.TimeZoneId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .MaximumLength(LocationConstraints.TimeZoneIdMaxLength).WithMessage(ValidationMessages.MaximumLengthExceeded)
                .Must(timeZoneId => TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId!, out _)).WithMessage(ValidationMessages.UnknownTimeZoneId)
                .WithName("Часовий пояс");
        }
    }
}
