using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Auto
{
    internal sealed class AutoRequestModelBaseValidator : AbstractValidator<AutoRequestModelBase>
    {
        public AutoRequestModelBaseValidator()
        {
            RuleFor(x => x.Mark)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .MaximumLength(AutoConstraints.MarkMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName("Марка");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .MaximumLength(AutoConstraints.ModelMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName("Модель");

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .MaximumLength(AutoConstraints.ColorMaxLength).WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName("Колір");

            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName("Місткість");

            RuleFor(x => x.DepotId)
                .GreaterThan(0).WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName("Депо");

            RuleFor(x => x.Number)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .Matches(AutoNumberFormat.Pattern).WithMessage(AutoNumberFormat.InvalidFormatMessage)
                .WithName("Номерний знак");
        }
    }
}
