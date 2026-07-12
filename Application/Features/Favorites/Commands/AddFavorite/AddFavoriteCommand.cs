using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Favorites.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Favorites.Commands.AddFavorite;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record AddFavoriteCommand(
    long             UserId,
    FavoriteCategory Category,
    long             ItemId
) : IRequest<ApiResponse<FavoriteDto>>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class AddFavoriteCommandValidator : AbstractValidator<AddFavoriteCommand>
{
    public AddFavoriteCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a valid ID.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be one of: Tour, Hotel, Flight, Car.");

        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("ItemId must be a valid ID.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class AddFavoriteCommandHandler
    : IRequestHandler<AddFavoriteCommand, ApiResponse<FavoriteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper               _mapper;

    public AddFavoriteCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper  = mapper;
    }

    public async Task<ApiResponse<FavoriteDto>> Handle(
        AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify passenger exists
        var passenger = await _context.passengers
            .FindAsync([request.UserId], cancellationToken);

        if (passenger is null)
            throw new NotFoundException(nameof(passenger), request.UserId);

        // 2. Verify the referenced item exists and is active
        await ValidateItemAsync(request.Category, request.ItemId, cancellationToken);

        // 3. Check for duplicate (DB unique index UQ_favorites is the safety net)
        var categoryStr = request.Category.ToString().ToLower();

        var alreadyExists = await _context.favorites
            .AnyAsync(f =>
                f.user_id  == request.UserId &&
                f.category == categoryStr    &&
                f.item_id  == request.ItemId,
                cancellationToken);

        if (alreadyExists)
            throw new ConflictException(
                $"This {request.Category} is already in your favourites.");

        // 4. Persist
        var entity = new favorite
        {
            user_id  = request.UserId,
            category = categoryStr,
            item_id  = request.ItemId,
            added_at = DateTime.UtcNow
        };

        await _context.favorites.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // 5. AutoMapper: favorite → FavoriteDto (mapping defined in MappingProfile)
        var dto = _mapper.Map<FavoriteDto>(entity);

        return ApiResponse<FavoriteDto>.Ok(dto,
            $"{request.Category} added to favourites successfully.");
    }

    // ── Item validation (private — no service needed for simple exists checks) ─

    private async Task ValidateItemAsync(
        FavoriteCategory category, long itemId, CancellationToken ct)
    {
        if (category == FavoriteCategory.Tour)
        {
            var ok = await _context.tours
                .AnyAsync(t => t.id == itemId && t.status == "active", ct);
            if (!ok) throw new NotFoundException("Active Tour", itemId);
        }
        else if (category == FavoriteCategory.Hotel)
        {
            var ok = await _context.hotels
                .AnyAsync(h => h.id == itemId && h.status == "active", ct);
            if (!ok) throw new NotFoundException("Active Hotel", itemId);
        }
        else if (category == FavoriteCategory.Flight)
        {
            var ok = await _context.flights
                .AnyAsync(f => f.id == itemId && f.status == "active", ct);
            if (!ok) throw new NotFoundException("Active Flight", itemId);
        }
        else if (category == FavoriteCategory.Car)
        {
            var ok = await _context.cars
                .AnyAsync(c => c.id == itemId && c.status == "active", ct);
            if (!ok) throw new NotFoundException("Active Car", itemId);
        }
        else
        {
            // Fail-fast: a new FavoriteCategory was added without a validation branch.
            // This will surface immediately in integration tests, not silently in production.
            throw new BadRequestException(
                $"No item validation is registered for category '{category}'. "
              + "Update ValidateItemAsync to handle this category.");
        }
    }
}
