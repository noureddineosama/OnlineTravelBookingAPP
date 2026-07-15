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
            CreateMap<UpdateHotelRequestDTO, hotel>();
            CreateMap<hotel, UpdateHotelResponseDTO>().
                ForMember(op => op.Name, opt =>
                opt.MapFrom(op => op.name))
            .ForMember(op => op.Status, opt => opt.MapFrom(op => op.status))
            .ForMember(op => op.StarRating, opt => opt.MapFrom(op => op.star_rating))
            .ForMember(op => op.Slug, opt => opt.MapFrom(op => op.slug))
            .ForMember(op => op.Description, opt => opt.MapFrom(op => op.description))
            .ForMember(op => op.HotelId, opt => opt.MapFrom(op => op.id))
            .ForMember(op => op.UpdatedAt , opt => opt.MapFrom(op => op.updated_at));
        }
    }
}
