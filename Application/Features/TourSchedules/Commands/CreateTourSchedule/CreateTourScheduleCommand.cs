using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.TourSchedules.Commands.CreateTourSchedule;

public sealed record CreateTourScheduleCommand(
    long TourId,
    long PriceTierId,
    DateTime StartDate,
    DateTime? EndDate,
    int Capacity
) : IRequest<ApiResponse<long>>;

public sealed class CreateTourScheduleCommandValidator : AbstractValidator<CreateTourScheduleCommand>
{
    public CreateTourScheduleCommandValidator()
    {
        RuleFor(x => x.TourId).GreaterThan(0);
        RuleFor(x => x.PriceTierId).GreaterThan(0);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}

public sealed class CreateTourScheduleCommandHandler : IRequestHandler<CreateTourScheduleCommand, ApiResponse<long>>
{
    private readonly IUnitOfWork _uow;

    public CreateTourScheduleCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<long>> Handle(CreateTourScheduleCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify Tour exists
        var tourExists = await _uow.Repository<tour>().AnyAsync(t => t.id == request.TourId, cancellationToken);
        if (!tourExists)
            throw new NotFoundException(nameof(tour), request.TourId);

        // 2. Verify PriceTier belongs to this Tour
        var priceTierExists = await _uow.Repository<tour_price_tier>()
            .AnyAsync(pt => pt.id == request.PriceTierId && pt.tour_id == request.TourId, cancellationToken);
        if (!priceTierExists)
            throw new NotFoundException(nameof(tour_price_tier), request.PriceTierId);

        // 3. Check for duplicate schedule (same TourId and StartDate)
        var duplicateExists = await _uow.Repository<tour_schedule>()
            .AnyAsync(s => s.tour_id == request.TourId && s.start_date == request.StartDate, cancellationToken);

        if (duplicateExists)
            throw new ConflictException("A schedule with the same start date already exists for this tour.");

        // 4. Create schedule
        var schedule = new tour_schedule
        {
            tour_id = request.TourId,
            price_tier_id = request.PriceTierId,
            start_date = request.StartDate,
            end_date = request.EndDate,
            capacity = request.Capacity,
            available_slots = request.Capacity
        };

        await _uow.Repository<tour_schedule>().AddAsync(schedule, cancellationToken);

        // 5. Save changes (handle possible DB constraint violations from race conditions)
        try
        {
            await _uow.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_tour_schedules_tour_start") == true)
        {
            throw new ConflictException("A schedule with the same start date already exists for this tour.");
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("FK_tour_schedules_price_tier") == true)
        {
            throw new NotFoundException(nameof(tour_price_tier), request.PriceTierId);
        }

        return ApiResponse<long>.Ok(schedule.id, "Tour schedule created successfully.");
    }
}
