using Application.Features.Hotels.Queries;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.Handlers
{
    public class HotelDetailsValidator : AbstractValidator<HotelDetailsQuery>
    {
        public HotelDetailsValidator()
        {
            RuleFor(op => op.Id)
                .GreaterThan(0);
        }
    }
}
