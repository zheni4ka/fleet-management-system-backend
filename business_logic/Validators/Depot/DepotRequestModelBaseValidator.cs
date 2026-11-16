using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Depot
{
    public class DepotRequestModelBaseValidator : AbstractValidator<DepotRequestModelBase>
    {
        public DepotRequestModelBaseValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ValidationMessages.RequiredField)
                .MaximumLength(DepotConstraints.NameMaxLength).WithMessage(ValidationMessages.MaximumLengthExceeded)
                .WithName("Назва депо");
        }
    }
}
