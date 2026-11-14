using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Auto
{
    public class GetAutosBySearchFilterRequestModelValidator : AbstractValidator<GetAutosBySearchFilterRequestModel>
    {
        public GetAutosBySearchFilterRequestModelValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum()
                .When(x => x.Status.HasValue)
                .WithMessage(x => $"Статус автомобіля '{x.Status}' не підтримується.");

            RuleFor(x => x.Color)
                .MaximumLength(PagedListConstraints.SearchMaxLength)
                .WithMessage($"Колір у фільтрі не може перевищувати {PagedListConstraints.SearchMaxLength} символів.");
        }
    }
}
