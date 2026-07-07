using System.Data;
using Chopper.Services.Abstractions;
using Chopper.Services.Common;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Chopper.Services.AhaClaims;

internal sealed class AhaClaimService(
    ISqlConnectionFactory connectionFactory,
    ILogger<AhaClaimService> logger) : IAhaClaimService
{
    // Mirrors the legacy SavePage2: each section saves independently against its own
    // stored procedure and swallows its own failure, so one bad section doesn't block the rest.
    public async Task<bool> SavePage2Async(long claimId, SavePage2Request request, CancellationToken cancellationToken = default)
    {
        var medicationReviewOk = await SaveMedicationReviewAsync(claimId, request.MedicationReview, cancellationToken);
        var cognitiveAssessmentOk = await SaveCognitiveAssessmentAsync(claimId, request.CognitiveAssessment, cancellationToken);
        var painScreeningOk = await SavePainScreeningAsync(claimId, request.PainScreening, cancellationToken);
        var activitiesOfDailyLivingOk = await SaveActivitiesOfDailyLivingAsync(claimId, request.ActivitiesOfDailyLiving, cancellationToken);

        return medicationReviewOk && cognitiveAssessmentOk && painScreeningOk && activitiesOfDailyLivingOk;
    }

    private async Task<bool> SaveMedicationReviewAsync(long claimId, MedicationReviewSection? section, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveMedicationReview",
                new
                {
                    ClaimID = claimId,
                    section?.Question1,
                    section?.Question2,
                    section?.Question3,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save medication review for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveCognitiveAssessmentAsync(long claimId, CognitiveAssessmentSection? section, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveCognitiveAssessment",
                new
                {
                    ClaimID = claimId,
                    section?.DayOfTheWeek,
                    section?.MonthOfTheYear,
                    section?.Year,
                    section?.Ball,
                    section?.Flag,
                    section?.Tree,
                    WNL = section?.Wnl,
                    Dx = section?.Diagnosis,
                    PlanOf = section?.PlanGoalsTreatmentInterventionFollowUp,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save cognitive assessment for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SavePainScreeningAsync(long claimId, PainScreeningSection? section, CancellationToken cancellationToken)
    {
        // Legacy skips the SP call entirely (and never flags an error) when the section is absent.
        if (section is null)
        {
            return true;
        }

        try
        {
            using var connection = connectionFactory.CreateConnection();

            // Legacy quirk, preserved as-is: submitting "Others" forces PainEvaluationOtherCondition
            // to true and PainEvaluationOtherConditionText is populated from "Others" rather than its
            // own field.
            var painEvaluationOtherCondition = section.PainEvaluationOtherCondition;
            var painEvaluationOtherConditionText = section.PainEvaluationOtherConditionText;
            if (section.Others is not null)
            {
                painEvaluationOtherCondition = true;
                painEvaluationOtherConditionText = section.Others;
            }

            var command = new CommandDefinition(
                "uspSavePainScreening",
                new
                {
                    ClaimID = claimId,
                    section.PatientHaveComplaint,
                    PainLocation = section.PainIsLocated,
                    TreatmentOrMedication = section.ManagePainWith,
                    TreatmentHaveBeenEffectiveNA = section.TreatmentHaveBeenEffectiveNa,
                    MedicationEffective = section.TreatmentHaveBeenEffective,
                    PainRate = section.RatePainExperiencingNow,
                    section.Transportation,
                    section.BathingDressing,
                    section.WalkingAbility,
                    section.EnjoymentOfLife,
                    section.Toileting,
                    section.Sleep,
                    section.Mood,
                    NA = section.Na,
                    section.Employment,
                    Housework = section.HouseWork,
                    section.FoodPreparation,
                    Relationships = section.RelationshipWithOther,
                    section.PainDueTo,
                    PlanOf = section.PlanGoalsTreatmentInterventionFollowUp,
                    ArthritisDueInfection = section.ArthritisDueToInfection,
                    section.Others,
                    PainEvaluationOtherCondition = painEvaluationOtherCondition,
                    PainEvaluationOtherConditionText = painEvaluationOtherConditionText,
                    PainEvaluation_OtherActivities = section.PainEvaluationOtherActivities,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save pain screening for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveActivitiesOfDailyLivingAsync(long claimId, ActivitiesOfDailyLivingSection? section, CancellationToken cancellationToken)
    {
        // Legacy skips the SP call entirely (and never flags an error) when the section is absent.
        if (section is null)
        {
            return true;
        }

        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveActivitiesDailyLiving",
                new
                {
                    ClaimID = claimId,
                    section.Bathing,
                    section.BathingComments,
                    DressingUnDressing = section.DressingAndUndressing,
                    DressingUnDressingComments = section.DressingAndUndressingComments,
                    section.Eating,
                    section.EatingComments,
                    TransferingFrom = section.TransferringBedChair,
                    TransferingFromComments = section.TransferringBedChairComments,
                    section.VoluntarilyControl,
                    section.VoluntarilyControlComments,
                    section.UsingToilet,
                    section.UsingToiletComments,
                    section.Walking,
                    section.WalkingComments,
                    section.BedFast,
                    section.HistoryOfFalling,
                    HistoryOfFalling_Comments = section.HistoryOfFallingComments,
                    section.DependenceOnOxygen,
                    section.DependenceOnRespirator,
                    section.DependenceOnWheelchair,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save activities of daily living for claim {ClaimId}", claimId);
            return false;
        }
    }
}
