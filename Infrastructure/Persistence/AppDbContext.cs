#nullable disable
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext — the single source of truth for all database access.
/// Implements IApplicationDbContext so Application layer never depends on this concrete type.
/// </summary>
public partial class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public virtual DbSet<booking> bookings { get; set; }
    public virtual DbSet<car> cars { get; set; }
    public virtual DbSet<car_booking> car_bookings { get; set; }
    public virtual DbSet<car_booking_extra> car_booking_extras { get; set; }
    public virtual DbSet<car_brand> car_brands { get; set; }
    public virtual DbSet<car_category> car_categories { get; set; }
    public virtual DbSet<car_extra> car_extras { get; set; }
    public virtual DbSet<car_image> car_images { get; set; }
    public virtual DbSet<car_pricing_tier> car_pricing_tiers { get; set; }
    public virtual DbSet<favorite> favorites { get; set; }
    public virtual DbSet<flight> flights { get; set; }
    public virtual DbSet<flight_booking> flight_bookings { get; set; }
    public virtual DbSet<flight_booking_passenger> flight_booking_passengers { get; set; }
    public virtual DbSet<hotel> hotels { get; set; }
    public virtual DbSet<hotel_booking> hotel_bookings { get; set; }
    public virtual DbSet<hotel_image> hotel_images { get; set; }
    public virtual DbSet<location> locations { get; set; }
    public virtual DbSet<payment> payments { get; set; }
    public virtual DbSet<review> reviews { get; set; }
    public virtual DbSet<role> roles { get; set; }
    public virtual DbSet<room> rooms { get; set; }
    public virtual DbSet<room_availability> room_availabilities { get; set; }
    public virtual DbSet<room_extra> room_extras { get; set; }
    public virtual DbSet<room_image> room_images { get; set; }
    public virtual DbSet<tour> tours { get; set; }
    public virtual DbSet<tour_booking> tour_bookings { get; set; }
    public virtual DbSet<tour_image> tour_images { get; set; }
    public virtual DbSet<tour_inclusion> tour_inclusions { get; set; }
    public virtual DbSet<tour_price_tier> tour_price_tiers { get; set; }
    public virtual DbSet<tour_schedule> tour_schedules { get; set; }
    public virtual DbSet<passenger> passengers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── booking ───────────────────────────────────────────
        modelBuilder.Entity<booking>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__bookings__3213E83F6EE64705");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.currency).HasDefaultValue("USD").IsFixedLength();
            entity.Property(e => e.payment_status).HasDefaultValue("unpaid");
            entity.Property(e => e.status)
                .HasConversion(
                    v => v != null ? (int)Enum.Parse<Domain.Enums.BookingStatus>(v, true) : 0,
                    v => Enum.GetName(typeof(Domain.Enums.BookingStatus), v) ?? "Pending")
                .HasColumnType("int");
            entity.HasOne(d => d.coupon).WithMany(p => p.bookings)
                .HasConstraintName("FK_bookings_coupon");
            entity.HasOne(d => d.passenger).WithMany(p => p.bookings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_bookings_user");
        });

        // ── car ───────────────────────────────────────────────
        modelBuilder.Entity<car>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__cars__3213E83FD29F8504");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.status).HasDefaultValue("draft");
            entity.HasOne(d => d.brand).WithMany(p => p.cars)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_cars_brand");
            entity.HasOne(d => d.car_category).WithMany(p => p.cars)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_cars_category");
            entity.HasOne(d => d.dropoff_location).WithMany(p => p.cardropoff_locations)
                .HasConstraintName("FK_cars_dropoff_location");
            entity.HasOne(d => d.pickup_location).WithMany(p => p.carpickup_locations)
                .HasConstraintName("FK_cars_pickup_location");
        });

        // ── car_booking ───────────────────────────────────────
        modelBuilder.Entity<car_booking>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__car_book__3213E83FE8D298FB");
            entity.HasOne(d => d.booking).WithOne(p => p.car_booking)
                .HasConstraintName("FK_car_bookings_booking");
            entity.HasOne(d => d.car).WithMany(p => p.car_bookings)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_car_bookings_car");
            entity.HasOne(d => d.dropoff_location).WithMany(p => p.car_bookingdropoff_locations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_car_bookings_dropoff_location");
            entity.HasOne(d => d.pickup_location).WithMany(p => p.car_bookingpickup_locations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_car_bookings_pickup_location");
        });

        // ── car_booking_extra ─────────────────────────────────
        modelBuilder.Entity<car_booking_extra>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__car_book__3213E83F3CA189D1");
            entity.Property(e => e.quantity).HasDefaultValue(1);
            entity.HasOne(d => d.car_booking).WithMany(p => p.car_booking_extras)
                .HasConstraintName("FK_car_booking_extras_booking");
            entity.HasOne(d => d.car_extra).WithMany(p => p.car_booking_extras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_car_booking_extras_extra");
        });

        // ── car_brand ─────────────────────────────────────────
        modelBuilder.Entity<car_brand>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__car_bran__3213E83FC4C36A9B");
        });

        // ── car_category ──────────────────────────────────────
        modelBuilder.Entity<car_category>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__car_cate__3213E83FE7D737A6");
        });

        // ── car_extra ─────────────────────────────────────────
        modelBuilder.Entity<car_extra>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__car_extr__3213E83F91415630");
        });

        // ── car_image ─────────────────────────────────────────
        modelBuilder.Entity<car_image>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__car_imag__3213E83F1A84F5DE");
            entity.HasOne(d => d.car).WithMany(p => p.car_images)
                .HasConstraintName("FK_car_images_car");
        });

        // ── car_pricing_tier ──────────────────────────────────
        modelBuilder.Entity<car_pricing_tier>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__car_pric__3213E83F05E65D4C");
            entity.HasOne(d => d.car).WithMany(p => p.car_pricing_tiers)
                .HasConstraintName("FK_car_pricing_tiers_car");
        });

        // ── favorite ──────────────────────────────────────────
        modelBuilder.Entity<favorite>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__favorite__3213E83F3AE46643");
            entity.Property(e => e.added_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.passenger).WithMany(p => p.favorites)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_favorites_user");
        });

        // ── flight ────────────────────────────────────────────
        modelBuilder.Entity<flight>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__flights__3213E83F33D7A2D7");
            entity.Property(e => e.cabin_class).HasDefaultValue("economy");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.currency).HasDefaultValue("USD").IsFixedLength();
            entity.Property(e => e.destination_airport_code).IsFixedLength();
            entity.Property(e => e.origin_airport_code).IsFixedLength();
            entity.Property(e => e.status).HasDefaultValue("scheduled");
        });

        // ── flight_booking ────────────────────────────────────
        modelBuilder.Entity<flight_booking>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__flight_b__3213E83FA547652B");
            entity.HasOne(d => d.booking).WithOne(p => p.flight_booking)
                .HasConstraintName("FK_flight_bookings_booking");
            entity.HasOne(d => d.flight).WithMany(p => p.flight_bookingflights)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_flight_bookings_flight");
            entity.HasOne(d => d.return_flight).WithMany(p => p.flight_bookingreturn_flights)
                .HasConstraintName("FK_flight_bookings_return_flight");
        });

        // ── flight_booking_passenger ──────────────────────────
        modelBuilder.Entity<flight_booking_passenger>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__flight_b__3213E83F11541365");
            entity.HasOne(d => d.flight_booking).WithMany(p => p.flight_booking_passengers)
                .HasConstraintName("FK_flight_passengers_booking");
        });

        // ── hotel ─────────────────────────────────────────────
        modelBuilder.Entity<hotel>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__hotels__3213E83F0A769D6A");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.status).HasDefaultValue("draft");
            entity.HasOne(d => d.location).WithMany(p => p.hotels)
                .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_hotels_location");
        });

        // ── hotel_booking ─────────────────────────────────────
        modelBuilder.Entity<hotel_booking>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__hotel_bo__3213E83F0F36DF05");
            entity.Property(e => e.guests_adults).HasDefaultValue(1);
            entity.Property(e => e.quantity).HasDefaultValue(1);
            entity.HasOne(d => d.booking).WithOne(p => p.hotel_booking)
                .HasConstraintName("FK_hotel_bookings_booking");
            entity.HasOne(d => d.room).WithMany(p => p.hotel_bookings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_hotel_bookings_room");
        });

        // ── hotel_image ───────────────────────────────────────
        modelBuilder.Entity<hotel_image>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__hotel_im__3213E83F5E313734");
            entity.HasOne(d => d.hotel).WithMany(p => p.hotel_images)
                .HasConstraintName("FK_hotel_images_hotel");
        });

        // ── location ──────────────────────────────────────────
        modelBuilder.Entity<location>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__location__3213E83F32EDE253");
        });

        // ── payment ───────────────────────────────────────────
        modelBuilder.Entity<payment>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__payments__3213E83FA5C35E5F");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.currency).HasDefaultValue("USD").IsFixedLength();
            entity.Property(e => e.status).HasDefaultValue("pending");
            entity.HasOne(d => d.booking).WithMany(p => p.payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_payments_booking");
        });

        // ── review ────────────────────────────────────────────
        modelBuilder.Entity<review>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__reviews__3213E83F474DF86E");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.status).HasDefaultValue("pending");
            entity.HasOne(d => d.booking).WithMany(p => p.reviews)
                .HasConstraintName("FK_reviews_booking");
            entity.HasOne(d => d.passenger).WithMany(p => p.reviews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_reviews_user");
        });

        // ── role ──────────────────────────────────────────────
        modelBuilder.Entity<role>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__roles__3213E83F247A45DA");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasData(
                new role { id = 1, name = "Passenger", created_at = new System.DateTime(2026, 7, 7, 0, 0, 0, System.DateTimeKind.Utc) },
                new role { id = 2, name = "Admin", created_at = new System.DateTime(2026, 7, 7, 0, 0, 0, System.DateTimeKind.Utc) }
            );
        }); 

        // ── room ──────────────────────────────────────────────
        modelBuilder.Entity<room>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__rooms__3213E83FA2911DD9");
            entity.Property(e => e.occupancy_adults).HasDefaultValue(2);
            entity.Property(e => e.refundable).HasDefaultValue(true);
            entity.Property(e => e.status).HasDefaultValue("draft");
            entity.HasOne(d => d.hotel).WithMany(p => p.rooms)
                .HasConstraintName("FK_rooms_hotel");
        });

        // ── room_availability ─────────────────────────────────
        modelBuilder.Entity<room_availability>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__room_ava__3213E83F2D19570C");
            entity.HasOne(d => d.room).WithMany(p => p.room_availabilities)
                .HasConstraintName("FK_room_availability_room");
        });

        // ── room_extra ────────────────────────────────────────
        modelBuilder.Entity<room_extra>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__room_ext__3213E83F4B5F7045");
            entity.HasOne(d => d.room).WithMany(p => p.room_extras)
                .HasConstraintName("FK_room_extras_room");
        });

        // ── room_image ────────────────────────────────────────
        modelBuilder.Entity<room_image>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__room_ima__3213E83F658ACE73");
            entity.HasOne(d => d.room).WithMany(p => p.room_images)
                .HasConstraintName("FK_room_images_room");
        });

        // ── tour ──────────────────────────────────────────────
        modelBuilder.Entity<tour>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__tours__3213E83F0E0F030A");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.status).HasDefaultValue(Domain.Enums.TourStatus.Draft);
            entity.HasOne(d => d.location).WithMany(p => p.tours)
                .HasConstraintName("FK_tours_location");
        });

        // ── tour_booking ──────────────────────────────────────
        modelBuilder.Entity<tour_booking>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__tour_boo__3213E83F469EEF5F");
            entity.Property(e => e.adults_count).HasDefaultValue(1);
            entity.HasOne(d => d.booking).WithOne(p => p.tour_booking)
                .HasConstraintName("FK_tour_bookings_booking");
            entity.HasOne(d => d.tour_schedule).WithMany(p => p.tour_bookings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tour_bookings_schedule");
        });

        // ── tour_image ────────────────────────────────────────
        modelBuilder.Entity<tour_image>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__tour_ima__3213E83FA46D7C76");
            entity.HasOne(d => d.tour).WithMany(p => p.tour_images)
                .HasConstraintName("FK_tour_images_tour");
        });

        // ── tour_inclusion ────────────────────────────────────
        modelBuilder.Entity<tour_inclusion>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__tour_inc__3213E83F53BCBFD2");
            entity.HasOne(d => d.tour).WithMany(p => p.tour_inclusions)
                .HasConstraintName("FK_tour_inclusions_tour");
        });

        // ── tour_price_tier ───────────────────────────────────
        modelBuilder.Entity<tour_price_tier>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__tour_pri__3213E83FD09A27BF");
            entity.Property(e => e.currency).HasDefaultValue("USD").IsFixedLength();
            entity.HasOne(d => d.tour).WithMany(p => p.tour_price_tiers)
                .HasConstraintName("FK_tour_price_tiers_tour");
        });

        // ── tour_schedule ─────────────────────────────────────
        modelBuilder.Entity<tour_schedule>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__tour_sch__3213E83FF02F5E8A");
            entity.HasOne(d => d.price_tier).WithMany(p => p.tour_schedules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tour_schedules_price_tier");
            entity.HasOne(d => d.tour).WithMany(p => p.tour_schedules)
                .HasConstraintName("FK_tour_schedules_tour");
        });

        // ── passenger ─────────────────────────────────────────
        modelBuilder.Entity<passenger>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__users__3213E83F803EACAC");
            entity.Property(e => e.created_at).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.status).HasDefaultValue("unverified");
            entity.HasOne(d => d.location).WithMany(p => p.passengers)
                .HasConstraintName("FK_users_location");
            entity.HasOne(d => d.role).WithMany(p => p.passengers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_users_role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
