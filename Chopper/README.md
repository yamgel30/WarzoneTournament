# Chopper

New home for the healthcare **annual assessment / claims** SOAP service, being
migrated off ASP.NET Framework 4.5 onto ASP.NET Core (.NET 10). Named after
Tony Tony Chopper — the crew's doctor — since this service deals with
patients, providers, assessments, and claims.

This is a standalone solution, independent from `WarzoneTournament.*`. It does
not reuse WarzoneTournament's Domain/Application/Infrastructure layers — the
two are unrelated domains that happen to live in the same repo.

## Layout

```
Chopper/
  Chopper.slnx
  src/
    Chopper.Api/         ASP.NET Core Web API (controllers, Program.cs)
    Chopper.Services/    Business logic + data access (Dapper against
                          existing stored procedures)
```

No repository pattern and no EF Core yet — deliberately. The legacy database
has a lot of business logic baked into stored procedures; wrapping those in a
repository/EF layer is a separate, later decision. For now `Chopper.Services`
calls stored procedures directly via Dapper (`Chopper.Services/Common/SqlConnectionFactory.cs`).

`Chopper.Services/Patients` is a starter vertical slice (`IPatientService`,
`PatientDto`, a controller at `GET /api/patients` and `GET /api/patients/{id}`)
wired end-to-end through DI (`Program.cs` → `AddChopperServices`) so new
verticals (Providers, Claims, Assessments, ...) can be added by following the
same pattern. The stored procedure names in `PatientService` are placeholders
— swap them for the real legacy SP names once the SOAP source is available.

## Migration approach (strangler fig)

Migrate operation by operation instead of a big-bang rewrite:

1. Pick one SOAP operation.
2. Read its implementation (and the stored procedure(s) it calls).
3. Add the equivalent REST endpoint + service method here, calling the same
   stored procedure(s).
4. Verify behavior matches (same inputs → same outputs) against the old
   service.
5. Point callers at the new endpoint; retire the SOAP operation once nothing
   depends on it.

Repeat per operation. The old SOAP service keeps running throughout, so
nothing breaks mid-migration.

## Adding the legacy source

The ASP.NET Framework 4.5 SOAP project isn't in this repo yet. To start
porting operations, add it under `Chopper/legacy/` (kept out of the new
solution — reference only) and push it to this branch, e.g.:

```
git checkout claude/soap-aspnet-api-migration-k2etre
git pull origin claude/soap-aspnet-api-migration-k2etre
# copy the legacy project into Chopper/legacy/
git add Chopper/legacy/
git commit -m "Add legacy SOAP service source for reference"
git push origin claude/soap-aspnet-api-migration-k2etre
```

Also useful, if available: the WSDL, and a schema dump or list of the
stored procedures the SOAP service calls.

## Running locally

Requires the .NET 10 SDK and a SQL Server instance reachable via the
`ConnectionStrings:ChopperDb` setting (see `appsettings.Development.json`).

```
cd Chopper
dotnet run --project src/Chopper.Api
```
