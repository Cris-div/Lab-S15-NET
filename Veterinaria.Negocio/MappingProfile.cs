using AutoMapper;
using Veterinaria.Entidades.DTOs;
using Veterinaria.Entidades.Models;

namespace Veterinaria.Negocio;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Propietarios, PropietarioDto>();
        CreateMap<Mascotas, MascotaDto>()
            .ForMember(d => d.PropietarioNombre,
                o => o.MapFrom(s => s.Propietario.Nombre));

        CreateMap<PropietarioCrearDto, Propietarios>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Mascotas, o => o.Ignore());
        CreateMap<PropietarioActualizarDto, Propietarios>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Mascotas, o => o.Ignore());

        CreateMap<MascotaCrearDto, Mascotas>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Propietario, o => o.Ignore());
        CreateMap<MascotaActualizarDto, Mascotas>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Propietario, o => o.Ignore());
    }
}
