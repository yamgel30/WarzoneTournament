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
    public async Task<bool> SavePage1Async(long claimId, SavePage1Request request, CancellationToken cancellationToken = default)
    {
        var chiefComplaintOk = await SaveChiefComplaintPatientMedicalHistoryAsync(
            claimId, request.ChiefComplaintPatientMedicalHistory, request.AtHome, request.AccompaniedBy, request.TypeOfVisit, cancellationToken);
        var medicalFamilySocialHistoryOk = await SaveMedicalFamilySocialHistoryAsync(claimId, request.MedicalFamilySocialHistory, cancellationToken);
        var advanceDirectiveOk = await SaveAdvanceDirectiveAsync(claimId, request.AdvanceDirective, cancellationToken);
        var reviewOfSystemOk = await SaveReviewOfSystemAsync(claimId, request.ReviewOfSystem, cancellationToken);
        var myocardialInfarctionOk = await SaveMyocardialInfarctionAsync(claimId, request.MyocardialInfarction, cancellationToken);

        var isAllergyEligible = request.IsGhp && request.MemberAge < 21;
        var medicationListOk = await SaveMedicationListAsync(claimId, request.MedicationList, isAllergyEligible, cancellationToken);
        var allergiesMedicationListOk = true;
        if (isAllergyEligible)
        {
            allergiesMedicationListOk = await SaveAllergiesMedicationListAsync(claimId, request.MedicationList, cancellationToken);
        }

        return chiefComplaintOk && medicalFamilySocialHistoryOk && advanceDirectiveOk && reviewOfSystemOk
            && myocardialInfarctionOk && medicationListOk && allergiesMedicationListOk;
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

    private static DataTable BuildMedicationListTable()
    {
        var table = new DataTable();
        table.Columns.Add("MedicationName", typeof(string));
        table.Columns.Add("isAdherence", typeof(bool));
        table.Columns.Add("isHistoric", typeof(bool));
        table.Columns.Add("isConfirmed", typeof(bool));
        return table;
    }

    // uspSaveMedicationList2020 folds CurrentMedication (isAdherence = false) together with either
    // AllergiesMedicationList or AdherenceMedicationList (isAdherence = true) into a single
    // MedicationList2020-typed table -- legacy uses AllergiesMedicationList here only when the
    // member is GHP and under 21, otherwise it uses AdherenceMedicationList.
    private async Task<bool> SaveMedicationListAsync(long claimId, MedicationListSection? section, bool useAllergiesAsAdherenceSource, CancellationToken cancellationToken)
    {
        if (section is null)
        {
            return true;
        }

        try
        {
            using var connection = connectionFactory.CreateConnection();

            var currentMedication = section.CurrentMedication ?? [];
            var adherenceSource = useAllergiesAsAdherenceSource
                ? section.AllergiesMedicationList ?? []
                : section.AdherenceMedicationList ?? [];

            var medListTable = BuildMedicationListTable();
            if (currentMedication.Count > 0 || adherenceSource.Count > 0)
            {
                foreach (var m in currentMedication)
                {
                    medListTable.Rows.Add(m.MedicationName.Trim(), false, m.IsHistoric, m.IsConfirmed);
                }

                foreach (var m in adherenceSource)
                {
                    medListTable.Rows.Add(m.MedicationName.Trim(), true, m.IsHistoric, m.IsConfirmed);
                }
            }

            var command = new CommandDefinition(
                "uspSaveMedicationList2020",
                new
                {
                    ClaimID = claimId,
                    PatientCurrentlyNoUse = section.CurrentlyDoesNotUse,
                    MedList = medListTable.AsTableValuedParameter("MedicationList2020"),
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save medication list for claim {ClaimId}", claimId);
            return false;
        }
    }

    // Only called when the member is GHP and under 21. Unlike SaveMedicationListAsync, every row
    // here is isAdherence = false -- legacy never marks allergy-list rows as adherence in this table.
    private async Task<bool> SaveAllergiesMedicationListAsync(long claimId, MedicationListSection? section, CancellationToken cancellationToken)
    {
        if (section is null)
        {
            return true;
        }

        try
        {
            using var connection = connectionFactory.CreateConnection();

            var medListTable = BuildMedicationListTable();
            foreach (var m in section.AllergiesMedicationList ?? [])
            {
                medListTable.Rows.Add(m.MedicationName.Trim(), false, m.IsHistoric, m.IsConfirmed);
            }

            var command = new CommandDefinition(
                "uspSaveAlergMedicationList2020",
                new
                {
                    ClaimID = claimId,
                    section.NotKnowAllergies,
                    MedList = medListTable.AsTableValuedParameter("MedicationList2020"),
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save allergies medication list for claim {ClaimId}", claimId);
            return false;
        }
    }

    // Legacy calls uspSaveScreeningTest for visits before 2023 and uspSaveScreeningTest2023 for
    // visits from 2023 onward. Only the pre-2023 stored procedure is ported so far.
    public async Task<bool> SavePage3Async(long claimId, SavePage3Request request, CancellationToken cancellationToken = default)
    {
        var screeningScheduleOk = request.DateOfVisit.Year >= 2023
            ? await SaveScreeningSchedule2023Async(claimId, request.ScreeningSchedule, request.ScreeningSchedule2023Extras, cancellationToken)
            : await SaveScreeningScheduleAsync(claimId, request.ScreeningSchedule, cancellationToken);
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
                    Screening_HPV_Comment = section.ScreeningHpvComment,
                    Screening_HPV_Result = section.ScreeningHpvResult,
                    Screening_HPV_Ordered = section.ScreeningHpvOrdered,
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

    // uspSaveScreeningTest2023 shares most fields with uspSaveScreeningTest (hence reusing
    // ScreeningScheduleSection here) but drops COVID-19 vaccine tracking and the microalbumin
    // block, replaces GlaucomaTestDate with nothing (result/NAFor/prescribed are still sent), and
    // adds the fields in ScreeningSchedule2023Extras.
    private async Task<bool> SaveScreeningSchedule2023Async(
        long claimId,
        ScreeningScheduleSection? section,
        ScreeningSchedule2023Extras? extras,
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
                "uspSaveScreeningTest2023",
                new
                {
                    ClaimID = claimId,
                    BMDDate = ClampToSqlDateRange(section.BoneMineralDensityDate),

                    extras?.Retinopathy,
                    extras?.Proliferative,
                    ProliferativeEyeRT = extras?.ProliferativeEyeRt,
                    ProliferativeEyeLT = extras?.ProliferativeEyeLt,

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

                    Screening_Diabetes_Urine_Albumin_Date = ClampToSqlDateRange(extras?.UrineAlbuminDate),
                    Screening_Diabetes_Urine_Albumin_Result = extras?.UrineAlbuminResult,
                    Screening_Diabetes_Urine_Albumin_Comment = extras?.UrineAlbuminNaFor,
                    Screening_Diabetes_Urine_Albumin_Prescribed = extras?.UrineAlbuminPrescribed,

                    Screening_Diabetes_Urine_Creatinine_Date = ClampToSqlDateRange(extras?.UrineCreatinineDate),
                    Screening_Diabetes_Urine_Creatinine_Result = extras?.UrineCreatinineResult,
                    Screening_Diabetes_Urine_Creatinine_Comment = extras?.UrineCreatinineNaFor,
                    Screening_Diabetes_Urine_Creatinine_Prescribed = extras?.UrineCreatininePrescribed,

                    Screening_Diabetes_Albumin_Creatinine_Ratio = extras?.CreatinineAlbuminRatio,

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

                    Screening_Vaccine_TdTdap_Comments = extras?.TdTdapComments,
                    Screening_Vaccine_TdTdap_Prescribed = extras?.TdTdapPrescribed,
                    Screening_Vaccine_TdTdap_PatientRefuses = extras?.TdTdapPatientRefuses,
                    Screening_Vaccine_TdTdap_Done_Date = ClampToSqlDateRange(extras?.TdTdapDoneDate),

                    Screening_Vaccine_ZosterVaccineOrdered = extras?.ZosterVaccineOrdered,
                    Screening_Vaccine_ZosterVaccineRefuse = extras?.ZosterVaccineRefuse,
                    // Legacy sends these two Zoster dates as-is, with no Globals.ValidateDateMinMaxRange check.
                    Screening_Vaccine_ZosterVaccineShotDate1 = extras?.ZosterVaccineShotDate1,
                    Screening_Vaccine_ZosterVaccineShotDate2 = extras?.ZosterVaccineShotDate2,

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
                    Screening_HPV_Comment = section.ScreeningHpvComment,
                    Screening_HPV_Result = section.ScreeningHpvResult,
                    Screening_HPV_Ordered = section.ScreeningHpvOrdered,

                    Retinopathy_Negative = extras?.RetinopathyNegative,
                    Retinopathy_Negative_Eye = extras?.RetinopathyNegativeEye,
                    Screening_Eye_Severity = extras?.EyeSeverity,
                    Screening_Eye_SeverityLevel = extras?.EyeSeverityLevel,
                    Screening_Proliferative_Eye = extras?.ProliferativeEye,
                    Screening_Retinopathy_Eye = extras?.RetinopathyEye,
                    Screening_MacularEdema = extras?.MacularEdema,
                    Screening_MacularEdema_Eye = extras?.MacularEdemaEye,
                    ScreeningRetinopathy_NA = extras?.ScreeningRetinopathyNa,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save 2023+ screening schedule for claim {ClaimId}", claimId);
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

    // Page 4 -- ~20 sections total; only the ones ported so far are wired in here (see README).
    public async Task<bool> SavePage4Async(long claimId, SavePage4Request request, CancellationToken cancellationToken = default)
    {
        var bmiOk = await SaveBmiAssociatedDiagnosesAsync(claimId, request.BmiAssociatedDiagnoses, cancellationToken);
        var rheumatoidArthritisOk = await SaveRheumatoidArthritisAsync(claimId, request.RheumatoidArthritis, cancellationToken);
        var assessmentPlanOk = await SaveAssessmentPlanOfTreatmentAsync(claimId, request.AssessmentPlanOfTreatment, cancellationToken);
        var cancerDiagnosesOk = await SaveCancerDiagnosesAsync(claimId, request.CancerDiagnoses, request.CancerDiagnosisNa, cancellationToken);
        var ckdOk = await SaveCkdAsync(claimId, request.Ckd, cancellationToken);
        var pressureSoresOk = await SavePressureSoresAsync(claimId, request.PressureSores, cancellationToken);
        var majorDepressionOk = await SaveMajorDepressionAsync(claimId, request.MajorDepression, cancellationToken);
        var congenitalDiseasesOk = await SaveCongenitalDiseasesAsync(claimId, request.CongenitalDiseases, cancellationToken);
        var pressureSoreListOk = await SavePressureSoreListAsync(claimId, request.PressureSoreList, cancellationToken);
        var cardiovascularDiseasesOk = await SaveCardiovascularDiseasesAsync(claimId, request.CardiovascularDiseases, cancellationToken);
        var diseasesOfTheSkinOk = await SaveDiseasesOfTheSkinAsync(claimId, request.DiseasesOfTheSkin, request.DiseasesOfTheSkinNa, cancellationToken);
        var eyesAndNeurologyOk = await SaveEyesAndNeurologyAsync(claimId, request.EyesAndNeurology, request.DateOfVisit, request.IsGhp, cancellationToken);
        var imLabRefOk = await SaveImLabRefAsync(claimId, request.ImLabRef, cancellationToken);
        var malnutritionCriteriaOk = await SaveMalnutritionCriteriaAsync(claimId, request.MalnutritionCriteria, cancellationToken);
        var screeningSubstanceUseOk = await SaveScreeningSubstanceUseAsync(claimId, request.ScreeningSubstanceUse, cancellationToken);

        var socialDeterminantsOk = request.DateOfVisit.Year < 2023
            ? await SaveSocialDeterminants2020Async(claimId, request.SocialDeterminants2020, cancellationToken)
            : await SaveSocialDeterminants2023Async(claimId, request.SocialDeterminants2023, cancellationToken);

        var screeningResultOk = await SaveScreeningResultAsync(
            claimId, request.ScreeningSubstanceUseResult, request.SocialDeterminantsResult, request.MalnutritionCriteriaResult, cancellationToken);

        var gastrointestinalDiseasesOk = await SaveGastrointestinalDiseasesAsync(claimId, request.GastrointestinalDiseases, cancellationToken);

        var otherConditionAdditionalOk = true;
        if (request.AtHome)
        {
            otherConditionAdditionalOk = await SaveOtherConditionAdditionalAsync(claimId, request.OtherConditionAdditionalRecommendation, cancellationToken);
        }

        var otherConditionsOk = await SaveOtherConditionsAsync(
            claimId, request.OtherConditions, request.OtherConditionDefaultRejectCode, request.OtherConditionRejectCodeDescription, cancellationToken);

        var ghpOnlyOk = true;
        if (request.IsGhp)
        {
            var gastrointestinalGhpOk = await SaveGastrointestinalGhpAsync(claimId, request.GastrointestinalDiseases, cancellationToken);
            var musculoskeletalGhpOk = await SaveMusculoskeletalGhpAsync(claimId, request.MusculoskeletalGhp, cancellationToken);
            ghpOnlyOk = gastrointestinalGhpOk && musculoskeletalGhpOk;
        }

        return bmiOk && rheumatoidArthritisOk && assessmentPlanOk && cancerDiagnosesOk && ckdOk && pressureSoresOk
            && majorDepressionOk && congenitalDiseasesOk && pressureSoreListOk && cardiovascularDiseasesOk && diseasesOfTheSkinOk
            && eyesAndNeurologyOk && imLabRefOk && malnutritionCriteriaOk && screeningSubstanceUseOk && socialDeterminantsOk
            && screeningResultOk && gastrointestinalDiseasesOk && otherConditionAdditionalOk && otherConditionsOk && ghpOnlyOk;
    }

    private async Task<bool> SaveImLabRefAsync(long claimId, ImLabRefSection? section, CancellationToken cancellationToken)
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
                "uspSaveImLabRef",
                new
                {
                    ClaimID = claimId,
                    Immunizations_NA = section.ImmunizationsNa,
                    Immunizations_ParentRefuses = section.ImmunizationsParentRefuses,
                    Immunizations_HepB1dose = section.ImmunizationsHepB1Dose,
                    Immunizations_HebB2dose = section.ImmunizationsHebB2Dose,
                    Immunizations_HebB3dose = section.ImmunizationsHebB3Dose,
                    Immunizations_HepA1dose = section.ImmunizationsHepA1Dose,
                    Immunizations_HebA2dose = section.ImmunizationsHebA2Dose,
                    Immunizations_DTaP1dose = section.ImmunizationsDTaP1Dose,
                    Immunizations_DTaP2dose = section.ImmunizationsDTaP2Dose,
                    Immunizations_DTaP3dose = section.ImmunizationsDTaP3Dose,
                    Immunizations_DTaP4dose = section.ImmunizationsDTaP4Dose,
                    Immunizations_DTaP5dose = section.ImmunizationsDTaP5Dose,
                    Immunizations_Hib1dose = section.ImmunizationsHib1Dose,
                    Immunizations_Hib2dose = section.ImmunizationsHib2Dose,
                    Immunizations_Hib3dose = section.ImmunizationsHib3Dose,
                    Immunizations_Hib4dose = section.ImmunizationsHib4Dose,
                    Immunizations_PCV13_1dose = section.ImmunizationsPcv13_1Dose,
                    Immunizations_PCV13_2dose = section.ImmunizationsPcv13_2Dose,
                    Immunizations_PCV13_3dose = section.ImmunizationsPcv13_3Dose,
                    Immunizations_PCV13_4dose = section.ImmunizationsPcv13_4Dose,
                    Immunizations_IPV1dose = section.ImmunizationsIpv1Dose,
                    Immunizations_IPV2dose = section.ImmunizationsIpv2Dose,
                    Immunizations_IPV3dose = section.ImmunizationsIpv3Dose,
                    Immunizations_IPV4dose = section.ImmunizationsIpv4Dose,
                    Immunizations_MMR1dose = section.ImmunizationsMmr1Dose,
                    Immunizations_MMR2dose = section.ImmunizationsMmr2Dose,
                    Immunizations_Varicella1dose = section.ImmunizationsVaricella1Dose,
                    Immunizations_Varicella2dose = section.ImmunizationsVaricella2Dose,
                    Immunizations_Tdap = section.ImmunizationsTdap,
                    Immunizations_Rotavirus1dose = section.ImmunizationsRotavirus1Dose,
                    Immunizations_Rotavirus2dose = section.ImmunizationsRotavirus2Dose,
                    Immunizations_Influenza = section.ImmunizationsInfluenza,
                    Immunizations_MenningococcalMCV = section.ImmunizationsMenningococcalMcv,
                    Immunizations_HPV1dose = section.ImmunizationsHpv1Dose,
                    Immunizations_HPV2dose = section.ImmunizationsHpv2Dose,
                    Immunizations_HPV3dose = section.ImmunizationsHpv3Dose,
                    Immunizations_Others = section.ImmunizationsOthers,
                    Lab_NA = section.LabNa,
                    Lab_HgbHct = section.LabHgbHct,
                    Lab_TB = section.LabTb,
                    Lab_UA = section.LabUa,
                    Lab_LipidProfile = section.LabLipidProfile,
                    Lab_BloodLeadTest = section.LabBloodLeadTest,
                    Lab_VIH = section.LabVih,
                    Lab_NAAT = section.LabNaat,
                    Lab_VDRL = section.LabVdrl,
                    Lab_Other = section.LabOther,
                    section.VisionText,
                    section.HearingText,
                    Referrals_NA = section.ReferralsNa,
                    Referrals_WIC = section.ReferralsWic,
                    Referrals_PhysicalTherapy = section.ReferralsPhysicalTherapy,
                    Referrals_OccupationTherapy = section.ReferralsOccupationTherapy,
                    Referrals_SpeechTherapy = section.ReferralsSpeechTherapy,
                    Referrals_Audiology = section.ReferralsAudiology,
                    Referrals_Dental = section.ReferralsDental,
                    Referrals_BehavioralHealth = section.ReferralsBehavioralHealth,
                    Referrals_EarlyIntervention = section.ReferralsEarlyIntervention,
                    Referrals_MentalHealthSpecialist = section.ReferralsMentalHealthSpecialist,
                    Referrals_Nutritionist = section.ReferralsNutritionist,
                    Referrals_Optometrist = section.ReferralsOptometrist,
                    Referrals_Ophthalmology = section.ReferralsOphthalmology,
                    Referrals_OtherSpecialtyText = section.ReferralsOtherSpecialtyText,
                    Immuno_Others_Checkbox = section.ImmunoOthersCheckbox,
                    Labs_Others_Checkbox = section.LabsOthersCheckbox,
                    Referals_Others_Checkbox = section.ReferalsOthersCheckbox,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save immunizations/labs/referrals for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveOtherConditionAdditionalAsync(long claimId, string? additionalRecommendation, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveOtherConditionAdditional",
                new { ClaimID = claimId, OtherCurrentConditionAdditional = additionalRecommendation },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save other condition additional recommendation for claim {ClaimId}", claimId);
            return false;
        }
    }

    // Legacy skips the SP call entirely when the list is null or empty. The stored procedure
    // itself hardcodes ICDCodeType to 10 for every row regardless of what's sent in that column
    // (it never reads dxTable.ICDCodeType), so AppShared.GetICDCodeType is not needed here.
    private async Task<bool> SaveOtherConditionsAsync(
        long claimId,
        IReadOnlyList<OtherConditionItem>? items,
        string? defaultRejectCode,
        string? rejectCodeDescription,
        CancellationToken cancellationToken)
    {
        if (items is null || items.Count == 0)
        {
            return true;
        }

        try
        {
            using var connection = connectionFactory.CreateConnection();

            var table = new DataTable();
            table.Columns.Add("ClaimID", typeof(long));
            table.Columns.Add("DxCode", typeof(string));
            table.Columns.Add("IsDummy", typeof(bool));
            table.Columns.Add("sDxText", typeof(string));
            table.Columns.Add("sDxReason", typeof(string));
            table.Columns.Add("Controlled", typeof(bool));
            table.Columns.Add("ICDCodeType", typeof(int));
            table.Columns.Add("RejectCode", typeof(string));
            table.Columns.Add("RejectedNotes", typeof(string));

            foreach (var item in items)
            {
                var dxCode = item.DiagnosesCode ?? string.Empty;
                table.Rows.Add(
                    claimId,
                    dxCode,
                    dxCode.Length == 0,
                    item.Diagnoses,
                    item.Treatment,
                    item.Controlled ?? false,
                    10,
                    defaultRejectCode,
                    rejectCodeDescription);
            }

            var command = new CommandDefinition(
                "uspClaimsDX_Save_New_V2024",
                new
                {
                    biClaimID = claimId,
                    dxTable = table.AsTableValuedParameter("AHADxOtherConditions"),
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save other conditions for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SavePulmonaryDiseasesAsync(long claimId, PulmonaryDiseasesSection? section, CancellationToken cancellationToken)
    {
        // Legacy always calls the SP, defaulting to an empty section when none is provided.
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSavePulmonaryDiseases",
                new
                {
                    ClaimID = claimId,
                    NA = section?.Na ?? false,
                    section?.Asthma,
                    section?.AsthmaComments,
                    section?.AsthmaDescription,
                    section?.AcuteBronchitis,
                    section?.AcuteBronchitisComments,
                    section?.ChronicBronchitis,
                    section?.ChronicBronchitisComments,
                    COPD = section?.Copd,
                    COPDComments = section?.CopdComments,
                    section?.PulmonaryFibrosis,
                    section?.PulmonaryFibrosisComments,
                    section?.AcuteLaryngopharyngitis,
                    section?.AcuteLaryngopharyngitisComments,
                    section?.AcuteNasopharyngitis,
                    section?.AcuteNasopharyngitisComments,
                    section?.UpperRespiratoryTractInfection,
                    section?.UpperRespiratoryTractInfectionComments,
                    PulmonaryDiseases_OtherCondition_Checkbox = section?.OtherConditionCheckbox,
                    PulmonaryDiseases_OtherCondition = section?.OtherCondition,
                    PulmonaryDiseases_OtherConditionTreatment = section?.OtherConditionTreatment,
                    section?.LungTransplant,
                    section?.LungTransplantTreatmentPlan,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save pulmonary diseases for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveGastrointestinalDiseasesAsync(long claimId, GastrointestinalDiseasesSection? section, CancellationToken cancellationToken)
    {
        // Legacy always calls the SP, defaulting to an empty section when none is provided.
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveGastrointestinal",
                new
                {
                    ClaimID = claimId,
                    NA = section?.Na ?? false,
                    NASH = section?.NonalcoholicSteatohepatitis,
                    section?.MetabolicSyndrome,
                    section?.Hyperkalemia,
                    section?.Hypokalemia,
                    TreatmentPlan = section?.GastrointestinalTreatmentPlan,
                    section?.LiverTransplant,
                    GERD = section?.Gerd,
                    section?.ChronicHepatitis,
                    section?.DiverticularDisease,
                    section?.PepticUlcerDisease,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save gastrointestinal diseases for claim {ClaimId}", claimId);
            return false;
        }
    }

    // Legacy calls this same uspSaveGastrointestinal procedure a second time for GHP members, with
    // only these 6 fields -- redundant with SaveGastrointestinalDiseasesAsync above, but that's
    // what the legacy page save does, so it's preserved as-is.
    private async Task<bool> SaveGastrointestinalGhpAsync(long claimId, GastrointestinalDiseasesSection? section, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveGastrointestinal",
                new
                {
                    ClaimID = claimId,
                    NA = section?.Na ?? false,
                    NASH = section?.NonalcoholicSteatohepatitis,
                    section?.MetabolicSyndrome,
                    section?.Hyperkalemia,
                    section?.Hypokalemia,
                    TreatmentPlan = section?.GastrointestinalTreatmentPlan,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save GHP gastrointestinal for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveMusculoskeletalGhpAsync(long claimId, MusculoskeletalGhpSection? section, CancellationToken cancellationToken)
    {
        // Legacy always calls the SP, defaulting to an empty section when none is provided.
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveMusculoskeletal",
                new
                {
                    ClaimID = claimId,
                    NA = section?.Na ?? false,
                    section?.Spondylosis,
                    section?.CervicalDiscDisorder,
                    section?.CervicothoracicRadiculopathy,
                    section?.CervicalRegion,
                    section?.CervicothoracicRegion,
                    TreatmentPlan = section?.MusculoskeletalTreatmentPlan,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save GHP musculoskeletal for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveSocialDeterminants2020Async(long claimId, SocialDeterminants2020Section? section, CancellationToken cancellationToken)
    {
        // Legacy always calls the SP, defaulting to an empty section when none is provided.
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveClaims_SocialDeterminants",
                new
                {
                    biClaimID = claimId,
                    problems_living_alone = section?.ProblemsLivingAlone ?? false,
                    illiteracy = section?.Illiteracy ?? false,
                    homelessness = section?.Homelessness ?? false,
                    inadequate_home = section?.InadequateHome ?? false,
                    discord_with_nll = section?.DiscordWithNll ?? false,
                    problems_residential_institution = section?.ProblemsResidentialInstitution ?? false,
                    lack_of_food_and_water = section?.LackOfFoodAndWater ?? false,
                    extreme_poverty = section?.ExtremePoverty ?? false,
                    worried_about_losing_housing = section?.WorriedAboutLosingHousing ?? false,
                    not_able_to_pay_rx = section?.NotAbleToPayRx ?? false,
                    not_able_to_pay_utilities = section?.NotAbleToPayUtilities ?? false,
                    not_able_to_pay_medical_care = section?.NotAbleToPayMedicalCare ?? false,
                    not_able_to_pay_phone = section?.NotAbleToPayPhone ?? false,
                    not_able_to_pay_transportation = section?.NotAbleToPayTransportation ?? false,
                    not_able_to_pay_clothing = section?.NotAbleToPayClothing ?? false,
                    problems_in_relationship = section?.ProblemsInRelationship ?? false,
                    absence_family_member_military = section?.AbsenceFamilyMemberMilitary ?? false,
                    disappearance_family_member = section?.DisappearanceFamilyMember ?? false,
                    other_absence_family_member = section?.OtherAbsenceFamilyMember ?? false,
                    disruption_separation = section?.DisruptionSeparation ?? false,
                    dependent_at_home = section?.DependentAtHome ?? false,
                    alcoholism_drug_addiction_family = section?.AlcoholismDrugAddictionFamily ?? false,
                    innapropriate_diet = section?.InnapropriateDiet ?? false,
                    other_reduced_mobility = section?.OtherReducedMobility ?? false,
                    need_personal_care = section?.NeedPersonalCare ?? false,
                    need_at_home = section?.NeedAtHome ?? false,
                    need_continuous_supervision = section?.NeedContinuousSupervision ?? false,
                    other_problems_provider_dependency = section?.OtherProblemsProviderDependency ?? false,
                    unavailability_other_helping_agencies = section?.UnavailabilityOtherHelpingAgencies ?? false,
                    need_assisstance_daily_activities = section?.NeedAssisstanceDailyActivities ?? false,
                    bedridden_few_to_no_resources = section?.BedriddenFewToNoResources ?? false,
                    partialy_depends_no_resource = section?.PartialyDependsNoResource ?? false,
                    socialdeterminants_na = section?.Na ?? false,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save 2020 social determinants for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveSocialDeterminants2023Async(long claimId, SocialDeterminants2023Section? section, CancellationToken cancellationToken)
    {
        // Legacy always calls the SP, defaulting to an empty section when none is provided.
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveClaims_SocialDeterminants_2023",
                new
                {
                    biClaimID = claimId,
                    section?.IsAutosufficientInRequestForTransport,
                    section?.HasSafeRoof,
                    section?.HasSufficientFundsForFood,
                    section?.FeelSafeInLivingPlace,
                    socialdeterminants_na = section?.Na ?? false,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save 2023 social determinants for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveMalnutritionCriteriaAsync(long claimId, MalnutritionCriteriaSection? section, CancellationToken cancellationToken)
    {
        // Legacy always calls the SP, defaulting to an empty section when none is provided.
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveClaims_MalnutritionCriteria",
                new
                {
                    biClaimID = claimId,
                    involuntary_weight_loss = section?.InvoluntaryWeightLoss ?? false,
                    involuntary_weight_loss10to5at6monthand20to10over6month = section?.InvoluntaryWeightLoss10To5At6MonthAnd20To10Over6Month ?? false,
                    involuntary_weight_lossless5at6monthandless10over6month = section?.InvoluntaryWeightLossLess5At6MonthAndLess10Over6Month ?? false,
                    involuntary_weight_lossmore10at6monthandmore20over6month = section?.InvoluntaryWeightLossMore10At6MonthAndMore20Over6Month ?? false,
                    Low_bmi = section?.LowBmi ?? false,
                    low_bmiless18 = section?.LowBmiLess18 ?? false,
                    low_bmiless20 = section?.LowBmiLess20 ?? false,
                    reduced_muscle = section?.ReducedMuscle ?? false,
                    reduced_muscle_severly = section?.ReducedMuscleSeverly ?? false,
                    reduced_muscle_mild = section?.ReducedMuscleMild ?? false,
                    reduced_food_intake = section?.ReducedFoodIntake ?? false,
                    disease_burden = section?.DiseaseBurden ?? false,
                    other_criteria = section?.OtherCriteria ?? false,
                    other_criteria_description = section?.OtherCriteriaDescription ?? string.Empty,
                    albumin = section?.Albumin ?? false,
                    less2albumin = section?.Less2Albumin ?? false,
                    less25albumin = section?.Less25Albumin ?? false,
                    less35albumin = section?.Less35Albumin ?? false,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save malnutrition criteria for claim {ClaimId}", claimId);
            return false;
        }
    }

    // Legacy always deletes existing rows for this claim before re-inserting one row per list item.
    private async Task<bool> SaveScreeningSubstanceUseAsync(long claimId, IReadOnlyList<ScreeningSubstanceUseItem>? items, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();

            var deleteCommand = new CommandDefinition(
                "uspDeleteClaims_ScreeningSubstanceUse",
                new { biClaimID = claimId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            await connection.ExecuteAsync(deleteCommand);

            if (items is not null)
            {
                foreach (var item in items)
                {
                    var insertCommand = new CommandDefinition(
                        "uspSaveClaims_ScreeningSubstanceUse",
                        new
                        {
                            biClaimID = claimId,
                            criteria_id = item.CriteriaId ?? false,
                            screening_substance_use_other = item.Other ?? string.Empty,
                            screening_substance_use_q1 = item.Q1 ?? false,
                            screening_substance_use_q2 = item.Q2 ?? false,
                            screening_substance_use_q3 = item.Q3 ?? false,
                            screening_substance_use_q4 = item.Q4 ?? false,
                            screening_substance_use_q5 = item.Q5 ?? false,
                            screening_substance_use_q6 = item.Q6 ?? false,
                            screening_substance_use_q7 = item.Q7 ?? false,
                            screening_substance_use_q8 = item.Q8 ?? false,
                            screening_substance_use_q9 = item.Q9 ?? false,
                            screening_substance_use_q10 = item.Q10 ?? false,
                            screening_substance_use_q11 = item.Q11 ?? false,
                            screening_substance_use_q12 = item.Q12 ?? false,
                            screening_substance_use_q13 = item.Q13 ?? false,
                            screening_substance_use_total = item.Total ?? false,
                        },
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken);
                    await connection.ExecuteAsync(insertCommand);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save screening substance use for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveScreeningResultAsync(
        long claimId, string? screeningSubstanceUseResult, string? socialDeterminantsResult, string? malnutritionCriteriaResult, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveScreeningResult",
                new
                {
                    ClaimID = claimId,
                    ScreeningSubstanceUseListResult = screeningSubstanceUseResult ?? string.Empty,
                    ScreeningMalnutritionCriteriaResult = malnutritionCriteriaResult ?? string.Empty,
                    ScreeningSocialDeterminants2020Result = socialDeterminantsResult ?? string.Empty,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save screening result for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveBmiAssociatedDiagnosesAsync(long claimId, BmiAssociatedDiagnosesSection? section, CancellationToken cancellationToken)
    {
        // Legacy always calls the SP, defaulting to an empty section (NA = false, everything else
        // omitted) when none is provided.
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveBMIAssociatedDiagnoses",
                new
                {
                    ClaimID = claimId,
                    NA = section?.Na ?? false,
                    section?.Obesity,
                    section?.MorbidObesity,
                    section?.Malnutrition,
                    BMIPlanTreatment = section?.EvaluationTreatmentPlan,
                    section?.MalnutritionGradeTypeText,
                    section?.DeficiencyBComplex,
                    section?.DeficiencyVitaminB12,
                    section?.DeficiencyVitaminB6,
                    section?.DeficiencyOtherVitaminNutrients,
                    section?.DeficiencyOtherVitaminNutrientsComments,
                    section?.MalnutritionScreeningAssesment,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save BMI associated diagnoses for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveRheumatoidArthritisAsync(long claimId, RheumatoidArthritisSection? section, CancellationToken cancellationToken)
    {
        // Legacy always calls the SP, defaulting to an empty section when none is provided.
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveRheumatoidArthritis",
                new
                {
                    ClaimID = claimId,
                    NA = section?.Na ?? false,
                    section?.RaNoManifestations,
                    section?.RaWithPolyneuropathy,
                    section?.RaWithMyopathy,
                    section?.RaOtherManifestations,
                    DMARDs = section?.Dmards,
                    DMARDsSpecify = section?.DmardsSpecify,
                    PtRefuses = section?.PtRefuses,
                    section?.OtherTreatmentConditions,
                    section?.Arthritis,
                    section?.ArthritisLocationType,
                    NSAIDS = section?.Nsaids,
                    NSAIDSOtherTreatment = section?.NsaidsOtherTreatment,
                    section?.AffectedJoints,
                    section?.InflammatoryPolyarthritis,
                    section?.InflammatoryPolyarthritisComments,
                    section?.ArthropathySequelaViralInfection,
                    section?.ArthropathySequelaViralInfectionComments,
                    section?.ArtritisPsoriatrica,
                    section?.Osteoartritis,
                    section?.ArtritisPsoriatricaComment,
                    section?.OsteoartritisComment,
                    RAOtherManifestationsCheckBox = section?.RaOtherManifestationsCheckBox,
                    section?.Osteoporosis,
                    section?.Osteopenia,
                    section?.OsteoTreatmentPlan,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save rheumatoid arthritis for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveAssessmentPlanOfTreatmentAsync(long claimId, AssessmentPlanOfTreatmentSection? section, CancellationToken cancellationToken)
    {
        // Legacy always calls the SP, defaulting to an empty section when none is provided.
        try
        {
            using var connection = connectionFactory.CreateConnection();

            // When "No" (no diabetes complications) is set, legacy blanks out the whole detail
            // block below regardless of what was passed in.
            var clearDetails = section?.No ?? false;

            var command = new CommandDefinition(
                "uspSaveAssessmentPlanOfTreatment",
                new
                {
                    ClaimID = claimId,
                    AssessmentPlanTreatment_No = section?.No,
                    AssessmentPlanTreatment_DMType = clearDetails ? null : section?.DmType,
                    AssessmentPlanTreatment_Controlled = clearDetails ? null : section?.Controlled,
                    AssessmentPlanTreatment_PoorlyController = clearDetails ? null : section?.PoorlyController,
                    AssessmentPlanTreatment_DMComments = clearDetails ? null : section?.DmComments,
                    AssessmentPlanTreatment_DiabeticNeuropathy = clearDetails ? null : section?.DiabeticNeuropathy,
                    AssessmentPlanTreatment_DiabeticPVD = clearDetails ? null : section?.DiabeticPvd,
                    AssessmentPlanTreatment_DiabeticNephropathy = clearDetails ? null : section?.DiabeticNephropathy,
                    AssessmentPlanTreatment_OtherComplication = clearDetails ? string.Empty : section?.OtherDiabeticComplication,
                    AssessmentPlanTreatment_DiabeticNeuropathyComments = clearDetails ? null : section?.DiabeticNeuropathyComments,
                    AssessmentPlanTreatment_DiabeticNephropathyComments = clearDetails ? null : section?.DiabeticNephropathyComments,
                    AssessmentPlanTreatment_DiabeticPVDComments = clearDetails ? null : section?.DiabeticPvdComments,
                    AssessmentPlanTreatment_OtherComplicationComments = clearDetails ? string.Empty : section?.OtherDiabeticComplicationComments,
                    AssessmentPlanTreatment_DiabeticCataracts = clearDetails ? null : section?.DiabeticCataracts,
                    AssessmentPlanTreatment_DiabeticCataractsComments = clearDetails ? null : section?.DiabeticCataractsComments,
                    AssessmentPlanTreatment_DMSecundary = section?.DmSecondary,
                    AssessmentPlanTreatment_Retinopathy = clearDetails ? null : section?.Retinopathy,
                    AssessmentPlanTreatment_RetinopathyComments = clearDetails ? null : section?.RetinopathyComments,
                    AssessmentPlanTreatment_Proliferative = clearDetails ? null : section?.Proliferative,
                    AssessmentPlanTreatment_ProliferativeComments = clearDetails ? null : section?.ProliferativeComments,
                    AssessmentPlanTreatment_Dermatitis = clearDetails ? null : section?.Dermatitis,
                    AssessmentPlanTreatment_DermatitisComments = clearDetails ? null : section?.DermatitisComments,
                    AssessmentPlanTreatment_Periodontal = clearDetails ? null : section?.Periodontal,
                    AssessmentPlanTreatment_PeriodontalComments = clearDetails ? null : section?.PeriodontalComments,
                    DMSecondaryText = section?.DmSecondaryText,
                    section?.OutOfControl,
                    section?.UncontrolledWithHyperglycemia,
                    section?.UncontrolledWithHypoglycemia,
                    HyperlipidemiaDueDM = section?.HyperlipidemiaDueDm,
                    section?.DmPlanAndTreatmentComments1,
                    section?.DmPlanAndTreatmentComments2,
                    section?.DmPlanAndTreatmentComments3,
                    DiabeticArthropathy = clearDetails ? null : section?.DiabeticArthropathy,
                    DiabeticArthropathyComment = clearDetails ? null : section?.DiabeticArthropathyComment,
                    section?.GestionalDiabetes,
                    section?.GestionalDiabetesComment,
                    AssessmentPlanTreatment_Remission = section?.Remission,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save assessment plan of treatment for claim {ClaimId}", claimId);
            return false;
        }
    }

    // Legacy always deletes existing rows in the shared Claims_DX table for this claim (and stamps
    // CancerDiagnosisNA) before re-inserting one dummy-coded row per diagnosis, via raw SQL text
    // rather than a stored procedure. Diagnoses from other sections (e.g. SaveOtherCondition, not
    // yet ported) write to the same table, so call order matters -- this matches legacy running
    // cancer diagnoses first.
    private async Task<bool> SaveCancerDiagnosesAsync(long claimId, IReadOnlyList<CancerDiagnosisItem>? diagnoses, bool na, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();

            var deleteCommand = new CommandDefinition(
                "DELETE FROM Claims_DX WHERE biClaimID=@ClaimID; UPDATE Claims_AHADetail SET CancerDiagnosisNA = @NA WHERE biClaimID = @ClaimID",
                new { ClaimID = claimId, NA = na },
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);
            await connection.ExecuteAsync(deleteCommand);

            if (diagnoses is not null)
            {
                var index = 0;
                foreach (var dx in diagnoses)
                {
                    index++;
                    var insertCommand = new CommandDefinition(
                        "uspClaimsDX_Save_New",
                        new
                        {
                            biClaimID = claimId,
                            nIndex = index,
                            sDx = "999.99",
                            bIsNew = false,
                            bIsDummy = true,
                            bIsRejected = false,
                            bIsDeleted = false,
                            sDxText = dx.Diagnoses,
                            sDxReason = dx.Treatment,
                            dx.Remission,
                            dx.Active,
                            Controlled = (bool?)null,
                            dx.History,
                            Primary = dx.Primary,
                            dx.Secondary,
                            dx.CurrentlyInChemotherapy,
                            dx.CurrentlyInRadiotherapy,
                            dx.CurrentlyInImmunotherapy,
                            dx.CurrentlyRefusesTreatment,
                        },
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken);
                    await connection.ExecuteAsync(insertCommand);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save cancer diagnoses for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveCkdAsync(long claimId, ChronicKidneyDiseaseSection? section, CancellationToken cancellationToken)
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
                "uspSaveCKD",
                new
                {
                    ClaimID = claimId,
                    CKD_NA = section.Na,
                    CKD_Stage = section.Stage,
                    CKD_DueToDM = section.DueToDm,
                    CKD_DueToOtherCondition = section.DueToOtherCondition,
                    CKD_Controlled = section.Controlled,
                    CKD_LowFatDiet = section.LowFatDiet,
                    CKD_Dialysis = section.Dialysis,
                    CKD_NoMeetDialysis = section.NoMeetDialysis,
                    CKD_AdditionalTreatment = section.AdditionalTreatment,
                    section.Hyperparathyroidism,
                    section.HyperparathyroidismTreatment,
                    GFR = section.Gfr,
                    section.SerumCalcium,
                    SerumPTH = section.SerumPth,
                    section.Nephropathy,
                    section.NephropathyType,
                    section.Nephritis,
                    section.NephritisType,
                    section.HasFistula,
                    CKDBox = section.CkdBox,
                    CKD_StressIncontinence = section.StressIncontinence,
                    CKD_UrgeIncontinence = section.UrgeIncontinence,
                    CKD_PostMicturitionDribble = section.PostMicturitionDribble,
                    CKD_OveractiveBladder = section.OveractiveBladder,
                    CKD_BladderTreatmentPlan = section.BladderTreatmentPlan,
                    CKD_KidneyTransplant = section.KidneyTransplant,
                    CKD_GFRDate = ClampToSqlDateRange(section.GfrDate),
                    CKD_GFROrdered = section.GfrOrdered,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save CKD for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SavePressureSoresAsync(long claimId, PressureSoresSection? section, CancellationToken cancellationToken)
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
                "uspSavePressureSores",
                new
                {
                    ClaimID = claimId,
                    PressureSores_NA = section.Na,
                    PressureSores_HighBackPressureUlcerStage = section.HighBackPressureUlcerStage,
                    PressureSores_LowBackPressureUlcerStage = section.LowBackPressureUlcerStage,
                    PressureSores_HipPressureUlcerStageLeft = section.HipPressureUlcerStageLeft,
                    PressureSores_HipPressureUlcerStageRight = section.HipPressureUlcerStageRight,
                    PressureSores_HipPressureUlcerLeft = section.HipPressureUlcerLeft,
                    PressureSores_HipPressureUlcerRight = section.HipPressureUlcerRight,
                    PressureSores_HeelPressureUlcerStageLeft = section.HeelPressureUlcerStageLeft,
                    PressureSores_HeelPressureUlcerStageRight = section.HeelPressureUlcerStageRight,
                    PressureSores_HeelPressureUlcerLeft = section.HeelPressureUlcerLeft,
                    PressureSores_HeelPressureUlcerRight = section.HeelPressureUlcerRight,
                    PressureSores_OtherAreasStage = section.OtherAreasStage,
                    PressureSores_OtherAreas = section.OtherAreas,
                    PressureSores_Healing = section.Healing,
                    PressureSores_Healed = section.Healed,
                    PressureSores_Worse = section.Worse,
                    PressureSores_Hydrocolloid = section.Hydrocolloid,
                    PressureSores_SilverDressing = section.SilverDressing,
                    PressureSores_Hydrogel = section.Hydrogel,
                    PressureSores_Antibiotic = section.Antibiotic,
                    PressureSores_Alginate = section.Alginate,
                    PressureSores_Enzyme = section.Enzyme,
                    PressureSores_TransparentDressing = section.TransparentDressing,
                    PressureSores_OthersTreatment = section.OthersTreatment,
                    PressureSores_ByPressure = section.ByPressure,
                    PressureSores_Chronicle = section.Chronicle,
                    PressureSores_AnatomicalSite = section.AnatomicalSite,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save pressure sores for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveMajorDepressionAsync(long claimId, MajorDepressionSection? section, CancellationToken cancellationToken)
    {
        // Legacy always calls the SP, defaulting to an empty section when none is provided.
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var phq9 = section?.Phq9;
            var command = new CommandDefinition(
                "uspSaveMayorDepression",
                new
                {
                    ClaimID = claimId,
                    NA = section?.Na ?? false,
                    section?.IsMajorDepression,
                    section?.InRemission,
                    section?.Recurrent,
                    section?.MildSeverity,
                    section?.ModerateSeverity,
                    section?.SevereSeverity,
                    MentalHealth_SeverWithoutPsychoticSymptoms = section?.SeverWithoutPsychoticSymptoms,
                    section?.TreatmentPlan,
                    section?.SingleEpisode,
                    section?.PsychoticSymptoms,
                    section?.BipolarDisorder,
                    section?.BipolarDisorderTypeAndSeverity,
                    section?.BipolarDisorderTreatmentPlan,
                    section?.SchizophreniaType,
                    section?.Schizophrenia,
                    section?.SchizophreniaTreatmentPlan,
                    section?.MoodDisorder,
                    section?.MoodDisorderComments,
                    PHQ9DoneDate = ClampToSqlDateRange(section?.Phq9DoneDate),
                    PHQ9ScoreResult = section?.Phq9ScoreResult,
                    section?.Dysthymia,
                    section?.DysthymiaComments,
                    UseOfSubtancesTreatment = section?.UseOfSubtancesTreatmentPlan,
                    PHQ9ReasonNotDoneID = section?.Phq9ReasonNotDoneId,
                    PHQ9ReasonNotDoneOther = section?.Phq9ReasonNotDoneOther,
                    MentalHealth_SubstanceAbuseFreeText = section?.SubstanceAbuseFreeText,
                    MentalHealth_ScreeningSubstanceUseDatePerformed = ClampToSqlDateRange(section?.ScreeningSubstanceUseDatePerformed),
                    MentalHealth_SubstanceAbuseCheckBox = section?.SubstanceAbuseCheckBox,
                    section?.GeneralizedAnxietyDisorder,
                    section?.OtherAnxiety,
                    section?.OtherAnxietyText,
                    section?.GeneralizedAnxietyDisorderComments,
                    PHQ9_Q1 = phq9?.Q1,
                    PHQ9_Q2 = phq9?.Q2,
                    PHQ9_Q3 = phq9?.Q3,
                    PHQ9_Q4 = phq9?.Q4,
                    PHQ9_Q5 = phq9?.Q5,
                    PHQ9_Q6 = phq9?.Q6,
                    PHQ9_Q7 = phq9?.Q7,
                    PHQ9_Q8 = phq9?.Q8,
                    PHQ9_Q9 = phq9?.Q9,
                    PHQ9_TotalCol1 = phq9?.TotalCol1,
                    PHQ9_TotalCol2 = phq9?.TotalCol2,
                    PHQ9_TotalCol3 = phq9?.TotalCol3,
                    PHQ9_NotDifficultAtAll = phq9?.NotDifficultAtAll,
                    PHQ9_SomewhatDifficult = phq9?.SomewhatDifficult,
                    PHQ9_VeryDifficult = phq9?.VeryDifficult,
                    PHQ9_ExtremeDifficult = phq9?.ExtremelyDifficult,
                    PHQ9_DepressedPastYear = phq9?.DepressedPastYear,
                    PHQ9_SuicidePastMonth = phq9?.SuicidePastMonth,
                    PHQ9_TriedSuicide = phq9?.TriedSuicide,
                    ADHD = section?.Adhd,
                    ADHDComments = section?.AdhdComments,
                    section?.Autism,
                    section?.AutismComments,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save major depression for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveCongenitalDiseasesAsync(long claimId, CongenitalDiseasesSection? section, CancellationToken cancellationToken)
    {
        // Legacy always calls the SP, defaulting to an empty section when none is provided.
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveCongenitalDiseases",
                new
                {
                    ClaimID = claimId,
                    CongenitalDiseases_NA = section?.Na ?? false,
                    CongenitalDiseases_SpinaBifida = section?.SpinaBifida,
                    CongenitalDiseases_SpinaBifidaComments = section?.SpinaBifidaComments,
                    CongenitalDiseases_Hydrocephalus = section?.Hydrocephalus,
                    CongenitalDiseases_HydrocephalusComments = section?.HydrocephalusComments,
                    CongenitalDiseases_ChiariMalformation = section?.ChiariMalformation,
                    CongenitalDiseases_ChiariMalformationComments = section?.ChiariMalformationComments,
                    CongenitalDiseases_Hemophilia = section?.Hemophilia,
                    CongenitalDiseases_HemophiliaComments = section?.HemophiliaComments,
                    CongenitalDiseases_Cranofacial = section?.Cranofacial,
                    CongenitalDiseases_CranofacialComments = section?.CranofacialComments,
                    CongenitalDiseases_DistrofiaMuscular = section?.DistrofiaMuscular,
                    CongenitalDiseases_DistrofiaMuscularComments = section?.DistrofiaMuscularComments,
                    CongenitalDiseases_CerebralPalsy = section?.CerebralPalsy,
                    CongenitalDiseases_CerebralPalsyText = section?.CerebralPalsyText,
                    CongenitalDiseases_CerebralPalsyComments = section?.CerebralPalsyComments,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save congenital diseases for claim {ClaimId}", claimId);
            return false;
        }
    }

    // Legacy always deletes existing rows in Claims_PressureSores for this claim (raw SQL text)
    // before re-inserting one row per list item via a stored procedure.
    private async Task<bool> SavePressureSoreListAsync(long claimId, IReadOnlyList<PressureSoreListItem>? items, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();

            var deleteCommand = new CommandDefinition(
                "DELETE FROM Claims_PressureSores WHERE biClaimID = @ClaimID",
                new { ClaimID = claimId },
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);
            await connection.ExecuteAsync(deleteCommand);

            if (items is not null)
            {
                var index = 0;
                foreach (var item in items)
                {
                    index++;
                    var insertCommand = new CommandDefinition(
                        "uspClaims_PressureSores_Save",
                        new
                        {
                            biClaimID = claimId,
                            nIndex = index,
                            ByVaricoseVainsInLegs = item.ByVaricoseVainsInLegs ?? false,
                            ByArteriosclerosisInExtremities = item.ByArteriosclerosisInExtremities ?? false,
                            ByDiabetic = item.ByDiabetic ?? false,
                            ByPressure = item.ByPressure ?? false,
                            ByPressureStage = item.ByPressureStage ?? -1,
                            AnatomicalSite = item.AnatomicalSite ?? string.Empty,
                            AnatomicalSiteOther = item.AnatomicalSiteOther ?? string.Empty,
                            ByOtherCondition = item.ByOtherCondition ?? false,
                            OtherConditionText = item.OtherConditionText ?? string.Empty,
                            Treatment = item.Treatment ?? string.Empty,
                        },
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken);
                    await connection.ExecuteAsync(insertCommand);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save pressure sore list for claim {ClaimId}", claimId);
            return false;
        }
    }

    private async Task<bool> SaveCardiovascularDiseasesAsync(long claimId, CardiovascularDiseasesSection? section, CancellationToken cancellationToken)
    {
        // Legacy always calls the SP, defaulting to an empty section when none is provided.
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspSaveCardiovascularDiseases",
                new
                {
                    ClaimID = claimId,
                    CardiovascularDiseasesNA = section?.Na ?? false,
                    section?.ArterialHypertension,
                    section?.PulmonaryHypertension,
                    section?.PulmonaryHypertensionType,
                    section?.HeartFailure,
                    section?.Congestive,
                    section?.Diastolic,
                    section?.Systolic,
                    section?.Chronic,
                    PVD = section?.Pvd,
                    section?.AtrilaFibrillation,
                    section?.AtrilFibrillationType,
                    section?.Arteriosclerosis,
                    section?.Aorta,
                    section?.Crowns,
                    section?.RenalArtery,
                    section?.ArteriosclerosisExtremities,
                    LegLT = section?.LegLt,
                    LegRT = section?.LegRt,
                    ArmLT = section?.ArmLt,
                    ArmRT = section?.ArmRt,
                    section?.IntermittentClaudication,
                    section?.RestPain,
                    section?.OtherComplications,
                    section?.OtherComplicationsText,
                    section?.HypertensionTreatmentPlan,
                    PVDTreatmentPlan = section?.PvdTreatmentPlan,
                    section?.ArteriosclerosisTreatmentPlan,
                    section?.AnginaPectoris,
                    SSS = section?.Sss,
                    SVT = section?.Svt,
                    section?.Pacemaker,
                    CAD = section?.Cad,
                    section?.Cardiomiopatia,
                    section?.MyocardialInfarction,
                    section?.Cardiomegaly,
                    section?.AtrioventricularBlock,
                    section?.AtrioventricularBlockDegree,
                    section?.VaricoseVeinsOfLowerExtremityWithPain,
                    section?.ConductionDisorder,
                    section?.MyocardialInfarctionTreatmentPlan,
                    section?.OldMyocardialInfarction,
                    section?.Hyperlipidemia,
                    section?.HyperlipidemiaText,
                    section?.HeartTransplant,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save cardiovascular diseases for claim {ClaimId}", claimId);
            return false;
        }
    }

    // Legacy always calls this SP. When the item list is null, it still sends a single default/empty
    // row (IndexRow 0); when the list is non-null but empty, it sends zero rows. Either way ClaimID
    // and NA are always sent.
    private async Task<bool> SaveDiseasesOfTheSkinAsync(long claimId, IReadOnlyList<DiseasesOfTheSkinItem>? items, bool na, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();

            var table = new DataTable();
            table.Columns.Add("biClaimID", typeof(long));
            table.Columns.Add("IndexRow", typeof(short));
            table.Columns.Add("Dermatitis", typeof(bool));
            table.Columns.Add("DermatitisTreatment", typeof(string));
            table.Columns.Add("DermatitisTypeLocation", typeof(string));
            table.Columns.Add("Psoriasis", typeof(bool));
            table.Columns.Add("PsoriasisType", typeof(string));
            table.Columns.Add("PsoriasicArthritis", typeof(bool));
            table.Columns.Add("PsoriasicArthritisLocation", typeof(string));
            table.Columns.Add("PsoriasisTreatment", typeof(string));
            table.Columns.Add("Ulcer", typeof(bool));
            table.Columns.Add("UlcerLocationAndDepth", typeof(string));
            table.Columns.Add("DueToArteriosclerosisInExtremities", typeof(bool));
            table.Columns.Add("DueToPVD", typeof(bool));
            table.Columns.Add("UlcerTreatment", typeof(string));
            table.Columns.Add("PressureUlcer", typeof(bool));
            table.Columns.Add("PressureUlcerStage", typeof(int));
            table.Columns.Add("PressureUlcerNoStage", typeof(bool));
            table.Columns.Add("PressureUlcerLocation", typeof(string));
            table.Columns.Add("PressureUlcerTreatment", typeof(string));
            table.Columns.Add("PressureUlcerOtherCause", typeof(bool));
            table.Columns.Add("PressureUlcerOtherCauseText", typeof(string));
            table.Columns.Add("PressureUlcerOtherCauseTreatment", typeof(string));
            table.Columns.Add("UlcerDepth", typeof(string));
            table.Columns.Add("UlcerLocation", typeof(string));
            table.Columns.Add("UlcerDueToDiabetes", typeof(bool));
            table.Columns.Add("UlcerDueToVaricoseVeins", typeof(bool));
            table.Columns.Add("UlcerDueToVaricoseVeinsWithInflamation", typeof(bool));
            table.Columns.Add("UlcerDueToIdiopathicVenousHypertension", typeof(bool));
            table.Columns.Add("UlcerDueToIdiopathicVenousHypertensionWithInflamation", typeof(bool));
            table.Columns.Add("UlcerDueToOtherCause", typeof(bool));
            table.Columns.Add("UlcerDueToOtherCauseText", typeof(string));

            void AddRow(DiseasesOfTheSkinItem? item, int indexRow)
            {
                table.Rows.Add(
                    claimId,
                    (short)indexRow,
                    item?.Dermatitis,
                    item?.DermatitisTreatment,
                    item?.DermatitisTypeLocation,
                    item?.Psoriasis,
                    item?.PsoriasisType,
                    item?.PsoriasicArthritis,
                    item?.PsoriasicArthritisLocation,
                    item?.PsoriasisTreatment,
                    item?.Ulcer,
                    item?.UlcerLocationAndDepth,
                    item?.DueToArteriosclerosisInExtremities,
                    item?.DueToPvd,
                    item?.UlcerTreatment,
                    item?.PressureUlcer,
                    item?.PressureUlcerStage,
                    item?.PressureUlcerNoStage,
                    item?.PressureUlcerLocation,
                    item?.PressureUlcerTreatment,
                    item?.PressureUlcerOtherCause,
                    item?.PressureUlcerOtherCauseText,
                    item?.PressureUlcerOtherCauseTreatment,
                    item?.UlcerDepth,
                    item?.UlcerLocation,
                    item?.UlcerDueToDiabetes,
                    item?.UlcerDueToVaricoseVeins,
                    item?.UlcerDueToVaricoseVeinsWithInflamation,
                    item?.UlcerDueToIdiopathicVenousHypertension,
                    item?.UlcerDueToIdiopathicVenousHypertensionWithInflamation,
                    item?.UlcerDueToOtherCause,
                    item?.UlcerDueToOtherCauseText);
            }

            if (items is not null)
            {
                var indexRow = 0;
                foreach (var item in items)
                {
                    indexRow++;
                    AddRow(item, indexRow);
                }
            }
            else
            {
                AddRow(null, 0);
            }

            var command = new CommandDefinition(
                "uspSaveDiseasesOfTheSkin2022",
                new
                {
                    ClaimID = claimId,
                    DiseasesSkinTable = table.AsTableValuedParameter("DiseasesOfTheSkin2022"),
                    NA = na,
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save diseases of the skin for claim {ClaimId}", claimId);
            return false;
        }
    }

    // Legacy branches on the visit year and, for 2023+, on whether the member is GHP: pre-2023
    // always saves via uspSaveEyeAndNeurology; 2023+ saves via uspSaveEyeAndNeurology2023 but only
    // when the member is NOT GHP -- a GHP member seen in 2023+ gets no Eye and Neurology save at
    // all.
    private async Task<bool> SaveEyesAndNeurologyAsync(long claimId, EyesAndNeurologySection? section, DateTime dateOfVisit, bool isGhp, CancellationToken cancellationToken)
    {
        if (dateOfVisit.Year >= 2023 && isGhp)
        {
            return true;
        }

        var procedureName = dateOfVisit.Year >= 2023 ? "uspSaveEyeAndNeurology2023" : "uspSaveEyeAndNeurology";

        try
        {
            using var connection = connectionFactory.CreateConnection();

            // DynamicParameters here (rather than an anonymous object) because DementiaSeverity
            // must be omitted entirely for the pre-2023 procedure -- it doesn't declare that
            // parameter, so sending it (even as NULL) would fail.
            var parameters = new DynamicParameters(new
            {
                ClaimID = claimId,
                EyesAndNeurologyNA = section?.Na ?? false,
                section?.Retinopathy,
                ProliferativeEyeRT = section?.ProliferativeEyeRt,
                ProliferativeEyeLT = section?.ProliferativeEyeLt,
                section?.Proliferative,
                section?.MacularEdema,
                MacularEdemaEyeRT = section?.MacularEdemaEyeRt,
                MacularEdemaEyeLT = section?.MacularEdemaEyeLt,
                section?.OtherComplicationRetinopathy,
                section?.Glaucoma,
                GlaucomaEyeRT = section?.GlaucomaEyeRt,
                GlaucomaEyeLT = section?.GlaucomaEyeLt,
                section?.GlaucomaType,
                section?.Cataract,
                CataractRT = section?.CataractRt,
                CataractLT = section?.CataractLt,
                section?.CataractType,
                section?.Epilepsy,
                section?.EpilepsyType,
                section?.Seizures,
                section?.SeizuresCause,
                section?.Polyneuropathy,
                section?.PolyneuropathyDueTo,
                section?.Neuropathy,
                section?.AutonomicNeuropathy,
                section?.Mononeuritis,
                section?.Neuralgia,
                section?.PolyneuropathyOtherSpecification,
                section?.RetinopathyTreatmentPlan,
                section?.GlaucomaTreatmentPlan,
                section?.CataractTreatmentPlan,
                section?.EpilepsyTreatmentPlan,
                section?.PolyneuropathyTreatmentPlan,
                section?.PolyneuropathyDueToCkb,
                RetinopathyEyeRT = section?.RetinopathyEyeRt,
                RetinopathyEyeLT = section?.RetinopathyEyeLt,
                section?.AlzheimerDisease,
                section?.Dementia,
                section?.RetinopathySeverity,
                section?.ProliferativeSeverity,
                section?.ProliferativeTreatmentPlan,
                section?.DementiaAlzheimerTreatmentPlan,
            });

            if (dateOfVisit.Year >= 2023)
            {
                parameters.Add("DementiaSeverity", section?.DementiaSeverity);
            }

            var command = new CommandDefinition(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save eyes and neurology for claim {ClaimId}", claimId);
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

