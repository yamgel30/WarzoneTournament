namespace Chopper.Services.AhaClaims;

public sealed record ImLabRefSection
{
    public bool? ImmunizationsNa { get; init; }
    public bool? ImmunizationsParentRefuses { get; init; }
    public bool? ImmunizationsHepB1Dose { get; init; }
    public bool? ImmunizationsHebB2Dose { get; init; }
    public bool? ImmunizationsHebB3Dose { get; init; }
    public bool? ImmunizationsHepA1Dose { get; init; }
    public bool? ImmunizationsHebA2Dose { get; init; }
    public bool? ImmunizationsDTaP1Dose { get; init; }
    public bool? ImmunizationsDTaP2Dose { get; init; }
    public bool? ImmunizationsDTaP3Dose { get; init; }
    public bool? ImmunizationsDTaP4Dose { get; init; }
    public bool? ImmunizationsDTaP5Dose { get; init; }
    public bool? ImmunizationsHib1Dose { get; init; }
    public bool? ImmunizationsHib2Dose { get; init; }
    public bool? ImmunizationsHib3Dose { get; init; }
    public bool? ImmunizationsHib4Dose { get; init; }
    public bool? ImmunizationsPcv13_1Dose { get; init; }
    public bool? ImmunizationsPcv13_2Dose { get; init; }
    public bool? ImmunizationsPcv13_3Dose { get; init; }
    public bool? ImmunizationsPcv13_4Dose { get; init; }
    public bool? ImmunizationsIpv1Dose { get; init; }
    public bool? ImmunizationsIpv2Dose { get; init; }
    public bool? ImmunizationsIpv3Dose { get; init; }
    public bool? ImmunizationsIpv4Dose { get; init; }
    public bool? ImmunizationsMmr1Dose { get; init; }
    public bool? ImmunizationsMmr2Dose { get; init; }
    public bool? ImmunizationsVaricella1Dose { get; init; }
    public bool? ImmunizationsVaricella2Dose { get; init; }
    public bool? ImmunizationsTdap { get; init; }
    public bool? ImmunizationsRotavirus1Dose { get; init; }
    public bool? ImmunizationsRotavirus2Dose { get; init; }
    public bool? ImmunizationsInfluenza { get; init; }
    public bool? ImmunizationsMenningococcalMcv { get; init; }
    public bool? ImmunizationsHpv1Dose { get; init; }
    public bool? ImmunizationsHpv2Dose { get; init; }
    public bool? ImmunizationsHpv3Dose { get; init; }
    public string? ImmunizationsOthers { get; init; }

    public bool? LabNa { get; init; }
    public bool? LabHgbHct { get; init; }
    public bool? LabTb { get; init; }
    public bool? LabUa { get; init; }
    public bool? LabLipidProfile { get; init; }
    public bool? LabBloodLeadTest { get; init; }
    public bool? LabVih { get; init; }
    public bool? LabNaat { get; init; }
    public bool? LabVdrl { get; init; }
    public string? LabOther { get; init; }

    // Read by GetAHA but not currently sent by the Page 4 save path.
    public string? LabHgbHctResult { get; init; }
    public string? LabTbResult { get; init; }
    public string? LabUaResult { get; init; }
    public string? LabLipidProfileResult { get; init; }
    public string? LabBloodLeadTestResult { get; init; }
    public string? LabVihResult { get; init; }
    public string? LabNaatResult { get; init; }
    public string? LabVdrlResult { get; init; }
    public string? LabOtherResult { get; init; }
    public bool? LabHgbHctOrdered { get; init; }
    public bool? LabTbOrdered { get; init; }
    public bool? LabUaOrdered { get; init; }
    public bool? LabLipidProfileOrdered { get; init; }
    public bool? LabBloodLeadTestOrdered { get; init; }
    public bool? LabVihOrdered { get; init; }
    public bool? LabNaatOrdered { get; init; }
    public bool? LabVdrlOrdered { get; init; }
    public bool? LabOtherOrdered { get; init; }

    public string? VisionText { get; init; }
    public string? HearingText { get; init; }

    public bool? ReferralsNa { get; init; }
    public bool? ReferralsWic { get; init; }
    public bool? ReferralsPhysicalTherapy { get; init; }
    public bool? ReferralsOccupationTherapy { get; init; }
    public bool? ReferralsSpeechTherapy { get; init; }
    public bool? ReferralsAudiology { get; init; }
    public bool? ReferralsDental { get; init; }
    public bool? ReferralsBehavioralHealth { get; init; }
    public bool? ReferralsEarlyIntervention { get; init; }
    public bool? ReferralsMentalHealthSpecialist { get; init; }
    public bool? ReferralsNutritionist { get; init; }
    public bool? ReferralsOptometrist { get; init; }
    public bool? ReferralsOphthalmology { get; init; }
    public string? ReferralsOtherSpecialtyText { get; init; }

    public bool? ImmunoOthersCheckbox { get; init; }
    public bool? LabsOthersCheckbox { get; init; }
    public bool? ReferalsOthersCheckbox { get; init; }
}
