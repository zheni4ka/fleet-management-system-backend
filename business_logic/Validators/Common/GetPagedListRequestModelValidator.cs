using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Common
{
    public class GetPagedListRequestModelValidator<TSortField> : AbstractValidator<GetPagedListRequestModel<TSortField>>
        where TSortField : struct, Enum
    {
        public GetPagedListRequestModelValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage(x => $"Номер сторінки має бути більшим за 0, отримано {x.Page}.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, PagedListConstraints.MaxPageSize)
                .WithMessage(x => $"Розмір сторінки має бути від 1 до {PagedListConstraints.MaxPageSize}, отримано {x.PageSize}.");

            RuleFor(x => x.Sort)
                .IsInEnum()
                .When(x => x.Sort.HasValue)
                .WithMessage(x => $"Поле сортування '{x.Sort}' не підтримується.");

            RuleFor(x => x.SortDirection)
                .IsInEnum()
                .When(x => x.SortDirection.HasValue)
                .WithMessage(x => $"Напрям сортування '{x.SortDirection}' не підтримується.");

            RuleFor(x => x.Search)
                .MaximumLength(PagedListConstraints.SearchMaxLength)
                .WithMessage($"Пошуковий запит не може перевищувати {PagedListConstraints.SearchMaxLength} символів.");
        }
    }
}
