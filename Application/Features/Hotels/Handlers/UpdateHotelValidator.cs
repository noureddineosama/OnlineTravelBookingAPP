using Application.Features.Hotels.Commands;
using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.Handlers
{
    public  class UpdateHotelValidator : AbstractValidator<UpdateHotelCommand>
    {
        public UpdateHotelValidator()
        {
            RuleFor(x => x.requestDTO.name)
          .NotEmpty()
          .MaximumLength(200);

            RuleFor(x => x.requestDTO.description)
                .MaximumLength(2000);

            RuleFor(x => x.requestDTO.location_id)
                .GreaterThan(0);

            RuleFor(x => x.requestDTO.star_rating)
                .InclusiveBetween((byte)1, (byte)5)
                .When(x => x.requestDTO.star_rating.HasValue);

            RuleFor(x => x.requestDTO.Status)
                .NotNull();

            RuleFor(x => x)
                .Must(x => x.requestDTO.check_out_time > x.requestDTO.check_in_time)
                .When(x => x.requestDTO.check_in_time.HasValue && x.requestDTO.check_out_time.HasValue);

        }
    }
}
