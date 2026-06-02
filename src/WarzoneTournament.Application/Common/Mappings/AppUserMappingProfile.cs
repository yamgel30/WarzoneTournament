using AutoMapper;
using WarzoneTournament.Application.DTOs.Auth;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Application.Common.Mappings;

public class AppUserMappingProfile : Profile
{
    public AppUserMappingProfile()
    {
        CreateMap<AppUser, AppUserDto>();
    }
}
