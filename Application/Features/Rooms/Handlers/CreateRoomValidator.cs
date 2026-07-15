using Application.Features.Rooms.Commands;
using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Handlers
{
    public class CreateRoomValidator : AbstractValidator<CreateRoomCommand>
    {
        public CreateRoomValidator()
        {

            RuleFor(x => x.requestDTO.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.requestDTO.BedType)
                .NotEmpty()
                .MaximumLength(10);

            RuleFor(x => x.requestDTO.OccupancyAdults)
                .GreaterThan(0);

            RuleFor(x => x.requestDTO.OccupancyChildren)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.requestDTO.PricePerNight)
                .GreaterThan(0);
        }
    }
}
