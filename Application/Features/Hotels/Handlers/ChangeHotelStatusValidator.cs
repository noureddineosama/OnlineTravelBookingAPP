using Application.Features.Hotels.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.Handlers
{
    public class ChangeHotelStatusValidator : AbstractValidator<ChangeHotelStatusCommand>
    {
        public ChangeHotelStatusValidator()
        {
            RuleFor(x => x.requestDTO.HotelId)
           .GreaterThan(0);
        }
    }
}
