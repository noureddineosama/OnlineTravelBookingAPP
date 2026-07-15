using Application.Features.Hotels.Queries;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Hotels.Handlers
{
    //. Validations for the Values that will be bineded from the query string of the request
    public class SearchValidator 
                : AbstractValidator<SearchQuery>
    {
        public SearchValidator()
        {
            RuleFor(x => x.requestdTO.PageNumber)
                .NotNull()
                .GreaterThan(0);

            RuleFor(x => x.requestdTO.PageSize)
                .NotNull()
                .InclusiveBetween(1, 100);

            RuleFor(x => x.requestdTO.Adults)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.requestdTO.Children)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.requestdTO.StarRating)
                .InclusiveBetween((byte)1, (byte)5)
                .When(x => x.requestdTO.StarRating.HasValue);

            RuleFor(x => x.requestdTO.CheckOutDate)
                .GreaterThan(x => x.requestdTO.CheckInDate)
                .When(x =>
                    x.requestdTO.CheckOutDate.HasValue &&
                    x.requestdTO.CheckOutDate.HasValue);
        }
    }
}
