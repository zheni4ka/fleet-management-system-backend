using business_logic.Constants;
using business_logic.DTOs;
using FluentValidation;

namespace business_logic.Validators.Account
{
    public class ResetUserPasswordRequestModelValidator : AbstractValidator<ResetUserPasswordRequestModel>
    {
        public ResetUserPasswordRequestModelValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(_ => ValidationMessages.RequiredField)
                .WithName(_ => FieldDisplayNames.UserId);
        }
    }
}
