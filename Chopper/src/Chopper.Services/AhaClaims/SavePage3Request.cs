namespace Chopper.Services.AhaClaims;

public sealed record SavePage3Request
{
    // Legacy reads this off the top-level form header to decide which stored procedure to call
    // (pre-2023 vs. 2023+ screening form).
    public DateTime DateOfVisit { get; init; }

    public ScreeningScheduleSection? ScreeningSchedule { get; init; }

    public PhysicalExaminationSection? PhysicalExamination { get; init; }
}
