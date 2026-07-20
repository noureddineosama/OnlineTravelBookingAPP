using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.TourSchedules.Commands.AdminCancelTourSchedule;

public sealed record AdminCancelTourScheduleCommand(
    long ScheduleId,
    string Reason
) : IRequest<ApiResponse<string>>;

public sealed class AdminCancelTourScheduleCommandValidator : AbstractValidator<AdminCancelTourScheduleCommand>
{
    public AdminCancelTourScheduleCommandValidator()
    {
        RuleFor(x => x.ScheduleId)
            .GreaterThan(0).WithMessage("ScheduleId must be a valid ID.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("A cancellation reason must be provided.");
    }
}

public sealed class AdminCancelTourScheduleCommandHandler
    : IRequestHandler<AdminCancelTourScheduleCommand, ApiResponse<string>>
{
    private readonly IUnitOfWork _uow;

    public AdminCancelTourScheduleCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<string>> Handle(
        AdminCancelTourScheduleCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the schedule with its active bookings
        var schedule = await _uow.Repository<tour_schedule>().Query()
            .Include(s => s.tour_bookings)
                .ThenInclude(tb => tb.booking)
            .FirstOrDefaultAsync(s => s.id == request.ScheduleId, cancellationToken);

        if (schedule is null)
            throw new NotFoundException("Tour schedule", request.ScheduleId);

        // 2. Validate it's not already cancelled
        if (schedule.is_cancelled)
            throw new ConflictException("This tour schedule is already cancelled.");

        // 3. Update the schedule status
        schedule.is_cancelled = true;
        schedule.cancelled_at = DateTime.UtcNow;
        schedule.cancellation_reason = request.Reason;

        // 4. Cancel all active bookings on this schedule
        foreach (var tb in schedule.tour_bookings)
        {
            var booking = tb.booking;
            if (booking != null && booking.status != BookingStatus.Cancelled.ToString() && booking.status != BookingStatus.Completed.ToString())
            {
                booking.status = BookingStatus.Cancelled.ToString();
                booking.IsCancelled = true;
                booking.cancelled_at = DateTime.UtcNow;
                booking.cancellation_reason_type = CancellationReasonType.AdminCancelled;
                booking.cancellation_reason_details = $"Schedule cancelled: {request.Reason}";
                booking.updated_at = DateTime.UtcNow;
                _uow.Repository<booking>().Update(booking);
            }
        }

        _uow.Repository<tour_schedule>().Update(schedule);

        // 5. Save changes
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.Ok("Cancelled.", "Tour schedule and all associated active bookings were successfully cancelled.");
    }
}
