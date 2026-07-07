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
    private static readonly DateTime SqlDateTimeMin = new(1753, 1, 1);
    private static readonly DateTime SqlDateTimeMax = new(2999, 12, 31);

    // Mirrors the legacy SavePage1: each section saves independently against its own
    // stored procedure and swallows its own failure, so one bad section doesn't block the rest.
    // MedicationList/AllergiesMedicationList aren't included yet -- see SavePage1Request.
    public async Task<bool> SavePage1Async(long claimId, SavePage1Request request, CancellationToken cancellationToken = default)
    {
        var chiefComplaintOk = await SaveChiefComplaintPatientMedicalHistoryAsync(
            claimId, request.ChiefComplaintPatientMedicalHistory, request.AtHome, request.AccompaniedBy, request.TypeOfVisit, cancellationToken);
        var medicalFamilySocialHistoryOk = await SaveMedicalFamilySocialHistoryAsync(claimId, request.MedicalFamilySocialHistory, cancellationToken);
        var advanceDirectiveOk = await SaveAdvanceDirectiveAsync(claimId, request.AdvanceDirective, cancellationToken);
        var reviewOfSystemOk = await SaveReviewOfSystemAsync(claimId, request.ReviewOfSystem, cancellationToken);
        var myocardialInfarctionOk = await SaveMyocardialInfarctionAsync(claimId, request.MyocardialInfarction, cancellationToken);

        return chiefComplaintOk && medicalFamilySocialHistoryOk && advanceDirectiveOk && reviewOfSystemOk && myocardialInfarctionOk;
    }

    private async Task<bool> SaveChiefComplaintPatientMedicalHistoryAsync(
        long claimId,
        ChiefComplaintPatientMedicalHistorySection? section,
        bool atHome,
        string? accompaniedBy,
        int typeOfVisit,
        CancellationToken cancellationToken)
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
                "uspSaveChiefComplaintPatientMedHistory",
                new
                {
                    ClaimID = claimId,
                    HistoryCurrentIllnesses = section.HistoryOfPresentIllness,
                    HasRecentHosp = section.RecentHospitalization,
                    RecentHospDate = section.RecentHospitalizationDate,
                    section.AllergiesNotes,
                    section.NoAllergies,
                    section.TotalColectomy,
                    section.TotalColectomyDate,
                    section.BilateralMastectomy,
                    section.BilateralMastectomyDate,
                    section.OtherSurgery,
                    section.OtherSurgeryDate,
                    section.NoSurgery,
                    AtHome = atHome,
                    section.UnilateralMastectomyLeft,
                    section.UnilateralMastectomyLeftDate,
                    section.UnilateralMastectomyRight,
                    section.UnilateralMastectomyRightDate,
                    section.HistoryPresentIllnessSelectedText,
                    AccompaniedBy = accompaniedBy,
                    TypeOfVisit = typeOfVisit,
                    choseRiskofHIV = section.ChoseRiskofHiv,
                    choseOtherSTD = section.ChoseOtherStd,
                    choseQuittingTabacco = section.ChoseQuittingTabacco,
                    choseDrinkingAlcohol = section.ChoseDrinkingAlcohol,
                    choseUseIllicitDrugs = section.ChoseUseIllicitDrugs,
                    section.PatientConcentTelecomm,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save chief complaint / patient medical history for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveMedicalFamilySocialHistoryAsync(long claimId, MedicalFamilySocialHistorySection? section, CancellationToken cancellationToken)
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
                "uspSaveMedicalFamilySocialHistory",
                new
                {
                    ClaimID = claimId,
                    NA = section.Na,
                    section.DmPatient,
                    section.DmMother,
                    section.DmFather,
                    section.DmSiblings,
                    section.CvdPatient,
                    section.CvdMother,
                    section.CvdFather,
                    section.CvdSiblings,
                    CholPatient = section.CholesterolPatient,
                    CholMother = section.CholesterolMother,
                    CholFather = section.CholesterolFather,
                    CholSiblings = section.CholesterolSiblings,
                    section.CancerPatient,
                    section.CancerMother,
                    section.CancerFather,
                    section.CancerSiblings,
                    section.AlzheimerPatient,
                    section.AlzheimerMother,
                    section.AlzheimerFather,
                    section.AlzheimerSiblings,
                    VIHPatient = section.VihPatient,
                    VIHMother = section.VihMother,
                    VIHFather = section.VihFather,
                    VIHSiblings = section.VihSiblings,
                    RiskForHIV = section.RiskForHiv,
                    RiskForSTD = section.RiskForStd,
                    section.CounselTabaccoUse,
                    section.CounselIllicitDrugUse,
                    section.CounselAlcoholUse,
                    PatientNA = section.PatientNa,
                    MotherNA = section.MotherNa,
                    FatherNA = section.FatherNa,
                    BrotherNA = section.SiblingsNa,
                    section.HistoryAlcoholism,
                    section.HistoryDrugDependence,
                    section.HistoryCaffeineDependence,
                    section.Nicotine,
                    section.Opiates,
                    section.Cannabis,
                    section.Sedatives,
                    section.Hypnotics,
                    section.Anxiolytics,
                    section.OtherDrugs,
                    section.OtherDrugsText,
                    section.OtherConditionText,
                    section.OtherPatient,
                    section.OtherMother,
                    section.OtherFather,
                    section.OtherSiblings,
                    section.HistoryOfMotherPregnancy,
                    section.FisicalActivityScreening,
                    FisicalActivityScreening_NA = section.FisicalActivityScreeningNa,
                    FisicalActivityScreening_DailyActivityRecommended = section.FisicalActivityScreeningDailyActivityRecommended,
                    NutricionalScreening_NA = section.NutricionalScreeningNa,
                    NutricionalScreening_AdequateIntake = section.NutricionalScreeningAdequateIntake,
                    NutricionalScreening_BalancedNutritiousDiet = section.NutricionalScreeningBalancedNutriciousDiet,
                    NutricionalScreening_Breastmilk = section.NutricionalScreeningBreastmilk,
                    NutricionalScreening_Cereal = section.NutricionalScreeningCereal,
                    NutricionalScreening_CowMilk = section.NutricionalScreeningCowMilk,
                    NutricionalScreening_FeedsItself = section.NutricionalScreeningFeedsItself,
                    NutricionalScreening_Formula = section.NutricionalScreeningFormula,
                    NutricionalScreening_JunkFood = section.NutricionalScreeningJunkFood,
                    NutricionalScreening_Others = section.NutricionalScreeningOther,
                    NutricionalScreening_Overweight = section.NutricionalScreeningOverweight,
                    NutricionalScreening_SodaJuices = section.NutricionalScreeningSodaJuices,
                    NutricionalScreening_SolidFoot = section.NutricionalScreeningSolidFoot,
                    NutricionalScreening_SupplementVitamins = section.NutricionalScreeningSupplementVitamins,
                    NutricionalScreening_UnderWeight = section.NutricionalScreeningUnderWeight,
                    NutricionalScreening_FoodAllergies = section.NutricionalScreeningFoodAllergies,
                    NutricionalScreening_SpecialDiets = section.NutricionalScreeningSpecialDiets,
                    NutricionalScreening_Others_Checkbox = section.NutricionalScreeningOthersCheckbox,
                    DevelopmentHealth_NA = section.DevelopmentHealthNa,
                    DevelopmentScreening_CommunicationArea = section.DevelopmentScreeningCommunicationArea,
                    DevelopmentScreening_FineMotorSkillArea = section.DevelopmentScreeningFineMotorSkillArea,
                    DevelopmentScreening_GrossMotorSkillsArea = section.DevelopmentScreeningGrossMotorSkillsArea,
                    DevelopmentScreening_SocialIndividualSkillsArea = section.DevelopmentScreeningSocialIndividualSkillsArea,
                    DevelopmentScreening_ProblemResolutionSkillArea = section.DevelopmentScreeningProblemResolutionSkillArea,
                    DevelopmentScreening_BehavioralHealthArea = section.DevelopmentScreeningBehavioralHealthArea,
                    BehavioralHealth_NA = section.BehavioralHealthNa,
                    BehavioralHealth_PhysicalMentalSelftRegulation = section.BehavioralHealthPhysicalMentalSelfRegulation,
                    BehavioralHealth_HabilityToFollowsInstructionsRules = section.BehavioralHealthHabilityToFollowsInstructionsRules,
                    BehavioralHealth_SocialCommunication = section.BehavioralHealthSocialCommunication,
                    BehavioralHealth_AdaptativeFunctioning = section.BehavioralHealthAdaptativeFunctioning,
                    BehavioralHealth_Autonomy = section.BehavioralHealthAutonomy,
                    BehavioralHealth_CapacityToBeAffectiveEmpathic = section.BehavioralHealthCapacityToBeAffectiveEmpathic,
                    BehavioralHealth_InteractionWithPeople = section.BehavioralHealthInteractionWithPeople,
                    BehavioralHealth_UsesAlcoholDrugs = section.BehavioralHealthUsesAlcoholDrugs,
                    AppropriateEducation_NA = section.AppropriateEducationNa,
                    AppropriateEducation_AppropiateUseCarSeat = section.AppropriateEducationAppropriateUseCarSeat,
                    AppropriateEducation_BottleProp = section.AppropriateEducationBottleProp,
                    AppropriateEducation_PasiveSmoke = section.AppropriateEducationPassiveSmoke,
                    AppropriateEducation_InfantCryingWhatToDo = section.AppropriateEducationInfantCryingWhatToDo,
                    AppropriateEducation_ShakeBabyPrevention = section.AppropriateEducationShakeBabyPrevention,
                    AppropriateEducation_Firearm = section.AppropriateEducationFirearm,
                    AppropriateEducation_Pacifiers = section.AppropriateEducationPacifiers,
                    AppropriateEducation_ParentsReadToChild = section.AppropriateEducationParentsReadToChild,
                    AppropriateEducation_Emergency911 = section.AppropriateEducationEmergency911,
                    AppropriateEducation_FingerFoodChoking = section.AppropriateEducationFingerFoodChoking,
                    AppropriateEducation_DisciplinePrais = section.AppropriateEducationDisciplinePraise,
                    AppropriateEducation_DrowningPrevention = section.AppropriateEducationDrowningPrevention,
                    AppropriateEducation_NeverLeaveToddlerAlone = section.AppropriateEducationNeverLeaveToddlerAlone,
                    AppropriateEducation_ToiletTraining = section.AppropriateEducationToiletTraining,
                    AppropriateEducation_NutritionExercise = section.AppropriateEducationNutritionExercise,
                    AppropriateEducation_EstablishRoutineBedMealsToiletingEtc = section.AppropriateEducationEstablishRoutineBedMealsToiletingEtc,
                    AppropriateEducation_UseSportProtection = section.AppropriateEducationUseSportProtection,
                    AppropriateEducation_Bullying = section.AppropriateEducationBullying,
                    AppropriateEducation_OralHealth = section.AppropriateEducationOralHealth,
                    AppropriateEducation_Others = section.AppropriateEducationOthers,
                    AppropriateEducation_SportInjuryPrevention = section.AppropriateEducationSportInjuryPrevention,
                    AppropriateEducation_DrowningSunSafety = section.AppropriateEducationDrowningSunSafety,
                    AppropriateEducation_SafeAtHome = section.AppropriateEducationSafeAtHome,
                    AppropriateEducation_CorrectUseSeatbelt = section.AppropriateEducationCorrectUseSeatbelt,
                    AppropriateEducation_SexualEducationSTD = section.AppropriateEducationSexualEducationStd,
                    AppropriateEducation_DepresionAnxiety = section.AppropriateEducationDepressionAnxiety,
                    AppropriateEducation_TabaccoAlcoholDrugsRxDrugsInhalants = section.AppropriateEducationTabaccoAlcoholDrugsRxDrugsInhalants,
                    AppropriateEducation_RiskOfTattoosPiercing = section.AppropriateEducationRiskOfTattoosPiercing,
                    AppropriateEducation_Autocontrol = section.AppropriateEducationAutocontrol,
                    AppropriateEducation_Others_Checkbox = section.AppropriateEducationOthersCheckbox,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save medical/family/social history for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveAdvanceDirectiveAsync(long claimId, AdvanceDirectiveSection? section, CancellationToken cancellationToken)
    {
        // Legacy skips the SP call entirely (and never flags an error) when the section is absent.
        if (section is null)
        {
            return true;
        }

        try
        {
            using var connection = connectionFactory.CreateConnection();

            // Legacy clamps out-of-range dates to the minimum valid SQL Server `datetime` rather
            // than letting the insert fail.
            var executeOn = section.AdvanceCarePlanExecuteOn;
            if (executeOn.HasValue && (executeOn < SqlDateTimeMin || executeOn > SqlDateTimeMax))
            {
                executeOn = SqlDateTimeMin;
            }

            var command = new CommandDefinition(
                "uspSaveAdvanceDirectives",
                new
                {
                    ClaimID = claimId,
                    RefuseToCompleteAdv = section.RefuseToCompleteAdvance,
                    AdvCarePlanDiscussed = section.AdvanceCarePlanDiscussed,
                    AdvCarePlanExecuteOn = section.AdvanceCarePlanExecutedOnCheck,
                    AdvCarePlanExecuteOnDate = executeOn,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save advance directive for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveReviewOfSystemAsync(long claimId, ReviewOfSystemSection? section, CancellationToken cancellationToken)
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
                "uspSaveReviewOfSystem",
                new
                {
                    ClaimID = claimId,
                    section.Constitutional,
                    HEENTOral = section.Heentoral,
                    section.AllergicImmunologic,
                    section.HematologicLymphatic,
                    section.Cardiovascular,
                    section.Gastrointestinal,
                    section.Genitourinary,
                    section.Respiratory,
                    section.Musculoskeletal,
                    section.Neurological,
                    section.Endocrine,
                    section.Integumentary,
                    section.Psychiatric,
                    UrinaryIncontinence = section.UrinaryIncontinenceLeaking,
                    UrinaryIncontinence_BladderExercises = section.UrinaryIncontinenceBladderExercises,
                    UrinaryIncontinence_TreatmentWithMedicine = section.UrinaryIncontinenceTreatmentWithMedicine,
                    UrinaryIncontinence_SurgicalIntervention = section.UrinaryIncontinenceSurgicalIntervention,
                    PositiveNotes = section.DescribePositiveRos,
                    UrinaryIncontinence_Other = section.UrinaryIncontinenceOther,
                    UrinaryIncontinence_CheckBoxOther = section.UrinaryIncontinenceCheckBoxOther,
                    section.HearingDifficulty,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save review of system for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveMyocardialInfarctionAsync(long claimId, MyocardialInfarctionSection? section, CancellationToken cancellationToken)
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
                "uspSaveMyocardialInfarction",
                new
                {
                    ClaimID = claimId,
                    OldMi = section.OldMi,
                    section.BetaBlocker,
                    section.BetaBlockerType,
                    OldMIOtherTreatment = section.OtherTreatmentCircumstances,
                    MedicalHistory_AMI_6_Months = section.Ami6Months,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save myocardial infarction for claim {ClaimId}", claimId);
            return false;
        }
    }

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

