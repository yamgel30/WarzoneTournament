using System.Data;
using Chopper.Services.Abstractions;
using Chopper.Services.Common;
using Dapper;

namespace Chopper.Services.AhaClaims;

// Legacy's GetAHA logs and re-throws on failure rather than swallowing it (unlike SaveClaim's
// section savers) -- exceptions here are left to propagate to ASP.NET Core's normal error
// handling too, matching every other read-side service ported so far (ClaimSearchService,
// ProviderService, ClaimConditionService, FormReferenceService). A null return means the claim
// genuinely wasn't found, not that something went wrong.
internal sealed class AhaClaimReadService(ISqlConnectionFactory connectionFactory) : IAhaClaimReadService
{
    public async Task<AhaFormHeader?> GetFormHeaderAsync(long claimId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var tables = await LoadAhaDataSetAsync(connection, claimId, cancellationToken);

        if (tables.Count == 0 || tables[0].Count == 0)
        {
            return null;
        }

        return MapHeader(tables, claimId);
    }

    public async Task<AhaClaimSnapshot?> GetClaimSnapshotAsync(long claimId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var tables = await LoadAhaDataSetAsync(connection, claimId, cancellationToken);

        if (tables.Count == 0 || tables[0].Count == 0)
        {
            return null;
        }

        var row = tables[0][0];

        return new AhaClaimSnapshot
        {
            Header = MapHeader(tables, claimId),
            ChiefComplaintPatientMedicalHistory = MapChiefComplaint(row),
            MedicalFamilySocialHistory = MapMedicalFamilySocialHistory(row),
            AdvanceDirective = MapAdvanceDirective(row),
            ReviewOfSystem = MapReviewOfSystem(row),
            MedicationList = MapMedicationList(row, tables.Count > 5 ? tables[5] : []),
            MedicationReview = MapMedicationReview(row),
            CognitiveAssessment = MapCognitiveAssessment(row),
            PainScreening = MapPainScreening(row),
            ActivitiesOfDailyLiving = MapActivitiesOfDailyLiving(row),
            ScreeningSchedule = MapScreeningSchedule(row),
            ScreeningSchedule2023Extras = MapScreeningSchedule2023Extras(row),
        };
    }

    private static AhaFormHeader MapHeader(IReadOnlyList<IReadOnlyList<dynamic>> tables, long claimId)
    {
        var row = tables[0][0];

        // Table 2, row 0, column sPayerID -- a different result set than the rest of the
        // header, exactly as legacy reads it.
        string? payerId = tables.Count > 2 && tables[2].Count > 0 ? (string?)tables[2][0].sPayerID : null;

        // MemberDob is read twice in legacy: once from birthDate, then unconditionally
        // overwritten by PatientBirth when that column is present. Both draw from the same
        // row here, so PatientBirth simply wins when set.
        DateTime? memberDob = GetDateOrNull(row, "birthDate");
        var patientBirth = GetDateOrNull(row, "PatientBirth");
        if (patientBirth.HasValue)
        {
            memberDob = patientBirth;
        }

        return new AhaFormHeader
        {
            Id = claimId,
            SubmittedDate = GetDateOrNull(row, "SubmittedDate") ?? DateTime.Now,
            Language = GetString(row, "AHALanguage"),
            BillingNpi = GetString(row, "BillingNPI"),
            DateOfVisit = GetDateOrNull(row, "ServiceDate"),
            HealthPlan = GetString(row, "PayerName"),
            MemberDob = memberDob,
            MemberGender = GetString(row, "genderID"),
            MemberId = GetString(row, "MemberID"),
            MemberName = GetString(row, "MemberName"),
            PayerId = payerId,
            ProviderName = GetString(row, "ProvName"),
            RenderingNpi = GetString(row, "RenderingNPI"),
            AtHome = GetBoolOrNull(row, "AtHome"),
            PlaceOfService = GetIntOrNull(row, "nPOS"),
            Status = GetIntOrNull(row, "iStatus"),
            ApprovedOrRejectedDate = GetDateOrNull(row, "dApproved"),
            MemberFirstName = GetString(row, "MemberFName"),
            MemberMiddleName = GetString(row, "MemberMName"),
            MemberLastName = GetString(row, "MemberLName"),
            RenderingName = GetString(row, "RenderingName"),
            BillingName = GetString(row, "BillingName"),
            IpaName = GetString(row, "IPAName"),
            AccompaniedBy = GetString(row, "AccompaniedBy"),
            TypeOfVisit = GetIntOrNull(row, "TypeOfVisit"),
            ConcurrencyId = GetLongOrNull(row, "ConcurrencyID") ?? 0,
            ClaimClassTag = GetIntOrNull(row, "nClaimClassTag"),
            MemberLanguage = GetString(row, "Member_Language"),
            MemberLanguageOther = GetString(row, "Member_Language_other"),
            Race = GetString(row, "Member_Race"),
            Ethnicity = GetString(row, "Member_Ethnicity"),
            Phone = GetString(row, "Member_Phone"),
            Email = GetString(row, "Member_EMail"),
            AdditionalHealthPlan = GetString(row, "Member_Additional_Health_Plan"),
            AdditionalHealthPlanOther = GetString(row, "Member_Additional_Health_Plan_Other"),
            SexualOrientation = GetString(row, "Member_Sexual_Orientation"),
            SexAtBirth = GetString(row, "Member_Sex_Birth"),
            Pronoun = GetString(row, "Member_Pronoun"),
            GenderIdentity = GetString(row, "Member_Gender_Identity"),
            SexualOrientationSomethingElse = GetString(row, "Member_Sexual_OrientationSomethingelse"),
            PronounOtherPronoun = GetString(row, "Member_PronounOther_Pronoun"),
            OtherRace = GetString(row, "Member_Other_Race"),
            GenderIdentityAdditionalGender = GetString(row, "Member_Gender_Identity_AdditionalGender"),
        };
    }

    private static ChiefComplaintPatientMedicalHistorySection MapChiefComplaint(IDictionary<string, object> row) => new()
    {
        HistoryOfPresentIllness = GetString(row, "p1_sHistoryPresentIllness"),
        RecentHospitalization = GetBoolOrNull(row, "p1_bHasRecentHospitalization"),
        RecentHospitalizationDate = GetDateOrNull(row, "p1_dHospitalizationDate"),
        RecentSurgery = GetBoolOrNull(row, "p1_bHasRecentSurgery"),
        RecentSurgeryDate = GetDateOrNull(row, "p1_dRecentSurgery"),
        AllergiesNotes = GetString(row, "p1_sHasAllergiesNote"),
        NoAllergies = GetBoolOrNull(row, "p1_bHasAllergies"),
        TotalColectomy = GetBoolOrNull(row, "MedFamSocialHist_TotalColectomy"),
        TotalColectomyDate = GetString(row, "MedFamSocialHist_TotalColectomyDate"),
        BilateralMastectomy = GetBoolOrNull(row, "MedFamSocialHist_BilateralMastectomy"),
        BilateralMastectomyDate = GetString(row, "MedFamSocialHist_BilateralMastectomyDate"),
        UnilateralMastectomyLeft = GetBoolOrNull(row, "UnilateralMastectomyLeft"),
        UnilateralMastectomyLeftDate = GetString(row, "UnilateralMastectomyLeftDate"),
        UnilateralMastectomyRight = GetBoolOrNull(row, "UnilateralMastectomyRight"),
        UnilateralMastectomyRightDate = GetString(row, "UnilateralMastectomyRightDate"),
        OtherSurgery = GetString(row, "MedFamSocialHist_OtherSurgery"),
        OtherSurgeryDate = GetString(row, "MedFamSocialHist_OtherSurgeryDate"),
        NoSurgery = GetBoolOrNull(row, "MedFamSocialHist_NoSurgery"),
        HistoryPresentIllnessSelectedText = GetString(row, "HistoryPresentIllnessSelectedText"),
        ChoseDrinkingAlcohol = GetBoolOrNull(row, "ChoseDrinkingAlcohol"),
        ChoseOtherStd = GetBoolOrNull(row, "ChoseOtherSTD"),
        ChoseQuittingTabacco = GetBoolOrNull(row, "ChoseQuittingTabacco"),
        ChoseRiskofHiv = GetBoolOrNull(row, "ChoseRiskofHIV"),
        ChoseUseIllicitDrugs = GetBoolOrNull(row, "ChoseUseIllicitDrugs"),
    };

    private static MedicalFamilySocialHistorySection MapMedicalFamilySocialHistory(IDictionary<string, object> row) => new()
    {
        Na = GetBoolOrNull(row, "MedFamSocialHist_NA"),
        DmPatient = GetBoolOrNull(row, "p1_bFamHistDmPatient"),
        DmMother = GetBoolOrNull(row, "p1_bFamHistDmMother"),
        DmFather = GetBoolOrNull(row, "p1_bFamHistDmFather"),
        DmSiblings = GetBoolOrNull(row, "p1_bFamHistDmSiblings"),
        CvdPatient = GetBoolOrNull(row, "p1_bFamHistCVDPatient"),
        CvdMother = GetBoolOrNull(row, "p1_bFamHistCVDMother"),
        CvdFather = GetBoolOrNull(row, "p1_bFamHistCVDFather"),
        CvdSiblings = GetBoolOrNull(row, "p1_bFamHistCVDSiblings"),
        CholesterolPatient = GetBoolOrNull(row, "p1_bFamHistCholPatient"),
        CholesterolMother = GetBoolOrNull(row, "p1_bFamHistCholMother"),
        CholesterolFather = GetBoolOrNull(row, "p1_bFamHistCholFather"),
        CholesterolSiblings = GetBoolOrNull(row, "p1_bFamHistCholSiblings"),
        CancerPatient = GetBoolOrNull(row, "p1_bFamHistCancerPatient"),
        CancerMother = GetBoolOrNull(row, "p1_bFamHistCancerMother"),
        CancerFather = GetBoolOrNull(row, "p1_bFamHistCancerFather"),
        CancerSiblings = GetBoolOrNull(row, "p1_bFamHistCancerSiblings"),
        AlzheimerPatient = GetBoolOrNull(row, "p1_bFamHistAlzheimerPatient"),
        AlzheimerMother = GetBoolOrNull(row, "p1_bFamHistAlzheimerMother"),
        AlzheimerFather = GetBoolOrNull(row, "p1_bFamHistAlzheimerFather"),
        AlzheimerSiblings = GetBoolOrNull(row, "p1_bFamHistAlzheimerSiblings"),
        PatientNa = GetBoolOrNull(row, "MedFamSocialHist_PatientNA"),
        MotherNa = GetBoolOrNull(row, "MedFamSocialHist_MotherNA"),
        FatherNa = GetBoolOrNull(row, "MedFamSocialHist_FatherNA"),
        SiblingsNa = GetBoolOrNull(row, "MedFamSocialHist_BrotherNA"),
        OtherConditionText = GetString(row, "p1_sFamHistOtherCondition"),
        OtherPatient = GetBoolOrNull(row, "p1_bFamHistOtherPatient"),
        OtherMother = GetBoolOrNull(row, "p1_bFamHistOtherMother"),
        OtherFather = GetBoolOrNull(row, "p1_bFamHistOtherFather"),
        OtherSiblings = GetBoolOrNull(row, "p1_bFamHistOtherSiblings"),
        VihPatient = GetBoolOrNull(row, "FamHistVIHPatient"),
        VihMother = GetBoolOrNull(row, "FamHistVIHMother"),
        VihFather = GetBoolOrNull(row, "FamHistVIHFather"),
        VihSiblings = GetBoolOrNull(row, "FamHistVIHSiblings"),
        HistoryAlcoholism = GetBoolOrNull(row, "MedFamSocialHist_HistoryAlcoholism"),
        HistoryDrugDependence = GetBoolOrNull(row, "MedFamSocialHist_HistoryDrugDependence"),
        HistoryCaffeineDependence = GetBoolOrNull(row, "HistoryCaffeineDependence"),
        RiskForStd = GetBoolOrNull(row, "p1_bHabitsHasRiskForStd"),
        RiskForHiv = GetBoolOrNull(row, "p1_bHabitsHasRiskForHiv"),
        CounselTabaccoUse = GetBoolOrNull(row, "p1_bHabitsHasCounselTabaccoUse"),
        CounselIllicitDrugUse = GetBoolOrNull(row, "p1_bHabitsHasCounselIllicitDrugUse"),
        CounselAlcoholUse = GetBoolOrNull(row, "p1_bHabitsHasCounselAlcoholMisUse"),
        Nicotine = GetBoolOrNull(row, "Nicotine"),
        Opiates = GetBoolOrNull(row, "Opiates"),
        Cannabis = GetBoolOrNull(row, "Cannabis"),
        Sedatives = GetBoolOrNull(row, "Sedatives"),
        Hypnotics = GetBoolOrNull(row, "Hypnotics"),
        Anxiolytics = GetBoolOrNull(row, "Anxiolytics"),
        OtherDrugs = GetBoolOrNull(row, "OtherDrugs"),
        OtherDrugsText = GetString(row, "OtherDrugsText"),
        HistoryOfMotherPregnancy = GetString(row, "HistoryOfMotherPregnancy"),
        FisicalActivityScreening = GetString(row, "FisicalActivityScreening"),
        FisicalActivityScreeningNa = GetBoolOrNull(row, "FisicalActivityScreening_NA"),
        FisicalActivityScreeningDailyActivityRecommended = GetBoolOrNull(row, "FisicalActivityScreening_DailyActivityRecommended"),
        NutricionalScreeningNa = GetBoolOrNull(row, "NutricionalScreening_NA"),
        NutricionalScreeningAdequateIntake = GetBoolOrNull(row, "NutricionalScreening_AdequateIntake"),
        NutricionalScreeningBalancedNutriciousDiet = GetBoolOrNull(row, "NutricionalScreening_BalancedNutritiousDiet"),
        NutricionalScreeningBreastmilk = GetBoolOrNull(row, "NutricionalScreening_Breastmilk"),
        NutricionalScreeningCereal = GetBoolOrNull(row, "NutricionalScreening_Cereal"),
        NutricionalScreeningCowMilk = GetBoolOrNull(row, "NutricionalScreening_CowMilk"),
        NutricionalScreeningFeedsItself = GetBoolOrNull(row, "NutricionalScreening_FeedsItself"),
        NutricionalScreeningFormula = GetBoolOrNull(row, "NutricionalScreening_Formula"),
        NutricionalScreeningJunkFood = GetBoolOrNull(row, "NutricionalScreening_JunkFood"),
        NutricionalScreeningOther = GetString(row, "NutricionalScreening_Others"),
        NutricionalScreeningOverweight = GetBoolOrNull(row, "NutricionalScreening_Overweight"),
        NutricionalScreeningSodaJuices = GetBoolOrNull(row, "NutricionalScreening_SodaJuices"),
        NutricionalScreeningSolidFoot = GetBoolOrNull(row, "NutricionalScreening_SolidFoot"),
        NutricionalScreeningSupplementVitamins = GetBoolOrNull(row, "NutricionalScreening_SupplementVitamins"),
        NutricionalScreeningUnderWeight = GetBoolOrNull(row, "NutricionalScreening_UnderWeight"),
        NutricionalScreeningFoodAllergies = GetBoolOrNull(row, "NutricionalScreening_FoodAllergies"),
        NutricionalScreeningSpecialDiets = GetBoolOrNull(row, "NutricionalScreening_SpecialDiets"),
        NutricionalScreeningOthersCheckbox = GetBoolOrNull(row, "NutricionalScreening_Others_Checkbox"),
        DevelopmentHealthNa = GetBoolOrNull(row, "DevelopmentHealth_NA"),
        DevelopmentScreeningCommunicationArea = GetBoolOrNull(row, "DevelopmentScreening_CommunicationArea"),
        DevelopmentScreeningFineMotorSkillArea = GetBoolOrNull(row, "DevelopmentScreening_FineMotorSkillArea"),
        DevelopmentScreeningGrossMotorSkillsArea = GetBoolOrNull(row, "DevelopmentScreening_GrossMotorSkillsArea"),
        DevelopmentScreeningSocialIndividualSkillsArea = GetBoolOrNull(row, "DevelopmentScreening_SocialIndividualSkillsArea"),
        DevelopmentScreeningProblemResolutionSkillArea = GetBoolOrNull(row, "DevelopmentScreening_ProblemResolutionSkillArea"),
        DevelopmentScreeningBehavioralHealthArea = GetBoolOrNull(row, "DevelopmentScreening_BehavioralHealthArea"),
        BehavioralHealthNa = GetBoolOrNull(row, "BehavioralHealth_NA"),
        BehavioralHealthPhysicalMentalSelfRegulation = GetBoolOrNull(row, "BehavioralHealth_PhysicalMentalSelftRegulation"),
        BehavioralHealthHabilityToFollowsInstructionsRules = GetBoolOrNull(row, "BehavioralHealth_HabilityToFollowsInstructionsRules"),
        BehavioralHealthSocialCommunication = GetBoolOrNull(row, "BehavioralHealth_SocialCommunication"),
        BehavioralHealthAdaptativeFunctioning = GetBoolOrNull(row, "BehavioralHealth_AdaptativeFunctioning"),
        BehavioralHealthAutonomy = GetBoolOrNull(row, "BehavioralHealth_Autonomy"),
        BehavioralHealthCapacityToBeAffectiveEmpathic = GetBoolOrNull(row, "BehavioralHealth_CapacityToBeAffectiveEmpathic"),
        BehavioralHealthInteractionWithPeople = GetBoolOrNull(row, "BehavioralHealth_InteractionWithPeople"),
        BehavioralHealthUsesAlcoholDrugs = GetBoolOrNull(row, "BehavioralHealth_UsesAlcoholDrugs"),
        AppropriateEducationNa = GetBoolOrNull(row, "AppropriateEducation_NA"),
        AppropriateEducationAppropriateUseCarSeat = GetBoolOrNull(row, "AppropriateEducation_AppropiateUseCarSeat"),
        AppropriateEducationBottleProp = GetBoolOrNull(row, "AppropriateEducation_BottleProp"),
        AppropriateEducationPassiveSmoke = GetBoolOrNull(row, "AppropriateEducation_PasiveSmoke"),
        AppropriateEducationInfantCryingWhatToDo = GetBoolOrNull(row, "AppropriateEducation_InfantCryingWhatToDo"),
        AppropriateEducationShakeBabyPrevention = GetBoolOrNull(row, "AppropriateEducation_ShakeBabyPrevention"),
        AppropriateEducationFirearm = GetBoolOrNull(row, "AppropriateEducation_Firearm"),
        AppropriateEducationPacifiers = GetBoolOrNull(row, "AppropriateEducation_Pacifiers"),
        AppropriateEducationParentsReadToChild = GetBoolOrNull(row, "AppropriateEducation_ParentsReadToChild"),
        AppropriateEducationEmergency911 = GetBoolOrNull(row, "AppropriateEducation_Emergency911"),
        AppropriateEducationFingerFoodChoking = GetBoolOrNull(row, "AppropriateEducation_FingerFoodChoking"),
        AppropriateEducationDisciplinePraise = GetBoolOrNull(row, "AppropriateEducation_DisciplinePrais"),
        AppropriateEducationDrowningPrevention = GetBoolOrNull(row, "AppropriateEducation_DrowningPrevention"),
        AppropriateEducationNeverLeaveToddlerAlone = GetBoolOrNull(row, "AppropriateEducation_NeverLeaveToddlerAlone"),
        AppropriateEducationToiletTraining = GetBoolOrNull(row, "AppropriateEducation_ToiletTraining"),
        AppropriateEducationNutritionExercise = GetBoolOrNull(row, "AppropriateEducation_NutritionExercise"),
        AppropriateEducationEstablishRoutineBedMealsToiletingEtc = GetBoolOrNull(row, "AppropriateEducation_EstablishRoutineBedMealsToiletingEtc"),
        AppropriateEducationUseSportProtection = GetBoolOrNull(row, "AppropriateEducation_UseSportProtection"),
        AppropriateEducationBullying = GetBoolOrNull(row, "AppropriateEducation_Bullying"),
        AppropriateEducationOralHealth = GetBoolOrNull(row, "AppropriateEducation_OralHealth"),
        AppropriateEducationOthers = GetString(row, "AppropriateEducation_Others"),
        AppropriateEducationSportInjuryPrevention = GetBoolOrNull(row, "AppropriateEducation_SportInjuryPrevention"),
        AppropriateEducationDrowningSunSafety = GetBoolOrNull(row, "AppropriateEducation_DrowningSunSafety"),
        AppropriateEducationSafeAtHome = GetBoolOrNull(row, "AppropriateEducation_SafeAtHome"),
        AppropriateEducationCorrectUseSeatbelt = GetBoolOrNull(row, "AppropriateEducation_CorrectUseSeatbelt"),
        AppropriateEducationSexualEducationStd = GetBoolOrNull(row, "AppropriateEducation_SexualEducationSTD"),
        AppropriateEducationDepressionAnxiety = GetBoolOrNull(row, "AppropriateEducation_DepresionAnxiety"),
        AppropriateEducationTabaccoAlcoholDrugsRxDrugsInhalants = GetBoolOrNull(row, "AppropriateEducation_TabaccoAlcoholDrugsRxDrugsInhalants"),
        AppropriateEducationRiskOfTattoosPiercing = GetBoolOrNull(row, "AppropriateEducation_RiskOfTattoosPiercing"),
        AppropriateEducationAutocontrol = GetBoolOrNull(row, "AppropriateEducation_Autocontrol"),
        AppropriateEducationOthersCheckbox = GetBoolOrNull(row, "AppropriateEducation_Others_Checkbox"),
    };

    private static AdvanceDirectiveSection MapAdvanceDirective(IDictionary<string, object> row) => new()
    {
        RefuseToCompleteAdvance = GetBoolOrNull(row, "RefuseToCompleteAdvance"),
        AdvanceCarePlanDiscussed = GetBoolOrNull(row, "AdvCarePlanDiscussed"),
        AdvanceCarePlanExecuteOn = GetDateOrNull(row, "AdvCarePlanExecuteOnDate"),
        AdvanceCarePlanExecutedOnCheck = GetBoolOrNull(row, "AdvCarePlanExecuteOn"),
    };

    private static ReviewOfSystemSection MapReviewOfSystem(IDictionary<string, object> row) => new()
    {
        Constitutional = GetString(row, "p1_sROSConstitutional"),
        Heentoral = GetString(row, "p1_sROSHEENT"),
        AllergicImmunologic = GetString(row, "p1_sROSAllergic"),
        HematologicLymphatic = GetString(row, "p1_sROSHematologic"),
        Cardiovascular = GetString(row, "p1_sROSCardiovascular"),
        Gastrointestinal = GetString(row, "p1_sROSGastrointestinal"),
        Genitourinary = GetString(row, "p1_sROSGenitourinary"),
        Respiratory = GetString(row, "p1_sROSRespiratory"),
        Musculoskeletal = GetString(row, "p1_sROSMusculoskeletal"),
        Neurological = GetString(row, "p1_sROSNeurological"),
        Endocrine = GetString(row, "p1_sROSEndocrine"),
        Integumentary = GetString(row, "p1_sROSIntegumentary"),
        Psychiatric = GetString(row, "p1_sROSPsychiatric"),
        UrinaryIncontinenceLeaking = GetBoolOrNull(row, "p1_bHasUrinaryIncontinence"),
        HearingDifficulty = GetIntOrNull(row, "HearingDifficulty"),
        DescribePositiveRos = GetString(row, "p1_sROSNotes"),
        UrinaryIncontinenceBladderExercises = GetBoolOrNull(row, "UrinaryIncontinence_BladderExercises"),
        UrinaryIncontinenceSurgicalIntervention = GetBoolOrNull(row, "UrinaryIncontinence_SurgicalIntervention"),
        UrinaryIncontinenceTreatmentWithMedicine = GetBoolOrNull(row, "UrinaryIncontinence_TreatmentWithMedicine"),
        UrinaryIncontinenceCheckBoxOther = GetBoolOrNull(row, "UrinaryIncontinence_CheckBoxOther"),
        UrinaryIncontinenceOther = GetString(row, "UrinaryIncontinence_Other"),
    };

    // Table 5 holds every medication row (current + adherence); AllergiesMedicationList comes from
    // a different table read in a later batch. isAdherence splits the row between the two lists,
    // exactly as legacy's For Each loop does.
    private static MedicationListSection MapMedicationList(IDictionary<string, object> row, IReadOnlyList<dynamic> medicationRows)
    {
        var currentMedication = new List<MedicationItem>();
        var adherenceMedicationList = new List<MedicationItem>();

        foreach (var medRow in medicationRows)
        {
            IDictionary<string, object> medDict = medRow;
            var item = new MedicationItem
            {
                MedicationName = GetString(medDict, "MedicationName") ?? string.Empty,
                IsHistoric = GetBoolOrNull(medDict, "isHistoric") ?? false,
                IsConfirmed = GetBoolOrNull(medDict, "isConfirmed") ?? false,
            };

            if (GetBoolOrNull(medDict, "IsAdherenceMedication") ?? false)
            {
                adherenceMedicationList.Add(item);
            }
            else
            {
                currentMedication.Add(item);
            }
        }

        return new MedicationListSection
        {
            CurrentlyDoesNotUse = GetBoolOrNull(row, "PatientCurrentlyNoUse"),
            NotKnowAllergies = GetBoolOrNull(row, "NotKnowAllergies_MedList"),
            CurrentMedication = currentMedication,
            AdherenceMedicationList = adherenceMedicationList,
        };
    }

    private static MedicationReviewSection MapMedicationReview(IDictionary<string, object> row) => new()
    {
        Question1 = GetBoolOrNull(row, "p1_bMedReviewPatHasKnowledgeOfMedications"),
        Question2 = GetBoolOrNull(row, "p1_bMedReviewPatCanIdMedicationFrequency"),
        Question3 = GetBoolOrNull(row, "p1_bMedReviewPatIsUsingMedicationCorrectly"),
    };

    private static CognitiveAssessmentSection MapCognitiveAssessment(IDictionary<string, object> row) => new()
    {
        DayOfTheWeek = GetBoolOrNull(row, "p2_bCognitiveAssemntDayOfWeekCorrect"),
        MonthOfTheYear = GetBoolOrNull(row, "p2_bCognitiveAssemntMonthOfYearCorrect"),
        Year = GetBoolOrNull(row, "p2_bCognitiveAssemntYearCorrect"),
        Ball = GetBoolOrNull(row, "p2_bCognitiveAssemntBallCorrect"),
        Flag = GetBoolOrNull(row, "p2_bCognitiveAssemntFlagCorrect"),
        Tree = GetBoolOrNull(row, "p2_bCognitiveAssemntTreeCorrect"),
        Wnl = GetBoolOrNull(row, "p2_bCognitiveAssemntIsWnl"),
        Diagnosis = GetString(row, "p2_sCognitiveAssemntDx"),
        PlanGoalsTreatmentInterventionFollowUp = GetString(row, "p2_sCognitiveAssemntPlan"),
    };

    private static PainScreeningSection MapPainScreening(IDictionary<string, object> row) => new()
    {
        PatientHaveComplaint = GetBoolOrNull(row, "p4_bPainScreeningHasPain"),
        PainIsLocated = GetString(row, "p4_sPainScreeningPainLocation"),
        ManagePainWith = GetString(row, "p4_sPainScreeningTreatmentOrMedications"),
        TreatmentHaveBeenEffective = GetBoolOrNull(row, "p4_bPainScreeningMedicationsEffective"),
        TreatmentHaveBeenEffectiveNa = GetBoolOrNull(row, "TreatmentHaveBeenEffectiveNA"),
        RatePainExperiencingNow = GetIntOrNull(row, "p4_nPainScreeningRate"),
        Transportation = GetBoolOrNull(row, "p4_bPainScreeningInterferedTransport"),
        BathingDressing = GetBoolOrNull(row, "p4_bPainScreeningInterferedBathing"),
        WalkingAbility = GetBoolOrNull(row, "p4_bPainScreeningInterferedWalking"),
        EnjoymentOfLife = GetBoolOrNull(row, "p4_bPainScreeningInterferedLife"),
        Toileting = GetBoolOrNull(row, "p4_bPainScreeningInterferedToileting"),
        Sleep = GetBoolOrNull(row, "p4_bPainScreeningInterferedSleep"),
        Mood = GetBoolOrNull(row, "p4_bPainScreeningInterferedMood"),
        Na = GetBoolOrNull(row, "PainScreeningInterferedNA"),
        Employment = GetBoolOrNull(row, "p4_bPainScreeningInterferedEmployment"),
        HouseWork = GetBoolOrNull(row, "p4_bPainScreeningInterferedHousework"),
        FoodPreparation = GetBoolOrNull(row, "p4_bPainScreeningInterferedCooking"),
        RelationshipWithOther = GetBoolOrNull(row, "p4_bPainScreeningInterferedRelationships"),
        PainDueTo = GetString(row, "p4_sPainScreeningDx"),
        PlanGoalsTreatmentInterventionFollowUp = GetString(row, "p4_sPainScreeningPlan"),
        CausalCondition = GetString(row, "PainScreeing_CausalCondition"),
        ArthritisDueToInfection = GetBoolOrNull(row, "ArthritisDueInfection"),
        Others = GetString(row, "PainScreening_Other"),
        PainEvaluationOtherCondition = GetBoolOrNull(row, "PainEvaluationOtherCondition"),
        PainEvaluationOtherConditionText = GetString(row, "PainEvaluationOtherConditionText"),
        PainEvaluationOtherActivities = GetBoolOrNull(row, "PainEvaluation_OtherActivities"),
    };

    private static ActivitiesOfDailyLivingSection MapActivitiesOfDailyLiving(IDictionary<string, object> row) => new()
    {
        Bathing = GetBoolOrNull(row, "p5_bFootActivitiesIndependentBath"),
        BathingComments = GetString(row, "p5_bFootActivitiesBathComments"),
        DressingAndUndressing = GetBoolOrNull(row, "p5_bFootActivitiesIndependentDress"),
        DressingAndUndressingComments = GetString(row, "p5_bFootActivitiesDressComments"),
        Eating = GetBoolOrNull(row, "p5_bFootActivitiesIndependentEat"),
        EatingComments = GetString(row, "p5_bFootActivitiesEatComments"),
        TransferringBedChair = GetBoolOrNull(row, "p5_bFootActivitiesIndependentMobility"),
        TransferringBedChairComments = GetString(row, "p5_bFootActivitiesMobilityComments"),
        VoluntarilyControl = GetBoolOrNull(row, "p5_bFootActivitiesIndependentNaturalDischarge"),
        VoluntarilyControlComments = GetString(row, "p5_bFootActivitiesNaturalDischargeComments"),
        UsingToilet = GetBoolOrNull(row, "p5_bFootActivitiesIndependentToilet"),
        UsingToiletComments = GetString(row, "p5_bFootActivitiesToiletComments"),
        Walking = GetBoolOrNull(row, "p5_bFootActivitiesIndependentWalking"),
        WalkingComments = GetString(row, "p5_bFootActivitiesWalkingComments"),
        BedFast = GetBoolOrNull(row, "ActivitiesDaily_BedFast"),
        HistoryOfFalling = GetBoolOrNull(row, "HistoryOfFalling"),
        HistoryOfFallingComments = GetString(row, "ActivitiesOfDailyLiving_HistoryOfFalling_Comments"),
        DependenceOnOxygen = GetBoolOrNull(row, "DependenceOnOxygen"),
        DependenceOnRespirator = GetBoolOrNull(row, "DependenceOnRespirator"),
        DependenceOnWheelchair = GetBoolOrNull(row, "DependenceOnWheelchair"),
    };

    // Legacy computes localized (ES/EN), comma-joined summary strings for
    // ColorectalColonoscopyResult/ColorectalFlexibleSigmoidoscopyResult from the individual
    // checkboxes below, capped at 8 terms, for report display. Not replicated here -- it's
    // presentation formatting, not data, and every checkbox it would summarize is already exposed
    // as a plain boolean.
    private static ScreeningScheduleSection MapScreeningSchedule(IDictionary<string, object> row)
    {
        var isDiabeticGate = GetBoolOrNull(row, "AssessmentPlanTreatment_No") ?? false;

        return new ScreeningScheduleSection
        {
            BoneMineralDensityDate = GetDateOrNull(row, "p4_dScreeningSchRecBmdDone"),
            BoneMineralDensityResult = GetString(row, "Screening_BMD_Result"),
            BoneMineralDensityPrescribed = GetBoolOrNull(row, "Screening_BMD_Prescribed"),
            BoneMineralDensityNaFor = GetString(row, "BoneMineralDensity_NAFor"),
            BoneMineralDensityRxOrdered = GetBoolOrNull(row, "Screening_BMD_RxOrdered"),
            BoneMineralDensityResultNa = GetBoolOrNull(row, "ScreeningSchedule_BoneMineralDensityResult_NA"),
            BoneMineralDensityResultNormal = GetBoolOrNull(row, "ScreeningSchedule_BoneMineralDensityResult_Normal"),
            BoneMineralDensityResultOsteopenia = GetBoolOrNull(row, "ScreeningSchedule_BoneMineralDensityResult_Osteopenia"),
            BoneMineralDensityResultOsteoporosis = GetBoolOrNull(row, "ScreeningSchedule_BoneMineralDensityResult_Osteoporosis"),
            BoneMineralDensityResultOther = GetBoolOrNull(row, "ScreeningSchedule_BoneMineralDensityResult_Other"),

            CardiovascularLdlDate = GetDateOrNull(row, "Screening_Cardio_LDL_DoneDate"),
            CardiovascularLdlResult = GetString(row, "Screening_Cardio_LDL_Result"),
            CardiovascularLdlPrescribed = GetBoolOrNull(row, "Screening_Cardio_LDL_Prescribed"),
            CardiovascularLdlNaFor = GetString(row, "Screening_Cardio_LDL_NAFor"),
            CardiovascularBetaDate = GetDateOrNull(row, "Screening_Cardio_BetaBlocker_DoneDate"),
            CardiovascularBetaResult = GetString(row, "Screening_Cardio_BetaBlocker_Result"),
            CardiovascularBetaPrescribed = GetBoolOrNull(row, "Screening_Cardio_BetaBlocker_Prescribed"),
            CardiovascularBetaNaFor = GetString(row, "Screening_Cardio_BetaBlocke_NAFor"),

            ColorectalCancerScreeningDate = GetDateOrNull(row, "p4_dScreeningSchRecColonCancerDone"),
            ColorectalCancerScreeningResult = GetString(row, "Screening_ColorectalCancer_Result"),
            ColorectalCancerScreeningPrescribed = GetBoolOrNull(row, "Screening_ColorectalCancer_Prescribe"),
            ColorectalCancerScreeningSelectedIndex = GetIntOrNull(row, "Screening_ColorectalCancer_SelectedIndex"),
            ColorectalCancerScreeningNaFor = GetString(row, "Screening_ColorectalCancer_NAFor"),

            ColorectalColonoscopy = GetBoolOrNull(row, "ColorectalColonoscopy"),
            ColorectalColonoscopyDate = GetDateOrNull(row, "ColorectalColonoscopyDate"),
            ColorectalColonoscopyNaFor = GetString(row, "ColorectalColonoscopyNAFor"),
            ColorectalColonoscopyPrescribe = GetBoolOrNull(row, "ColorectalColonoscopyPrescribe"),
            ColorectalColonoscopyResultNa = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_ColonoscopyResult_NA"),
            ColorectalColonoscopyResultNegative = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_ColonoscopyResult_Negative"),
            ColorectalColonoscopyResultDiverticles = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_ColonoscopyResult_Diverticles"),
            ColorectalColonoscopyResultBleedingAreas = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_ColonoscopyResult_BleedingAreas"),
            ColorectalColonoscopyResultCaInColon = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_ColonoscopyResult_CAInColon"),
            ColorectalColonoscopyResultCaInRectum = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_ColonoscopyResult_CAInRectum"),
            ColorectalColonoscopyResultColitis = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_ColonoscopyResult_Colitis"),
            ColorectalColonoscopyResultUlcerativeColitis = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_ColonoscopyResult_UlcerativeOlitis"),
            ColorectalColonoscopyResultCrohnsDisease = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_ColonoscopyResult_CrohnsDisease"),
            ColorectalColonoscopyResultPolyps = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_ColonoscopyResult_Polyps"),
            ColorectalColonoscopyResultOther = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_ColonoscopyResult_Other"),

            ColorectalFitDna = GetBoolOrNull(row, "ColorectalFITDNA"),
            ColorectalFitDnaDate = GetDateOrNull(row, "ColorectalFITDNADate"),
            ColorectalFitDnaResult = GetString(row, "ColorectalFITDNAResult"),
            ColorectalFitDnaNaFor = GetString(row, "ColorectalFITDNANAFor"),
            ColorectalFitDnaPrescribe = GetBoolOrNull(row, "ColorectalFITDNAPrescribe"),

            ColorectalColonographyCt = GetBoolOrNull(row, "ColorectalColonographyCT"),
            ColorectalColonographyCtDate = GetDateOrNull(row, "ColorectalColonographyCTDate"),
            ColorectalColonographyCtResult = GetString(row, "ColorectalColonographyCTResult"),
            ColorectalColonographyCtNaFor = GetString(row, "ColorectalColonographyCTNAFor"),
            ColorectalColonographyCtPrescribe = GetBoolOrNull(row, "ColorectalColonographyCTPrescribe"),

            ColorectalOccultBlood = GetBoolOrNull(row, "ColorectalOccultBlood"),
            ColorectalOccultBloodDate = GetDateOrNull(row, "ColorectalOccultBloodDate"),
            ColorectalOccultBloodResult = GetString(row, "ColorectalOccultBloodResult"),
            ColorectalOccultBloodNaFor = GetString(row, "ColorectalOccultBloodNAFor"),
            ColorectalOccultBloodPrescribe = GetBoolOrNull(row, "ColorectalOccultBloodPrescribe"),

            ColorectalFlexibleSigmoidoscopy = GetBoolOrNull(row, "ColorectalFlexibleSigmoidoscopy"),
            ColorectalFlexibleSigmoidoscopyDate = GetDateOrNull(row, "ColorectalFlexibleSigmoidoscopyDate"),
            ColorectalFlexibleSigmoidoscopyNaFor = GetString(row, "ColorectalFlexibleSigmoidoscopyNAFor"),
            ColorectalFlexibleSigmoidoscopyPrescribe = GetBoolOrNull(row, "ColorectalFlexibleSigmoidoscopyPrescribe"),
            ColorectalFlexibleSigmoidoscopyResultNa = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_FlexibleSigmoidoscopyResult_NA"),
            ColorectalFlexibleSigmoidoscopyResultNegative = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_FlexibleSigmoidoscopyResult_Negative"),
            ColorectalFlexibleSigmoidoscopyResultAnalFissure = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_FlexibleSigmoidoscopyResult_AnalFissure"),
            ColorectalFlexibleSigmoidoscopyResultAnorectalAbscess = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_FlexibleSigmoidoscopyResult_AnorectalAbscess"),
            ColorectalFlexibleSigmoidoscopyResultIntestinalOcclusion = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_FlexibleSigmoidoscopyResult_IntestinalOcclusion"),
            ColorectalFlexibleSigmoidoscopyResultCaEnElSigmoideo = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_FlexibleSigmoidoscopyResult_CAEnElSigmoideo"),
            ColorectalFlexibleSigmoidoscopyResultCaInRectum = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_FlexibleSigmoidoscopyResult_CAInRectum"),
            ColorectalFlexibleSigmoidoscopyResultCaInTheRectosigmoidJunction = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_FlexibleSigmoidoscopyResult_CAInTheRectosigmoidJunction"),
            ColorectalFlexibleSigmoidoscopyResultColorectalPolyps = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_FlexibleSigmoidoscopyResult_ColorectalPolyps"),
            ColorectalFlexibleSigmoidoscopyResultDiverticles = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_FlexibleSigmoidoscopyResult_Diverticles"),
            ColorectalFlexibleSigmoidoscopyResultHemorrhoids = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_FlexibleSigmoidoscopyResult_Hemorrhoids"),
            ColorectalFlexibleSigmoidoscopyResultHirschsprungDisease = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_FlexibleSigmoidoscopyResult_HirschsprungDisease"),
            ColorectalFlexibleSigmoidoscopyResultInflammatoryBowelDisease = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_FlexibleSigmoidoscopyResult_InflammatoryBowelDisease"),
            ColorectalFlexibleSigmoidoscopyResultInflammationOrInfection = GetBoolOrNull(row, "ScreeningSchedule_Colorectal_FlexibleSigmoidoscopyResult_InflammationOrInfection"),

            IsDiabetic = GetBoolOrNull(row, "Screening_IsDiabetics"),

            // Legacy skips this whole block (GoTo notDiabeticScreening) when
            // AssessmentPlanTreatment_No is true -- despite the "isDiabetic" local variable name,
            // that flag actually gates whether diabetes screening fields are populated at all.
            DiabetesScreeningDilatedEyeExamDate = isDiabeticGate ? null : GetDateOrNull(row, "Screening_Diabetes_DilatedEye_Date"),
            DiabetesScreeningDilatedEyeExamResult = isDiabeticGate ? null : GetString(row, "Screening_Diabetes_DilatedEye_Result"),
            DiabetesScreeningDilatedEyeExamPrescribed = isDiabeticGate ? null : GetBoolOrNull(row, "Screening_Diabetes_DilatedEye_Prescribed"),
            DiabetesScreeningDilatedEyeExamNaFor = isDiabeticGate ? null : GetString(row, "Screening_Diabetes_DilatedEye_NAFor"),
            DiabetesScreeningHga1CDate = isDiabeticGate ? null : GetDateOrNull(row, "DiabetesScreening_HGA1C_Date"),
            DiabetesScreeningHga1CResult = isDiabeticGate ? null : GetString(row, "DiabetesScreening_HGA1C_Result"),
            DiabetesScreeningHga1CPrescribed = isDiabeticGate ? null : GetBoolOrNull(row, "DiabetesScreening_HGA1C_Prescribed"),
            DiabetesScreeningHga1CNaFor = isDiabeticGate ? null : GetString(row, "DiabetesScreening_HGA1C_NAFor"),
            // DiabetesScreeningMicroalbumin* is never populated: legacy's GHP branch that would
            // read it is permanently disabled (If False Then), so it always takes the Urine
            // Albumin/Creatinine path instead (see ScreeningSchedule2023Extras).
            GlaucomaTestDate = isDiabeticGate ? null : GetDateOrNull(row, "p4_dScreeningSchGlaucomaDone"),
            GlaucomaTestResult = isDiabeticGate ? null : GetString(row, "Screening_Diabetes_GlaucomaTest_Result"),
            GlaucomaTestPrescribed = isDiabeticGate ? null : GetBoolOrNull(row, "Screening_Diabetes_GlaucomaTest_Prescribed"),
            GlaucomaTestNaFor = isDiabeticGate ? null : GetString(row, "Screening_Diabetes_GlaucomaTest_NAFor"),

            DiabetesScreeningLdlDate = GetDateOrNull(row, "Screening_Diabetes_LDL_Date"),
            DiabetesScreeningLdlResult = GetString(row, "Screening_Diabetes_LDL_Result"),
            DiabetesScreeningLdlPrescribed = GetBoolOrNull(row, "Screening_Diabetes_LDL_Prescribed"),
            DiabetesScreeningLdlNaFor = GetString(row, "Screening_Diabetes_LDL_NAFor"),

            MammogramProstateCancerDate = GetDateOrNull(row, "Screening_Diabetes_MammogramProstate_Date"),
            MammogramProstateCancerResult = GetString(row, "Screening_Diabetes_MammogramProstate_Result"),
            MammogramProstateCancerPrescribed = GetBoolOrNull(row, "Screening_Diabetes_MammogramProstate_Prescribed"),
            MammogramProstateCancerNaFor = GetString(row, "Screening_Diabetes_MammogramProstate_NAFor"),

            FluShotDate = GetDateOrNull(row, "p4_dScreeningSchRecFluShotDone"),
            FluShotComments = GetString(row, "Screening_Diabetes_FluShot_Comments"),
            FluShotPrescribed = GetBoolOrNull(row, "Screening_Diabetes_FluShot_Prescribed"),
            FluShotPatientRefuses = GetBoolOrNull(row, "ScreeningSchedule_FluShot_PatientRefuses"),

            PneumococcalShotDate = GetDateOrNull(row, "p4_dScreeningSchRecPneumococcalDone"),
            PneumococcalShotComments = GetString(row, "Screening_Diabetes_PneumococcalShot_Comments"),
            PneumococcalShotPrescribed = GetBoolOrNull(row, "Screening_Diabetes_PneumococcalShot_Prescribed"),
            PneumococcalShotPatientRefuses = GetBoolOrNull(row, "ScreeningSchedule_PneumococcalShot_PatientRefuses"),

            Covid19VaccineHouse = GetString(row, "COVID19VaccineHouse"),
            Covid19VaccineShot = GetIntOrNull(row, "COVID19VaccineShot"),
            Covid19VaccineShotDate1 = GetDateOrNull(row, "COVID19VaccineShotDate1"),
            Covid19VaccineShotDate2 = GetDateOrNull(row, "COVID19VaccineShotDate2"),
            Covid19VaccineShotDate3 = GetDateOrNull(row, "COVID19VaccineShotDate3"),
            Covid19VaccineRefuse = GetBoolOrNull(row, "COVID19VaccineRefuse"),
            Covid19VaccineOrdered = GetBoolOrNull(row, "COVID19VaccineOrdered"),

            PapSmearDate = GetDateOrNull(row, "PAPSMEAR_Date"),
            PapSmearResult = GetString(row, "PAPSMEAR_Result"),
            PapSmearNaFor = GetString(row, "PAPSMEAR_NAFor"),
            PapSmearPrescribed = GetBoolOrNull(row, "PAPSMEAR_Prescribed"),

            ProstateCancerDate = GetDateOrNull(row, "ProstateCancerDate"),
            ProstateCancerResult = GetString(row, "ProstateCancerResult"),
            ProstateCancerNaFor = GetString(row, "ProstateCancerNAFor"),
            ProstateCancerPrescribed = GetBoolOrNull(row, "ProstateCancerPrescribed"),

            MammogramCancerDate = GetDateOrNull(row, "MammogramCancerDate"),
            MammogramCancerResult = GetString(row, "MammogramCancerResult"),
            MammogramCancerNaFor = GetString(row, "MammogramCancerNAFor"),
            MammogramCancerPrescribed = GetBoolOrNull(row, "MammogramCancerPrescribed"),
            MammogramCancerResultNa = GetBoolOrNull(row, "ScreeningSchedule_Mammogram_CancerResult_NA"),
            MammogramCancerResultCategory0 = GetBoolOrNull(row, "ScreeningSchedule_Mammogram_CancerResult_Category_0"),
            MammogramCancerResultCategory1 = GetBoolOrNull(row, "ScreeningSchedule_Mammogram_CancerResult_Category_1"),
            MammogramCancerResultCategory2 = GetBoolOrNull(row, "ScreeningSchedule_Mammogram_CancerResult_Category_2"),
            MammogramCancerResultCategory3 = GetBoolOrNull(row, "ScreeningSchedule_Mammogram_CancerResult_Category_3"),
            MammogramCancerResultCategory4 = GetBoolOrNull(row, "ScreeningSchedule_Mammogram_CancerResult_Category_4"),
            MammogramCancerResultCategory5 = GetBoolOrNull(row, "ScreeningSchedule_Mammogram_CancerResult_Category_5"),
            MammogramCancerResultCategory6 = GetBoolOrNull(row, "ScreeningSchedule_Mammogram_CancerResult_Category_6"),

            ScreeningHpvDate = GetDateOrNull(row, "Screening_HPV_Date"),
            ScreeningHpvResult = GetString(row, "Screening_HPV_Result"),
            ScreeningHpvOrdered = GetBoolOrNull(row, "Screening_HPV_Ordered"),
            ScreeningHpvComment = GetString(row, "Screening_HPV_Comment"),
        };
    }

    private static ScreeningSchedule2023Extras MapScreeningSchedule2023Extras(IDictionary<string, object> row) => new()
    {
        UrineAlbuminDate = GetDateOrNull(row, "Screening_Diabetes_Urine_Albumin_Date"),
        UrineAlbuminResult = GetString(row, "Screening_Diabetes_Urine_Albumin_Result"),
        UrineAlbuminPrescribed = GetBoolOrNull(row, "Screening_Diabetes_Urine_Albumin_Prescribed"),
        UrineAlbuminNaFor = GetString(row, "Screening_Diabetes_Urine_Albumin_Comment"),
        UrineCreatinineDate = GetDateOrNull(row, "Screening_Diabetes_Urine_Creatinine_Date"),
        UrineCreatinineResult = GetString(row, "Screening_Diabetes_Urine_Creatinine_Result"),
        UrineCreatininePrescribed = GetBoolOrNull(row, "Screening_Diabetes_Urine_Creatinine_Prescribed"),
        UrineCreatinineNaFor = GetString(row, "Screening_Diabetes_Urine_Creatinine_Comment"),
        CreatinineAlbuminRatio = GetDecimalOrNull(row, "Screening_Diabetes_Albumin_Creatinine_Ratio"),
        TdTdapDoneDate = GetDateOrNull(row, "Screening_Vaccine_TdTdap_Done_Date"),
        TdTdapComments = GetString(row, "Screening_Vaccine_TdTdap_Comments"),
        TdTdapPrescribed = GetBoolOrNull(row, "Screening_Vaccine_TdTdap_Prescribed"),
        TdTdapPatientRefuses = GetBoolOrNull(row, "Screening_Vaccine_TdTdap_PatientRefuses"),
        ZosterVaccineOrdered = GetBoolOrNull(row, "Screening_Vaccine_ZosterVaccineOrdered"),
        ZosterVaccineRefuse = GetBoolOrNull(row, "Screening_Vaccine_ZosterVaccineRefuse"),
        ZosterVaccineShotDate1 = GetDateOrNull(row, "Screening_Vaccine_ZosterVaccineShotDate1"),
        ZosterVaccineShotDate2 = GetDateOrNull(row, "Screening_Vaccine_ZosterVaccineShotDate2"),
        RetinopathyNegative = GetBoolOrNull(row, "Retinopathy_Negative"),
    };

    // uspGetAHA2 returns an 11-table result set (DataSet in legacy); read once and keep every
    // table around so later batches can pull whichever ones they need without a second round trip.
    private static async Task<IReadOnlyList<IReadOnlyList<dynamic>>> LoadAhaDataSetAsync(IDbConnection connection, long claimId, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(
            "uspGetAHA2",
            new { biClaimID = claimId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var tables = new List<IReadOnlyList<dynamic>>();
        while (!multi.IsConsumed)
        {
            var rows = (await multi.ReadAsync()).ToList();
            tables.Add(rows);
        }

        return tables;
    }

    // Column types for uspGetAHA2's ~300 columns aren't independently confirmed against a real SP
    // definition (not part of the SP data provided so far), so reads go through a tolerant
    // Convert.ToXxx rather than a strict typed access, mirroring the same reasoning used for the
    // claim list/search stored procedures.
    private static object? GetRawValue(IDictionary<string, object> row, string column)
        => row.TryGetValue(column, out var value) && value is not null and not DBNull ? value : null;

    private static string? GetString(IDictionary<string, object> row, string column) => GetRawValue(row, column)?.ToString();

    private static DateTime? GetDateOrNull(IDictionary<string, object> row, string column) => GetRawValue(row, column) is { } v ? Convert.ToDateTime(v) : null;

    private static int? GetIntOrNull(IDictionary<string, object> row, string column) => GetRawValue(row, column) is { } v ? Convert.ToInt32(v) : null;

    private static long? GetLongOrNull(IDictionary<string, object> row, string column) => GetRawValue(row, column) is { } v ? Convert.ToInt64(v) : null;

    private static bool? GetBoolOrNull(IDictionary<string, object> row, string column) => GetRawValue(row, column) is { } v ? Convert.ToBoolean(v) : null;

    private static decimal? GetDecimalOrNull(IDictionary<string, object> row, string column) => GetRawValue(row, column) is { } v ? Convert.ToDecimal(v) : null;
}
