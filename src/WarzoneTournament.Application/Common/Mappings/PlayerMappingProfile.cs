using AutoMapper;
using WarzoneTournament.Application.DTOs.Player;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Application.Common.Mappings;

public class PlayerMappingProfile : Profile
{
    public PlayerMappingProfile()
    {
        CreateMap<Player, PlayerDto>();

        CreateMap<CreatePlayerDto, Player>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.IsVerified, opt => opt.MapFrom(_ => false))
            .ForMember(d => d.IsBanned, opt => opt.MapFrom(_ => false))
            .ForMember(d => d.BanReason, opt => opt.Ignore())
            .ForMember(d => d.TotalKills, opt => opt.MapFrom(_ => 0))
            .ForMember(d => d.TotalDeaths, opt => opt.MapFrom(_ => 0))
            .ForMember(d => d.TotalMatches, opt => opt.MapFrom(_ => 0))
            .ForMember(d => d.TotalWins, opt => opt.MapFrom(_ => 0))
            .ForMember(d => d.TeamPlayers, opt => opt.Ignore())
            .ForMember(d => d.MatchStats, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore());
    }
}
