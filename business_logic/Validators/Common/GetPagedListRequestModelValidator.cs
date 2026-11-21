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
                .WithMessage(_ => ValidationMessages.PositiveNumberRequired)
                .WithName(_ => FieldDisplayNames.Page);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, PagedListConstraints.MaxPageSize)
                .WithMessage(_ => ValidationMessages.NumberOutOfRange)
                .WithName(_ => FieldDisplayNames.PageSize);

            RuleFor(x => x.Sort)
                .IsInEnum()
                .When(x => x.Sort.HasValue)
                .WithMessage(_ => ValidationMessages.UnsupportedValue)
                .WithName(_ => FieldDisplayNames.SortField);

            RuleFor(x => x.SortDirection)
                .IsInEnum()
                .When(x => x.SortDirection.HasValue)
                .WithMessage(_ => ValidationMessages.UnsupportedValue)
                .WithName(_ => FieldDisplayNames.SortDirection);

            RuleFor(x => x.Search)
                .MaximumLength(PagedListConstraints.SearchMaxLength)
                .WithMessage(_ => ValidationMessages.MaximumLengthExceeded)
                .WithName(_ => FieldDisplayNames.SearchQuery);
        }
    }
}
