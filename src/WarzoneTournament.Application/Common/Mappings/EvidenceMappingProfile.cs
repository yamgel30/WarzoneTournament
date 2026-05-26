using AutoMapper;
using WarzoneTournament.Application.DTOs.Evidence;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Application.Common.Mappings;

public class EvidenceMappingProfile : Profile
{
    public EvidenceMappingProfile()
    {
        CreateMap<MatchEvidence, EvidenceDto>()
            .ForMember(d => d.SubmittedByTeamName, opt => opt.Ignore())
            .ForMember(d => d.SubmittedByPlayerUsername, opt => opt.Ignore())
            .ForMember(d => d.OcrResult, opt => opt.Ignore())
            .ForMember(d => d.Reviews, opt => opt.Ignore());

        CreateMap<OCRExtractionResult, OcrResultDto>();

        CreateMap<EvidenceReview, EvidenceReviewDto>();
    }
}
