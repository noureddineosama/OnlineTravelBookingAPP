using Application.Features.FlightBookings.DTOs;
using Application.Features.Flights.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Profiles;

public sealed class FlightBookingProfile : Profile
{
    public FlightBookingProfile()
    {
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
    }
}
