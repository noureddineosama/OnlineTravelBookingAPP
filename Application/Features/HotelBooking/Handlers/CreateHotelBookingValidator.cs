using Application.Features.HotelBooking.Commands;
using Application.Features.HotelBooking.DTOs;
using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelBooking.Handlers
{
    public class CreateHotelBookingValidator : AbstractValidator<CreateHotelBookingCommand>
    {
        public CreateHotelBookingValidator()
        {
            RuleFor(x => x.requestDTO.room_id)
                .NotEqual(0)
                .GreaterThan(0)
                .WithMessage("Room Id must be greater than zero.");

            RuleFor(x => x.requestDTO.check_in_date)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Check-in date cannot be in the past.");

            RuleFor(x => x.requestDTO.check_out_date)
                .GreaterThan(x => x.requestDTO.check_in_date)
                .WithMessage("Check-out date must be after check-in date.");

            RuleFor(x => x.requestDTO.guests_adults)
                .GreaterThan(0)
                .WithMessage("At least one adult is required.");

            RuleFor(x => x.requestDTO.guests_children)
                .GreaterThanOrEqualTo(0)
                .WithMessage("guests_children count cannot be negative.");
        }
    }
}
