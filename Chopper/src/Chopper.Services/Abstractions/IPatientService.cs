using Chopper.Services.Patients;

namespace Chopper.Services.Abstractions;

public interface IPatientService
{
    Task<PatientDto?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PatientDto>> SearchAsync(string? lastName, CancellationToken cancellationToken = default);
}
