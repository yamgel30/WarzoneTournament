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

    // Legacy calls uspSaveScreeningTest for visits before 2023 and uspSaveScreeningTest2023 for
    // visits from 2023 onward. Only the pre-2023 stored procedure is ported so far.
    public async Task<bool> SavePage3Async(long claimId, SavePage3Request request, CancellationToken cancellationToken = default)
    {
        if (request.DateOfVisit.Year >= 2023)
        {
            throw new NotSupportedException(
                "Page 3 for visits from 2023 onward (uspSaveScreeningTest2023) isn't ported yet.");
        }

        var screeningScheduleOk = await SaveScreeningScheduleAsync(claimId, request.ScreeningSchedule, cancellationToken);
        var physicalExaminationOk = await SavePhysicalExaminationAsync(claimId, request.PhysicalExamination, cancellationToken);

        return screeningScheduleOk && physicalExaminationOk;
    }

    private async Task<bool> SavePhysicalExaminationAsync(long claimId, PhysicalExaminationSection? section, CancellationToken cancellationToken)
    {
        // Legacy skips the SP call entirely (and never flags an error) when the section is absent.
        if (section is null)
        {
            return true;
        }

        try
        {
            using var connection = connectionFactory.CreateConnection();
            var heenOral = section.HeenOralOptions;
            var constitutional = section.ConstitutionalOptions;
            var integumentary = section.IntegumentaryOptions;
            var respiratory = section.RespiratoryOptions;
            var gastrointestinal = section.GastrointestinalOptions;
            var genitourinary = section.GenitourinaryOptions;
            var neck = section.NeckOptions;
            var chest = section.ChestOptions;
            var cardiovascular = section.CardiovascularOptions;
            var abdomen = section.AbdomenOptions;
            var genitaliaGroinButtocks = section.GenitaliaGroinButtocksOptions;
            var musculoskeletal = section.MusculoskeletalOptions;
            var skin = section.SkinOptions;
            var psychiatricNeurologic = section.PsychiatricNeurologicOptions;
            var hematologicLymphaticImmunologic = section.HematologicLymphaticImmunologicOptions;

            var command = new CommandDefinition(
                "uspSavePhysicalExamination",
                new
                {
                    ClaimID = claimId,

                    // Legacy sends explicit defaults instead of NULL when these are missing.
                    Temperature = section.Temperature ?? 0m,
                    TemperatureType = section.TemperatureType ?? string.Empty,
                    section.Pulse,
                    section.Breathing,
                    BloodPresure1 = section.BloodPressure1,
                    BloodPresure2 = section.BloodPressure2,
                    Height = section.Height ?? 0m,
                    HeightType = section.HeightType ?? string.Empty,
                    Weight = section.Weight ?? 0m,
                    WeightType = section.WeightType ?? "lbs",
                    BMI = section.Bmi ?? 0m,

                    HEENTNotes = section.HeenOralNotes,
                    HEENOralOptions_Peerl = heenOral?.Peerl,
                    HEENOralOptions_NoTeeth = heenOral?.NoTeeth,
                    HEENOralOptions_DryMouth = heenOral?.DryMouth,
                    HEENOralOptions_DryNose = heenOral?.DryNose,
                    HEENOralOptions_BleedingGums = heenOral?.BleedingGums,
                    HEENOralOptions_WNL = heenOral?.Wnl,
                    HEENOralOptions_Strabismus = heenOral?.Strabismus,
                    HEENOralOptions_Ptosis = heenOral?.Ptosis,
                    HEENOralOptions_Redreflex = heenOral?.RedReflex,
                    HEENOralOptions_AbnormalPupillaryReflex = heenOral?.AbnormalPupillaryReflex,
                    HEENOralOptions_BlockedNasolacrimalDucts = heenOral?.BlockedNasolacrimalDucts,
                    HEENOralOptions_NasalDischarge = heenOral?.NasalDischarge,
                    HEENOralOptions_ExudatingTonsils = heenOral?.ExudatingTonsils,
                    HEENOptions_Normocephalic = heenOral?.Normocephalic,
                    HEENOptions_ScalpLessionsMasses = heenOral?.ScalpLessionsMasses,
                    HEENOptions_NeckSupple = heenOral?.NeckSupple,
                    HEENOptions_Adenopathies = heenOral?.Adenopathies,
                    HEENOptions_ClearOropharynxn = heenOral?.ClearOropharynxn,
                    HEENOptions_LessionExudate = heenOral?.LessionExudate,
                    HEENOptions_TympanicMembranesIntact = heenOral?.TympanicMembranesIntact,
                    HEENOptions_EqualAirConductionAndAcousticReflexes = heenOral?.EqualAirConductionAndAcousticReflexes,
                    HEENOptions_NoNystagmus = heenOral?.NoNystagmus,
                    HEENOptions_EOMI = heenOral?.Eomi,
                    HEENOptions_Other = heenOral?.Other,
                    HEENOptions_None = heenOral?.None,

                    ConstitutionalOptions_Notes = section.ConstitutionalNotes,
                    ConstitutionalOptions_WellDeveloped = constitutional?.WellDeveloped,
                    ConstitutionalOptions_PoorDeveloped = constitutional?.PoorDeveloped,
                    ConstitutionalOptions_AdequateNourishment = constitutional?.AdequateNourishment,
                    ConstitutionalOptions_InadequateNourishment = constitutional?.InadequateNourishment,
                    ConstitutionalOptions_InAcuteDistress = constitutional?.InAcuteDistress,
                    ConstitutionalOptions_NoAcuteDistress = constitutional?.NoAcuteDistress,
                    ConstitutionalOptions_CAOX = constitutional?.Caox,
                    ConstitutionalOptions_Others = constitutional?.Others,
                    ConstitutionalOptions_None = constitutional?.None,

                    IntegumentaryOptions_Notes = section.IntegumentaryNotes,
                    IntegumentaryOptions_Warm = integumentary?.Warm,
                    IntegumentaryOptions_Cold = integumentary?.Cold,
                    IntegumentaryOptions_AdequatePerfusion = integumentary?.AdequatePerfusion,
                    IntegumentaryOptions_InadequatePerfusion = integumentary?.InadequatePerfusion,
                    IntegumentaryOptions_AdequateSkinTurgor = integumentary?.AdequateSkinTurgor,
                    IntegumentaryOptions_InadequateSkinTurgor = integumentary?.InadequateSkinTurgor,
                    IntegumentaryOptions_Acne = integumentary?.Acne,
                    IntegumentaryOptions_Rash = integumentary?.Rash,
                    IntegumentaryOptions_SkinSpots = integumentary?.SkinSpots,
                    IntegumentaryOptions_Others = integumentary?.Others,
                    Integumentary_None = integumentary?.None,

                    RespiratoryOptions_Notes = section.RespiratoryNotes,
                    RespiratoryOptions_ClearToAuscultations = respiratory?.ClearToAuscultations,
                    RespiratoryOptions_Wheezes = respiratory?.Wheezes,
                    RespiratoryOptions_RonchiOrRales = respiratory?.RonchiOrRales,
                    RespiratoryOptions_AdequatePercussionSounds = respiratory?.AdequatePercussionSounds,
                    RespiratoryOptions_InadequatePercussionSounds = respiratory?.InadequatePercussionSounds,
                    RespiratoryOptions_PainUponPalpitation = respiratory?.PainUponPalpitation,
                    RespiratoryOptions_Others = respiratory?.Others,
                    RespiratoryOptions_None = respiratory?.None,

                    GastrointestinalOptions_Notes = section.GastrointestinalNotes,
                    GastrointestinalOptions_GoodDentation = gastrointestinal?.GoodDentation,
                    GastrointestinalOptions_PoorDentation = gastrointestinal?.PoorDentation,
                    GastrointestinalOptions_HardToPalpation = gastrointestinal?.HardToPalpation,
                    GastrointestinalOptions_SoftToPalpation = gastrointestinal?.SoftToPalpation,
                    GastrointestinalOptions_Tenderness = gastrointestinal?.Tenderness,
                    GastrointestinalOptions_Visceromegaly = gastrointestinal?.Visceromegaly,
                    GastrointestinalOptions_WNL = gastrointestinal?.Wnl,

                    GenitourinaryOptions_Notes = section.GenitourinaryNotes,
                    GenitourinaryOptions_DeferedGeneralAppereance = genitourinary?.DeferedGeneralAppereance,
                    GenitourinaryOptions_WhithinNormalLimits = genitourinary?.WhithinNormalLimits,

                    NeckNotes = section.NeckNotes,
                    NeckOptions_Masses = neck?.Masses,
                    NeckOptions_OverallAppearance = neck?.OverallAppearance,
                    NeckOptions_Symmetry = neck?.Symmetry,
                    NeckOptions_NormalTrachelPosition = neck?.NormalTrachealPosition,
                    NeckOptions_Tracheostomy = neck?.Tracheostomy,
                    NeckOptions_Crepitus = neck?.Crepitus,
                    NeckOptions_ThyroidEnlargement = neck?.ThyroidEnlargement,
                    NeckOptions_ThyroidTenderness = neck?.ThyroidTenderness,
                    NeckOptions_ThyroidMass = neck?.ThyroidMass,
                    NeckOptions_WNL = neck?.Wnl,
                    NeckOption_Rigity = neck?.Rigity,
                    NeckOption_MovementLimitation = neck?.MovementLimitation,
                    NeckOption_Crackle = neck?.Crackle,

                    ChestNotes = section.ChestNotes,
                    ChestOptions_IntercostalRetractions = chest?.IntercostalRetractions,
                    ChestOptions_UseOfAccesoryMuscles = chest?.UseOfAccesoryMuscles,
                    ChestOptions_DiaphragmaticMovement = chest?.DiaphragmaticMovement,
                    ChestOptions_Dullness = chest?.Dullness,
                    ChestOptions_Flatness = chest?.Flatness,
                    ChestOptions_Hyperresonance = chest?.Hyperresonance,
                    ChestOptions_TactileFremitus = chest?.TactileFremitus,
                    ChestOptions_NormalBreathSounds = chest?.NormalBreathSounds,
                    ChestOptions_AdventitiousSounds = chest?.AdventitiousSounds,
                    ChestOptions_Rubs = chest?.Rubs,
                    ChestOptions_Crackels = chest?.Crackels,
                    ChestOptions_WheezingsSymmetryBreasts = chest?.WheezingSymmetryBreasts,
                    ChestOptions_NippleDischargeBreastsMassesLumps = chest?.NippleDischargeBreastsMassesLumps,
                    ChestOptions_BreastsTenderness = chest?.BreastsTenderness,
                    ChestOptions_WNL = chest?.Wnl,
                    ChestOptions_BreastsMasses = chest?.BreastsMasses,
                    ChestOptions_NippleDischarge = chest?.NippleDischarge,
                    ChestOptions_RTFootToeAmputation = chest?.RtFootToeAmputation,
                    ChestOptions_LTFootToeAmputation = chest?.LtFootToeAmputation,

                    CardiovascularNotes = section.CardiovascularNotes,
                    CardiovascularOptions_AbnormalHeartSound = cardiovascular?.AbnormalHeartSound,
                    CardiovascularOptions_MurmursDecreasedPedalPulses = cardiovascular?.MurmursDecreasedPedalPulses,
                    CardiovascularOptions_LegEdema = cardiovascular?.LegEdema,
                    CardiovascularOptions_Varicosities = cardiovascular?.Varicosities,
                    CardiovascularOptions_AbnormalTemperature = cardiovascular?.AbnormalTemperature,
                    CardiovascularOptions_WNL = cardiovascular?.Wnl,
                    CardiovascularOptions_DecreasedPedalPulses = cardiovascular?.DecreasedPedalPulses,
                    CardiovascularOptions_RegularRateRhytm = cardiovascular?.RegularRateRhytm,
                    CardiovascularOptions_IrregularRateRhytm = cardiovascular?.IrregularRateRhytm,
                    CardiovascularOptions_Murmurs = cardiovascular?.Murmurs,
                    CardiovascularOptions_Gallops = cardiovascular?.Gallops,
                    CardiovascularOptions_Rubs = cardiovascular?.Rubs,
                    CardiovascularOptions_PainUponPrecordialPalpation = cardiovascular?.PainUponPrecordialPalpation,
                    CardiovascularOptions_Other = cardiovascular?.Other,
                    CardiovascularOptions_None = cardiovascular?.None,

                    AmputationLegRT_BKA = section.AmputationLegRtBka,
                    AmputationLegRT_AKA = section.AmputationLegRtAka,
                    AmputationLegRT_Toe = section.AmputationLegRtToe,
                    AmputationLegLT_BKA = section.AmputationLegLtBka,
                    AmputationLegLT_AKA = section.AmputationLegLtAka,
                    AmputationLegLT_Toe = section.AmputationLegLtToe,

                    AbdomenNotes = section.AbdomenNotes,
                    AbdomenOptions_Masses = abdomen?.Masses,
                    AbdomenOptions_Tenderness = abdomen?.Tenderness,
                    AbdomenOptions_Hernia = abdomen?.Hernia,
                    AbdomenOptions_LiverEnlargement = abdomen?.LiverEnlargement,
                    AbdomenOptions_SpleenEnlargement = abdomen?.SpleenEnlargement,
                    AbdomenOptions_Colostomy = abdomen?.Colostomy,
                    AbdomenOptions_Ileostomy = abdomen?.Ileostomy,
                    AbdomenOptions_Gastrostomy = abdomen?.Gastrostomy,
                    AbdomenOptions_WNL = abdomen?.Wnl,
                    AbdomenOptions_Cystostomy = abdomen?.Cystostomy,
                    AbdomenOptions_UmbilicalInfection = abdomen?.UmbilicalInfection,
                    AbdomenOptions_Distention = abdomen?.Distention,
                    AbdomenOptions_Constipation = abdomen?.Constipation,
                    AbdomenOptions_Colics = abdomen?.Colics,
                    AbdomenOptions_Reflux = abdomen?.Reflux,
                    AbdomenOptions_Rebound = abdomen?.Rebound,
                    AbdomenOptions_Guarding = abdomen?.Guarding,

                    GenitaliaNotes = section.GenitaliaGroinButtocksNotes,
                    GenitaliaGroinButtocksNotesOptions_DefferedGeneralAppearance = genitaliaGroinButtocks?.DefferedGeneralAppearance,
                    GenitaliaGroinButtocksNotesOptions_HairDistribution = genitaliaGroinButtocks?.HairDistribution,
                    GenitaliaGroinButtocksNotesOptions_Lesions = genitaliaGroinButtocks?.Lesions,
                    GenitaliaGroinButtocksNotesOptions_Cyst = genitaliaGroinButtocks?.Cyst,
                    GenitaliaGroinButtocksNotesOptions_Rashes = genitaliaGroinButtocks?.Rashes,
                    GenitaliaGroinButtocksNotesOptions_Size = genitaliaGroinButtocks?.Size,
                    GenitaliaGroinButtocksNotesOptions_Symmetry = genitaliaGroinButtocks?.Symmetry,
                    GenitaliaGroinButtocksNotesOptions_Masses = genitaliaGroinButtocks?.Masses,
                    GenitaliaGroinButtocksNotesOptions_Discharge = genitaliaGroinButtocks?.Discharge,
                    GenitaliaGroinButtocksNotesOptions_Scarring = genitaliaGroinButtocks?.Scarring,
                    GenitaliaGroinButtocksNotesOptions_Deformities = genitaliaGroinButtocks?.Deformities,
                    GenitaliaGroinButtocksNotesOptions_Nodularity = genitaliaGroinButtocks?.Nodularity,
                    GenitaliaGroinButtocksNotesOptions_Tenderness = genitaliaGroinButtocks?.Tenderness,
                    GenitaliaGroinButtocksNotesOptions_Enlargement = genitaliaGroinButtocks?.Enlargement,
                    GenitaliaGroinButtocksNotesOptions_Hemorrhoids = genitaliaGroinButtocks?.Hemorrhoids,
                    GenitaliaGroinButtocksNotesOptions_Prolapse = genitaliaGroinButtocks?.Prolapse,
                    GenitaliaGroinButtocksNotesOptions_EstrogenEffect = genitaliaGroinButtocks?.EstrogenEffect,
                    GenitaliaGroinButtocksNotesOptions_PelvicSupport = genitaliaGroinButtocks?.PelvicSupport,
                    GenitaliaGroinButtocksNotesOptions_Cystocele = genitaliaGroinButtocks?.Cystocele,
                    GenitaliaGroinButtocksNotesOptions_Rectocele = genitaliaGroinButtocks?.Rectocele,
                    GenitaliaGroinButtocksNotesOptions_WNL = genitaliaGroinButtocks?.Wnl,
                    GenitaliaGroinButtocksNotesOptions_Urostomy = genitaliaGroinButtocks?.Urostomy,
                    GenitaliaGroinButtocksNotesOptions_FoleyCatheterUse = genitaliaGroinButtocks?.FoleyCatheterUse,

                    MusculokeletalNotes = section.MusculoskeletalNotes,
                    MusculokeletalOptions_AbnormalGait = musculoskeletal?.AbnormalGait,
                    MusculokeletalOptions_ClubbingNails = musculoskeletal?.ClubbingNails,
                    MusculokeletalOptions_CyanosisDigits = musculoskeletal?.CyanosisDigits,
                    MusculokeletalOptions_UpperExtremitiesAsymmetry = musculoskeletal?.UpperExtremitiesAsymmetry,
                    MusculokeletalOptions_LowerExtremitiesAsymmetry = musculoskeletal?.LowerExtremitiesAsymmetry,
                    MusculokeletalOptions_Dislocation = musculoskeletal?.Dislocation,
                    MusculokeletalOptions_DislocationNotes = musculoskeletal?.DislocationNotes,
                    MusculokeletalOptions_AbnormalMuscleStrengthTone = musculoskeletal?.AbnormalMuscleStrengthTone,
                    MusculokeletalOptions_Flaccid = musculoskeletal?.Flaccid,
                    MusculokeletalOptions_CogWheel = musculoskeletal?.CogWheel,
                    MusculokeletalOptions_Spastic = musculoskeletal?.Spastic,
                    MusculokeletalOptions_AbnormalMovements = musculoskeletal?.AbnormalMovements,
                    MusculokeletalOptions_WNL = musculoskeletal?.Wnl,
                    MusculoskeletalOption_NoDeformitiesOrDeformations = musculoskeletal?.NoDeformitiesOrDeformations,
                    MusculoskeletalOption_NormalGait = musculoskeletal?.NormalGait,
                    MusculoskeletalOption_AdequateROM = musculoskeletal?.AdequateRom,
                    MusculoskeletalOption_InadequateROM = musculoskeletal?.InadequateRom,

                    SkinNotes = section.SkinNotes,
                    SkinOptions_Rashes = skin?.Rashes,
                    SkinOptions_Lesions = skin?.Lesions,
                    SkinOptions_Ulcers = skin?.Ulcers,
                    SkinOptions_Nodules = skin?.Nodules,
                    SkinOptions_Induration = skin?.Induration,
                    SkinOptions_Tightening = skin?.Tightening,
                    SkinOptions_WNL = skin?.Wnl,
                    SkinOptions_PurpuricLesionsNoted = skin?.PurpuricLesionsNoted,

                    PsychiatricNotes = section.PsychiatricNeurologicNotes,
                    PsychiatricNeurologicOptions_CranialNervesWithDeficits = psychiatricNeurologic?.CranialNervesWithDeficits,
                    PsychiatricNeurologicOptions_Babinsky = psychiatricNeurologic?.Babinsky,
                    PsychiatricNeurologicOptions_SentationByTouch = psychiatricNeurologic?.SensationByTouch,
                    PsychiatricNeurologicOptions_NoSensationTouchLegs = psychiatricNeurologic?.NoSensationTouchLegs,
                    PsychiatricNeurologicOptions_OrientedToTime = psychiatricNeurologic?.OrientedToTime,
                    PsychiatricNeurologicOptions_PlaceAndPerson = psychiatricNeurologic?.PlaceAndPerson,
                    PsychiatricNeurologicOptions_DepressedMode = psychiatricNeurologic?.DepressedMode,
                    PsychiatricNeurologicOptions_Anxiety = psychiatricNeurologic?.Anxiety,
                    PsychiatricNeurologicOptions_Agitation = psychiatricNeurologic?.Agitation,
                    PsychiatricNeurologicOptions_WNL = psychiatricNeurologic?.Wnl,
                    PsychiatricNeurologicOptions_Hemiplejia = psychiatricNeurologic?.Hemiplejia,
                    PsychiatricNeurologicOptions_Cuadriplejia = psychiatricNeurologic?.Cuadriplejia,
                    PsychiatricNeurologicOptions_Paraplejia = psychiatricNeurologic?.Paraplejia,
                    NeurologicOption_AmbulatingWOLimitation = psychiatricNeurologic?.AmbulatingWoLimitation,
                    NeurologicOption_NormalMuscleStrengthTone = psychiatricNeurologic?.NormalMuscleStrengthTone,
                    NeurologicOption_AbnormalMuscleStrengthTone = psychiatricNeurologic?.AbnormalMuscleStrengthTone,
                    NeurologicOption_FocalDeficits = psychiatricNeurologic?.FocalDeficits,

                    HemotalogicNotes = section.HematologicLymphaticImmunologicNotes,
                    HematologicLymphaticImmunologicOptions_LymphNodes = hematologicLymphaticImmunologic?.LymphNodes,
                    HematologicLymphaticImmunologicOptions_LymphNodesNotes = hematologicLymphaticImmunologic?.LymphNodesNotes,
                    HematologicLymphaticImmunologicOptions_WNL = hematologicLymphaticImmunologic?.Wnl,

                    section.HeadCircumference,
                    PercentilWT = section.PercentilWt,
                    PercentilHT = section.PercentilHt,
                    section.PercentilHead,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save physical examination for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveScreeningScheduleAsync(long claimId, ScreeningScheduleSection? section, CancellationToken cancellationToken)
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
                "uspSaveScreeningTest",
                new
                {
                    ClaimID = claimId,
                    BMDDate = ClampToSqlDateRange(section.BoneMineralDensityDate),
                    BoneMineralDensityResult_Normal = section.BoneMineralDensityResultNormal,
                    BoneMineralDensityResult_Osteopenia = section.BoneMineralDensityResultOsteopenia,
                    BoneMineralDensityResult_Osteoporosis = section.BoneMineralDensityResultOsteoporosis,
                    BoneMineralDensityResult_NA = section.BoneMineralDensityResultNa,
                    BoneMineralDensityResult_Other = section.BoneMineralDensityResultOther,
                    BMDResult = section.BoneMineralDensityResult,
                    BoneMineralDensity_NAFor = section.BoneMineralDensityNaFor,
                    BMDPrescribed = section.BoneMineralDensityPrescribed,
                    BMDRxOrdered = section.BoneMineralDensityRxOrdered,

                    CardiovascularLDLDate = ClampToSqlDateRange(section.CardiovascularLdlDate),
                    CardiovascularLDLResult = section.CardiovascularLdlResult,
                    Screening_Cardio_LDL_NAFor = section.CardiovascularLdlNaFor,
                    CardiovascularLDLPrescribed = section.CardiovascularLdlPrescribed,

                    CardiovascularBetaDate = ClampToSqlDateRange(section.CardiovascularBetaDate),
                    CardiovascularBetaResult = section.CardiovascularBetaResult,
                    Screening_Cardio_BetaBlocke_NAFor = section.CardiovascularBetaNaFor,
                    CardiovascularBetaPrescribed = section.CardiovascularBetaPrescribed,

                    ColorectalCancerSelectedIndex = section.ColorectalCancerScreeningSelectedIndex,
                    ColorectalCancerDoneDate = ClampToSqlDateRange(section.ColorectalCancerScreeningDate),
                    ColorectalCancerResult = section.ColorectalCancerScreeningResult,
                    Screening_ColorectalCancer_NAFor = section.ColorectalCancerScreeningNaFor,
                    ColorectalCancerPrescribed = section.ColorectalCancerScreeningPrescribed,

                    section.IsDiabetic,

                    DiabetesDilatedEyeDate = ClampToSqlDateRange(section.DiabetesScreeningDilatedEyeExamDate),
                    DiabetesDilatedEyeResult = section.DiabetesScreeningDilatedEyeExamResult,
                    Screening_Diabetes_DilatedEye_NAFor = section.DiabetesScreeningDilatedEyeExamNaFor,
                    DiabetesDilatedEyePrescribed = section.DiabetesScreeningDilatedEyeExamPrescribed,

                    DiabetesLDLDate = ClampToSqlDateRange(section.DiabetesScreeningLdlDate),
                    DiabetesLDLResult = section.DiabetesScreeningLdlResult,
                    Screening_Diabetes_LDL_NAFor = section.DiabetesScreeningLdlNaFor,
                    DiabetesLDLPrescribed = section.DiabetesScreeningLdlPrescribed,

                    DiabetesScreening_HGA1C_Date = ClampToSqlDateRange(section.DiabetesScreeningHga1CDate),
                    DiabetesScreening_HGA1C_Result = section.DiabetesScreeningHga1CResult,
                    DiabetesScreening_HGA1C_NAFor = section.DiabetesScreeningHga1CNaFor,
                    DiabetesScreening_HGA1C_Prescribed = section.DiabetesScreeningHga1CPrescribed,

                    DiabetesMicroalbuminDate = ClampToSqlDateRange(section.DiabetesScreeningMicroalbuminDate),
                    DiabetesMicroalbuminResult = section.DiabetesScreeningMicroalbuminResult,
                    Screening_Diabetes_Microalbumin_NAFor = section.DiabetesScreeningMicroalbuminNaFor,
                    DiabetesMicroalbuminPrescribed = section.DiabetesScreeningMicroalbuminPrescribed,

                    DiabetesGlaucomaTestDate = ClampToSqlDateRange(section.GlaucomaTestDate),
                    DiabetesGlaucomaResult = section.GlaucomaTestResult,
                    Screening_Diabetes_GlaucomaTest_NAFor = section.GlaucomaTestNaFor,
                    DiabetesGlaucomaPrescribed = section.GlaucomaTestPrescribed,

                    DiabetesMammogramProstateDate = ClampToSqlDateRange(section.MammogramProstateCancerDate),
                    DiabetesMammogramProstateResult = section.MammogramProstateCancerResult,
                    Screening_Diabetes_MammogramProstate_NAFor = section.MammogramProstateCancerNaFor,
                    DiabetesMammogramProstatePrescribe = section.MammogramProstateCancerPrescribed,

                    DiabetesFluShotDate = ClampToSqlDateRange(section.FluShotDate),
                    DiabetesFluShotComments = section.FluShotComments,
                    DiabetesFluShotPrescribed = section.FluShotPrescribed,
                    DiabetesFluShotPatientRefuses = section.FluShotPatientRefuses,

                    DiabetesPneumococcalShotDate = ClampToSqlDateRange(section.PneumococcalShotDate),
                    DiabetesPneumococcalComments = section.PneumococcalShotComments,
                    DiabetesPneumococcalPrescribed = section.PneumococcalShotPrescribed,
                    DiabetesPneumococcalPatientRefuses = section.PneumococcalShotPatientRefuses,

                    COVID19VaccineHouse = section.Covid19VaccineHouse,
                    COVID19VaccineShot = section.Covid19VaccineShot,
                    COVID19VaccineRefuse = section.Covid19VaccineRefuse,
                    COVID19VaccineOrdered = section.Covid19VaccineOrdered,
                    COVID19VaccineShotDate1 = ClampToSqlDateRange(section.Covid19VaccineShotDate1),
                    COVID19VaccineShotDate2 = ClampToSqlDateRange(section.Covid19VaccineShotDate2),
                    COVID19VaccineShotDate3 = ClampToSqlDateRange(section.Covid19VaccineShotDate3),

                    section.ColorectalColonoscopy,
                    ColorectalColonoscopyDate = ClampToSqlDateRange(section.ColorectalColonoscopyDate),
                    Colorectal_ColonoscopyResult_NA = section.ColorectalColonoscopyResultNa,
                    Colorectal_ColonoscopyResult_Negative = section.ColorectalColonoscopyResultNegative,
                    Colorectal_ColonoscopyResult_Diverticles = section.ColorectalColonoscopyResultDiverticles,
                    Colorectal_ColonoscopyResult_BleedingAreas = section.ColorectalColonoscopyResultBleedingAreas,
                    Colorectal_ColonoscopyResult_CAInColon = section.ColorectalColonoscopyResultCaInColon,
                    Colorectal_ColonoscopyResult_CAInRectum = section.ColorectalColonoscopyResultCaInRectum,
                    Colorectal_ColonoscopyResult_Colitis = section.ColorectalColonoscopyResultColitis,
                    Colorectal_ColonoscopyResult_UlcerativeOlitis = section.ColorectalColonoscopyResultUlcerativeColitis,
                    Colorectal_ColonoscopyResult_CrohnsDisease = section.ColorectalColonoscopyResultCrohnsDisease,
                    Colorectal_ColonoscopyResult_Polyps = section.ColorectalColonoscopyResultPolyps,
                    Colorectal_ColonoscopyResult_Other = section.ColorectalColonoscopyResultOther,
                    ColorectalColonoscopyNAFor = section.ColorectalColonoscopyNaFor,
                    ColorectalColonoscopyPrescribe = section.ColorectalColonoscopyPrescribe,

                    section.ColorectalOccultBlood,
                    ColorectalOccultBloodDate = ClampToSqlDateRange(section.ColorectalOccultBloodDate),
                    section.ColorectalOccultBloodResult,
                    ColorectalOccultBloodNAFor = section.ColorectalOccultBloodNaFor,
                    section.ColorectalOccultBloodPrescribe,

                    section.ColorectalFlexibleSigmoidoscopy,
                    ColorectalFlexibleSigmoidoscopyDate = ClampToSqlDateRange(section.ColorectalFlexibleSigmoidoscopyDate),
                    Colorectal_FlexibleSigmoidoscopyResult_NA = section.ColorectalFlexibleSigmoidoscopyResultNa,
                    Colorectal_FlexibleSigmoidoscopyResult_Negative = section.ColorectalFlexibleSigmoidoscopyResultNegative,
                    Colorectal_FlexibleSigmoidoscopyResult_AnalFissure = section.ColorectalFlexibleSigmoidoscopyResultAnalFissure,
                    Colorectal_FlexibleSigmoidoscopyResult_AnorectalAbscess = section.ColorectalFlexibleSigmoidoscopyResultAnorectalAbscess,
                    Colorectal_FlexibleSigmoidoscopyResult_IntestinalOcclusion = section.ColorectalFlexibleSigmoidoscopyResultIntestinalOcclusion,
                    Colorectal_FlexibleSigmoidoscopyResult_CAEnElSigmoideo = section.ColorectalFlexibleSigmoidoscopyResultCaEnElSigmoideo,
                    Colorectal_FlexibleSigmoidoscopyResult_CAInRectum = section.ColorectalFlexibleSigmoidoscopyResultCaInRectum,
                    Colorectal_FlexibleSigmoidoscopyResult_CAInTheRectosigmoidJunction = section.ColorectalFlexibleSigmoidoscopyResultCaInTheRectosigmoidJunction,
                    Colorectal_FlexibleSigmoidoscopyResult_ColorectalPolyps = section.ColorectalFlexibleSigmoidoscopyResultColorectalPolyps,
                    Colorectal_FlexibleSigmoidoscopyResult_Diverticles = section.ColorectalFlexibleSigmoidoscopyResultDiverticles,
                    Colorectal_FlexibleSigmoidoscopyResult_Hemorrhoids = section.ColorectalFlexibleSigmoidoscopyResultHemorrhoids,
                    Colorectal_FlexibleSigmoidoscopyResult_HirschsprungDisease = section.ColorectalFlexibleSigmoidoscopyResultHirschsprungDisease,
                    Colorectal_FlexibleSigmoidoscopyResult_InflammatoryBowelDisease = section.ColorectalFlexibleSigmoidoscopyResultInflammatoryBowelDisease,
                    Colorectal_FlexibleSigmoidoscopyResult_InflammationOrInfection = section.ColorectalFlexibleSigmoidoscopyResultInflammationOrInfection,
                    section.ColorectalFlexibleSigmoidoscopyResult,
                    ColorectalFlexibleSigmoidoscopyNAFor = section.ColorectalFlexibleSigmoidoscopyNaFor,
                    section.ColorectalFlexibleSigmoidoscopyPrescribe,

                    ColorectalFITDNA = section.ColorectalFitDna,
                    ColorectalFITDNADate = ClampToSqlDateRange(section.ColorectalFitDnaDate),
                    ColorectalFITDNAResult = section.ColorectalFitDnaResult,
                    ColorectalFITDNANAFor = section.ColorectalFitDnaNaFor,
                    ColorectalFITDNAPrescribe = section.ColorectalFitDnaPrescribe,

                    ColorectalColonographyCT = section.ColorectalColonographyCt,
                    ColorectalColonographyCTDate = ClampToSqlDateRange(section.ColorectalColonographyCtDate),
                    ColorectalColonographyCTResult = section.ColorectalColonographyCtResult,
                    ColorectalColonographyCTNAFor = section.ColorectalColonographyCtNaFor,
                    ColorectalColonographyCTPrescribe = section.ColorectalColonographyCtPrescribe,

                    MammogramCancerDate = ClampToSqlDateRange(section.MammogramCancerDate),
                    Mammogram_CancerResult_Category_0 = section.MammogramCancerResultCategory0,
                    Mammogram_CancerResult_Category_1 = section.MammogramCancerResultCategory1,
                    Mammogram_CancerResult_Category_2 = section.MammogramCancerResultCategory2,
                    Mammogram_CancerResult_Category_3 = section.MammogramCancerResultCategory3,
                    Mammogram_CancerResult_Category_4 = section.MammogramCancerResultCategory4,
                    Mammogram_CancerResult_Category_5 = section.MammogramCancerResultCategory5,
                    Mammogram_CancerResult_Category_6 = section.MammogramCancerResultCategory6,
                    Mammogram_CancerResult_NA = section.MammogramCancerResultNa,
                    section.MammogramCancerResult,
                    MammogramCancerNAFor = section.MammogramCancerNaFor,
                    section.MammogramCancerPrescribed,

                    PAPSMEAR_Date = ClampToSqlDateRange(section.PapSmearDate),
                    PAPSMEAR_Result = section.PapSmearResult,
                    PAPSMEAR_NAFor = section.PapSmearNaFor,
                    PAPSMEAR_Prescribed = section.PapSmearPrescribed,

                    ProstateCancerDate = ClampToSqlDateRange(section.ProstateCancerDate),
                    section.ProstateCancerResult,
                    ProstateCancerNAFor = section.ProstateCancerNaFor,
                    section.ProstateCancerPrescribed,

                    Screening_HPV_Date = ClampToSqlDateRange(section.ScreeningHpvDate),
                    section.ScreeningHpvComment,
                    section.ScreeningHpvResult,
                    section.ScreeningHpvOrdered,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save screening schedule for claim {ClaimId}", claimId);
            return false;
        }
    }

    // Legacy calls Globals.ValidateDateMinMaxRange (source not available) before sending most
    // ScreeningSchedule dates, clearing the value rather than sending it if the check fails. The
    // actual bounds aren't known, so this substitutes the same SQL Server `datetime` min/max used
    // for AdvanceDirective -- narrower down to whatever Globals actually enforces if that source
    // turns up.
    private static DateTime? ClampToSqlDateRange(DateTime? value) =>
        value is null || value < SqlDateTimeMin || value > SqlDateTimeMax ? null : value;

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

