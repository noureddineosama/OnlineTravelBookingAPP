using Application.Features.Rooms.Queries;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Rooms.Handlers
{
    public class SearchRoomValidator : AbstractValidator<SearchRoomQuery>
    {
        public SearchRoomValidator()
        {
            RuleFor(x => x.requestDTO.Adults)
            .GreaterThan(0)
            .When(x => x.requestDTO.Adults.HasValue);

            RuleFor(x => x.requestDTO.Children)
                .GreaterThanOrEqualTo(0)
                .When(x => x.requestDTO.Children.HasValue);

            RuleFor(x => x.requestDTO.MinPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.requestDTO.MinPrice.HasValue);

            RuleFor(x => x.requestDTO.MaxPrice)
                .GreaterThan(0)
                .When(x => x.requestDTO.MaxPrice.HasValue);

            RuleFor(x => x.page)
                .GreaterThan(0);

            RuleFor(x => x.pageSize)
                .InclusiveBetween(1, 100);

            RuleFor(x => x)
                .Must(x => !x.requestDTO.MinPrice.HasValue ||
                           !x.requestDTO.MaxPrice.HasValue ||
                           x.requestDTO.MinPrice <= x.requestDTO.MaxPrice)
                .WithMessage("MinPrice must be less than or equal to MaxPrice.");

            RuleFor(x => x.requestDTO.BedType)
                .MaximumLength(10)
                .When(x => !string.IsNullOrWhiteSpace(x.requestDTO.BedType));
        }
    }
}
