using AutoMapper;
using WarzoneTournament.Application.DTOs.Team;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Application.Common.Mappings;

public class TeamMappingProfile : Profile
{
    public TeamMappingProfile()
    {
        CreateMap<Team, TeamDto>()
            .ForMember(d => d.CaptainUsername, opt => opt.Ignore())
            .ForMember(d => d.PlayerCount, opt => opt.Ignore())
            .ForMember(d => d.Players, opt => opt.Ignore());

        CreateMap<CreateTeamDto, Team>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Captain, opt => opt.Ignore())
            .ForMember(d => d.TeamPlayers, opt => opt.Ignore())
            .ForMember(d => d.TournamentTeams, opt => opt.Ignore())
            .ForMember(d => d.MatchResults, opt => opt.Ignore())
            .ForMember(d => d.DiscordRoleId, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore());
    }
}
