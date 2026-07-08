namespace Chopper.Services.AhaClaims;

// Legacy calls this "ScreeningSchedule". Only the pre-2023 field set (uspSaveScreeningTest) is
// modeled so far -- the 2023+ variant (uspSaveScreeningTest2023) is a separate, similarly large
// stored procedure/section not yet ported.
public sealed record ScreeningScheduleSection
{
    // Bone Mineral Density
    public DateTime? BoneMineralDensityDate { get; init; }
    public bool? BoneMineralDensityResultNormal { get; init; }
    public bool? BoneMineralDensityResultOsteopenia { get; init; }
    public bool? BoneMineralDensityResultOsteoporosis { get; init; }
    public bool? BoneMineralDensityResultNa { get; init; }
    public bool? BoneMineralDensityResultOther { get; init; }
    public string? BoneMineralDensityResult { get; init; }
    public string? BoneMineralDensityNaFor { get; init; }
    public bool? BoneMineralDensityPrescribed { get; init; }
    public bool? BoneMineralDensityRxOrdered { get; init; }

    // Cardiovascular
    public DateTime? CardiovascularLdlDate { get; init; }
    public string? CardiovascularLdlResult { get; init; }
    public string? CardiovascularLdlNaFor { get; init; }
    public bool? CardiovascularLdlPrescribed { get; init; }

    public DateTime? CardiovascularBetaDate { get; init; }
    public string? CardiovascularBetaResult { get; init; }
    public string? CardiovascularBetaNaFor { get; init; }
    public bool? CardiovascularBetaPrescribed { get; init; }

    // Colorectal cancer (general)
    public int? ColorectalCancerScreeningSelectedIndex { get; init; }
    public DateTime? ColorectalCancerScreeningDate { get; init; }
    public string? ColorectalCancerScreeningResult { get; init; }
    public string? ColorectalCancerScreeningNaFor { get; init; }
    public bool? ColorectalCancerScreeningPrescribed { get; init; }

    public bool? IsDiabetic { get; init; }

    // Diabetes screening
    public DateTime? DiabetesScreeningDilatedEyeExamDate { get; init; }
    public string? DiabetesScreeningDilatedEyeExamResult { get; init; }
    public string? DiabetesScreeningDilatedEyeExamNaFor { get; init; }
    public bool? DiabetesScreeningDilatedEyeExamPrescribed { get; init; }

    public DateTime? DiabetesScreeningLdlDate { get; init; }
    public string? DiabetesScreeningLdlResult { get; init; }
    public string? DiabetesScreeningLdlNaFor { get; init; }
    public bool? DiabetesScreeningLdlPrescribed { get; init; }

    public DateTime? DiabetesScreeningHga1CDate { get; init; }
    public string? DiabetesScreeningHga1CResult { get; init; }
    public string? DiabetesScreeningHga1CNaFor { get; init; }
    public bool? DiabetesScreeningHga1CPrescribed { get; init; }

    public DateTime? DiabetesScreeningMicroalbuminDate { get; init; }
    public string? DiabetesScreeningMicroalbuminResult { get; init; }
    public string? DiabetesScreeningMicroalbuminNaFor { get; init; }
    public bool? DiabetesScreeningMicroalbuminPrescribed { get; init; }

    public DateTime? GlaucomaTestDate { get; init; }
    public string? GlaucomaTestResult { get; init; }
    public string? GlaucomaTestNaFor { get; init; }
    public bool? GlaucomaTestPrescribed { get; init; }

    public DateTime? MammogramProstateCancerDate { get; init; }
    public string? MammogramProstateCancerResult { get; init; }
    public string? MammogramProstateCancerNaFor { get; init; }
    public bool? MammogramProstateCancerPrescribed { get; init; }

    // Immunizations
    public DateTime? FluShotDate { get; init; }
    public string? FluShotComments { get; init; }
    public bool? FluShotPrescribed { get; init; }
    public bool? FluShotPatientRefuses { get; init; }

    public DateTime? PneumococcalShotDate { get; init; }
    public string? PneumococcalShotComments { get; init; }
    public bool? PneumococcalShotPrescribed { get; init; }
    public bool? PneumococcalShotPatientRefuses { get; init; }

    public string? Covid19VaccineHouse { get; init; }
    public int? Covid19VaccineShot { get; init; }
    public bool? Covid19VaccineRefuse { get; init; }
    public bool? Covid19VaccineOrdered { get; init; }
    public DateTime? Covid19VaccineShotDate1 { get; init; }
    public DateTime? Covid19VaccineShotDate2 { get; init; }
    public DateTime? Covid19VaccineShotDate3 { get; init; }

    // Colorectal: colonoscopy
    public bool? ColorectalColonoscopy { get; init; }
    public DateTime? ColorectalColonoscopyDate { get; init; }
    public bool? ColorectalColonoscopyResultNa { get; init; }
    public bool? ColorectalColonoscopyResultNegative { get; init; }
    public bool? ColorectalColonoscopyResultDiverticles { get; init; }
    public bool? ColorectalColonoscopyResultBleedingAreas { get; init; }
    public bool? ColorectalColonoscopyResultCaInColon { get; init; }
    public bool? ColorectalColonoscopyResultCaInRectum { get; init; }
    public bool? ColorectalColonoscopyResultColitis { get; init; }
    public bool? ColorectalColonoscopyResultUlcerativeColitis { get; init; }
    public bool? ColorectalColonoscopyResultCrohnsDisease { get; init; }
    public bool? ColorectalColonoscopyResultPolyps { get; init; }
    public bool? ColorectalColonoscopyResultOther { get; init; }
    // Note: legacy computes a human-readable summary string from the flags above but never
    // actually sends it to uspSaveScreeningTest (the parameter add is commented out in the
    // source) -- so there's no ColorectalColonoscopyResult text field here; it wouldn't persist.
    public string? ColorectalColonoscopyNaFor { get; init; }
    public bool? ColorectalColonoscopyPrescribe { get; init; }

    public bool? ColorectalOccultBlood { get; init; }
    public DateTime? ColorectalOccultBloodDate { get; init; }
    public string? ColorectalOccultBloodResult { get; init; }
    public string? ColorectalOccultBloodNaFor { get; init; }
    public bool? ColorectalOccultBloodPrescribe { get; init; }

    // Colorectal: flexible sigmoidoscopy
    public bool? ColorectalFlexibleSigmoidoscopy { get; init; }
    public DateTime? ColorectalFlexibleSigmoidoscopyDate { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyResultNa { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyResultNegative { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyResultAnalFissure { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyResultAnorectalAbscess { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyResultIntestinalOcclusion { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyResultCaEnElSigmoideo { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyResultCaInRectum { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyResultCaInTheRectosigmoidJunction { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyResultColorectalPolyps { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyResultDiverticles { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyResultHemorrhoids { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyResultHirschsprungDisease { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyResultInflammatoryBowelDisease { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyResultInflammationOrInfection { get; init; }
    public string? ColorectalFlexibleSigmoidoscopyResult { get; init; }
    public string? ColorectalFlexibleSigmoidoscopyNaFor { get; init; }
    public bool? ColorectalFlexibleSigmoidoscopyPrescribe { get; init; }

    // Colorectal: FIT-DNA / CT colonography
    public bool? ColorectalFitDna { get; init; }
    public DateTime? ColorectalFitDnaDate { get; init; }
    public string? ColorectalFitDnaResult { get; init; }
    public string? ColorectalFitDnaNaFor { get; init; }
    public bool? ColorectalFitDnaPrescribe { get; init; }

    public bool? ColorectalColonographyCt { get; init; }
    public DateTime? ColorectalColonographyCtDate { get; init; }
    public string? ColorectalColonographyCtResult { get; init; }
    public string? ColorectalColonographyCtNaFor { get; init; }
    public bool? ColorectalColonographyCtPrescribe { get; init; }

    // Mammogram / prostate / PAP smear / HPV
    public DateTime? MammogramCancerDate { get; init; }
    public bool? MammogramCancerResultCategory0 { get; init; }
    public bool? MammogramCancerResultCategory1 { get; init; }
    public bool? MammogramCancerResultCategory2 { get; init; }
    public bool? MammogramCancerResultCategory3 { get; init; }
    public bool? MammogramCancerResultCategory4 { get; init; }
    public bool? MammogramCancerResultCategory5 { get; init; }
    public bool? MammogramCancerResultCategory6 { get; init; }
    public bool? MammogramCancerResultNa { get; init; }
    public string? MammogramCancerResult { get; init; }
    public string? MammogramCancerNaFor { get; init; }
    public bool? MammogramCancerPrescribed { get; init; }

    public DateTime? PapSmearDate { get; init; }
    public string? PapSmearResult { get; init; }
    public string? PapSmearNaFor { get; init; }
    public bool? PapSmearPrescribed { get; init; }

    public DateTime? ProstateCancerDate { get; init; }
    public string? ProstateCancerResult { get; init; }
    public string? ProstateCancerNaFor { get; init; }
    public bool? ProstateCancerPrescribed { get; init; }

    public DateTime? ScreeningHpvDate { get; init; }
    public string? ScreeningHpvComment { get; init; }
    public string? ScreeningHpvResult { get; init; }
    public bool? ScreeningHpvOrdered { get; init; }
}
