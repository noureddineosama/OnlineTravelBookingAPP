using Application.Common.Caching;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Tours.Cache;
using Application.Features.Tours.DTOs;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Features.Tours.Commands.CreateTour;



using Domain.Enums;

public sealed record CreateTourCommand(
    string Title,
    string? Summary,
    string? FullDescription,
    string? MainImageUrl,
    int? DurationDays,
    int? LocationId,
    string? Difficulty,
    TourStatus Status,
    CreateTourPriceTierDto? PriceTier = null,
    CreateTourScheduleDto? Schedule = null) : IRequest<ApiResponse<CreateTourResponse>>;

public sealed class CreateTourCommandValidator : AbstractValidator<CreateTourCommand>
{
    public CreateTourCommandValidator()
    {
        RuleFor(v => v.Title)
            .MaximumLength(200)
            .NotEmpty();

        RuleFor(v => v.Status)
            .IsInEnum();
    }
}

internal sealed class CreateTourCommandHandler : IRequestHandler<CreateTourCommand, ApiResponse<CreateTourResponse>>
{
    private readonly IUnitOfWork    _uow;
    private readonly IMapper        _mapper;
    private readonly ICacheService  _cache;

    public CreateTourCommandHandler(IUnitOfWork uow, IMapper mapper, ICacheService cache)
    {
        _uow    = uow;
        _mapper = mapper;
        _cache  = cache;
    }

    public async Task<ApiResponse<CreateTourResponse>> Handle(CreateTourCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<tour>(request);
        
        // Generate a simple unique slug based on title
        var baseSlug = entity.title.ToLower().Replace(" ", "-").Replace("'", "").Replace("\"", "");
        entity.slug = baseSlug + "-" + Guid.NewGuid().ToString("N")[..6];

        await _uow.Repository<tour>().AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        long? scheduleId = null;
        long? priceTierId = null;

        if (request.PriceTier != null && request.Schedule != null)
        {
            var priceTier = new tour_price_tier
            {
                tour_id = entity.id,
                name = request.PriceTier.Name,
                adult_price = request.PriceTier.AdultPrice,
                child_price = request.PriceTier.ChildPrice,
                infant_price = request.PriceTier.InfantPrice,
                currency = request.PriceTier.Currency
            };

            await _uow.Repository<tour_price_tier>().AddAsync(priceTier, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var schedule = new tour_schedule
            {
                tour_id = entity.id,
                price_tier_id = priceTier.id,
                start_date = request.Schedule.StartDate,
                end_date = request.Schedule.EndDate,
                capacity = request.Schedule.AvailableSlots,
                available_slots = request.Schedule.AvailableSlots
            };

            await _uow.Repository<tour_schedule>().AddAsync(schedule, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            scheduleId = schedule.id;
            priceTierId = priceTier.id;
        }

        // Invalidate the tour list cache so the new tour appears immediately.
        await _cache.RemoveByPrefixAsync(TourCacheKeys.ListPrefix, cancellationToken);

        return ApiResponse<CreateTourResponse>.Ok(new CreateTourResponse(entity.id, scheduleId, priceTierId));
    }
}
