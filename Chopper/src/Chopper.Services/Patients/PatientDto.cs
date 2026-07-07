namespace Chopper.Services.Patients;

public sealed record PatientDto(
    int PatientId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? MemberNumber);
