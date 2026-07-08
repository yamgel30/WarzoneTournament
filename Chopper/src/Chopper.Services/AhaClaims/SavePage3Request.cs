namespace Chopper.Services.AhaClaims;

public sealed record SavePage3Request
{
    // Legacy reads this off the top-level form header to decide which stored procedure to call
    // (pre-2023 vs. 2023+ screening form).
    public DateTime DateOfVisit { get; init; }

    public ScreeningScheduleSection? ScreeningSchedule { get; init; }

    // Only used when DateOfVisit falls in 2023 or later (uspSaveScreeningTest2023).
    public ScreeningSchedule2023Extras? ScreeningSchedule2023Extras { get; init; }

    public PhysicalExaminationSection? PhysicalExamination { get; init; }
}
