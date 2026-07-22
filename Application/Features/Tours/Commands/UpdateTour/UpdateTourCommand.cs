using Application.Common.Caching;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Tours.Cache;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tours.Commands.UpdateTour;

public sealed record UpdateTourCommand(
    long Id,
    string Title,
    string? Summary,
    string? FullDescription,
    string? MainImageUrl,
    int? DurationDays,
    int? LocationId,
    string? Difficulty,
    TourStatus Status) : IRequest<ApiResponse<bool>>;

// ── Validation ───────────────────────────────────────────────────────────────

public sealed class UpdateTourCommandValidator : AbstractValidator<UpdateTourCommand>
{
    public UpdateTourCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be a valid ID.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid TourStatus.");
    }
}

// ── Handler ──────────────────────────────────────────────────────────────────

internal sealed class UpdateTourCommandHandler : IRequestHandler<UpdateTourCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork   _uow;
    private readonly ICacheService _cache;

    public UpdateTourCommandHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow   = uow;
        _cache = cache;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateTourCommand request, CancellationToken cancellationToken)
    {
        var entity = await _uow.Repository<tour>().Query()
            .FirstOrDefaultAsync(t => t.id == request.Id, cancellationToken);

        if (entity == null)
            return ApiResponse<bool>.Fail("Tour not found.", 404);

        if (entity.is_deleted)
            return ApiResponse<bool>.Fail("Cannot update a deleted tour.", 400);

        entity.title = request.Title;
        entity.summary = request.Summary;
        entity.full_description = request.FullDescription;
        entity.main_image_url = request.MainImageUrl;
        entity.duration_days = request.DurationDays;
        entity.location_id = request.LocationId;
        entity.difficulty = request.Difficulty;
        entity.status = request.Status;

        _uow.Repository<tour>().Update(entity);
        await _uow.SaveChangesAsync(cancellationToken);

        // Invalidate the specific tour detail and every list page.
        await _cache.RemoveAsync(TourCacheKeys.ById(request.Id), cancellationToken);
        await _cache.RemoveAsync(TourCacheKeys.BySlug(entity.slug ?? string.Empty), cancellationToken);
        await _cache.RemoveByPrefixAsync(TourCacheKeys.ListPrefix, cancellationToken);

        return ApiResponse<bool>.Ok(true);
    }
}
