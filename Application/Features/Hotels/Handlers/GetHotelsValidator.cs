using Application.Features.Hotels.Queries;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.Handlers
{
    public class GetHotelsValidator: AbstractValidator<GetPagedHotelsQuery>
    {
        public GetHotelsValidator()
        {
            RuleFor(x => x.requestDTO.PageNumber)
           .GreaterThan(0);

            RuleFor(x => x.requestDTO.PageSize)
                .InclusiveBetween(1, 100);

            RuleFor(x => x.requestDTO.StarRating)
                .InclusiveBetween((byte)1, (byte)5)
                .When(x => x.requestDTO.StarRating.HasValue);
        }
    }
}
