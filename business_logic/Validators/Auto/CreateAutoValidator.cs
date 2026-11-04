using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Auto
{
    public class CreateAutoValidator : AbstractValidator<CreateAutoModel>
    {
        public CreateAutoValidator()
        {
            RuleFor(x => x.Mark)
                .NotEmpty().WithMessage("Name of this car is required.")
                .MaximumLength(16).WithMessage("Name of this car must be less than 16");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model of this car is required.")
                .MaximumLength(24).WithMessage("Model of this car must be less than 24");

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("Color of this car is required.");

            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Capacity must be greater than 0.");

            RuleFor(x => x.Number)
                .NotEmpty().WithMessage("Number is required.")
                .Matches(AutoNumberFormat.Pattern).WithMessage("Number has an invalid format.");
        }
    }

    internal static class AutoNumberFormat
    {
        public const string Pattern = @"^[А-ЩЬЮЯҐЄІЇ][А-ЩЬЮЯҐЄІЇ][0-9]{0,4}[А-ЩЬЮЯҐЄІЇ][А-ЩЬЮЯҐЄІЇ]$";
    }
}
