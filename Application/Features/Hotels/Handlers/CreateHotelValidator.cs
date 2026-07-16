using Application.Features.Hotels.DTOs;
using Application.Features.Hotels.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Application.Features.Hotels.Handlers
{
    public class CreateHotelValidator : AbstractValidator<CreateHotelCommand>
    {
        public CreateHotelValidator()
        {
            RuleFor(x => x.requestDTO.Name)
           .NotEmpty()
           .MaximumLength(200);

            RuleFor(x => x.requestDTO.Description)
                .MaximumLength(2000);

            RuleFor(x => x.requestDTO.location.country)
                .NotEmpty();

            RuleFor(x => x.requestDTO.location.city)
                .NotEmpty();

            RuleFor(x => x.requestDTO.location.address_line)
                .NotEmpty();

            RuleFor(x => x.requestDTO.StarRating)
                .InclusiveBetween((byte)1, (byte)5)
                .When(x => x.requestDTO.StarRating.HasValue);

            RuleFor(x => x.requestDTO.Status)
                .NotEmpty();

            RuleFor(x => x)
                .Must(x => x.requestDTO.CheckOutTime > x.requestDTO.CheckInTime)
                .When(x => x.requestDTO.CheckInTime.HasValue && x.requestDTO.CheckOutTime.HasValue)
                .WithMessage("Check-out time must be after check-in time.");
        }
    }
}
