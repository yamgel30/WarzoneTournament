namespace Chopper.Services.AhaClaims;

public sealed record MedicalFamilySocialHistorySection
{
    public bool? Na { get; init; }

    // Family history: Diabetes Mellitus / Cardiovascular Disease / Cholesterol / Cancer / Alzheimer / HIV
    public bool? DmPatient { get; init; }
    public bool? DmMother { get; init; }
    public bool? DmFather { get; init; }
    public bool? DmSiblings { get; init; }

    public bool? CvdPatient { get; init; }
    public bool? CvdMother { get; init; }
    public bool? CvdFather { get; init; }
    public bool? CvdSiblings { get; init; }

    public bool? CholesterolPatient { get; init; }
    public bool? CholesterolMother { get; init; }
    public bool? CholesterolFather { get; init; }
    public bool? CholesterolSiblings { get; init; }

    public bool? CancerPatient { get; init; }
    public bool? CancerMother { get; init; }
    public bool? CancerFather { get; init; }
    public bool? CancerSiblings { get; init; }

    public bool? AlzheimerPatient { get; init; }
    public bool? AlzheimerMother { get; init; }
    public bool? AlzheimerFather { get; init; }
    public bool? AlzheimerSiblings { get; init; }

    public bool? VihPatient { get; init; }
    public bool? VihMother { get; init; }
    public bool? VihFather { get; init; }
    public bool? VihSiblings { get; init; }

    // Risk factors / counseling
    public bool? RiskForHiv { get; init; }
    public bool? RiskForStd { get; init; }
    public bool? CounselTabaccoUse { get; init; }
    public bool? CounselIllicitDrugUse { get; init; }
    public bool? CounselAlcoholUse { get; init; }

    public bool? PatientNa { get; init; }
    public bool? MotherNa { get; init; }
    public bool? FatherNa { get; init; }
    public bool? SiblingsNa { get; init; }

    // Substance use history
    public bool? HistoryAlcoholism { get; init; }
    public bool? HistoryDrugDependence { get; init; }
    public bool? HistoryCaffeineDependence { get; init; }

    public bool? Nicotine { get; init; }
    public bool? Opiates { get; init; }
    public bool? Cannabis { get; init; }
    public bool? Sedatives { get; init; }
    public bool? Hypnotics { get; init; }
    public bool? Anxiolytics { get; init; }
    public bool? OtherDrugs { get; init; }
    public string? OtherDrugsText { get; init; }

    public string? OtherConditionText { get; init; }
    public bool? OtherPatient { get; init; }
    public bool? OtherMother { get; init; }
    public bool? OtherFather { get; init; }
    public bool? OtherSiblings { get; init; }

    public string? HistoryOfMotherPregnancy { get; init; }

    // Physical activity screening
    public string? FisicalActivityScreening { get; init; }
    public bool? FisicalActivityScreeningNa { get; init; }
    public bool? FisicalActivityScreeningDailyActivityRecommended { get; init; }

    // Nutritional screening
    public bool? NutricionalScreeningNa { get; init; }
    public bool? NutricionalScreeningAdequateIntake { get; init; }
    public bool? NutricionalScreeningBalancedNutriciousDiet { get; init; }
    public bool? NutricionalScreeningBreastmilk { get; init; }
    public bool? NutricionalScreeningCereal { get; init; }
    public bool? NutricionalScreeningCowMilk { get; init; }
    public bool? NutricionalScreeningFeedsItself { get; init; }
    public bool? NutricionalScreeningFormula { get; init; }
    public bool? NutricionalScreeningJunkFood { get; init; }
    public string? NutricionalScreeningOther { get; init; }
    public bool? NutricionalScreeningOverweight { get; init; }
    public bool? NutricionalScreeningSodaJuices { get; init; }
    public bool? NutricionalScreeningSolidFoot { get; init; }
    public bool? NutricionalScreeningSupplementVitamins { get; init; }
    public bool? NutricionalScreeningUnderWeight { get; init; }
    public bool? NutricionalScreeningFoodAllergies { get; init; }
    public bool? NutricionalScreeningSpecialDiets { get; init; }
    public bool? NutricionalScreeningOthersCheckbox { get; init; }

    // Development screening
    public bool? DevelopmentHealthNa { get; init; }
    public bool? DevelopmentScreeningCommunicationArea { get; init; }
    public bool? DevelopmentScreeningFineMotorSkillArea { get; init; }
    public bool? DevelopmentScreeningGrossMotorSkillsArea { get; init; }
    public bool? DevelopmentScreeningSocialIndividualSkillsArea { get; init; }
    public bool? DevelopmentScreeningProblemResolutionSkillArea { get; init; }
    public bool? DevelopmentScreeningBehavioralHealthArea { get; init; }

    // Behavioral health
    public bool? BehavioralHealthNa { get; init; }
    public bool? BehavioralHealthPhysicalMentalSelfRegulation { get; init; }
    public bool? BehavioralHealthHabilityToFollowsInstructionsRules { get; init; }
    public bool? BehavioralHealthSocialCommunication { get; init; }
    public bool? BehavioralHealthAdaptativeFunctioning { get; init; }
    public bool? BehavioralHealthAutonomy { get; init; }
    public bool? BehavioralHealthCapacityToBeAffectiveEmpathic { get; init; }
    public bool? BehavioralHealthInteractionWithPeople { get; init; }
    public bool? BehavioralHealthUsesAlcoholDrugs { get; init; }

    // Appropriate education
    public bool? AppropriateEducationNa { get; init; }
    public bool? AppropriateEducationAppropriateUseCarSeat { get; init; }
    public bool? AppropriateEducationBottleProp { get; init; }
    public bool? AppropriateEducationPassiveSmoke { get; init; }
    public bool? AppropriateEducationInfantCryingWhatToDo { get; init; }
    public bool? AppropriateEducationShakeBabyPrevention { get; init; }
    public bool? AppropriateEducationFirearm { get; init; }
    public bool? AppropriateEducationPacifiers { get; init; }
    public bool? AppropriateEducationParentsReadToChild { get; init; }
    public bool? AppropriateEducationEmergency911 { get; init; }
    public bool? AppropriateEducationFingerFoodChoking { get; init; }
    public bool? AppropriateEducationDisciplinePraise { get; init; }
    public bool? AppropriateEducationDrowningPrevention { get; init; }
    public bool? AppropriateEducationNeverLeaveToddlerAlone { get; init; }
    public bool? AppropriateEducationToiletTraining { get; init; }
    public bool? AppropriateEducationNutritionExercise { get; init; }
    public bool? AppropriateEducationEstablishRoutineBedMealsToiletingEtc { get; init; }
    public bool? AppropriateEducationUseSportProtection { get; init; }
    public bool? AppropriateEducationBullying { get; init; }
    public bool? AppropriateEducationOralHealth { get; init; }
    public string? AppropriateEducationOthers { get; init; }
    public bool? AppropriateEducationSportInjuryPrevention { get; init; }
    public bool? AppropriateEducationDrowningSunSafety { get; init; }
    public bool? AppropriateEducationSafeAtHome { get; init; }
    public bool? AppropriateEducationCorrectUseSeatbelt { get; init; }
    public bool? AppropriateEducationSexualEducationStd { get; init; }
    public bool? AppropriateEducationDepressionAnxiety { get; init; }
    public bool? AppropriateEducationTabaccoAlcoholDrugsRxDrugsInhalants { get; init; }
    public bool? AppropriateEducationRiskOfTattoosPiercing { get; init; }
    public bool? AppropriateEducationAutocontrol { get; init; }
    public bool? AppropriateEducationOthersCheckbox { get; init; }
}
