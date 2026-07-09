using Application.Features.HotelBooking.DTOs;
using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.HotelBooking.Handlers
{
    public class CreateHotelBookingValidator : AbstractValidator<CreateHotelBookingRequestDTO>
    {
        public CreateHotelBookingValidator()
        {
            RuleFor(x => x.RoomId)
                .GreaterThan(0)
                .WithMessage("Room Id must be greater than zero.");

            RuleFor(x => x.CheckInDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Check-in date cannot be in the past.");

            RuleFor(x => x.CheckOutDate)
                .GreaterThan(x => x.CheckInDate)
                .WithMessage("Check-out date must be after check-in date.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");

            RuleFor(x => x.Adults)
                .GreaterThan(0)
                .WithMessage("At least one adult is required.");

            RuleFor(x => x.Children)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Children count cannot be negative.");
        }
    }
}
