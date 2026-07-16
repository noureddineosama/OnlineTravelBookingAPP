using Application.Features.Hotels.Commands;
using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.Handlers
{
    public class DeleteHotelValidator : AbstractValidator<DeleteHotelCommand>
    {
        public DeleteHotelValidator()
        {
            RuleFor(x => x.id)
                .GreaterThan(0);
        }
    }
}
