using Chopper.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Chopper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PatientsController(IPatientService patientService) : ControllerBase
{
    [HttpGet("{patientId:int}")]
    public async Task<IActionResult> GetById(int patientId, CancellationToken cancellationToken)
    {
        var patient = await patientService.GetByIdAsync(patientId, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? lastName, CancellationToken cancellationToken)
    {
        var patients = await patientService.SearchAsync(lastName, cancellationToken);
        return Ok(patients);
    }
}
