using Application.Features.HotelAvailability.DTOs;
using Application.Features.HotelBooking.DTOs;
using Application.Features.Hotels.DTOs;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Profiles
{
    public class HotelProfile : Profile
    {
        public HotelProfile()
        {
            CreateMap<CreateHotelBookingRequestDTO, CheckRoomAvailabilityRequestDTO>();
        }
    }
}
