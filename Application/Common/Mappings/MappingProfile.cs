using AutoMapper;
using Domain.Entities;
using Application.Features.Passengers.DTOs;

namespace Application.Common.Mappings;

/// <summary>
/// AutoMapper profile that defines all entity-to-DTO mappings.
/// Auto-discovered by AddAutoMapper() in DependencyInjection.cs.
/// </summary>
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── Passenger ────────────────────────────────────────
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
    }
}
