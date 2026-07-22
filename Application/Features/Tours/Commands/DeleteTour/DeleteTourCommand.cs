using Application.Common.Caching;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Tours.Cache;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tours.Commands.DeleteTour;

public sealed record DeleteTourCommand(long Id) : IRequest<ApiResponse<bool>>;

// ── Validation ───────────────────────────────────────────────────────────────

public sealed class DeleteTourCommandValidator : AbstractValidator<DeleteTourCommand>
{
    public DeleteTourCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be a valid ID.");
    }
}

// ── Handler ──────────────────────────────────────────────────────────────────

internal sealed class DeleteTourCommandHandler : IRequestHandler<DeleteTourCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork           _uow;
    private readonly ICurrentIUserService  _currentUserService;
    private readonly ICacheService         _cache;

    public DeleteTourCommandHandler(
        IUnitOfWork uow,
        ICurrentIUserService currentUserService,
        ICacheService cache)
    {
        _uow                = uow;
        _currentUserService = currentUserService;
        _cache              = cache;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteTourCommand request, CancellationToken cancellationToken)
    {
        var entity = await _uow.Repository<tour>().Query()
            .Include(t => t.tour_images)
            .Include(t => t.tour_inclusions)
            .Include(t => t.tour_price_tiers)
            .Include(t => t.tour_schedules)
                .ThenInclude(s => s.tour_bookings)
                    .ThenInclude(tb => tb.booking)
            .FirstOrDefaultAsync(t => t.id == request.Id, cancellationToken);

        if (entity == null)
            return ApiResponse<bool>.Fail("Tour not found.", 404);

        if (entity.is_deleted)
            return ApiResponse<bool>.Fail("Tour is already deleted.", 400);

        var hasBookings = entity.tour_schedules.Any(s => s.tour_bookings.Any());
        long adminId = _currentUserService.UserId;

        // Since _context.Database.BeginTransactionAsync is not exposed via IUnitOfWork,
        // and EfRepository doesn't expose RemoveRange easily, we need to handle this.
        // IUnitOfWork is primarily designed for aggregate root persistence. 
        // For complex cascading deletes/transactions we can still use the underlying DbContext if needed, 
        // or loop the deletes. Let's loop the deletes which is standard for repository patterns,
        // or just use `Remove` on each. EF Core handles cascades if configured, but here it seems manual.

        // Actually, we don't need a manual transaction if we just do operations and call SaveChangesAsync once.
        // EF Core SaveChangesAsync is always wrapped in a transaction by default.
        // So we can remove the explicit BeginTransactionAsync and just rely on SaveChangesAsync.

        if (!hasBookings)
        {
            // Hard Delete
            var favorites = await _uow.Repository<favorite>().GetListOfEntityAsync(f => f.item_id == request.Id && f.category == "tour", cancellationToken);
            foreach(var fav in favorites)
                _uow.Repository<favorite>().Remove(fav);

            foreach(var img in entity.tour_images)
                _uow.Repository<tour_image>().Remove(img);
                
            foreach(var inc in entity.tour_inclusions)
                _uow.Repository<tour_inclusion>().Remove(inc);
                
            foreach(var sched in entity.tour_schedules)
                _uow.Repository<tour_schedule>().Remove(sched);
                
            foreach(var pt in entity.tour_price_tiers)
                _uow.Repository<tour_price_tier>().Remove(pt);

            _uow.Repository<tour>().Remove(entity);
        }
            else
            {
                // Soft Delete & Cancel Bookings
                string reason = "Cancelled by Admin";
                entity.SoftDelete(adminId, reason);

                foreach (var schedule in entity.tour_schedules)
                {
                    foreach (var tb in schedule.tour_bookings)
                    {
                        var booking = tb.booking;
                        if (booking != null && booking.status != BookingStatus.Cancelled.ToString() && booking.status != BookingStatus.Completed.ToString())
                        {
                            booking.status = BookingStatus.Cancelled.ToString();
                            booking.IsCancelled = true;
                            booking.cancelled_at = DateTime.UtcNow;
                            booking.cancellation_reason_type = CancellationReasonType.AdminCancelled;
                            booking.cancellation_reason_details = reason;
                        }
                    }
                }
                
                _uow.Repository<tour>().Update(entity);
            }

            await _uow.SaveChangesAsync(cancellationToken);

            // Invalidate cache regardless of hard vs soft delete path.
            await _cache.RemoveAsync(TourCacheKeys.ById(request.Id), cancellationToken);
            await _cache.RemoveAsync(TourCacheKeys.BySlug(entity.slug ?? string.Empty), cancellationToken);
            await _cache.RemoveByPrefixAsync(TourCacheKeys.ListPrefix, cancellationToken);

            return ApiResponse<bool>.Ok(true);
    }
}
