using Application.Features.Booking.Commands;
using Application.Features.Booking.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Booking.Handlers
{
    public  class CancelBookingValidator : AbstractValidator<CancelBookingCommand>
    {
        public CancelBookingValidator()
        {
            RuleFor(x => x.requestDTO.CancellationReason)
                .NotEmpty()
                .MaximumLength(250);

            RuleFor(x => x.requestDTO.Notes)
                .MaximumLength(1000);
        }
    }
}
