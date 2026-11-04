using business_logic.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace business_logic.Validators.AM
{
    public class EditAutoMaintenanceModelValidator: AbstractValidator<EditAutoMaintenanceModel>
    {
        public EditAutoMaintenanceModelValidator() 
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.AutoId).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().WithMessage("Maintenance name is required.");
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
            RuleFor(x => x.ServiceDate).LessThanOrEqualTo(DateTime.Now);
        }
    }
}
