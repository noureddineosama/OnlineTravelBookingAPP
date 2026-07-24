using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

/// <summary>
/// Abstraction over the database context. Implemented by Infrastructure.
/// This ensures the Application layer never depends on the concrete DbContext.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<booking> bookings { get; }
    DbSet<car> cars { get; }
    DbSet<car_booking> car_bookings { get; }
    DbSet<car_booking_extra> car_booking_extras { get; }
    DbSet<car_brand> car_brands { get; }
    DbSet<car_category> car_categories { get; }
    DbSet<car_extra> car_extras { get; }
    DbSet<car_image> car_images { get; }
    DbSet<car_pricing_tier> car_pricing_tiers { get; }
    DbSet<favorite> favorites { get; }
    DbSet<flight> flights { get; }
    DbSet<flight_booking> flight_bookings { get; }
    DbSet<flight_booking_passenger> flight_booking_passengers { get; }
    DbSet<hotel> hotels { get; }
    DbSet<hotel_booking> hotel_bookings { get; }
    DbSet<hotel_image> hotel_images { get; }
    DbSet<location> locations { get; }
    DbSet<payment> payments { get; }
    DbSet<review> reviews { get; }
    DbSet<role> roles { get; }
    DbSet<room> rooms { get; }
    DbSet<room_availability> room_availabilities { get; }
    DbSet<room_extra> room_extras { get; }
    DbSet<room_image> room_images { get; }
    DbSet<tour> tours { get; }
    DbSet<tour_booking> tour_bookings { get; }
    DbSet<tour_image> tour_images { get; }
    DbSet<tour_inclusion> tour_inclusions { get; }
    DbSet<tour_price_tier> tour_price_tiers { get; }
    DbSet<tour_schedule> tour_schedules { get; }
    DbSet<passenger> passengers { get; }

    Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
