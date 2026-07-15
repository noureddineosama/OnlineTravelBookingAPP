using Application.Features.HotelBooking.DTOs;
using Application.Features.Rooms.DTOs;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Profiles
{
    public class RoomPrfoile : Profile
    {
        public RoomPrfoile()
        {
            CreateMap<UpdateRoomRequestDTO, room>().
               ForMember(op => op.name, opt => opt.MapFrom(op => op.Name))
               .ForMember(op => op.bed_type, opt => opt.MapFrom(op => op.BedType)).
               ForMember(op => op.occupancy_adults, opt => opt.MapFrom(op => op.OccupancyAdults)).
               ForMember(op => op.occupancy_children, opt => opt.MapFrom(op => op.OccupancyChildren)).
               ForMember(op => op.refundable, opt => opt.MapFrom(op => op.Refundable)).
               ForMember(op => op.price_per_night, opt => opt.MapFrom(op => op.PricePerNight));

            CreateMap<CreateRoomRequestDTO, room>().
                ForMember(op => op.name, opt => opt.MapFrom(op => op.Name))
               .ForMember(op => op.bed_type, opt => opt.MapFrom(op => op.BedType)).
               ForMember(op => op.occupancy_adults, opt => opt.MapFrom(op => op.OccupancyAdults)).
               ForMember(op => op.occupancy_children, opt => opt.MapFrom(op => op.OccupancyChildren)).
               ForMember(op => op.refundable, opt => opt.MapFrom(op => op.Refundable)).
               ForMember(op => op.price_per_night, opt => opt.MapFrom(op => op.PricePerNight));

            CreateMap<CreateRoomExtraRequestDTO, room_extra>()
                .ForMember(op => op.name, opt => opt.MapFrom(op => op.Name))
                .ForMember(op => op.price, opt => opt.MapFrom(op => op.Price));

            CreateMap<UpdateRoomExtraRequestDTO, room_extra>()
                .ForMember(op => op.name, opt => opt.MapFrom(op => op.Name))
                .ForMember(op => op.price, opt => opt.MapFrom(op => op.Price));
        }
    }
}
