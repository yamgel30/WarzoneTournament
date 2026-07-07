# Chopper

New home for the healthcare **AHA (Annual Health Assessment) claims** SOAP
service, being migrated off ASP.NET Framework 4.5 onto ASP.NET Core (.NET 10).
Named after Tony Tony Chopper — the crew's doctor — since this service deals
with patients, providers, assessments, and claims.

This is a standalone solution, independent from `WarzoneTournament.*`. It does
not reuse WarzoneTournament's Domain/Application/Infrastructure layers — the
two are unrelated domains that happen to live in the same repo.

## Layout

```
Chopper/
  Chopper.slnx
  legacy/
    AHADataAdapter.vb    Legacy VB data-access layer (reference only, not
                          part of the build) — every stored procedure call
                          in the old app currently lives in this one class.
    AHAEDM.vb            Legacy VB model/data-contract layer (reference only)
                          — AHAFormItem, AHAFormItemShort, and every
                          `*Section` class SaveClaim writes to the DB.
  src/
    Chopper.Api/         ASP.NET Core Web API (controllers, Program.cs)
    Chopper.Services/    Business logic + data access (Dapper against
                          existing stored procedures)
```

No repository pattern and no EF Core yet — deliberately. The legacy database
has a lot of business logic baked into stored procedures; wrapping those in a
repository/EF layer is a separate, later decision. For now `Chopper.Services`
calls stored procedures directly via Dapper (`Chopper.Services/Common/SqlConnectionFactory.cs`).

Legacy error handling (`Globals.LogError`, `ErrorLog_Insert`, `_saveError`)
is intentionally **not** ported — new code logs via `ILogger` instead and
lets unexpected exceptions bubble up to ASP.NET Core's normal error handling.
Where the legacy method swallowed an error and returned `False`/`-1` to the
caller (rather than throwing), that specific behavior is preserved so
existing callers keep seeing the same success/failure signal.

## What's ported so far

`legacy/AHADataAdapter.vb` is one large class covering everything the old
service does. It breaks down into two very different shapes of work:

- **~20 standalone operations** — small, one-or-two-stored-procedure calls
  (session logging, acknowledgement logging, concurrency/eligibility
  verification, AI transcript/audio saves). These map cleanly to REST
  endpoints and are what's been ported first:

  | Legacy method | New endpoint |
  |---|---|
  | `LogSession` | `POST /api/sessions` |
  | `LogAckowledgement` / `GetLogAckowledgement` | `POST` / `GET /api/acknowledgements/welcome-letter` |
  | `LogFunctQuadMessage` / `GetLogFunctQuadMessage` | `POST` / `GET /api/acknowledgements/functional-quadriplegia` |
  | `LogInflammatoryPolyarthritisMessage` / `GetLogInflammatoryPolyarthritisMessage` | `POST` / `GET /api/acknowledgements/inflammatory-polyarthritis` |
  | `ValidateConcurrencyID` | `GET /api/claims/{claimId}/concurrency` |
  | `VerifyMemberHasTHAForYear` | `GET /api/claims/tha-verification` |
  | `VerifyIfFormExistsForDOS` | `GET /api/claims/form-exists` |
  | `AHAVerifySubProjectIsActive` | `GET /api/sub-projects/{projectName}/active` |
  | `SaveAIInfo` | `POST /api/ai-info/transcript` |
  | `SaveAudio` | `POST /api/ai-info/audio` |

- **`SaveClaim` and its ~35 private `Save*Section` helpers** — this is the
  actual AHA form submission: one big operation that writes the whole
  assessment (demographics, medical history, screenings, exam findings,
  diagnoses, etc.) across dozens of stored procedures. Turns out it's
  **not atomic** — `SavePage1`-`SavePage4` each call a fixed list of section
  savers, and every section saver catches its own DB error, flags
  `_saveError`, and keeps going rather than rolling back or stopping. That
  means it genuinely can be migrated section-by-section / page-by-page, the
  same "poco a poco" way as the list above — it just needed the model
  classes first to know each section's fields.

  Legacy fields aren't plain values — every field (`StringField`,
  `DateField`, `BooleanField`, ...) wraps a value plus UI validation
  metadata (`FieldID`, `HasError`, `ErrorDescription`, `IsFromAI`, ...) left
  over from the old WebForms client-side validation. The new DTOs carry
  **plain values only** (`bool?`, `string?`, `int?`, ...) — the adapter only
  ever reads `.Value` when building stored-procedure parameters, so the
  metadata never reached the database anyway.

  Ported: **Page 2** (`PUT /api/aha-claims/{claimId}/pages/2`) — Medication
  Review, Cognitive Assessment, Pain Screening, Activities of Daily Living.
  Matches legacy behavior exactly, including that Medication Review and
  Cognitive Assessment always call their stored procedure (even with an
  empty section), while Pain Screening and Activities of Daily Living skip
  entirely when their section is absent — and a legacy quirk in Pain
  Screening (submitting `Others` forces `PainEvaluationOtherCondition` to
  `true` and copies `Others` into `PainEvaluationOtherConditionText`) is
  preserved rather than "fixed".

  Not started: Page 1 (6 sections, includes two large ones — Chief
  Complaint/Patient Medical History and Medical/Family/Social History),
  Page 3 (Screening Test + the very large Physical Examination section
  tree), Page 4 (~20 sections, several GHP/year/at-home conditional).

  `ErrorLog_Insert` and `InsertDBDebugLog` (the legacy error/debug logging
  infrastructure) are deliberately skipped rather than ported.

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

## Next

Continue `SaveClaim` page-by-page: Page 1, then Page 3, then Page 4 (see
above for what's in each). Same recipe each time — read the section
class(es) in `legacy/AHAEDM.vb`, read the matching `Save*Section` method(s)
in `legacy/AHADataAdapter.vb`, add a plain-value DTO + service method +
endpoint under `Chopper.Services/AhaClaims` and `Chopper.Api/Controllers/AhaClaimsController.cs`.

Also useful, if available: the `.asmx`/WSDL for the SOAP service itself, to
confirm which `AHADataAdapter` methods are actually exposed as SOAP
operations (this migration currently assumes each `Public` method on the
adapter corresponds to one operation, since the SOAP layer itself hasn't
been provided yet).

## Running locally

Requires the .NET 10 SDK and a SQL Server instance reachable via the
`ConnectionStrings:ChopperDb` setting (see `appsettings.Development.json`).

```
cd Chopper
dotnet run --project src/Chopper.Api
```
