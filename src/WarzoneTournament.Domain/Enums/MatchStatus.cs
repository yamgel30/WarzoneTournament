namespace WarzoneTournament.Domain.Enums;

public enum MatchStatus
{
    Pending = 0,
    CheckIn = 1,
    InProgress = 2,
    WaitingEvidence = 3,
    UnderReview = 4,
    Completed = 5,
    Disputed = 6,
    Cancelled = 7
}
