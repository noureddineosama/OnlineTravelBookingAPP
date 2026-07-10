using Application.Features.HotelBooking.DTOs;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Profiles
{
    public class HotelBookingProfile : Profile
    {
        public HotelBookingProfile()
        {
            CreateMap<CreateHotelBookingRequestDTO, hotel_booking>();
        }
    }
}
