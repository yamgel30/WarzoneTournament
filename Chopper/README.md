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

  Ported: **Page 1** (`PUT /api/aha-claims/{claimId}/pages/1`) — Chief
  Complaint/Patient Medical History, Medical/Family/Social History (~100
  fields, the single largest section in the form), Advance Directives,
  Review of System, Myocardial Infarction. `AdvanceDirective`'s legacy
  clamp of out-of-range dates to the minimum valid SQL `datetime`
  (1753-01-01) before insert is preserved.

  **Not ported: Medication List / Allergies Medication List** (also part of
  Page 1). The legacy save uses a SQL Server table-valued parameter
  (`SqlDbType.Structured`) and the VB source never sets `SqlParameter.TypeName`,
  so the actual server-side table type name isn't available from the code —
  need that (or the CREATE TYPE definition) to port these two sections
  without guessing.

  Ported: **Page 3** (`PUT /api/aha-claims/{claimId}/pages/3`) — Screening
  Schedule, both the pre-2023 form (~135 fields; `uspSaveScreeningTest`)
  and the 2023+ form (`uspSaveScreeningTest2023`). Legacy branches on
  `DateOfVisit.Year`, and this does the same. The two stored procedures
  share ~85% of their fields (all modeled once on `ScreeningScheduleSection`);
  `ScreeningSchedule2023Extras` carries what's new for 2023+ (detailed
  retinopathy/eye findings, urine albumin/creatinine, Td/Tdap and Zoster
  vaccines). 2023+ drops COVID-19 vaccine tracking, the microalbumin block,
  and the glaucoma test date (result/NAFor/prescribed are still sent) —
  those `ScreeningScheduleSection` fields are simply not sent when saving
  a 2023+ visit. One field, `ColorectalColonoscopyResult`, is intentionally
  dropped from both: legacy computes it from the individual
  `Colorectal_ColonoscopyResult_*` flags but the parameter add is commented
  out in the source, so it never actually reaches the database today.

  Legacy calls `Globals.ValidateDateMinMaxRange(date)` before sending most
  Screening Schedule dates, clearing the value instead of sending it if the
  check fails. That helper's source isn't available, so its exact bounds
  are unknown — this substitutes the same SQL Server `datetime` min/max
  (1753-01-01 to 2999-12-31) used for `AdvanceDirective`. Worth tightening
  once/if the real `Globals` source turns up.

  Ported: **Physical Examination** (~230 fields across vitals, amputation,
  and 15 body-system sub-sections: HEENT/Oral, Constitutional,
  Integumentary, Respiratory, Gastrointestinal, Genitourinary, Neck, Chest,
  Cardiovascular, Abdomen, Genitalia/Groin/Buttocks, Musculoskeletal, Skin,
  Psychiatric/Neurologic, Hematologic/Lymphatic/Immunologic) — turned out
  to be a single stored procedure (`uspSavePhysicalExamination`), not the
  fan-out its ~2,700-line model tree suggested. Vitals
  (Temperature/Height/Weight/BMI) keep the legacy behavior of sending an
  explicit default (0, or `"lbs"` for `WeightType`) instead of `NULL` when
  missing.

  **This page (and Page 1) always send every parameter to the stored
  procedure**, relying on Dapper to convert an absent value to `NULL`.
  Legacy instead *omits* many parameters entirely when their field is
  absent. For most parameters this is equivalent, but if any stored
  procedure declares a non-`NULL` default for a parameter (`@Foo BIT =
  0`), SQL Server only applies that default when the parameter is omitted
  from the call — an explicit `NULL` overrides it. Worth checking against
  the actual stored procedure definitions once available; not something
  that's verifiable from the VB source alone.

  **Page 3 is now fully ported.**

  Ported (partial): **Page 4** (`PUT /api/aha-claims/{claimId}/pages/4`) —
  6 of ~20 sections: BMI Associated Diagnoses, Rheumatoid Arthritis,
  Assessment Plan of Treatment, Cancer Diagnoses, CKD, Pressure Sores.

  `AssessmentPlanOfTreatment` has a real business rule preserved as-is:
  when `No` (no diabetes complications) is set, legacy blanks out the
  whole diabetic-complication detail block regardless of what the caller
  sent — see the field-level comments on `AssessmentPlanOfTreatmentSection`
  for exactly which fields that covers.

  `CancerDiagnoses` (a list) doesn't use a stored procedure for the whole
  section: legacy runs a raw `DELETE FROM Claims_DX ... UPDATE
  Claims_AHADetail SET CancerDiagnosisNA = ...` first, then calls
  `uspClaimsDX_Save_New` once per diagnosis with a hardcoded dummy ICD
  code (`999.99`). Other sections (like `SaveOtherCondition`, not ported)
  write to the same `Claims_DX` table, so save order matters — this
  matches legacy running cancer diagnoses first, ahead of other
  conditions.

  A handful of fields (`BMIAssociatedDiagnosesSection.Na`,
  `RheumatoidArthritisSection.Na`, and `ChronicKidneyDiseaseSection`'s
  `Gfr`/`SerumCalcium`/`SerumPth`) are passed to `AddWithValue` as the raw
  field-wrapper object in the legacy code instead of `.Value` — almost
  certainly a bug that would throw at the ADO.NET layer and get silently
  swallowed by the surrounding try/catch (`_saveError = True`, no rethrow).
  This migration sends the actual `.Value` instead, since that's clearly
  the intended behavior everywhere else in the codebase — worth confirming
  against the real stored procedures, since if the legacy bug is somehow
  load-bearing (SPs default these columns and never actually receive a
  value today), sending real data changes what gets persisted.

  Not started: the other ~14 Page 4 sections — Congenital Diseases,
  Pressure Sores List, Diseases of the Skin, Other Condition (blocked —
  see below), Cardiovascular Diseases, Pulmonary Diseases, Im/Lab Ref,
  Malnutrition Criteria, Screening Substance Use, Eye and Neurology
  (+2023 variant), Social Determinants (+2023 variant), Screening Result,
  Gastrointestinal Diseases, and the GHP-only Gastrointestinal/Musculoskeletal
  pair.

  **Blocked: Other Condition.** Same table-valued-parameter problem as
  Page 1's Medication List (`SqlParameter.TypeName` never set in the VB
  source), plus it calls `AppShared.GetICDCodeType`,
  `AppShared.GetDefaultRejectCode`, and `AppShared.GetRejectCodeDescription`
  — helper methods that live in a class not present in either legacy file
  provided so far. Needs that `AppShared` source (or at least those three
  methods) in addition to the SQL type name to port.

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

Keep working through Page 4's remaining ~14 sections (see above for the
list). Same recipe each time — read the section class(es) in
`legacy/AHAEDM.vb`, read the matching `Save*Section` method(s) in
`legacy/AHADataAdapter.vb`, add a plain-value DTO + service method +
endpoint under `Chopper.Services/AhaClaims` and `Chopper.Api/Controllers/AhaClaimsController.cs`.

Also need:
- The SQL Server table type name for `MedicationListSection` /
  `AllergiesMedicationList` (see above) to finish Page 1.
- The `AppShared` class source (at least `GetICDCodeType`,
  `GetDefaultRejectCode`, `GetRejectCodeDescription`) plus the SQL table
  type name for its own table-valued parameter, to unblock Page 4's Other
  Condition section.
- The `Globals` class source (at least `ValidateDateMinMaxRange`,
  `LogError`, `ValidatePayerID`) if exact legacy behavior matters beyond
  what's already been reasonably approximated.

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
