using System.Data;
using Chopper.Services.Abstractions;
using Chopper.Services.ClaimConditions;
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
        IDictionary<string, object> headerRow = row;
        var header = MapHeader(tables, claimId);

        // Legacy computes _isGHP from Globals.ValidatePayerID(aha) right after Screening Schedule
        // and reuses it below to gate the "2025" Physical Examination option groups (GHP visits
        // from 2025 onward only). Globals.ValidatePayerID's source isn't available, but the
        // simpler PayerID = "660653763" formula confirmed elsewhere in this file is used here too.
        var isGhp2025 = header.PayerId == "660653763" && header.DateOfVisit is { Year: > 2024 };

        var (cancerDiagnosis, otherCurrentConditions) = MapCancerDiagnosisAndOtherConditions(tables.Count > 1 ? tables[1] : []);
        var (socialDeterminants2020, socialDeterminants2023) = MapSocialDeterminants(tables.Count > 12 ? tables[12] : [], headerRow, header);

        return new AhaClaimSnapshot
        {
            Header = header,
            ChiefComplaintPatientMedicalHistory = MapChiefComplaint(row),
            MedicalFamilySocialHistory = MapMedicalFamilySocialHistory(row),
            AdvanceDirective = MapAdvanceDirective(row),
            ReviewOfSystem = MapReviewOfSystem(row),
            MedicationList = MapMedicationList(row, tables.Count > 5 ? tables[5] : [], tables.Count > 7 ? tables[7] : []),
            MedicationReview = MapMedicationReview(row),
            CognitiveAssessment = MapCognitiveAssessment(row),
            PainScreening = MapPainScreening(row),
            ActivitiesOfDailyLiving = MapActivitiesOfDailyLiving(row),
            ScreeningSchedule = MapScreeningSchedule(row),
            ScreeningSchedule2023Extras = MapScreeningSchedule2023Extras(row),
            PhysicalExamination = MapPhysicalExamination(row, isGhp2025),
            AssessmentPlanOfTreatment = MapAssessmentPlanOfTreatment(row),
            CongenitalDiseases = MapCongenitalDiseases(row),
            Ckd = MapCkd(row),
            PressureSores = MapPressureSores(row),
            RheumatoidArthritis = MapRheumatoidArthritis(row),
            DepressionInventory = MapDepressionInventory(row),
            DmeUse = MapDmeUse(row),
            BmiAssociatedDiagnoses = MapBmiAssociatedDiagnoses(row),
            MyocardialInfarction = MapMyocardialInfarction(row),
            OtherCurrentConditionsAdditional = MapOtherCurrentConditionsAdditional(row),
            MajorDepression = MapMajorDepression(row),
            CardiovascularDiseases = MapCardiovascularDiseases(row),
            PulmonaryDiseases = MapPulmonaryDiseases(row),
            GastrointestinalDiseases = MapGastrointestinalDiseases(row),
            MusculoskeletalGhp = MapMusculoskeletalGhp(row),
            ImLabRef = MapImLabRef(row),
            EyesAndNeurology = MapEyesAndNeurology(row),
            CancerDiagnosis = cancerDiagnosis,
            OtherCurrentConditions = otherCurrentConditions,
            PressureSoresList = MapPressureSoresList(tables.Count > 3 ? tables[3] : []),
            DiseasesOfTheSkin = MapDiseasesOfTheSkin(tables.Count > 4 ? tables[4] : []),
            DxHistorySelectionList = MapDxHistorySelectionList(tables.Count > 6 ? tables[6] : [], claimId),
            SuspiciousDxHxSelectionList = MapSuspiciousDxHxSelectionList(tables.Count > 8 ? tables[8] : [], claimId),
            MalnutritionCriteria = MapMalnutritionCriteria(tables.Count > 10 ? tables[10] : [], row),
            ScreeningSubstanceUseList = MapScreeningSubstanceUseList(tables.Count > 11 ? tables[11] : []),
            ScreeningSubstanceUseListResult = GetString(row, "Screening_Substance_Result"),
            SocialDeterminantsNa = GetBoolOrNull(row, "SocialDeterminants_NA") ?? false,
            SocialDeterminants2020 = socialDeterminants2020,
            SocialDeterminants2023 = socialDeterminants2023,
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
    private static MedicationListSection MapMedicationList(IDictionary<string, object> row, IReadOnlyList<dynamic> medicationRows, IReadOnlyList<dynamic> allergyRows)
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

        // Tables(7) -- a separate result set from the current/adherence medication rows above.
        var allergiesMedicationList = allergyRows
            .Select(allergyRow =>
            {
                IDictionary<string, object> allergyDict = allergyRow;
                return new MedicationItem
                {
                    MedicationName = GetString(allergyDict, "AlergMedName") ?? string.Empty,
                    IsHistoric = GetBoolOrNull(allergyDict, "isHistoric") ?? false,
                };
            })
            .ToList();

        return new MedicationListSection
        {
            CurrentlyDoesNotUse = GetBoolOrNull(row, "PatientCurrentlyNoUse"),
            NotKnowAllergies = GetBoolOrNull(row, "NotKnowAllergies_MedList"),
            CurrentMedication = currentMedication,
            AdherenceMedicationList = adherenceMedicationList,
            AllergiesMedicationList = allergiesMedicationList,
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

    // The "2025" option groups (isGhp2025) only apply to GHP visits from 2025 onward -- a newer
    // addition legacy gates the same way in several of these sub-sections.
    private static PhysicalExaminationSection MapPhysicalExamination(IDictionary<string, object> row, bool isGhp2025) => new()
    {
        Temperature = GetDecimalOrNull(row, "p2_nTempeture"),
        TemperatureType = GetString(row, "p2_sTempetureType"),
        Pulse = GetIntOrNull(row, "p2_nPulse"),
        Breathing = GetIntOrNull(row, "p2_nBreathing"),
        BloodPressure1 = GetIntOrNull(row, "p2_nBloodPressure1"),
        BloodPressure2 = GetIntOrNull(row, "p2_nBloodPressure2"),
        Height = GetDecimalOrNull(row, "p2_nHeight"),
        HeightType = GetString(row, "p2_sHeightType"),
        Weight = GetDecimalOrNull(row, "p2_nWeight"),
        WeightType = GetString(row, "p2_sWeightType"),
        Bmi = GetDecimalOrNull(row, "p2_nBMI"),
        HeadCircumference = GetDecimalOrNull(row, "HeadCircumference"),
        PercentilHt = GetDecimalOrNull(row, "PercentilHT"),
        PercentilWt = GetDecimalOrNull(row, "PercentilWT"),
        PercentilHead = GetDecimalOrNull(row, "PercentilHead"),

        HeenOralNotes = GetString(row, "p2_sHEENTNotes"),
        HeenOralOptions = new HeenOralOptions
        {
            BleedingGums = GetBoolOrNull(row, "HEENOralOptions_BleedingGums"),
            DryMouth = GetBoolOrNull(row, "HEENOralOptions_DryMouth"),
            DryNose = GetBoolOrNull(row, "HEENOralOptions_DryNose"),
            NoTeeth = GetBoolOrNull(row, "HEENOralOptions_NoTeeth"),
            Peerl = GetBoolOrNull(row, "HEENOralOptions_Peerl"),
            Wnl = GetBoolOrNull(row, "HEENOralOptions_WNL"),
            Strabismus = GetBoolOrNull(row, "HEENOralOptions_Strabismus"),
            Ptosis = GetBoolOrNull(row, "HEENOralOptions_Ptosis"),
            RedReflex = GetBoolOrNull(row, "HEENOralOptions_Redreflex"),
            AbnormalPupillaryReflex = GetBoolOrNull(row, "HEENOralOptions_AbnormalPupillaryReflex"),
            BlockedNasolacrimalDucts = GetBoolOrNull(row, "HEENOralOptions_BlockedNasolacrimalDucts"),
            NasalDischarge = GetBoolOrNull(row, "HEENOralOptions_NasalDischarge"),
            ExudatingTonsils = GetBoolOrNull(row, "HEENOralOptions_ExudatingTonsils"),
            Normocephalic = isGhp2025 ? GetBoolOrNull(row, "HEENOptions_Normocephalic") : null,
            ScalpLessionsMasses = isGhp2025 ? GetBoolOrNull(row, "HEENOptions_ScalpLessionsMasses") : null,
            NeckSupple = isGhp2025 ? GetBoolOrNull(row, "HEENOptions_NeckSupple") : null,
            Adenopathies = isGhp2025 ? GetBoolOrNull(row, "HEENOptions_Adenopathies") : null,
            ClearOropharynxn = isGhp2025 ? GetBoolOrNull(row, "HEENOptions_ClearOropharynxn") : null,
            LessionExudate = isGhp2025 ? GetBoolOrNull(row, "HEENOptions_LessionExudate") : null,
            TympanicMembranesIntact = isGhp2025 ? GetBoolOrNull(row, "HEENOptions_TympanicMembranesIntact") : null,
            EqualAirConductionAndAcousticReflexes = isGhp2025 ? GetBoolOrNull(row, "HEENOptions_EqualAirConductionAndAcousticReflexes") : null,
            NoNystagmus = isGhp2025 ? GetBoolOrNull(row, "HEENOptions_NoNystagmus") : null,
            Eomi = isGhp2025 ? GetBoolOrNull(row, "HEENOptions_EOMI") : null,
            Other = isGhp2025 ? GetBoolOrNull(row, "HEENOptions_Other") : null,
            None = isGhp2025 ? GetBoolOrNull(row, "HEENOptions_None") : null,
        },

        ConstitutionalNotes = isGhp2025 ? GetString(row, "ConstitutionalOptions_Notes") : null,
        ConstitutionalOptions = isGhp2025
            ? new ConstitutionalOptions
            {
                WellDeveloped = GetBoolOrNull(row, "ConstitutionalOptions_WellDeveloped"),
                PoorDeveloped = GetBoolOrNull(row, "ConstitutionalOptions_PoorDeveloped"),
                AdequateNourishment = GetBoolOrNull(row, "ConstitutionalOptions_AdequateNourishment"),
                InadequateNourishment = GetBoolOrNull(row, "ConstitutionalOptions_InadequateNourishment"),
                InAcuteDistress = GetBoolOrNull(row, "ConstitutionalOptions_InAcuteDistress"),
                NoAcuteDistress = GetBoolOrNull(row, "ConstitutionalOptions_NoAcuteDistress"),
                Caox = GetBoolOrNull(row, "ConstitutionalOptions_CAOX"),
                Others = GetBoolOrNull(row, "ConstitutionalOptions_Others"),
                None = GetBoolOrNull(row, "ConstitutionalOptions_None"),
            }
            : null,

        IntegumentaryNotes = isGhp2025 ? GetString(row, "IntegumentaryOptions_Notes") : null,
        IntegumentaryOptions = isGhp2025
            ? new IntegumentaryOptions
            {
                Warm = GetBoolOrNull(row, "IntegumentaryOptions_Warm"),
                Cold = GetBoolOrNull(row, "IntegumentaryOptions_Cold"),
                AdequatePerfusion = GetBoolOrNull(row, "IntegumentaryOptions_AdequatePerfusion"),
                InadequatePerfusion = GetBoolOrNull(row, "IntegumentaryOptions_InadequatePerfusion"),
                AdequateSkinTurgor = GetBoolOrNull(row, "IntegumentaryOptions_AdequateSkinTurgor"),
                InadequateSkinTurgor = GetBoolOrNull(row, "IntegumentaryOptions_InadequateSkinTurgor"),
                Acne = GetBoolOrNull(row, "IntegumentaryOptions_Acne"),
                Rash = GetBoolOrNull(row, "IntegumentaryOptions_Rash"),
                SkinSpots = GetBoolOrNull(row, "IntegumentaryOptions_SkinSpots"),
                Others = GetBoolOrNull(row, "IntegumentaryOptions_Others"),
                None = GetBoolOrNull(row, "Integumentary_None"),
            }
            : null,

        RespiratoryNotes = isGhp2025 ? GetString(row, "RespiratoryOptions_Notes") : null,
        RespiratoryOptions = isGhp2025
            ? new RespiratoryOptions
            {
                ClearToAuscultations = GetBoolOrNull(row, "RespiratoryOptions_ClearToAuscultations"),
                Wheezes = GetBoolOrNull(row, "RespiratoryOptions_Wheezes"),
                RonchiOrRales = GetBoolOrNull(row, "RespiratoryOptions_RonchiOrRales"),
                AdequatePercussionSounds = GetBoolOrNull(row, "RespiratoryOptions_AdequatePercussionSounds"),
                InadequatePercussionSounds = GetBoolOrNull(row, "RespiratoryOptions_InadequatePercussionSounds"),
                PainUponPalpitation = GetBoolOrNull(row, "RespiratoryOptions_PainUponPalpitation"),
                Others = GetBoolOrNull(row, "RespiratoryOptions_Others"),
                None = GetBoolOrNull(row, "RespiratoryOptions_None"),
            }
            : null,

        GastrointestinalNotes = isGhp2025 ? GetString(row, "GastrointestinalOptions_Notes") : null,
        GastrointestinalOptions = isGhp2025
            ? new GastrointestinalOptions
            {
                GoodDentation = GetBoolOrNull(row, "GastrointestinalOptions_GoodDentation"),
                PoorDentation = GetBoolOrNull(row, "GastrointestinalOptions_PoorDentation"),
                HardToPalpation = GetBoolOrNull(row, "GastrointestinalOptions_HardToPalpation"),
                SoftToPalpation = GetBoolOrNull(row, "GastrointestinalOptions_SoftToPalpation"),
                Tenderness = GetBoolOrNull(row, "GastrointestinalOptions_Tenderness"),
                Visceromegaly = GetBoolOrNull(row, "GastrointestinalOptions_Visceromegaly"),
                Wnl = GetBoolOrNull(row, "GastrointestinalOptions_WNL"),
            }
            : null,

        GenitourinaryNotes = isGhp2025 ? GetString(row, "GenitourinaryOptions_Notes") : null,
        GenitourinaryOptions = isGhp2025
            ? new GenitourinaryOptions
            {
                DeferedGeneralAppereance = GetBoolOrNull(row, "GenitourinaryOptions_DeferedGeneralAppereance"),
                WhithinNormalLimits = GetBoolOrNull(row, "GenitourinaryOptions_WhithinNormalLimits"),
            }
            : null,

        NeckNotes = GetString(row, "p2_sNeckNotes"),
        NeckOptions = new NeckOptions
        {
            Crepitus = GetBoolOrNull(row, "NeckOptions_Crepitus"),
            Masses = GetBoolOrNull(row, "NeckOptions_Masses"),
            NormalTrachealPosition = GetBoolOrNull(row, "NeckOptions_NormalTrachelPosition"),
            OverallAppearance = GetBoolOrNull(row, "NeckOptions_OverallAppearance"),
            Symmetry = GetBoolOrNull(row, "NeckOptions_Symmetry"),
            ThyroidEnlargement = GetBoolOrNull(row, "NeckOptions_ThyroidEnlargement"),
            ThyroidMass = GetBoolOrNull(row, "NeckOptions_ThyroidMass"),
            ThyroidTenderness = GetBoolOrNull(row, "NeckOptions_ThyroidTenderness"),
            Tracheostomy = GetBoolOrNull(row, "NeckOptions_Tracheostomy"),
            Wnl = GetBoolOrNull(row, "NeckOptions_WNL"),
            Rigity = GetBoolOrNull(row, "NeckOption_Rigity"),
            MovementLimitation = GetBoolOrNull(row, "NeckOption_MovementLimitation"),
            Crackle = GetBoolOrNull(row, "NeckOption_Crackle"),
        },

        ChestNotes = GetString(row, "p2_sChestNotes"),
        ChestOptions = new ChestOptions
        {
            AdventitiousSounds = GetBoolOrNull(row, "ChestOptions_AdventitiousSounds"),
            BreastsTenderness = GetBoolOrNull(row, "ChestOptions_BreastsTenderness"),
            Crackels = GetBoolOrNull(row, "ChestOptions_Crackels"),
            DiaphragmaticMovement = GetBoolOrNull(row, "ChestOptions_DiaphragmaticMovement"),
            Dullness = GetBoolOrNull(row, "ChestOptions_Dullness"),
            Flatness = GetBoolOrNull(row, "ChestOptions_Flatness"),
            Hyperresonance = GetBoolOrNull(row, "ChestOptions_Hyperresonance"),
            IntercostalRetractions = GetBoolOrNull(row, "ChestOptions_IntercostalRetractions"),
            NippleDischargeBreastsMassesLumps = GetBoolOrNull(row, "ChestOptions_NippleDischargeBreastsMassesLumps"),
            NormalBreathSounds = GetBoolOrNull(row, "ChestOptions_NormalBreathSounds"),
            Rubs = GetBoolOrNull(row, "ChestOptions_Rubs"),
            TactileFremitus = GetBoolOrNull(row, "ChestOptions_TactileFremitus"),
            UseOfAccesoryMuscles = GetBoolOrNull(row, "ChestOptions_UseOfAccesoryMuscles"),
            WheezingSymmetryBreasts = GetBoolOrNull(row, "ChestOptions_WheezingsSymmetryBreasts"),
            Wnl = GetBoolOrNull(row, "ChestOptions_WNL"),
            BreastsMasses = GetBoolOrNull(row, "ChestOptions_BreastsMasses"),
            NippleDischarge = GetBoolOrNull(row, "ChestOptions_NippleDischarge"),
            RtFootToeAmputation = GetBoolOrNull(row, "ChestOptions_RTFootToeAmputation"),
            LtFootToeAmputation = GetBoolOrNull(row, "ChestOptions_LTFootToeAmputation"),
        },

        CardiovascularNotes = GetString(row, "CardiovascularNotes"),
        CardiovascularOptions = new CardiovascularOptions
        {
            AbnormalHeartSound = GetBoolOrNull(row, "CardiovascularOptions_AbnormalHeartSound"),
            AbnormalTemperature = GetBoolOrNull(row, "CardiovascularOptions_AbnormalTemperature"),
            LegEdema = GetBoolOrNull(row, "CardiovascularOptions_LegEdema"),
            MurmursDecreasedPedalPulses = GetBoolOrNull(row, "CardiovascularOptions_MurmursDecreasedPedalPulses"),
            Varicosities = GetBoolOrNull(row, "CardiovascularOptions_Varicosities"),
            Wnl = GetBoolOrNull(row, "CardiovascularOptions_WNL"),
            DecreasedPedalPulses = GetBoolOrNull(row, "CardiovascularOptions_DecreasedPedalPulses"),
            RegularRateRhytm = isGhp2025 ? GetBoolOrNull(row, "CardiovascularOptions_RegularRateRhytm") : null,
            IrregularRateRhytm = isGhp2025 ? GetBoolOrNull(row, "CardiovascularOptions_IrregularRateRhytm") : null,
            Murmurs = isGhp2025 ? GetBoolOrNull(row, "CardiovascularOptions_Murmurs") : null,
            Gallops = isGhp2025 ? GetBoolOrNull(row, "CardiovascularOptions_Gallops") : null,
            Rubs = isGhp2025 ? GetBoolOrNull(row, "CardiovascularOptions_Rubs") : null,
            PainUponPrecordialPalpation = isGhp2025 ? GetBoolOrNull(row, "CardiovascularOptions_PainUponPrecordialPalpation") : null,
            Other = isGhp2025 ? GetBoolOrNull(row, "CardiovascularOptions_Other") : null,
            None = isGhp2025 ? GetBoolOrNull(row, "CardiovascularOptions_None") : null,
        },

        AmputationLegRtBka = GetBoolOrNull(row, "AmputationLegRT_BKA"),
        AmputationLegRtAka = GetBoolOrNull(row, "AmputationLegRT_AKA"),
        AmputationLegRtToe = GetBoolOrNull(row, "AmputationLegRT_Toe"),
        AmputationLegLtBka = GetBoolOrNull(row, "AmputationLegLT_BKA"),
        AmputationLegLtAka = GetBoolOrNull(row, "AmputationLegLT_AKA"),
        AmputationLegLtToe = GetBoolOrNull(row, "AmputationLegLT_Toe"),

        AbdomenNotes = GetString(row, "p2_sAbdomenNotes"),
        AbdomenOptions = new AbdomenOptions
        {
            Colostomy = GetBoolOrNull(row, "AbdomenOptions_Colostomy"),
            Gastrostomy = GetBoolOrNull(row, "AbdomenOptions_Gastrostomy"),
            Hernia = GetBoolOrNull(row, "AbdomenOptions_Hernia"),
            Ileostomy = GetBoolOrNull(row, "AbdomenOptions_Ileostomy"),
            LiverEnlargement = GetBoolOrNull(row, "AbdomenOptions_LiverEnlargement"),
            Masses = GetBoolOrNull(row, "AbdomenOptions_Masses"),
            SpleenEnlargement = GetBoolOrNull(row, "AbdomenOptions_SpleenEnlargement"),
            Tenderness = GetBoolOrNull(row, "AbdomenOptions_Tenderness"),
            Wnl = GetBoolOrNull(row, "AbdomenOptions_WNL"),
            Cystostomy = GetBoolOrNull(row, "AbdomenOptions_Cystostomy"),
            UmbilicalInfection = GetBoolOrNull(row, "AbdomenOptions_UmbilicalInfection"),
            Distention = GetBoolOrNull(row, "AbdomenOptions_Distention"),
            Constipation = GetBoolOrNull(row, "AbdomenOptions_Constipation"),
            Colics = GetBoolOrNull(row, "AbdomenOptions_Colics"),
            Reflux = GetBoolOrNull(row, "AbdomenOptions_Reflux"),
            Rebound = GetBoolOrNull(row, "AbdomenOptions_Rebound"),
            Guarding = GetBoolOrNull(row, "AbdomenOptions_Guarding"),
        },

        GenitaliaGroinButtocksNotes = GetString(row, "p2_sGenitaliaNotes"),
        GenitaliaGroinButtocksOptions = new GenitaliaGroinButtocksOptions
        {
            Cyst = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Cyst"),
            Cystocele = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Cystocele"),
            DefferedGeneralAppearance = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_DefferedGeneralAppearance"),
            Deformities = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Deformities"),
            Discharge = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Discharge"),
            Enlargement = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Enlargement"),
            EstrogenEffect = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_EstrogenEffect"),
            HairDistribution = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_HairDistribution"),
            Hemorrhoids = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Hemorrhoids"),
            Lesions = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Lesions"),
            Masses = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Masses"),
            Nodularity = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Nodularity"),
            PelvicSupport = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_PelvicSupport"),
            Prolapse = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Prolapse"),
            Rashes = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Rashes"),
            Rectocele = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Rectocele"),
            Scarring = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Scarring"),
            Size = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Size"),
            Symmetry = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Symmetry"),
            Tenderness = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Tenderness"),
            Wnl = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_WNL"),
            Urostomy = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_Urostomy"),
            FoleyCatheterUse = GetBoolOrNull(row, "GenitaliaGroinButtocksNotesOptions_FoleyCatheterUse"),
        },

        MusculoskeletalNotes = GetString(row, "MusculokeletalNotes"),
        MusculoskeletalOptions = new MusculoskeletalOptions
        {
            AbnormalGait = GetBoolOrNull(row, "MusculokeletalOptions_AbnormalGait"),
            AbnormalMovements = GetBoolOrNull(row, "MusculokeletalOptions_AbnormalMovements"),
            AbnormalMuscleStrengthTone = GetBoolOrNull(row, "MusculokeletalOptions_AbnormalMuscleStrengthTone"),
            ClubbingNails = GetBoolOrNull(row, "MusculokeletalOptions_ClubbingNails"),
            CogWheel = GetBoolOrNull(row, "MusculokeletalOptions_CogWheel"),
            CyanosisDigits = GetBoolOrNull(row, "MusculokeletalOptions_CyanosisDigits"),
            Dislocation = GetBoolOrNull(row, "MusculokeletalOptions_Dislocation"),
            DislocationNotes = GetString(row, "MusculokeletalOptions_DislocationNotes"),
            Flaccid = GetBoolOrNull(row, "MusculokeletalOptions_Flaccid"),
            LowerExtremitiesAsymmetry = GetBoolOrNull(row, "MusculokeletalOptions_LowerExtremitiesAsymmetry"),
            Spastic = GetBoolOrNull(row, "MusculokeletalOptions_Spastic"),
            UpperExtremitiesAsymmetry = GetBoolOrNull(row, "MusculokeletalOptions_UpperExtremitiesAsymmetry"),
            Wnl = GetBoolOrNull(row, "MusculokeletalOptions_WNL"),
            NoDeformitiesOrDeformations = isGhp2025 ? GetBoolOrNull(row, "MusculoskeletalOption_NoDeformitiesOrDeformations") : null,
            NormalGait = isGhp2025 ? GetBoolOrNull(row, "MusculoskeletalOption_NormalGait") : null,
            AdequateRom = isGhp2025 ? GetBoolOrNull(row, "MusculoskeletalOption_AdequateROM") : null,
            InadequateRom = isGhp2025 ? GetBoolOrNull(row, "MusculoskeletalOption_InadequateROM") : null,
        },

        SkinNotes = GetString(row, "p2_sSkinNotes"),
        SkinOptions = new SkinOptions
        {
            Induration = GetBoolOrNull(row, "SkinOptions_Induration"),
            Lesions = GetBoolOrNull(row, "SkinOptions_Lesions"),
            Nodules = GetBoolOrNull(row, "SkinOptions_Nodules"),
            Rashes = GetBoolOrNull(row, "SkinOptions_Rashes"),
            Tightening = GetBoolOrNull(row, "SkinOptions_Tightening"),
            Ulcers = GetBoolOrNull(row, "SkinOptions_Ulcers"),
            PurpuricLesionsNoted = GetBoolOrNull(row, "SkinOptions_PurpuricLesionsNoted"),
            Wnl = GetBoolOrNull(row, "SkinOptions_WNL"),
        },

        PsychiatricNeurologicNotes = GetString(row, "p2_sPsychiatricNotes"),
        PsychiatricNeurologicOptions = new PsychiatricNeurologicOptions
        {
            Agitation = GetBoolOrNull(row, "PsychiatricNeurologicOptions_Agitation"),
            Anxiety = GetBoolOrNull(row, "PsychiatricNeurologicOptions_Anxiety"),
            Babinsky = GetBoolOrNull(row, "PsychiatricNeurologicOptions_Babinsky"),
            CranialNervesWithDeficits = GetBoolOrNull(row, "PsychiatricNeurologicOptions_CranialNervesWithDeficits"),
            DepressedMode = GetBoolOrNull(row, "PsychiatricNeurologicOptions_DepressedMode"),
            NoSensationTouchLegs = GetBoolOrNull(row, "PsychiatricNeurologicOptions_NoSensationTouchLegs"),
            OrientedToTime = GetBoolOrNull(row, "PsychiatricNeurologicOptions_OrientedToTime"),
            PlaceAndPerson = GetBoolOrNull(row, "PsychiatricNeurologicOptions_PlaceAndPerson"),
            SensationByTouch = GetBoolOrNull(row, "PsychiatricNeurologicOptions_SentationByTouch"),
            Wnl = GetBoolOrNull(row, "PsychiatricNeurologicOptions_WNL"),
            Hemiplejia = GetBoolOrNull(row, "PsychiatricNeurologicOptions_Hemiplejia"),
            Cuadriplejia = GetBoolOrNull(row, "PsychiatricNeurologicOptions_Cuadriplejia"),
            Paraplejia = GetBoolOrNull(row, "PsychiatricNeurologicOptions_Paraplejia"),
            AmbulatingWoLimitation = isGhp2025 ? GetBoolOrNull(row, "NeurologicOption_AmbulatingWOLimitation") : null,
            NormalMuscleStrengthTone = isGhp2025 ? GetBoolOrNull(row, "NeurologicOption_NormalMuscleStrengthTone") : null,
            AbnormalMuscleStrengthTone = isGhp2025 ? GetBoolOrNull(row, "NeurologicOption_AbnormalMuscleStrengthTone") : null,
            FocalDeficits = isGhp2025 ? GetBoolOrNull(row, "NeurologicOption_FocalDeficits") : null,
        },

        HematologicLymphaticImmunologicNotes = GetString(row, "p2_sHemotalogicNotes"),
        HematologicLymphaticImmunologicOptions = new HematologicLymphaticImmunologicOptions
        {
            LymphNodes = GetBoolOrNull(row, "HematologicLymphaticImmunologicOptions_LymphNodes"),
            LymphNodesNotes = GetString(row, "HematologicLymphaticImmunologicOptions_LymphNodesNotes"),
            Wnl = GetBoolOrNull(row, "HematologicLymphaticImmunologicOptions_WNL"),
        },
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
        // Legacy sets these from AssessmentPlanTreatment_Retinopathy/Proliferative first, then
        // unconditionally overwrites (twice, in duplicated code) with the plain Retinopathy/
        // Proliferative columns if those aren't null -- net effect is "prefer Retinopathy/
        // Proliferative, fall back to the AssessmentPlanTreatment_* column". Note this means the
        // AssessmentPlanOfTreatmentSection.Retinopathy/Proliferative properties (used by
        // SaveClaim) are never actually populated by GetAHA -- only the *Comments siblings are.
        Retinopathy = GetBoolOrNull(row, "Retinopathy") ?? GetBoolOrNull(row, "AssessmentPlanTreatment_Retinopathy"),
        Proliferative = GetBoolOrNull(row, "Proliferative") ?? GetBoolOrNull(row, "AssessmentPlanTreatment_Proliferative"),
        ProliferativeEyeRt = GetBoolOrNull(row, "ProliferativeEyeRT"),
        ProliferativeEyeLt = GetBoolOrNull(row, "ProliferativeEyeLT"),
        RetinopathyNegativeEye = GetIntOrNull(row, "Retinopathy_Negative_Eye"),
        RetinopathyEye = GetIntOrNull(row, "Screening_Retinopathy_Eye"),
        ProliferativeEye = GetIntOrNull(row, "Screening_Proliferative_Eye"),
        EyeSeverity = GetBoolOrNull(row, "Screening_Eye_Severity"),
        EyeSeverityLevel = GetIntOrNull(row, "Screening_Eye_SeverityLevel"),
        MacularEdema = GetBoolOrNull(row, "Screening_MacularEdema"),
        MacularEdemaEye = GetIntOrNull(row, "Screening_MacularEdema_Eye"),
        ScreeningRetinopathyNa = GetBoolOrNull(row, "ScreeningRetinopathy_NA"),
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

    // Same "isDiabetic" gate as MapScreeningSchedule: when AssessmentPlanTreatment_No is true,
    // legacy skips (GoTo notDiabetic) the whole diabetic-complication detail block below --
    // No/RetinopathyComments/ProliferativeComments/Dermatitis/Periodontal/PoorlyController and
    // everything after them (which isn't gated) are still populated either way.
    private static AssessmentPlanOfTreatmentSection MapAssessmentPlanOfTreatment(IDictionary<string, object> row)
    {
        var isDiabeticGate = GetBoolOrNull(row, "AssessmentPlanTreatment_No") ?? false;

        return new AssessmentPlanOfTreatmentSection
        {
            No = GetBoolOrNull(row, "AssessmentPlanTreatment_No"),

            DmType = isDiabeticGate ? null : GetBoolOrNull(row, "AssessmentPlanTreatment_DMType"),
            Controlled = isDiabeticGate ? null : GetBoolOrNull(row, "AssessmentPlanTreatment_Controlled"),
            DmSecondary = isDiabeticGate ? null : GetBoolOrNull(row, "AssessmentPlanTreatment_DMSecundary"),
            DmComments = isDiabeticGate ? null : GetString(row, "AssessmentPlanTreatment_DMComments"),
            DiabeticNeuropathy = isDiabeticGate ? null : GetBoolOrNull(row, "AssessmentPlanTreatment_DiabeticNeuropathy"),
            DiabeticNeuropathyComments = isDiabeticGate ? null : GetString(row, "AssessmentPlanTreatment_DiabeticNeuropathyComments"),
            DiabeticPvd = isDiabeticGate ? null : GetBoolOrNull(row, "AssessmentPlanTreatment_DiabeticPVD"),
            DiabeticPvdComments = isDiabeticGate ? null : GetString(row, "AssessmentPlanTreatment_DiabeticPVDComments"),
            DiabeticNephropathy = isDiabeticGate ? null : GetBoolOrNull(row, "AssessmentPlanTreatment_DiabeticNephropathy"),
            DiabeticNephropathyComments = isDiabeticGate ? null : GetString(row, "AssessmentPlanTreatment_DiabeticNephropathyComments"),
            DiabeticCataracts = isDiabeticGate ? null : GetBoolOrNull(row, "AssessmentPlanTreatment_DiabeticCataracts"),
            DiabeticCataractsComments = isDiabeticGate ? null : GetString(row, "AssessmentPlanTreatment_DiabeticCataractsComments"),
            OtherDiabeticComplication = isDiabeticGate ? null : GetString(row, "AssessmentPlanTreatment_OtherComplication"),
            OtherDiabeticComplicationComments = isDiabeticGate ? null : GetString(row, "AssessmentPlanTreatment_OtherComplicationComments"),

            // Legacy assigns these into aha.ScreeningSchedule.Retinopathy/Proliferative, not here
            // -- see MapScreeningSchedule2023Extras. Only the *Comments siblings land on this
            // section.
            RetinopathyComments = isDiabeticGate ? null : GetString(row, "AssessmentPlanTreatment_RetinopathyComments"),
            ProliferativeComments = isDiabeticGate ? null : GetString(row, "AssessmentPlanTreatment_ProliferativeComments"),

            Remission = isDiabeticGate ? null : GetBoolOrNull(row, "AssessmentPlanTreatment_Remission"),

            Dermatitis = GetBoolOrNull(row, "AssessmentPlanTreatment_Dermatitis"),
            DermatitisComments = GetString(row, "AssessmentPlanTreatment_DermatitisComments"),
            Periodontal = GetBoolOrNull(row, "AssessmentPlanTreatment_Periodontal"),
            PeriodontalComments = GetString(row, "AssessmentPlanTreatment_PeriodontalComments"),
            PoorlyController = GetBoolOrNull(row, "AssessmentPlanTreatment_PoorlyController"),

            DmSecondaryText = GetString(row, "DMSecondaryText"),
            OutOfControl = GetBoolOrNull(row, "OutOfControl"),
            UncontrolledWithHyperglycemia = GetBoolOrNull(row, "UncontrolledWithHyperglycemia"),
            UncontrolledWithHypoglycemia = GetBoolOrNull(row, "UncontrolledWithHypoglycemia"),
            HyperlipidemiaDueDm = GetBoolOrNull(row, "HyperlipidemiaDueDM"),
            DmPlanAndTreatmentComments1 = GetString(row, "DMPlanAndTreatmentComments1"),
            DmPlanAndTreatmentComments2 = GetString(row, "DMPlanAndTreatmentComments2"),
            DmPlanAndTreatmentComments3 = GetString(row, "DMPlanAndTreatmentComments3"),
            DiabeticArthropathy = GetBoolOrNull(row, "DiabeticArthropathy"),
            DiabeticArthropathyComment = GetString(row, "DiabeticArthropathyComment"),
            GestionalDiabetes = GetBoolOrNull(row, "GestionalDiabetes"),
            GestionalDiabetesComment = GetString(row, "GestionalDiabetesComment"),
        };
    }

    private static CongenitalDiseasesSection MapCongenitalDiseases(IDictionary<string, object> row) => new()
    {
        Na = GetBoolOrNull(row, "CongenitalDiseases_NA"),
        SpinaBifida = GetBoolOrNull(row, "CongenitalDiseases_SpinaBifida"),
        SpinaBifidaComments = GetString(row, "CongenitalDiseases_SpinaBifidaComments"),
        Hydrocephalus = GetBoolOrNull(row, "CongenitalDiseases_Hydrocephalus"),
        HydrocephalusComments = GetString(row, "CongenitalDiseases_HydrocephalusComments"),
        ChiariMalformation = GetBoolOrNull(row, "CongenitalDiseases_ChiariMalformation"),
        ChiariMalformationComments = GetString(row, "CongenitalDiseases_ChiariMalformationComments"),
        Hemophilia = GetBoolOrNull(row, "CongenitalDiseases_Hemophilia"),
        HemophiliaComments = GetString(row, "CongenitalDiseases_HemophiliaComments"),
        Cranofacial = GetBoolOrNull(row, "CongenitalDiseases_Cranofacial"),
        CranofacialComments = GetString(row, "CongenitalDiseases_CranofacialComments"),
        DistrofiaMuscular = GetBoolOrNull(row, "CongenitalDiseases_DistrofiaMuscular"),
        DistrofiaMuscularComments = GetString(row, "CongenitalDiseases_DistrofiaMuscularComments"),
        CerebralPalsy = GetBoolOrNull(row, "CongenitalDiseases_CerebralPalsy"),
        CerebralPalsyText = GetString(row, "CongenitalDiseases_CerebralPalsyText"),
        CerebralPalsyComments = GetString(row, "CongenitalDiseases_CerebralPalsyComments"),
    };

    private static ChronicKidneyDiseaseSection MapCkd(IDictionary<string, object> row) => new()
    {
        Na = GetBoolOrNull(row, "CKD_NA"),
        Stage = GetIntOrNull(row, "CKD_Stage"),
        DueToDm = GetBoolOrNull(row, "CKD_DueToDM"),
        DueToOtherCondition = GetString(row, "CKD_DueToOtherCondition"),
        Controlled = GetBoolOrNull(row, "CKD_Controlled"),
        LowFatDiet = GetBoolOrNull(row, "CKD_LowFatDiet"),
        Dialysis = GetBoolOrNull(row, "CKD_Dialysis"),
        NoMeetDialysis = GetBoolOrNull(row, "CKD_NoMeetDialysis"),
        AdditionalTreatment = GetString(row, "CKD_AdditionalTreatment"),
        Hyperparathyroidism = GetBoolOrNull(row, "Hyperparathyroidism"),
        HyperparathyroidismTreatment = GetString(row, "HyperparathyroidismTreatment"),
        Gfr = GetString(row, "CKD_GFR"),
        SerumCalcium = GetString(row, "CKD_SerumCalcium"),
        SerumPth = GetString(row, "CKD_SerumPTH"),
        Nephropathy = GetBoolOrNull(row, "Nephropathy"),
        NephropathyType = GetString(row, "NephropathyType"),
        Nephritis = GetBoolOrNull(row, "Nephritis"),
        NephritisType = GetString(row, "NephritisType"),
        HasFistula = GetBoolOrNull(row, "HasFistula"),
        CkdBox = GetBoolOrNull(row, "CKDBox"),
        StressIncontinence = GetBoolOrNull(row, "CKD_StressIncontinence"),
        UrgeIncontinence = GetBoolOrNull(row, "CKD_UrgeIncontinence"),
        PostMicturitionDribble = GetBoolOrNull(row, "CKD_PostMicturitionDribble"),
        OveractiveBladder = GetBoolOrNull(row, "CKD_OveractiveBladder"),
        BladderTreatmentPlan = GetString(row, "CKD_BladderTreatmentPlan"),
        KidneyTransplant = GetBoolOrNull(row, "CKD_KidneyTransplant"),
        GfrDate = GetDateOrNull(row, "CKD_GFRDate"),
        GfrOrdered = GetBoolOrNull(row, "CKD_GFROrdered"),
    };

    private static PressureSoresSection MapPressureSores(IDictionary<string, object> row) => new()
    {
        Na = GetBoolOrNull(row, "PressureSores_NA"),
        ByPressure = GetBoolOrNull(row, "PressureSores_ByPressure"),
        Chronicle = GetBoolOrNull(row, "PressureSores_Chronicle"),
        AnatomicalSite = GetString(row, "PressureSores_AnatomicalSite"),
        HighBackPressureUlcerStage = GetIntOrNull(row, "PressureSores_HighBackPressureUlcerStage"),
        LowBackPressureUlcerStage = GetIntOrNull(row, "PressureSores_LowBackPressureUlcerStage"),
        HipPressureUlcerStageLeft = GetIntOrNull(row, "PressureSores_HipPressureUlcerStageLeft"),
        HipPressureUlcerStageRight = GetIntOrNull(row, "PressureSores_HipPressureUlcerStageRight"),
        HipPressureUlcerLeft = GetBoolOrNull(row, "PressureSores_HipPressureUlcerLeft"),
        HipPressureUlcerRight = GetBoolOrNull(row, "PressureSores_HipPressureUlcerRight"),
        HeelPressureUlcerStageLeft = GetIntOrNull(row, "PressureSores_HeelPressureUlcerStageLeft"),
        HeelPressureUlcerStageRight = GetIntOrNull(row, "PressureSores_HeelPressureUlcerStageRight"),
        HeelPressureUlcerLeft = GetBoolOrNull(row, "PressureSores_HeelPressureUlcerLeft"),
        HeelPressureUlcerRight = GetBoolOrNull(row, "PressureSores_HeelPressureUlcerRight"),
        OtherAreasStage = GetIntOrNull(row, "PressureSores_OtherAreasStage"),
        OtherAreas = GetString(row, "PressureSores_OtherAreas"),
        Hydrocolloid = GetBoolOrNull(row, "PressureSores_Hydrocolloid"),
        Healing = GetBoolOrNull(row, "PressureSores_Healing"),
        Healed = GetBoolOrNull(row, "PressureSores_Healed"),
        Worse = GetBoolOrNull(row, "PressureSores_Worse"),
        SilverDressing = GetBoolOrNull(row, "PressureSores_SilverDressing"),
        Hydrogel = GetBoolOrNull(row, "PressureSores_Hydrogel"),
        Antibiotic = GetBoolOrNull(row, "PressureSores_Antibiotic"),
        Alginate = GetBoolOrNull(row, "PressureSores_Alginate"),
        Enzyme = GetBoolOrNull(row, "PressureSores_Enzyme"),
        TransparentDressing = GetBoolOrNull(row, "PressureSores_TransparentDressing"),
        OthersTreatment = GetString(row, "PressureSores_OthersTreatment"),
    };

    private static RheumatoidArthritisSection MapRheumatoidArthritis(IDictionary<string, object> row) => new()
    {
        Na = GetBoolOrNull(row, "RA_NA"),
        RaNoManifestations = GetBoolOrNull(row, "RA_RANoManifestations"),
        RaWithPolyneuropathy = GetBoolOrNull(row, "RA_RAWithPolyneuropathy"),
        RaWithMyopathy = GetBoolOrNull(row, "RA_RAWithMyopathy"),
        RaOtherManifestations = GetString(row, "RA_RAOtherManifestations"),
        RaOtherManifestationsCheckBox = GetBoolOrNull(row, "RAOtherManifestationsCheckBox"),
        Dmards = GetBoolOrNull(row, "RA_DMARDs"),
        DmardsSpecify = GetString(row, "RA_DMARDsSpecify"),
        PtRefuses = GetBoolOrNull(row, "RA_PtRefuses"),
        OtherTreatmentConditions = GetString(row, "RA_OtherTreatmentConditions"),
        ArtritisPsoriatrica = GetBoolOrNull(row, "RheumatoidArthritis_ArtritisPsoriatrica"),
        Osteoartritis = GetBoolOrNull(row, "RheumatoidArthritis_Osteoartritis"),
        ArtritisPsoriatricaComment = GetString(row, "RheumatoidArthritis_ArtritisPsoriatricaComment"),
        OsteoartritisComment = GetString(row, "RheumatoidArthritis_OsteoartritisComment"),
        Arthritis = GetBoolOrNull(row, "Arthritis"),
        ArthritisLocationType = GetString(row, "ArthritisLocationType"),
        Nsaids = GetBoolOrNull(row, "NSAIDS"),
        NsaidsOtherTreatment = GetString(row, "NSAIDSOtherTreatment"),
        AffectedJoints = GetString(row, "AffectedJoints"),
        InflammatoryPolyarthritis = GetBoolOrNull(row, "InflammatoryPolyarthritis"),
        InflammatoryPolyarthritisComments = GetString(row, "InflammatoryPolyarthritisComments"),
        ArthropathySequelaViralInfection = GetBoolOrNull(row, "ArthropathySequelaViralInfection"),
        ArthropathySequelaViralInfectionComments = GetString(row, "ArthropathySequelaViralInfectionComments"),
        Osteopenia = GetBoolOrNull(row, "Osteopenia"),
        Osteoporosis = GetBoolOrNull(row, "Osteoporosis"),
        OsteoTreatmentPlan = GetString(row, "OsteoTreatmentPlan"),
    };

    private static DepressionInventorySection MapDepressionInventory(IDictionary<string, object> row) => new()
    {
        Choose1 = GetIntOrNull(row, "p3_nDepressionInvSadLevel"),
        Choose2 = GetIntOrNull(row, "p3_nDepressionInvLostInterestLevel"),
        Choose3 = GetIntOrNull(row, "p3_nDepressionInvLackEnergyLevel"),
        Choose4 = GetIntOrNull(row, "p3_nDepressionInvConfidentLevel"),
        Choose5 = GetIntOrNull(row, "p3_nDepressionInvGuiltLevel"),
        Choose6 = GetIntOrNull(row, "p3_nDepressionInvLifeInterestLevel"),
        Choose7 = GetIntOrNull(row, "p3_nDepressionInvConcentrationLevel"),
        Choose8a = GetIntOrNull(row, "p3_nDepressionInvRestlessLevel"),
        Choose8b = GetIntOrNull(row, "p3_nDepressionInvSubduedLevel"),
        Choose9 = GetIntOrNull(row, "p3_nDepressionInvSleepLevel"),
        Choose10a = GetIntOrNull(row, "p3_nDepressionInvReducedAppetiteLevel"),
        Choose10b = GetIntOrNull(row, "p3_nDepressionInvIncreaseAppetiteLevel"),
        IsMild = GetBoolOrNull(row, "p3_bDepressionInvIsMild"),
        IsSevere = GetBoolOrNull(row, "p3_bDepressionInvIsSevere"),
        IsMajor = GetBoolOrNull(row, "p3_bDepressionInvIsMajor"),
        IsModerate = GetBoolOrNull(row, "p3_bDepressionInvIsModerate"),
        PlanOfTreatment = GetString(row, "p3_sDepressionInvTreatment"),
    };

    private static DmeUseSection MapDmeUse(IDictionary<string, object> row) => new()
    {
        UsingOxygen = GetBoolOrNull(row, "DME_UsingOxygen"),
        DueToHypoxiaInAir = GetBoolOrNull(row, "DME_DuetoHypoxiaInAir"),
        Cpap = GetBoolOrNull(row, "DME_CPAP"),
        AboveKneeProsthesis = GetBoolOrNull(row, "DME_AboveKneeProsthesis"),
        BelowKneeProsthesis = GetBoolOrNull(row, "DME_BelowKneeProsthesis"),
        HasSuppliesNeeded = GetBoolOrNull(row, "DME_HasSuppliesNeeded"),
        Gastrostomy = GetBoolOrNull(row, "DME_Gastrostomy"),
        Colostomy = GetBoolOrNull(row, "DME_Colostomy"),
        Urostomy = GetBoolOrNull(row, "DME_Urostomy"),
        // See DmeUseSection.Tracheostomy: legacy checks DME_Tracheostomy for null but reads the
        // DME_Urostomy value -- reproduced as-is.
        Tracheostomy = GetRawValue(row, "DME_Tracheostomy") is not null ? GetBoolOrNull(row, "DME_Urostomy") : null,
        UsingWheelchair = GetBoolOrNull(row, "DME_UsingWheelchair"),
        UsingWheelchairReason = GetString(row, "DME_UsingWheelchairReason"),
        Comments = GetString(row, "DME_Comments"),
    };

    private static BmiAssociatedDiagnosesSection MapBmiAssociatedDiagnoses(IDictionary<string, object> row) => new()
    {
        Na = GetBoolOrNull(row, "NutritionNA"),
        Obesity = GetBoolOrNull(row, "Obesity"),
        MorbidObesity = GetBoolOrNull(row, "MorbidObesity"),
        Malnutrition = GetBoolOrNull(row, "Malnutrition"),
        EvaluationTreatmentPlan = GetString(row, "BMIPlanTreatment"),
        MalnutritionGradeTypeText = GetString(row, "MalnutritionGradeTypeText"),
        DeficiencyBComplex = GetBoolOrNull(row, "DeficiencyBComplex"),
        DeficiencyVitaminB12 = GetBoolOrNull(row, "DeficiencyVitaminB12"),
        DeficiencyVitaminB6 = GetBoolOrNull(row, "DeficiencyVitaminB6"),
        DeficiencyOtherVitaminNutrients = GetBoolOrNull(row, "DeficiencyOtherVitaminNutrients"),
        DeficiencyOtherVitaminNutrientsComments = GetString(row, "DeficiencyOtherVitaminNutrientsComments"),
        MalnutritionScreeningAssesment = GetString(row, "MalnutritionScreeningAssesment"),
    };

    private static MyocardialInfarctionSection MapMyocardialInfarction(IDictionary<string, object> row) => new()
    {
        OldMi = GetBoolOrNull(row, "OldMi"),
        BetaBlocker = GetBoolOrNull(row, "BetaBlocker"),
        BetaBlockerType = GetString(row, "BetaBlockerType"),
        OtherTreatmentCircumstances = GetString(row, "OldMIOtherTreatment"),
        Ami6Months = GetBoolOrNull(row, "MedicalHistory_AMI_6_Months"),
    };

    private static OtherCurrentConditionsAdditionalSection MapOtherCurrentConditionsAdditional(IDictionary<string, object> row) => new()
    {
        AdditionalRecomendation = GetString(row, "OtherCurrentConditionAdditionalRecomendation"),
    };

    // Legacy computes PHQ9.AllTotals as TotalCol1 + TotalCol2 + TotalCol3 purely for display --
    // not replicated here, same reasoning as the Screening Schedule summary strings (presentation
    // formatting derived entirely from data already exposed on TotalCol1-3).
    private static MajorDepressionSection MapMajorDepression(IDictionary<string, object> row) => new()
    {
        Na = GetBoolOrNull(row, "MajorDepressionNA"),
        IsMajorDepression = GetBoolOrNull(row, "MajorDepression"),
        InRemission = GetBoolOrNull(row, "MajorDepression_InRemission"),
        Recurrent = GetBoolOrNull(row, "MajorDepression_Recurrent"),
        MildSeverity = GetBoolOrNull(row, "MajorDepression_MildSeverity"),
        ModerateSeverity = GetBoolOrNull(row, "MajorDepression_ModerateSeverity"),
        SevereSeverity = GetBoolOrNull(row, "MajorDepression_SevereSeverity"),
        TreatmentPlan = GetString(row, "MajorDepression_TreatmentPlan"),
        UseOfSubtancesTreatmentPlan = GetString(row, "MajorDepression_UseOfSubtancesTreatmentPlan"),
        SeverWithoutPsychoticSymptoms = GetBoolOrNull(row, "MentalHealth_SeverWithoutPsychoticSymptoms"),
        SingleEpisode = GetBoolOrNull(row, "SingleEpisode"),
        PsychoticSymptoms = GetBoolOrNull(row, "PsychoticSymptoms"),
        BipolarDisorder = GetBoolOrNull(row, "BipolarDisorder"),
        BipolarDisorderTypeAndSeverity = GetString(row, "BipolarDisorderTypeAndSeverity"),
        BipolarDisorderTreatmentPlan = GetString(row, "BipolarDisorderTreatmentPlan"),
        Schizophrenia = GetBoolOrNull(row, "Schizophrenia"),
        SchizophreniaType = GetString(row, "SchizophreniaType"),
        SchizophreniaTreatmentPlan = GetString(row, "SchizophreniaTreatmentPlan"),
        MoodDisorder = GetBoolOrNull(row, "MoodDisorder"),
        MoodDisorderComments = GetString(row, "MoodDisorderComments"),
        Phq9DoneDate = GetDateOrNull(row, "PHQ9DoneDate"),
        Phq9ScoreResult = GetString(row, "PHQ9ScoreResult"),
        Dysthymia = GetBoolOrNull(row, "Dysthymia"),
        DysthymiaComments = GetString(row, "DysthymiaComments"),
        Phq9ReasonNotDoneOther = GetString(row, "PHQ9ReasonNotDoneOther"),
        Phq9ReasonNotDoneId = GetIntOrNull(row, "PHQ9ReasonNotDoneID"),
        GeneralizedAnxietyDisorder = GetBoolOrNull(row, "GeneralizedAnxietyDisorder"),
        OtherAnxiety = GetBoolOrNull(row, "OtherAnxiety"),
        OtherAnxietyText = GetString(row, "OtherAnxietyText"),
        GeneralizedAnxietyDisorderComments = GetString(row, "GeneralizedAnxietyDisorderComments"),
        Adhd = GetBoolOrNull(row, "ADHD"),
        AdhdComments = GetString(row, "ADHDComments"),
        Autism = GetBoolOrNull(row, "Autism"),
        AutismComments = GetString(row, "AutismComments"),
        SubstanceAbuseFreeText = GetString(row, "MentalHealth_SubstanceAbuseFreeText"),
        ScreeningSubstanceUseDatePerformed = GetDateOrNull(row, "MentalHealth_ScreeningSubstanceUseDatePerformed"),
        SubstanceAbuseCheckBox = GetBoolOrNull(row, "MentalHealth_SubstanceAbuseCheckBox"),
        Phq9 = new Phq9
        {
            Q1 = GetIntOrNull(row, "PHQ9_Q1"),
            Q2 = GetIntOrNull(row, "PHQ9_Q2"),
            Q3 = GetIntOrNull(row, "PHQ9_Q3"),
            Q4 = GetIntOrNull(row, "PHQ9_Q4"),
            Q5 = GetIntOrNull(row, "PHQ9_Q5"),
            Q6 = GetIntOrNull(row, "PHQ9_Q6"),
            Q7 = GetIntOrNull(row, "PHQ9_Q7"),
            Q8 = GetIntOrNull(row, "PHQ9_Q8"),
            Q9 = GetIntOrNull(row, "PHQ9_Q9"),
            TotalCol1 = GetIntOrNull(row, "PHQ9_TotalCol1"),
            TotalCol2 = GetIntOrNull(row, "PHQ9_TotalCol2"),
            TotalCol3 = GetIntOrNull(row, "PHQ9_TotalCol3"),
            NotDifficultAtAll = GetBoolOrNull(row, "PHQ9_NotDifficultAtAll"),
            SomewhatDifficult = GetBoolOrNull(row, "PHQ9_SomewhatDifficult"),
            VeryDifficult = GetBoolOrNull(row, "PHQ9_VeryDifficult"),
            ExtremelyDifficult = GetBoolOrNull(row, "PHQ9_ExtremeDifficult"),
            DepressedPastYear = GetBoolOrNull(row, "PHQ9_DepressedPastYear"),
            SuicidePastMonth = GetBoolOrNull(row, "PHQ9_SuicidePastMonth"),
            TriedSuicide = GetBoolOrNull(row, "PHQ9_TriedSuicide"),
        },
    };

    private static CardiovascularDiseasesSection MapCardiovascularDiseases(IDictionary<string, object> row) => new()
    {
        Na = GetBoolOrNull(row, "CardiovascularDiseasesNA"),
        ArterialHypertension = GetBoolOrNull(row, "ArterialHypertension"),
        PulmonaryHypertension = GetBoolOrNull(row, "PulmonaryHypertension"),
        PulmonaryHypertensionType = GetString(row, "PulmonaryHypertensionType"),
        HeartFailure = GetBoolOrNull(row, "HeartFailure"),
        Congestive = GetBoolOrNull(row, "Congestive"),
        Diastolic = GetBoolOrNull(row, "Diastolic"),
        Systolic = GetBoolOrNull(row, "Systolic"),
        Chronic = GetBoolOrNull(row, "Chronic"),
        Pvd = GetBoolOrNull(row, "PVD"),
        AtrilaFibrillation = GetBoolOrNull(row, "AtrilaFibrillation"),
        AtrilFibrillationType = GetString(row, "AtrilFibrillationType"),
        Arteriosclerosis = GetBoolOrNull(row, "Arteriosclerosis"),
        Aorta = GetBoolOrNull(row, "Aorta"),
        Crowns = GetBoolOrNull(row, "Crowns"),
        RenalArtery = GetBoolOrNull(row, "RenalArtery"),
        ArteriosclerosisExtremities = GetBoolOrNull(row, "ArteriosclerosisExtremities"),
        LegLt = GetBoolOrNull(row, "LegLT"),
        LegRt = GetBoolOrNull(row, "LegRT"),
        ArmLt = GetBoolOrNull(row, "ArmLT"),
        ArmRt = GetBoolOrNull(row, "ArmRT"),
        IntermittentClaudication = GetBoolOrNull(row, "IntermittentClaudication"),
        RestPain = GetBoolOrNull(row, "RestPain"),
        OtherComplications = GetBoolOrNull(row, "OtherComplications"),
        OtherComplicationsText = GetString(row, "OtherComplicationsText"),
        HypertensionTreatmentPlan = GetString(row, "HypertensionTreatmentPlan"),
        PvdTreatmentPlan = GetString(row, "PVDTreatmentPlan"),
        ArteriosclerosisTreatmentPlan = GetString(row, "ArteriosclerosisTreatmentPlan"),
        AnginaPectoris = GetBoolOrNull(row, "AnginaPectoris"),
        Sss = GetBoolOrNull(row, "SSS"),
        Svt = GetBoolOrNull(row, "SVT"),
        Pacemaker = GetBoolOrNull(row, "Pacemaker"),
        Cad = GetBoolOrNull(row, "CAD"),
        Cardiomiopatia = GetBoolOrNull(row, "Cardiomiopatia"),
        MyocardialInfarction = GetBoolOrNull(row, "Cardiovascular_MyocardialInfarction"),
        Cardiomegaly = GetBoolOrNull(row, "Cardiovascular_Cardiomegaly"),
        AtrioventricularBlock = GetBoolOrNull(row, "Cardiovascular_AtrioventricularBlock"),
        AtrioventricularBlockDegree = GetString(row, "Cardiovascular_AtrioventricularBlockDegree"),
        VaricoseVeinsOfLowerExtremityWithPain = GetBoolOrNull(row, "Cardiovascular_VaricoseVeinsOfLowerExtremityWithPain"),
        ConductionDisorder = GetBoolOrNull(row, "Cardiovascular_ConductionDisorder"),
        MyocardialInfarctionTreatmentPlan = GetString(row, "Cardiovascular_MyocardialInfarctionTreatmentPlan"),
        OldMyocardialInfarction = GetBoolOrNull(row, "OldMyocardialInfarction"),
        Hyperlipidemia = GetBoolOrNull(row, "Hyperlipidemia"),
        HyperlipidemiaText = GetString(row, "HyperlipidemiaText"),
        HeartTransplant = GetBoolOrNull(row, "HeartTransplant"),
    };

    private static PulmonaryDiseasesSection MapPulmonaryDiseases(IDictionary<string, object> row) => new()
    {
        Na = GetBoolOrNull(row, "PulmonaryDiseases_NA"),
        Asthma = GetBoolOrNull(row, "Asthma"),
        AsthmaComments = GetString(row, "AsthmaComments"),
        AsthmaDescription = GetString(row, "AsthmaDescription"),
        AcuteBronchitis = GetBoolOrNull(row, "AcuteBronchitis"),
        AcuteBronchitisComments = GetString(row, "AcuteBronchitisComments"),
        ChronicBronchitis = GetBoolOrNull(row, "ChronicBronchitis"),
        ChronicBronchitisComments = GetString(row, "ChronicBronchitisComments"),
        Copd = GetBoolOrNull(row, "COPD"),
        CopdComments = GetString(row, "COPDComments"),
        PulmonaryFibrosis = GetBoolOrNull(row, "PulmonaryFibrosis"),
        PulmonaryFibrosisComments = GetString(row, "PulmonaryFibrosisComments"),
        OtherConditionCheckbox = GetBoolOrNull(row, "PulmonaryDiseases_OtherCondition_Checkbox"),
        OtherCondition = GetString(row, "PulmonaryDiseases_OtherCondition"),
        OtherConditionTreatment = GetString(row, "PulmonaryDiseases_OtherConditionTreatment"),
        UpperRespiratoryTractInfection = GetBoolOrNull(row, "UpperRespiratoryTractInfection"),
        AcuteLaryngopharyngitis = GetBoolOrNull(row, "AcuteLaryngopharyngitis"),
        AcuteNasopharyngitis = GetBoolOrNull(row, "AcuteNasopharyngitis"),
        UpperRespiratoryTractInfectionComments = GetString(row, "UpperRespiratoryTractInfectionComments"),
        AcuteLaryngopharyngitisComments = GetString(row, "AcuteLaryngopharyngitisComments"),
        AcuteNasopharyngitisComments = GetString(row, "AcuteNasopharyngitisComments"),
        LungTransplant = GetBoolOrNull(row, "LungTransplant"),
        LungTransplantTreatmentPlan = GetString(row, "LungTransplantTreatmentPlan"),
    };

    // Superset of the older aha.Gastrointestinal object legacy also populates from the same
    // NA/NASH/MetabolicSyndrome/Hyperkalemia/Hypokalemia/TreatmentPlan columns -- see the comment
    // on AhaClaimSnapshot.GastrointestinalDiseases.
    private static GastrointestinalDiseasesSection MapGastrointestinalDiseases(IDictionary<string, object> row) => new()
    {
        Na = GetBoolOrNull(row, "Gastrointestinal_NA"),
        NonalcoholicSteatohepatitis = GetBoolOrNull(row, "Gastrointestinal_NASH"),
        MetabolicSyndrome = GetBoolOrNull(row, "Gastrointestinal_MetabolicSyndrome"),
        Hyperkalemia = GetBoolOrNull(row, "Gastrointestinal_Hyperkalemia"),
        Hypokalemia = GetBoolOrNull(row, "Gastrointestinal_Hypokalemia"),
        GastrointestinalTreatmentPlan = GetString(row, "Gastrointestinal_TreatmentPlan"),
        LiverTransplant = GetBoolOrNull(row, "LiverTransplant"),
        Gerd = GetBoolOrNull(row, "GERD"),
        ChronicHepatitis = GetBoolOrNull(row, "ChronicHepatitis"),
        DiverticularDisease = GetBoolOrNull(row, "DiverticularDisease"),
        PepticUlcerDisease = GetBoolOrNull(row, "PepticUlcerDisease"),
    };

    // Superset of the older aha.Musculoskeletal object legacy also populates from the exact same
    // columns -- see the comment on AhaClaimSnapshot.MusculoskeletalGhp.
    private static MusculoskeletalGhpSection MapMusculoskeletalGhp(IDictionary<string, object> row) => new()
    {
        Na = GetBoolOrNull(row, "Musculoskeletal_NA"),
        Spondylosis = GetBoolOrNull(row, "Musculoskeletal_Spondylosis"),
        CervicalDiscDisorder = GetBoolOrNull(row, "Musculoskeletal_CervicalDiscDisorder"),
        CervicothoracicRadiculopathy = GetBoolOrNull(row, "Musculoskeletal_CervicothoracicRadiculopathy"),
        CervicalRegion = GetBoolOrNull(row, "Musculoskeletal_CervicalRegion"),
        CervicothoracicRegion = GetBoolOrNull(row, "Musculoskeletal_CervicothoracicRegion"),
        MusculoskeletalTreatmentPlan = GetString(row, "Musculoskeletal_TreatmentPlan"),
    };

    private static ImLabRefSection MapImLabRef(IDictionary<string, object> row) => new()
    {
        ImmunizationsNa = GetBoolOrNull(row, "Immunizations_NA"),
        ImmunizationsParentRefuses = GetBoolOrNull(row, "Immunizations_ParentRefuses"),
        ImmunizationsHepB1Dose = GetBoolOrNull(row, "Immunizations_HepB1dose"),
        ImmunizationsHebB2Dose = GetBoolOrNull(row, "Immunizations_HebB2dose"),
        ImmunizationsHebB3Dose = GetBoolOrNull(row, "Immunizations_HebB3dose"),
        ImmunizationsHepA1Dose = GetBoolOrNull(row, "Immunizations_HepA1dose"),
        ImmunizationsHebA2Dose = GetBoolOrNull(row, "Immunizations_HebA2dose"),
        ImmunizationsDTaP1Dose = GetBoolOrNull(row, "Immunizations_DTaP1dose"),
        ImmunizationsDTaP2Dose = GetBoolOrNull(row, "Immunizations_DTaP2dose"),
        ImmunizationsDTaP3Dose = GetBoolOrNull(row, "Immunizations_DTaP3dose"),
        ImmunizationsDTaP4Dose = GetBoolOrNull(row, "Immunizations_DTaP4dose"),
        ImmunizationsDTaP5Dose = GetBoolOrNull(row, "Immunizations_DTaP5dose"),
        ImmunizationsHib1Dose = GetBoolOrNull(row, "Immunizations_Hib1dose"),
        ImmunizationsHib2Dose = GetBoolOrNull(row, "Immunizations_Hib2dose"),
        ImmunizationsHib3Dose = GetBoolOrNull(row, "Immunizations_Hib3dose"),
        ImmunizationsHib4Dose = GetBoolOrNull(row, "Immunizations_Hib4dose"),
        ImmunizationsPcv13_1Dose = GetBoolOrNull(row, "Immunizations_PCV13_1dose"),
        ImmunizationsPcv13_2Dose = GetBoolOrNull(row, "Immunizations_PCV13_2dose"),
        ImmunizationsPcv13_3Dose = GetBoolOrNull(row, "Immunizations_PCV13_3dose"),
        ImmunizationsPcv13_4Dose = GetBoolOrNull(row, "Immunizations_PCV13_4dose"),
        ImmunizationsIpv1Dose = GetBoolOrNull(row, "Immunizations_IPV1dose"),
        ImmunizationsIpv2Dose = GetBoolOrNull(row, "Immunizations_IPV2dose"),
        ImmunizationsIpv3Dose = GetBoolOrNull(row, "Immunizations_IPV3dose"),
        ImmunizationsIpv4Dose = GetBoolOrNull(row, "Immunizations_IPV4dose"),
        ImmunizationsMmr1Dose = GetBoolOrNull(row, "Immunizations_MMR1dose"),
        ImmunizationsMmr2Dose = GetBoolOrNull(row, "Immunizations_MMR2dose"),
        ImmunizationsVaricella1Dose = GetBoolOrNull(row, "Immunizations_Varicella1dose"),
        ImmunizationsVaricella2Dose = GetBoolOrNull(row, "Immunizations_Varicella2dose"),
        ImmunizationsTdap = GetBoolOrNull(row, "Immunizations_Tdap"),
        ImmunizationsRotavirus1Dose = GetBoolOrNull(row, "Immunizations_Rotavirus1dose"),
        ImmunizationsRotavirus2Dose = GetBoolOrNull(row, "Immunizations_Rotavirus2dose"),
        ImmunizationsInfluenza = GetBoolOrNull(row, "Immunizations_Influenza"),
        ImmunizationsMenningococcalMcv = GetBoolOrNull(row, "Immunizations_MenningococcalMCV"),
        ImmunizationsHpv1Dose = GetBoolOrNull(row, "Immunizations_HPV1dose"),
        ImmunizationsHpv2Dose = GetBoolOrNull(row, "Immunizations_HPV2dose"),
        ImmunizationsHpv3Dose = GetBoolOrNull(row, "Immunizations_HPV3dose"),
        ImmunizationsOthers = GetString(row, "Immunizations_Others"),

        LabNa = GetBoolOrNull(row, "Lab_NA"),
        LabHgbHct = GetBoolOrNull(row, "Lab_HgbHct"),
        LabTb = GetBoolOrNull(row, "Lab_TB"),
        LabUa = GetBoolOrNull(row, "Lab_UA"),
        LabLipidProfile = GetBoolOrNull(row, "Lab_LipidProfile"),
        LabBloodLeadTest = GetBoolOrNull(row, "Lab_BloodLeadTest"),
        LabVih = GetBoolOrNull(row, "Lab_VIH"),
        LabNaat = GetBoolOrNull(row, "Lab_NAAT"),
        LabVdrl = GetBoolOrNull(row, "Lab_VDRL"),
        LabOther = GetString(row, "Lab_Other"),

        LabHgbHctResult = GetString(row, "Lab_HgbHct_Result"),
        LabTbResult = GetString(row, "Lab_TB_Result"),
        LabUaResult = GetString(row, "Lab_UA_Result"),
        LabLipidProfileResult = GetString(row, "Lab_LipidProfile_Result"),
        LabBloodLeadTestResult = GetString(row, "Lab_BloodLeadTest_Result"),
        LabVihResult = GetString(row, "Lab_VIH_Result"),
        LabNaatResult = GetString(row, "Lab_NAAT_Result"),
        LabVdrlResult = GetString(row, "Lab_VDRL_Result"),
        LabOtherResult = GetString(row, "Lab_Other_Result"),
        LabHgbHctOrdered = GetBoolOrNull(row, "Lab_HgbHct_Ordered"),
        LabTbOrdered = GetBoolOrNull(row, "Lab_TB_Ordered"),
        LabUaOrdered = GetBoolOrNull(row, "Lab_UA_Ordered"),
        LabLipidProfileOrdered = GetBoolOrNull(row, "Lab_LipidProfile_Ordered"),
        LabBloodLeadTestOrdered = GetBoolOrNull(row, "Lab_BloodLeadTest_Ordered"),
        LabVihOrdered = GetBoolOrNull(row, "Lab_VIH_Ordered"),
        LabNaatOrdered = GetBoolOrNull(row, "Lab_NAAT_Ordered"),
        LabVdrlOrdered = GetBoolOrNull(row, "Lab_VDRL_Ordered"),
        LabOtherOrdered = GetBoolOrNull(row, "Lab_Other_Ordered"),

        VisionText = GetString(row, "VisionText"),
        HearingText = GetString(row, "HearingText"),

        ReferralsNa = GetBoolOrNull(row, "Referrals_NA"),
        ReferralsWic = GetBoolOrNull(row, "Referrals_WIC"),
        ReferralsPhysicalTherapy = GetBoolOrNull(row, "Referrals_PhysicalTherapy"),
        ReferralsOccupationTherapy = GetBoolOrNull(row, "Referrals_OccupationTherapy"),
        ReferralsSpeechTherapy = GetBoolOrNull(row, "Referrals_SpeechTherapy"),
        ReferralsAudiology = GetBoolOrNull(row, "Referrals_Audiology"),
        ReferralsDental = GetBoolOrNull(row, "Referrals_Dental"),
        ReferralsBehavioralHealth = GetBoolOrNull(row, "Referrals_BehavioralHealth"),
        ReferralsEarlyIntervention = GetBoolOrNull(row, "Referrals_EarlyIntervention"),
        ReferralsMentalHealthSpecialist = GetBoolOrNull(row, "Referrals_MentalHealthSpecialist"),
        ReferralsNutritionist = GetBoolOrNull(row, "Referrals_Nutritionist"),
        ReferralsOptometrist = GetBoolOrNull(row, "Referrals_Optometrist"),
        ReferralsOphthalmology = GetBoolOrNull(row, "Referrals_Ophthalmology"),
        ReferralsOtherSpecialtyText = GetString(row, "Referrals_OtherSpecialtyText"),

        ImmunoOthersCheckbox = GetBoolOrNull(row, "Immuno_Others_Checkbox"),
        LabsOthersCheckbox = GetBoolOrNull(row, "Labs_Others_Checkbox"),
        ReferalsOthersCheckbox = GetBoolOrNull(row, "Referals_Others_Checkbox"),
    };

    private static EyesAndNeurologySection MapEyesAndNeurology(IDictionary<string, object> row) => new()
    {
        Na = GetBoolOrNull(row, "EyesAndNeurologyNA"),
        Retinopathy = GetBoolOrNull(row, "Retinopathy"),
        Proliferative = GetBoolOrNull(row, "Proliferative"),
        ProliferativeEyeRt = GetBoolOrNull(row, "ProliferativeEyeRT"),
        ProliferativeEyeLt = GetBoolOrNull(row, "ProliferativeEyeLT"),
        MacularEdema = GetBoolOrNull(row, "MacularEdema"),
        MacularEdemaEyeRt = GetBoolOrNull(row, "MacularEdemaEyeRT"),
        MacularEdemaEyeLt = GetBoolOrNull(row, "MacularEdemaEyeLT"),
        OtherComplicationRetinopathy = GetString(row, "OtherComplicationRetinopathy"),
        Glaucoma = GetBoolOrNull(row, "Glaucoma"),
        GlaucomaEyeRt = GetBoolOrNull(row, "GlaucomaEyeRT"),
        GlaucomaEyeLt = GetBoolOrNull(row, "GlaucomaEyeLT"),
        GlaucomaType = GetString(row, "GlaucomaType"),
        Cataract = GetBoolOrNull(row, "Cataract"),
        CataractRt = GetBoolOrNull(row, "CataractRT"),
        CataractLt = GetBoolOrNull(row, "CataractLT"),
        CataractType = GetString(row, "CataractType"),
        Epilepsy = GetBoolOrNull(row, "Epilepsy"),
        EpilepsyType = GetString(row, "EpilepsyType"),
        Seizures = GetBoolOrNull(row, "Seizures"),
        SeizuresCause = GetString(row, "SeizuresCause"),
        Polyneuropathy = GetBoolOrNull(row, "Polyneuropathy"),
        PolyneuropathyDueTo = GetString(row, "PolyneuropathyDueTo"),
        PolyneuropathyDueToCkb = GetBoolOrNull(row, "PolyneuropathyDueToCkb"),
        Neuropathy = GetBoolOrNull(row, "Neuropathy"),
        AutonomicNeuropathy = GetBoolOrNull(row, "AutonomicNeuropathy"),
        Mononeuritis = GetBoolOrNull(row, "Mononeuritis"),
        Neuralgia = GetBoolOrNull(row, "Neuralgia"),
        PolyneuropathyOtherSpecification = GetString(row, "PolyneuropathyOtherSpecification"),
        RetinopathyTreatmentPlan = GetString(row, "RetinopathyTreatmentPlan"),
        GlaucomaTreatmentPlan = GetString(row, "GlaucomaTreatmentPlan"),
        CataractTreatmentPlan = GetString(row, "CataractTreatmentPlan"),
        EpilepsyTreatmentPlan = GetString(row, "EpilepsyTreatmentPlan"),
        PolyneuropathyTreatmentPlan = GetString(row, "PolyneuropathyTreatmentPlan"),
        RetinopathyEyeRt = GetBoolOrNull(row, "RetinopathyEyeRT"),
        RetinopathyEyeLt = GetBoolOrNull(row, "RetinopathyEyeLT"),
        RetinopathySeverity = GetIntOrNull(row, "RetinopathySeverity"),
        ProliferativeSeverity = GetIntOrNull(row, "ProliferativeSeverity"),
        ProliferativeTreatmentPlan = GetString(row, "ProliferativeTreatmentPlan"),
        AlzheimerDisease = GetBoolOrNull(row, "AlzheimerDisease"),
        Dementia = GetBoolOrNull(row, "Dementia"),
        DementiaSeverity = GetIntOrNull(row, "DementiaSeverity"),
        DementiaAlzheimerTreatmentPlan = GetString(row, "DementiaAlzheimerTreatmentPlan"),
    };

    // Tables(1) holds two disjoint sets of rows, distinguished by legacy via a DataTable.Select
    // filter rather than a discriminator column: rows with Controlled null and Remission/Active
    // both non-null are cancer diagnoses; rows with Controlled non-null and Remission/Active both
    // null are other current conditions. Rows matching neither predicate are skipped, same as
    // legacy (they simply don't appear in either filtered row set).
    private static (List<CancerDiagnosisItem> CancerDiagnosis, List<OtherConditionItem> OtherCurrentConditions) MapCancerDiagnosisAndOtherConditions(IReadOnlyList<dynamic> rows)
    {
        var cancerDiagnosis = new List<CancerDiagnosisItem>();
        var otherCurrentConditions = new List<OtherConditionItem>();

        foreach (var dynRow in rows)
        {
            IDictionary<string, object> row = dynRow;
            var controlled = GetRawValue(row, "Controlled");
            var remission = GetRawValue(row, "Remission");
            var active = GetRawValue(row, "Active");

            if (controlled is null && remission is not null && active is not null)
            {
                cancerDiagnosis.Add(new CancerDiagnosisItem
                {
                    Primary = GetBoolOrNull(row, "Primary"),
                    Secondary = GetBoolOrNull(row, "Secondary"),
                    Active = GetBoolOrNull(row, "Active"),
                    History = GetBoolOrNull(row, "History"),
                    Remission = GetBoolOrNull(row, "Remission"),
                    Diagnoses = GetString(row, "DxText"),
                    Treatment = GetString(row, "DxReason"),
                    CurrentlyInChemotherapy = GetBoolOrNull(row, "CurrentlyInChemotherapy"),
                    CurrentlyInRadiotherapy = GetBoolOrNull(row, "CurrentlyInRadiotherapy"),
                    CurrentlyInImmunotherapy = GetBoolOrNull(row, "CurrentlyInImmunotherapy"),
                    CurrentlyRefusesTreatment = GetBoolOrNull(row, "CurrentlyRefusesTreatment"),
                });
            }
            else if (controlled is not null && remission is null && active is null)
            {
                otherCurrentConditions.Add(new OtherConditionItem
                {
                    Controlled = GetBoolOrNull(row, "Controlled"),
                    Diagnoses = GetString(row, "DxText"),
                    Treatment = GetString(row, "DxReason"),
                    DiagnosesCode = GetString(row, "dxCode"),
                });
            }
        }

        return (cancerDiagnosis, otherCurrentConditions);
    }

    private static List<PressureSoreListItem> MapPressureSoresList(IReadOnlyList<dynamic> rows) => rows
        .Select(dynRow =>
        {
            IDictionary<string, object> row = dynRow;
            return new PressureSoreListItem
            {
                ByVaricoseVainsInLegs = GetBoolOrNull(row, "ByVaricoseVainsInLegs"),
                ByArteriosclerosisInExtremities = GetBoolOrNull(row, "ByArteriosclerosisInExtremities"),
                ByDiabetic = GetBoolOrNull(row, "ByDiabetic"),
                ByPressure = GetBoolOrNull(row, "ByPressure"),
                ByPressureStage = GetIntOrNull(row, "ByPressureStage"),
                AnatomicalSite = GetString(row, "AnatomicalSite"),
                AnatomicalSiteOther = GetString(row, "AnatomicalSiteOther"),
                ByOtherCondition = GetBoolOrNull(row, "ByOtherCondition"),
                OtherConditionText = GetString(row, "OtherConditionText"),
                Treatment = GetString(row, "Treatment"),
            };
        })
        .ToList();

    // Legacy explicitly defaults every boolean field here to False (and UlcerDueToOtherCauseText
    // to empty string) when the column is null, rather than leaving the field unset the way the
    // rest of GetAHA does -- reproduced with "?? false"/"?? string.Empty" rather than plain nulls.
    private static List<DiseasesOfTheSkinItem> MapDiseasesOfTheSkin(IReadOnlyList<dynamic> rows) => rows
        .Select(dynRow =>
        {
            IDictionary<string, object> row = dynRow;
            return new DiseasesOfTheSkinItem
            {
                Dermatitis = GetBoolOrNull(row, "Dermatitis") ?? false,
                DermatitisTreatment = GetString(row, "DermatitisTreatment"),
                DermatitisTypeLocation = GetString(row, "DermatitisTypeLocation"),
                Psoriasis = GetBoolOrNull(row, "Psoriasis") ?? false,
                PsoriasisType = GetString(row, "PsoriasisType"),
                PsoriasicArthritis = GetBoolOrNull(row, "PsoriasicArthritis") ?? false,
                PsoriasicArthritisLocation = GetString(row, "PsoriasicArthritisLocation"),
                PsoriasisTreatment = GetString(row, "PsoriasisTreatment"),
                Ulcer = GetBoolOrNull(row, "Ulcer") ?? false,
                UlcerLocationAndDepth = GetString(row, "UlcerLocationAndDepth"),
                UlcerDepth = GetString(row, "UlcerDepth"),
                UlcerLocation = GetString(row, "UlcerLocation"),
                DueToArteriosclerosisInExtremities = GetBoolOrNull(row, "DueToArteriosclerosisInExtremities") ?? false,
                DueToPvd = GetBoolOrNull(row, "DueToPVD") ?? false,
                UlcerTreatment = GetString(row, "UlcerTreatment"),
                PressureUlcer = GetBoolOrNull(row, "PressureUlcer") ?? false,
                PressureUlcerStage = GetIntOrNull(row, "PressureUlcerStage"),
                PressureUlcerNoStage = GetBoolOrNull(row, "PressureUlcerNoStage") ?? false,
                PressureUlcerLocation = GetString(row, "PressureUlcerLocation"),
                PressureUlcerTreatment = GetString(row, "PressureUlcerTreatment"),
                PressureUlcerOtherCause = GetBoolOrNull(row, "PressureUlcerOtherCause") ?? false,
                PressureUlcerOtherCauseText = GetString(row, "PressureUlcerOtherCauseText"),
                UlcerDueToDiabetes = GetBoolOrNull(row, "UlcerDueToDiabetes") ?? false,
                UlcerDueToVaricoseVeins = GetBoolOrNull(row, "UlcerDueToVaricoseVeins") ?? false,
                UlcerDueToVaricoseVeinsWithInflamation = GetBoolOrNull(row, "UlcerDueToVaricoseVeinsWithInflamation") ?? false,
                UlcerDueToIdiopathicVenousHypertension = GetBoolOrNull(row, "UlcerDueToIdiopathicVenousHypertension") ?? false,
                UlcerDueToIdiopathicVenousHypertensionWithInflamation = GetBoolOrNull(row, "UlcerDueToIdiopathicVenousHypertensionWithInflamation") ?? false,
                UlcerDueToOtherCause = GetBoolOrNull(row, "UlcerDueToOtherCause") ?? false,
                UlcerDueToOtherCauseText = GetString(row, "UlcerDueToOtherCauseText") ?? string.Empty,
                UlcerDueToPvdWithInflamation = GetBoolOrNull(row, "UlcerDueToPVDWithInflamation") ?? false,
            };
        })
        .ToList();

    // Legacy defaults ReasonForNo to -1 when the column is null, rather than leaving it unset.
    private static List<DxHistorySelectionItem> MapDxHistorySelectionList(IReadOnlyList<dynamic> rows, long claimId) => rows
        .Select(dynRow =>
        {
            IDictionary<string, object> row = dynRow;
            return new DxHistorySelectionItem
            {
                Id = GetLongOrNull(row, "Claims_AHADxHxSelectionID"),
                ClaimId = claimId,
                DxCode = GetString(row, "DxCode"),
                DxDescription = GetString(row, "DxDescription"),
                ProviderName = GetString(row, "ProviderName"),
                Source = GetString(row, "Source"),
                SelectionIndex = GetIntOrNull(row, "SelectionIndex") ?? 0,
                ReasonForNo = (short)(GetIntOrNull(row, "ReasonForNo") ?? -1),
            };
        })
        .ToList();

    private static List<SuspiciousConditionSelectionItem> MapSuspiciousDxHxSelectionList(IReadOnlyList<dynamic> rows, long claimId) => rows
        .Select(dynRow =>
        {
            IDictionary<string, object> row = dynRow;
            return new SuspiciousConditionSelectionItem
            {
                ClaimId = claimId,
                Detail = GetString(row, "Detail"),
                DxCode = GetString(row, "DxCode"),
                Condition = GetString(row, "Condition"),
                SelectionIndex = GetIntOrNull(row, "SelectionIndex") ?? 0,
                Hcc = GetString(row, "HCC"),
                Source = GetString(row, "Source"),
            };
        })
        .ToList();

    // Tables(10), a single row -- like Tables(0), but Result is sourced from Tables(0) instead
    // (headerRow here). Legacy explicitly defaults every boolean field to False when the column is
    // null, same quirk as DiseasesOfTheSkin above.
    private static MalnutritionCriteriaSection? MapMalnutritionCriteria(IReadOnlyList<dynamic> malnutritionRows, IDictionary<string, object> headerRow)
    {
        if (malnutritionRows.Count == 0)
        {
            return null;
        }

        IDictionary<string, object> row = malnutritionRows[0];
        return new MalnutritionCriteriaSection
        {
            InvoluntaryWeightLoss = GetBoolOrNull(row, "involuntary_weight_loss") ?? false,
            InvoluntaryWeightLossMore10At6MonthAndMore20Over6Month = GetBoolOrNull(row, "involuntary_weight_lossmore10at6monthandmore20over6month") ?? false,
            InvoluntaryWeightLoss10To5At6MonthAnd20To10Over6Month = GetBoolOrNull(row, "involuntary_weight_loss10to5at6monthand20to10over6month") ?? false,
            InvoluntaryWeightLossLess5At6MonthAndLess10Over6Month = GetBoolOrNull(row, "involuntary_weight_lossless5at6monthandless10over6month") ?? false,
            LowBmi = GetBoolOrNull(row, "Low_bmi") ?? false,
            LowBmiLess18 = GetBoolOrNull(row, "low_bmiless18") ?? false,
            LowBmiLess20 = GetBoolOrNull(row, "low_bmiless20") ?? false,
            ReducedMuscle = GetBoolOrNull(row, "reduced_muscle") ?? false,
            ReducedMuscleSeverly = GetBoolOrNull(row, "reduced_muscle_severly") ?? false,
            ReducedMuscleMild = GetBoolOrNull(row, "reduced_muscle_mild") ?? false,
            ReducedFoodIntake = GetBoolOrNull(row, "reduced_food_intake") ?? false,
            DiseaseBurden = GetBoolOrNull(row, "disease_burden") ?? false,
            OtherCriteria = GetBoolOrNull(row, "other_criteria") ?? false,
            OtherCriteriaDescription = GetString(row, "other_criteria_description") ?? string.Empty,
            Albumin = GetBoolOrNull(row, "albumin") ?? false,
            Less2Albumin = GetBoolOrNull(row, "less2albumin") ?? false,
            Less25Albumin = GetBoolOrNull(row, "less25albumin") ?? false,
            Less35Albumin = GetBoolOrNull(row, "less35albumin") ?? false,
            Result = GetString(headerRow, "Malnutrition_Criteria_Result"),
        };
    }

    // CriteriaId and Total are modeled as bool? (matching the already-verified Save-side DTO), even
    // though legacy reads both via CInt here -- either the underlying columns are genuinely bit
    // columns and legacy's CInt is just redundant, or there's a real int/bool mismatch that
    // couldn't be confirmed without the uspGetAHA2 definition. Convert.ToBoolean accepts any
    // integer (0 = false, nonzero = true) so this doesn't throw either way.
    private static List<ScreeningSubstanceUseItem> MapScreeningSubstanceUseList(IReadOnlyList<dynamic> rows) => rows
        .Select(dynRow =>
        {
            IDictionary<string, object> row = dynRow;
            return new ScreeningSubstanceUseItem
            {
                CriteriaId = GetBoolOrNull(row, "criteria_id"),
                Other = GetString(row, "screening_substance_use_other"),
                Q1 = GetBoolOrNull(row, "screening_substance_use_q1"),
                Q2 = GetBoolOrNull(row, "screening_substance_use_q2"),
                Q3 = GetBoolOrNull(row, "screening_substance_use_q3"),
                Q4 = GetBoolOrNull(row, "screening_substance_use_q4"),
                Q5 = GetBoolOrNull(row, "screening_substance_use_q5"),
                Q6 = GetBoolOrNull(row, "screening_substance_use_q6"),
                Q7 = GetBoolOrNull(row, "screening_substance_use_q7"),
                Q8 = GetBoolOrNull(row, "screening_substance_use_q8"),
                Q9 = GetBoolOrNull(row, "screening_substance_use_q9"),
                Q10 = GetBoolOrNull(row, "screening_substance_use_q10"),
                Q11 = GetBoolOrNull(row, "screening_substance_use_q11"),
                Q12 = GetBoolOrNull(row, "screening_substance_use_q12"),
                Q13 = GetBoolOrNull(row, "screening_substance_use_q13"),
                Total = GetBoolOrNull(row, "screening_substance_use_total"),
            };
        })
        .ToList();

    // Tables(12), a single row. Legacy picks the 2020 shape for visits before 2023, and also for
    // visits exactly in 2023 with ClaimClassTag = 4 (a carve-out) -- everything else 2023+ gets the
    // newer shape. Both Result fields are sourced from Tables(0), not Tables(12). If DateOfVisit is
    // somehow null, legacy would fail dereferencing it (Value.Value on a null Nullable) -- falls
    // through to the 2023 shape here instead, since that's the newer/current default.
    private static (SocialDeterminants2020Section? SocialDeterminants2020, SocialDeterminants2023Section? SocialDeterminants2023) MapSocialDeterminants(
        IReadOnlyList<dynamic> socialDeterminantsRows, IDictionary<string, object> headerRow, AhaFormHeader header)
    {
        if (socialDeterminantsRows.Count == 0)
        {
            return (null, null);
        }

        IDictionary<string, object> row = socialDeterminantsRows[0];
        var result = GetString(headerRow, "Social_Determinants_Result");
        var use2020Shape = header.DateOfVisit is { Year: < 2023 } || (header.DateOfVisit is { Year: 2023 } && header.ClaimClassTag == 4);

        if (use2020Shape)
        {
            return (new SocialDeterminants2020Section
            {
                ProblemsLivingAlone = GetBoolOrNull(row, "problems_living_alone"),
                Illiteracy = GetBoolOrNull(row, "illiteracy"),
                Homelessness = GetBoolOrNull(row, "homelessness"),
                InadequateHome = GetBoolOrNull(row, "inadequate_home"),
                DiscordWithNll = GetBoolOrNull(row, "discord_with_nll"),
                ProblemsResidentialInstitution = GetBoolOrNull(row, "problems_residential_institution"),
                LackOfFoodAndWater = GetBoolOrNull(row, "lack_of_food_and_water"),
                ExtremePoverty = GetBoolOrNull(row, "extreme_poverty"),
                WorriedAboutLosingHousing = GetBoolOrNull(row, "worried_about_losing_housing"),
                NotAbleToPayRx = GetBoolOrNull(row, "not_able_to_pay_rx"),
                NotAbleToPayUtilities = GetBoolOrNull(row, "not_able_to_pay_utilities"),
                NotAbleToPayMedicalCare = GetBoolOrNull(row, "not_able_to_pay_medical_care"),
                NotAbleToPayPhone = GetBoolOrNull(row, "not_able_to_pay_phone"),
                NotAbleToPayTransportation = GetBoolOrNull(row, "not_able_to_pay_transportation"),
                NotAbleToPayClothing = GetBoolOrNull(row, "not_able_to_pay_clothing"),
                ProblemsInRelationship = GetBoolOrNull(row, "problems_in_relationship"),
                AbsenceFamilyMemberMilitary = GetBoolOrNull(row, "absence_family_member_military"),
                OtherAbsenceFamilyMember = GetBoolOrNull(row, "other_absence_family_member"),
                DisappearanceFamilyMember = GetBoolOrNull(row, "disappearance_family_member"),
                DisruptionSeparation = GetBoolOrNull(row, "disruption_separation"),
                DependentAtHome = GetBoolOrNull(row, "dependent_at_home"),
                AlcoholismDrugAddictionFamily = GetBoolOrNull(row, "alcoholism_drug_addiction_family"),
                InnapropriateDiet = GetBoolOrNull(row, "innapropriate_diet"),
                OtherReducedMobility = GetBoolOrNull(row, "other_reduced_mobility"),
                NeedPersonalCare = GetBoolOrNull(row, "need_personal_care"),
                NeedAtHome = GetBoolOrNull(row, "need_at_home"),
                NeedContinuousSupervision = GetBoolOrNull(row, "need_continuous_supervision"),
                OtherProblemsProviderDependency = GetBoolOrNull(row, "other_problems_provider_dependency"),
                UnavailabilityOtherHelpingAgencies = GetBoolOrNull(row, "unavailability_other_helping_agencies"),
                NeedAssisstanceDailyActivities = GetBoolOrNull(row, "need_assisstance_daily_activities"),
                BedriddenFewToNoResources = GetBoolOrNull(row, "bedridden_few_to_no_resources"),
                PartialyDependsNoResource = GetBoolOrNull(row, "partialy_depends_no_resource"),
                Result = result,
            }, null);
        }

        return (null, new SocialDeterminants2023Section
        {
            IsAutosufficientInRequestForTransport = GetBoolOrNull(row, "IsAutosufficientInRequestForTransport"),
            HasSafeRoof = GetBoolOrNull(row, "HasSafeRoof"),
            HasSufficientFundsForFood = GetBoolOrNull(row, "HasSufficientFundsForFood"),
            FeelSafeInLivingPlace = GetBoolOrNull(row, "FeelSafeInLivingPlace"),
            Result = result,
        });
    }

    // uspGetAHA2 actually returns a 13-table result set (Tables(0) through Tables(12)) -- more than
    // the 11 originally documented before the list-type tables (Cancer Diagnosis, Pressure Sores
    // List, Diseases of the Skin, Dx History Selection, Allergies, Suspicious Dx Hx Selection,
    // Malnutrition Criteria, Screening Substance Use, Social Determinants) were read; read once and
    // keep every table around so later batches can pull whichever ones they need without a second
    // round trip.
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
