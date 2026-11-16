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
                .MaximumLength(200).WithMessage(ValidationMessages.MaximumLengthExceeded)
                .WithName("Місто");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .MaximumLength(200).WithMessage(ValidationMessages.MaximumLengthExceeded)
                .WithName("Країна");
        }
    }
}
