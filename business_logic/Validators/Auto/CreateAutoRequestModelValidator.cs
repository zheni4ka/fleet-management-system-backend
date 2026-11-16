using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Auto
{
    public class CreateAutoRequestModelValidator : AbstractValidator<CreateAutoRequestModel>
    {
        public CreateAutoRequestModelValidator()
        {
            RuleFor(x => x.Mark)
                .NotEmpty().WithMessage("Name of this car is required.")
                .MaximumLength(AutoConstraints.MarkMaxLength).WithMessage($"Name of this car must be less than {AutoConstraints.MarkMaxLength}");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model of this car is required.")
                .MaximumLength(AutoConstraints.ModelMaxLength).WithMessage($"Model of this car must be less than {AutoConstraints.ModelMaxLength}");

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("Color of this car is required.")
                .MaximumLength(AutoConstraints.ColorMaxLength).WithMessage($"Color of this car must be less than {AutoConstraints.ColorMaxLength}");

            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Capacity must be greater than 0.");

            RuleFor(x => x.Number)
                .NotEmpty().WithMessage("Number is required.")
                .Matches(AutoNumberFormat.Pattern).WithMessage("Number has an invalid format.");
        }
    }

    internal static class AutoNumberFormat
    {
        public const string Pattern = @"^[А-ЩЬЮЯҐЄІЇ][А-ЩЬЮЯҐЄІЇ][0-9]{4}[А-ЩЬЮЯҐЄІЇ][А-ЩЬЮЯҐЄІЇ]$";
    }
}
