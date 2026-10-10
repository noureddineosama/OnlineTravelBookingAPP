using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Application.Features.Favorites.DTOs;
using Application.Features.Passengers.DTOs;
using Application.Features.TourBookings.DTOs;
using Application.Features.HotelAvailability.DTOs;
using Application.Features.HotelBooking.DTOs;
using Application.Features.Hotels.DTOs;
using Application.Features.Tours.DTOs;
using Application.Features.Tours.Commands.CreateTour;
using Application.Features.FlightBookings.DTOs;
using Application.Features.Flights.DTOs;
using Application.Features.Rooms.DTOs;

namespace Application.Common.Mappings;

/// <summary>
/// AutoMapper profile that defines all entity-to-DTO mappings.
/// Auto-discovered by AddAutoMapper() in DependencyInjection.cs.
/// </summary>
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── Passenger ────────────────────────────────────────────────────────
        CreateMap<passenger, PassengerResponse>()
            .ForMember(d => d.Id,              opt => opt.MapFrom(s => s.id))
            .ForMember(d => d.Name,            opt => opt.MapFrom(s => s.name))
            .ForMember(d => d.Email,           opt => opt.MapFrom(s => s.email))
            .ForMember(d => d.Phone,           opt => opt.MapFrom(s => s.phone))
            .ForMember(d => d.Status,          opt => opt.MapFrom(s => s.status))
            .ForMember(d => d.IsEmailVerified, opt => opt.MapFrom(s => s.is_email_verified))
            .ForMember(d => d.RoleName,        opt => opt.MapFrom(s => s.role != null ? s.role.name : null))
            .ForMember(d => d.CreatedAt,       opt => opt.MapFrom(s => s.created_at))
            .ForMember(d => d.UpdatedAt,       opt => opt.MapFrom(s => s.updated_at));

        // ── TourBookingResponse (from booking aggregate with includes) ────────
        // Used by GetTourBookingByIdQuery and GetUserTourBookingsQuery.
        // The booking entity must be loaded with:
        //   .Include(b => b.tour_booking)
        //       .ThenInclude(tb => tb.tour_schedule)
        //           .ThenInclude(s => s.tour)
        //   .Include(b => b.tour_booking)
        //       .ThenInclude(tb => tb.tour_schedule)
        //           .ThenInclude(s => s.price_tier)
        CreateMap<booking, TourBookingResponse>()
            .ForMember(d => d.BookingId,         opt => opt.MapFrom(s => s.id))
            .ForMember(d => d.BookingNumber,     opt => opt.MapFrom(s => s.booking_number))
            .ForMember(d => d.Status,            opt => opt.MapFrom(s => s.status))
            .ForMember(d => d.TourTitle,         opt => opt.MapFrom(s =>
                s.tour_booking != null && s.tour_booking.tour_schedule != null &&
                s.tour_booking.tour_schedule.tour != null
                    ? s.tour_booking.tour_schedule.tour.title : string.Empty))
            .ForMember(d => d.TourSlug,          opt => opt.MapFrom(s =>
                s.tour_booking != null && s.tour_booking.tour_schedule != null &&
                s.tour_booking.tour_schedule.tour != null
                    ? s.tour_booking.tour_schedule.tour.slug : string.Empty))
            .ForMember(d => d.TourMainImageUrl,  opt => opt.MapFrom(s =>
                s.tour_booking != null && s.tour_booking.tour_schedule != null &&
                s.tour_booking.tour_schedule.tour != null
                    ? s.tour_booking.tour_schedule.tour.main_image_url : null))
            .ForMember(d => d.ScheduleStartDate, opt => opt.MapFrom(s =>
                s.tour_booking != null && s.tour_booking.tour_schedule != null
                    ? s.tour_booking.tour_schedule.start_date : default(DateTime)))
            .ForMember(d => d.ScheduleEndDate,   opt => opt.MapFrom(s =>
                s.tour_booking != null && s.tour_booking.tour_schedule != null
                    ? s.tour_booking.tour_schedule.end_date : null))
            .ForMember(d => d.AdultsCount,       opt => opt.MapFrom(s =>
                s.tour_booking != null ? s.tour_booking.adults_count : 0))
            .ForMember(d => d.ChildrenCount,     opt => opt.MapFrom(s =>
                s.tour_booking != null ? s.tour_booking.children_count : 0))
            .ForMember(d => d.InfantsCount,      opt => opt.MapFrom(s =>
                s.tour_booking != null ? s.tour_booking.infants_count : 0))
            .ForMember(d => d.PriceTierName,     opt => opt.MapFrom(s =>
                s.tour_booking != null && s.tour_booking.tour_schedule != null &&
                s.tour_booking.tour_schedule.price_tier != null
                    ? s.tour_booking.tour_schedule.price_tier.name : string.Empty))
            .ForMember(d => d.AdultPrice,        opt => opt.MapFrom(s =>
                s.tour_booking != null && s.tour_booking.tour_schedule != null &&
                s.tour_booking.tour_schedule.price_tier != null
                    ? s.tour_booking.tour_schedule.price_tier.adult_price : 0m))
            .ForMember(d => d.ChildPrice,        opt => opt.MapFrom(s =>
                s.tour_booking != null && s.tour_booking.tour_schedule != null &&
                s.tour_booking.tour_schedule.price_tier != null
                    ? s.tour_booking.tour_schedule.price_tier.child_price : null))
            .ForMember(d => d.InfantPrice,       opt => opt.MapFrom(s =>
                s.tour_booking != null && s.tour_booking.tour_schedule != null &&
                s.tour_booking.tour_schedule.price_tier != null
                    ? s.tour_booking.tour_schedule.price_tier.infant_price : null))
            .ForMember(d => d.Subtotal,          opt => opt.MapFrom(s => s.subtotal))
            .ForMember(d => d.TotalPrice,        opt => opt.MapFrom(s => s.total_price))
            .ForMember(d => d.Currency,          opt => opt.MapFrom(s => s.currency))
            .ForMember(d => d.PaymentStatus,     opt => opt.MapFrom(s => s.payment_status))
            .ForMember(d => d.CreatedAt,         opt => opt.MapFrom(s => s.created_at));

        // ── Favorite ─────────────────────────────────────────────────────────
        // category is stored as a lowercase string ("tour", "hotel", etc.).
        // Enum.Parse with ignoreCase:true converts it to the FavoriteCategory enum.
        CreateMap<favorite, FavoriteDto>()
            .ForMember(d => d.FavoriteId,    opt => opt.MapFrom(s => s.id))
            .ForMember(d => d.UserId,        opt => opt.MapFrom(s => s.user_id))
            .ForMember(d => d.ItemId,        opt => opt.MapFrom(s => s.item_id))
            .ForMember(d => d.AddedAt,       opt => opt.MapFrom(s => s.added_at))
            .ForMember(d => d.Category,      opt => opt.MapFrom(s =>
                Enum.Parse<FavoriteCategory>(s.category, ignoreCase: true)))
            .ForMember(d => d.CategoryLabel, opt => opt.MapFrom(s =>
                string.IsNullOrEmpty(s.category)
                    ? string.Empty
                    : char.ToUpper(s.category[0]) + s.category.Substring(1).ToLower()))
            // ── UI-enrichment fields: populated by GetMyFavoritesQueryHandler ──
            // AddFavorite returns these as null (client already has item context).
            .ForMember(d => d.Title,    opt => opt.Ignore())
            .ForMember(d => d.Subtitle, opt => opt.Ignore())
            .ForMember(d => d.ImageUrl, opt => opt.Ignore())
            .ForMember(d => d.Price,    opt => opt.Ignore())
            .ForMember(d => d.Currency, opt => opt.Ignore())
            .ForMember(d => d.Rating,   opt => opt.Ignore())
            .ForMember(d => d.Location, opt => opt.Ignore())
            .ForMember(d => d.BadgeText,opt => opt.Ignore());

        // ── Hotel & HotelBooking ─────────────────────────────────────────────
        CreateMap<CreateHotelBookingRequestDTO, CheckRoomAvailabilityRequestDTO>();
        CreateMap<CreateHotelBookingRequestDTO, hotel_booking>();

        // ── Tours ────────────────────────────────────────────────────────────
        CreateMap<tour_price_tier, TourPriceTierDto>()
            .ForMember(d => d.Id,          opt => opt.MapFrom(s => s.id))
            .ForMember(d => d.Name,        opt => opt.MapFrom(s => s.name))
            .ForMember(d => d.AdultPrice,  opt => opt.MapFrom(s => s.adult_price))
            .ForMember(d => d.ChildPrice,  opt => opt.MapFrom(s => s.child_price))
            .ForMember(d => d.InfantPrice, opt => opt.MapFrom(s => s.infant_price))
            .ForMember(d => d.Currency,    opt => opt.MapFrom(s => s.currency));

        CreateMap<tour_schedule, TourScheduleDto>()
            .ForMember(d => d.Id,             opt => opt.MapFrom(s => s.id))
            .ForMember(d => d.PriceTierId,    opt => opt.MapFrom(s => s.price_tier_id))
            .ForMember(d => d.StartDate,      opt => opt.MapFrom(s => s.start_date))
            .ForMember(d => d.EndDate,        opt => opt.MapFrom(s => s.end_date))
            .ForMember(d => d.Capacity,       opt => opt.MapFrom(s => s.capacity))
            .ForMember(d => d.AvailableSlots, opt => opt.MapFrom(s => s.available_slots));

        CreateMap<tour, TourDto>()
            .ForMember(d => d.Id,              opt => opt.MapFrom(s => s.id))
            .ForMember(d => d.Title,           opt => opt.MapFrom(s => s.title))
            .ForMember(d => d.Slug,            opt => opt.MapFrom(s => s.slug))
            .ForMember(d => d.Summary,         opt => opt.MapFrom(s => s.summary))
            .ForMember(d => d.FullDescription, opt => opt.MapFrom(s => s.full_description))
            .ForMember(d => d.MainImageUrl,    opt => opt.MapFrom(s => s.main_image_url))
            .ForMember(d => d.DurationDays,    opt => opt.MapFrom(s => s.duration_days))
            .ForMember(d => d.LocationId,      opt => opt.MapFrom(s => s.location_id))
            .ForMember(d => d.Difficulty,      opt => opt.MapFrom(s => s.difficulty))
            .ForMember(d => d.Status,          opt => opt.MapFrom(s => s.status))
            .ForMember(d => d.PriceTiers,      opt => opt.MapFrom(s => s.tour_price_tiers))
            .ForMember(d => d.Schedules,       opt => opt.MapFrom(s => s.tour_schedules));

        CreateMap<CreateTourCommand, tour>()
            .ForMember(d => d.title,            opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.summary,          opt => opt.MapFrom(s => s.Summary))
            .ForMember(d => d.full_description, opt => opt.MapFrom(s => s.FullDescription))
            .ForMember(d => d.main_image_url,   opt => opt.MapFrom(s => s.MainImageUrl))
            .ForMember(d => d.duration_days,    opt => opt.MapFrom(s => s.DurationDays))
            .ForMember(d => d.location_id,      opt => opt.MapFrom(s => s.LocationId))
            .ForMember(d => d.difficulty,       opt => opt.MapFrom(s => s.Difficulty))
            .ForMember(d => d.status,           opt => opt.MapFrom(s => s.Status))
            .ForMember(d => d.id,               opt => opt.Ignore())
            .ForMember(d => d.slug,             opt => opt.Ignore())
            .ForMember(d => d.created_at,       opt => opt.Ignore())
            .ForMember(d => d.updated_at,       opt => opt.Ignore())
            .ForMember(d => d.location,         opt => opt.Ignore())
            .ForMember(d => d.tour_images,      opt => opt.Ignore())
            .ForMember(d => d.tour_inclusions,  opt => opt.Ignore())
            .ForMember(d => d.tour_price_tiers, opt => opt.Ignore())
            .ForMember(d => d.tour_schedules,   opt => opt.Ignore());

        // ── Flights & Flight Bookings ────────────────────────────────────────
        CreateMap<flight, FlightResponse>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.id))
            .ForMember(d => d.FlightNumber, opt => opt.MapFrom(s => s.flight_number))
            .ForMember(d => d.CarrierName, opt => opt.MapFrom(s => s.carrier_name))
            .ForMember(d => d.OriginAirportCode, opt => opt.MapFrom(s => s.origin_airport_code))
            .ForMember(d => d.OriginCity, opt => opt.MapFrom(s => s.origin_city))
            .ForMember(d => d.DestinationAirportCode, opt => opt.MapFrom(s => s.destination_airport_code))
            .ForMember(d => d.DestinationCity, opt => opt.MapFrom(s => s.destination_city))
            .ForMember(d => d.DepartureAtUtc, opt => opt.MapFrom(s => s.departure_at_utc))
            .ForMember(d => d.ArrivalAtUtc, opt => opt.MapFrom(s => s.arrival_at_utc))
            .ForMember(d => d.DurationMinutes, opt => opt.MapFrom(s => s.duration_minutes))
            .ForMember(d => d.CabinClass, opt => opt.MapFrom(s => s.cabin_class))
            .ForMember(d => d.BasePrice, opt => opt.MapFrom(s => s.base_price))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.currency))
            .ForMember(d => d.SeatsAvailable, opt => opt.MapFrom(s => s.seats_available))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.status));

        CreateMap<flight_booking_passenger, FlightBookingPassengerResponse>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.id))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.title))
            .ForMember(d => d.FirstName, opt => opt.MapFrom(s => s.first_name))
            .ForMember(d => d.LastName, opt => opt.MapFrom(s => s.last_name))
            .ForMember(d => d.PassportNumber, opt => opt.MapFrom(s => s.passport_number));

        CreateMap<flight_booking, FlightBookingResponse>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.id))
            .ForMember(d => d.BookingId, opt => opt.MapFrom(s => s.booking_id))
            .ForMember(d => d.BookingNumber, opt => opt.MapFrom(s => s.booking.booking_number))
            .ForMember(d => d.FlightId, opt => opt.MapFrom(s => s.flight_id))
            .ForMember(d => d.ReturnFlightId, opt => opt.MapFrom(s => s.return_flight_id))
            .ForMember(d => d.TripType, opt => opt.MapFrom(s => s.trip_type))
            .ForMember(d => d.Price, opt => opt.MapFrom(s => s.price))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.booking.currency))
            .ForMember(d => d.BookingStatus, opt => opt.MapFrom(s => s.booking.status))
            .ForMember(d => d.PaymentStatus, opt => opt.MapFrom(s => s.booking.payment_status))
            .ForMember(
                d => d.Passengers,
                opt => opt.MapFrom(s => s.flight_booking_passengers));

        // ── Hotel Updates ────────────────────────────────────────────────────
        CreateMap<UpdateHotelRequestDTO, hotel>();
        CreateMap<hotel, UpdateHotelResponseDTO>()
            .ForMember(op => op.Name, opt => opt.MapFrom(op => op.name))
            .ForMember(op => op.Status, opt => opt.MapFrom(op => op.status))
            .ForMember(op => op.StarRating, opt => opt.MapFrom(op => op.star_rating))
            .ForMember(op => op.Slug, opt => opt.MapFrom(op => op.slug))
            .ForMember(op => op.Description, opt => opt.MapFrom(op => op.description))
            .ForMember(op => op.HotelId, opt => opt.MapFrom(op => op.id))
            .ForMember(op => op.UpdatedAt, opt => opt.MapFrom(op => op.updated_at));

        // ── Room & Room Extras ───────────────────────────────────────────────
        CreateMap<UpdateRoomRequestDTO, room>()
            .ForMember(op => op.name, opt => opt.MapFrom(op => op.Name))
            .ForMember(op => op.bed_type, opt => opt.MapFrom(op => op.BedType))
            .ForMember(op => op.occupancy_adults, opt => opt.MapFrom(op => op.OccupancyAdults))
            .ForMember(op => op.occupancy_children, opt => opt.MapFrom(op => op.OccupancyChildren))
            .ForMember(op => op.refundable, opt => opt.MapFrom(op => op.Refundable))
            .ForMember(op => op.price_per_night, opt => opt.MapFrom(op => op.PricePerNight));

        CreateMap<CreateRoomRequestDTO, room>()
            .ForMember(op => op.name, opt => opt.MapFrom(op => op.Name))
            .ForMember(op => op.bed_type, opt => opt.MapFrom(op => op.BedType))
            .ForMember(op => op.occupancy_adults, opt => opt.MapFrom(op => op.OccupancyAdults))
            .ForMember(op => op.occupancy_children, opt => opt.MapFrom(op => op.OccupancyChildren))
            .ForMember(op => op.refundable, opt => opt.MapFrom(op => op.Refundable))
            .ForMember(op => op.price_per_night, opt => opt.MapFrom(op => op.PricePerNight));

        CreateMap<CreateRoomExtraRequestDTO, room_extra>()
            .ForMember(op => op.name, opt => opt.MapFrom(op => op.Name))
            .ForMember(op => op.price, opt => opt.MapFrom(op => op.Price));

        CreateMap<UpdateRoomExtraRequestDTO, room_extra>()
            .ForMember(op => op.name, opt => opt.MapFrom(op => op.Name))
            .ForMember(op => op.price, opt => opt.MapFrom(op => op.Price));
    }
}
