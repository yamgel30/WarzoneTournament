using AutoMapper;
using WarzoneTournament.Application.DTOs.Tournament;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Application.Common.Mappings;

public class TournamentMappingProfile : Profile
{
    public TournamentMappingProfile()
    {
        CreateMap<Tournament, TournamentDto>()
            .ForMember(d => d.RegisteredTeamsCount, opt => opt.Ignore());

        CreateMap<Tournament, TournamentListDto>()
            .ForMember(d => d.RegisteredTeamsCount, opt => opt.Ignore());

        CreateMap<CreateTournamentDto, Tournament>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.MapFrom(_ => Domain.Enums.TournamentStatus.Draft))
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.CreatedBy, opt => opt.Ignore())
            .ForMember(d => d.UpdatedBy, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.WinnerTeamId, opt => opt.Ignore())
            .ForMember(d => d.TournamentTeams, opt => opt.Ignore())
            .ForMember(d => d.Rounds, opt => opt.Ignore())
            .ForMember(d => d.TournamentRules, opt => opt.Ignore())
            .ForMember(d => d.PrizeDistributions, opt => opt.Ignore())
            .ForMember(d => d.Matches, opt => opt.Ignore());
    }
}
