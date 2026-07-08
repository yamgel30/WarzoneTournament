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

  Ported: **Medication List / Allergies Medication List** (also part of
  Page 1), using the `MedicationList2020` table-valued parameter (now known
  from the real `CREATE TYPE` definition). `uspSaveMedicationList2020`
  folds `CurrentMedication` (`isAdherence = false`) together with either
  `AllergiesMedicationList` or `AdherenceMedicationList` (`isAdherence =
  true`) into one table — legacy uses `AllergiesMedicationList` here only
  when the member is GHP and under 21, otherwise `AdherenceMedicationList`.
  `uspSaveAlergMedicationList2020` is only called in that same GHP/under-21
  case, saving `AllergiesMedicationList` again on its own with every row's
  `isAdherence` forced to `false`. Since this service has no session state
  to derive GHP status or member age from (legacy computes both from the
  payer and member DOB), `SavePage1Request.IsGhp`/`MemberAge` take them as
  caller-supplied input instead.

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

  **Page 4 is now fully ported** (`PUT /api/aha-claims/{claimId}/pages/4`)
  — BMI Associated Diagnoses, Rheumatoid Arthritis, Assessment Plan of
  Treatment, Cancer Diagnoses, CKD, Pressure Sores, Major Depression,
  Congenital Diseases, Pressure Sore List, Cardiovascular Diseases,
  Diseases of the Skin, Eyes and Neurology, Im/Lab/Ref, Other Condition
  Additional, Other Condition, Pulmonary Diseases, Gastrointestinal
  Diseases (+ the GHP-only Gastrointestinal/Musculoskeletal pair),
  Social Determinants (both years), Malnutrition Criteria, Screening
  Substance Use, and Screening Result.

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
  the intended behavior everywhere else in the codebase. Confirmed against
  the real stored procedures: `uspSaveBMIAssociatedDiagnoses`/
  `uspSaveRheumatoidArthritis` both declare a plain `@NA bit`, and
  `uspSaveCKD` declares `@GFR`/`@SerumCalcium`/`@SerumPTH` as
  `varchar(100)` — nothing about those declarations suggests the legacy
  bug was somehow load-bearing, so sending real `.Value` data is correct.

  `EyesAndNeurology` shares one stored procedure pair the same way Screening
  Schedule does (see Page 3): `uspSaveEyeAndNeurology` (pre-2023) and
  `uspSaveEyeAndNeurology2023` take almost identical parameters, so both
  reuse `EyesAndNeurologySection` — `DementiaSeverity` is 2023+-only and is
  the one field that has to be *omitted entirely* (not just sent as
  `NULL`) for a pre-2023 save, since that older procedure doesn't declare
  the parameter at all. There's also a real GHP rule here: for visits from
  2023 onward, legacy skips Eyes and Neurology entirely for GHP members
  (no save at all, not even a blank one) — only non-GHP 2023+ visits call
  `uspSaveEyeAndNeurology2023`. Pre-2023 visits always save regardless of
  GHP.

  `Diseases of the Skin` uses the `DiseasesOfTheSkin2022` table-valued
  parameter (32 columns) with `uspSaveDiseasesOfTheSkin2022`. Legacy always
  calls this SP, even with no items: when the item list is `null` it still
  sends a single default/empty row (`IndexRow = 0`); when the list is
  non-null but empty it sends zero rows. Both cases are preserved as-is
  rather than skipping the call the way most other sections do when their
  section is absent.

  `GastrointestinalDiseasesSection` gets saved via `uspSaveGastrointestinal`
  unconditionally, then — for GHP members — legacy calls the **same stored
  procedure a second time** with only 6 of its 11 fields
  (`NA`/`NASH`/`MetabolicSyndrome`/`Hyperkalemia`/`Hypokalemia`/`TreatmentPlan`).
  That's not a mistake in this migration; it's what the legacy page save
  actually does, so the redundant second call is preserved as-is rather
  than "optimized away."

  `SaveClaimsMalnutritionCriteria`/`SaveClaimsScreeningSubstanceUse`/etc.
  are `Public` methods in the legacy adapter (unlike the `Private`
  `Save*Section` pattern elsewhere) but still take a `dbo` connection
  parameter and are only ever called from within the page-save flow — they
  aren't separately SOAP-exposed, so they're treated as ordinary Page 4
  sections here rather than standalone operations.

  `uspSaveScreeningResult` is a small aggregation step at the end of the
  page: it just persists 3 summary strings
  (`ScreeningSubstanceUseListResult`, `ScreeningSocialDeterminants2020Result`,
  `ScreeningMalnutritionCriteriaResult`). Legacy computes these from fields
  that were never modeled here (only `.Value` reads scattered around the
  UI layer, not sent to any of the ported stored procedures), so this
  migration takes them as plain caller-supplied strings
  (`SavePage4Request.ScreeningSubstanceUseResult` etc.) rather than trying
  to derive them.

  `Other Condition` uses the `AHADxOtherConditions` table-valued parameter
  with `uspClaimsDX_Save_New_V2024`. Legacy skips the call entirely when
  the item list is null or empty. The stored procedure's actual text
  confirmed it hardcodes `nICDCodeType` to `10` for every inserted row and
  never reads the TVP's `ICDCodeType` column at all — so
  `AppShared.GetICDCodeType` (source unavailable) turned out not to be
  needed. `AppShared.GetDefaultRejectCode()`/`GetRejectCodeDescription()`
  are still unavailable and are genuinely per-call constants (both take no
  parameters), so `SavePage4Request.OtherConditionDefaultRejectCode`/
  `OtherConditionRejectCodeDescription` take the same values as
  caller-supplied input instead, applied identically to every row exactly
  as the parameterless legacy calls did.

  `ErrorLog_Insert` and `InsertDBDebugLog` (the legacy error/debug logging
  infrastructure) are deliberately skipped rather than ported.

## Stored procedure cross-verification

Once the real stored procedure definitions and table-type (`CREATE TYPE`)
definitions became available, every parameter this migration sends was
cross-referenced against the actual `CREATE PROCEDURE` text — name
existence and declared SQL type — across all 33 stored-procedure calls in
`AhaClaimService.cs`. Result: zero name mismatches (every parameter name
this migration sends matches a real declared parameter). Three type
mismatches were found and fixed:

- `ChronicKidneyDiseaseSection.Gfr`/`SerumCalcium`/`SerumPth` were modeled
  as `decimal?`; `uspSaveCKD` actually declares them `varchar(100)`. Now
  `string?`.
- `PhysicalExaminationSection.Pulse`/`Breathing`/`BloodPressure1`/
  `BloodPressure2` were modeled as `decimal?`; `uspSavePhysicalExamination`
  actually declares them `smallint`. Now `int?`.
- `ScreeningSchedule2023Extras.UrineAlbuminResult`/`UrineCreatinineResult`
  were modeled as `decimal?`; `uspSaveScreeningTest2023` actually declares
  them `varchar`. Now `string?`.

One apparent mismatch turned out not to be a bug:
`EyesAndNeurologySection.RetinopathySeverity`/`ProliferativeSeverity` are
sent as `int?` to both `uspSaveEyeAndNeurology` (pre-2023, declares
`varchar`) and `uspSaveEyeAndNeurology2023` (declares `int`) — but
`AHADataAdapter.vb` sends `VerifyIntegerNull(...)` (an integer) to both
calls too, relying on SQL Server's implicit int-to-varchar conversion for
the pre-2023 case. Matches legacy exactly; left as-is.

## AHAService1 / IAHAService12

`legacy/AHAService1.vb` (`Implements IAHAService12`, `legacy/IAHAService1.vb`) is
the actual WCF SOAP service class — the layer that sits *above*
`AHADataAdapter`. Confirmed `SubmitAHA`, `UpdateAHA`, `PartialSaveAHA`,
`AIPartialSaveAHA`, and every `Resubmit*` variant call
`AHADataAdapter.SaveClaim` directly, which is exactly what's already ported
as `SavePage1`-`4`. Several already-ported standalone operations
(`LogAckowledgement`, `CreateSession`, `ValidateConcurrencyID`,
`VerifyMemberHasTHAForYear`, `AHAVerifySubProjectIsActive`,
`VerifyIfFormExistsForDOS`, `SaveInfo`/`SaveAudio`) turned out to be one-line
`AHADataAdapter` wrappers here too, confirming those are already fully
covered.

At 23,833 lines, the rest of the file breaks down very unevenly:

- Session/login/SSO (`Login`, `SingleSignOn`) — depends on external
  `AUS.Library`/`AUSAuthentication` assemblies not provided; not portable yet.
- List/search operations (`GetAHAListPending`, `...Rejected`,
  `...Submitted`, `...InProgress`, `...InProgress2021`,
  `...PendingSpecialCover`) — read-only, no external dependencies. **Ported,
  see below.**
- The full read side of a claim (`GetAHA`, ~2,850 lines; `GetMemberAHATemplate`,
  ~2,180 lines; `GetAHAShort`) — the mirror image of the already-ported
  `SaveClaim`, but for reads. Large but not blocked; not yet started.
- Submit/Update/PartialSave orchestrators — already known to call
  `AHADataAdapter.SaveClaim`; the session/business-rule validation wrapped
  around that call isn't ported yet.
- **Report generation (~9,000 lines, ~40% of the file)** —
  `GetAHAReport2016` through `GetAHAReport2026` (one pair of methods per
  year, Long + Short form), `GetAddendumFormReport`,
  `GetMemberSuspiciousConditionReport`, `GetProviderStatusLetterCSV`,
  `GetDxAndSuspisiousForm`, `GetMemberEmptyForm`,
  `GetMemberClinicalDataForm` — all built on
  `Microsoft.Reporting.WebForms.LocalReport` (RDLC), which needs the actual
  `.rdlc` report definitions (not provided) and isn't natively supported in
  .NET Core the way it was in .NET Framework. A fundamentally different
  problem from everything else in this migration; deliberately deferred.
- Addendum handling, provider/billing/eligibility lookups, Dx/condition
  operations — moderate size, no obvious blockers, not yet started.
- `GetPRAIReport` depends on an external `AUS.MMM.PRAI.Library` assembly;
  not portable without it.

### Ported: claim list/search (`POST /api/claim-search/*`)

`GetAHAListPending`, `GetAHAListPendingSpecialCover`, `GetAHAListRejected`,
`GetAHAListSubmitted`, `GetAHAListInProgress`, and `GetAHAListInProgress2021`
are ported as `ClaimSearchService`, one endpoint each under
`POST /api/claim-search/{pending,pending-special-cover,rejected,submitted,in-progress,in-progress-2021}`.
POST (not GET) because the search criteria includes lists (rendering/billing
NPIs) that don't fit cleanly in query strings.

A few real, deliberate behavior differences between these six endpoints are
preserved rather than unified:

- **Year/claim-class defaulting differs per endpoint.** `GetPending` and
  `GetSubmitted` only default `AHAYear` when the caller sends `0`, then
  derive `ClaimClass` from `AHAYear` via `SELECT ClaimClass FROM AHAYears
  WHERE AHAYear = @Year` (ported as a plain parameterized query, not a
  stored procedure, matching legacy). `GetPendingSpecialCover` and
  `GetRejected` **always** override both `AHAYear` and `ClaimClass` to the
  app-wide current-year constants, ignoring whatever the caller sent.
  `GetInProgress`/`GetInProgress2021` never touch `ClaimClass` at all — it
  passes straight from the caller to the stored procedure.
- `GetRejected` also always sends `MaxDayToResubmit` from the same kind of
  app-wide constant.
- `GetSubmitted` derives `StatusText`/`StatusTextToolTip`/`CanEdit`/
  `CanViewRejectNotes` from `StatusCode`, `RejectTypeID`, and
  `PaymentStatus` with real branching logic (e.g. status `"3"` +
  `RejectTypeID = 2` → "Administrative Denied"; status `"4"` +
  `PaymentStatus = "PAID"` → a formatted tooltip with check number/amount/
  date). Ported verbatim in `ClaimSearchService.MapSubmittedItem`.

Two things ported as reasonable-but-unconfirmed placeholders, worth
verifying once possible:

- **`AppShared.AHAVersion`/`AHAClaimClass`/`MaxDayToResubmit`** — app-wide
  constants from the unavailable `AppShared` class. Modeled as
  `AhaSearchOptions` (`AhaSearch:DefaultYear`/`DefaultClaimClass`/
  `MaxDayToResubmit` in configuration) rather than guessed values — set the
  real numbers there once known.
- **The `RenderingNPIs`/`BillingNPIs` table-valued parameter type name** is
  sent as `"Providers"`. The VB source doesn't set `SqlParameter.TypeName`
  here either (same gap as the three Page 1/4 sections that were blocked
  earlier), but `Providers` is the only user-defined table type in the
  `TVP.csv` data with a single `NPI varchar(10)` column, which is exactly
  the shape these inline `DataTable`s build (`.Columns.Add("NPI")`) — a
  strong inference, not a confirmed one, since `uspAHA_GetPending` and
  friends weren't part of the stored procedure definitions provided so far.
- Column types coming back from these list stored procedures also aren't
  confirmed (same reason). Row-mapping uses `Convert.ToXxx` on the boxed
  `DbDataReader` value instead of a strict typed `reader.GetXxx` call, to
  mirror VB's forgiving `CLng`/`CBool`/`CDate` conversions rather than risk
  an `InvalidCastException` on a type guess that turns out wrong.

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

**`SaveClaim` (all 4 pages) is now fully ported** — every section that was
previously blocked on the table-valued-parameter gap (Medication
List/Allergies Medication List, Diseases of the Skin, Other Condition) is
unblocked now that the real stored procedure and table-type definitions
are available. What's still open:
- `AppShared.GetDefaultRejectCode()`/`GetRejectCodeDescription()` (used by
  Other Condition) are still unavailable — currently taken as
  caller-supplied input (see above). Worth revisiting if/when that source
  turns up, in case they're not actually constant.
- The `Globals` class source (at least `ValidateDateMinMaxRange`,
  `LogError`, `ValidatePayerID`) if exact legacy behavior matters beyond
  what's already been reasonably approximated (SQL `datetime` min/max used
  as a stand-in).
- **Page 1 and Page 3 always send every parameter** to their stored
  procedures (relying on Dapper to convert an absent value to `NULL`),
  where legacy instead *omits* many parameters entirely when their field
  is absent. Now that real SP definitions are available, this is worth
  checking systematically for any parameter with a non-`NULL` default
  (`@Foo BIT = 0`) where an explicit `NULL` would override that default
  differently than an omitted parameter would.

`AHADataAdapter.vb`/`SaveClaim` is done. The migration has now moved on to
`AHAService1.vb` (see above) — claim list/search is ported; next up, roughly
in order of how implementable each is right now:

1. **Provider/billing/eligibility lookups and Dx/condition operations** —
   moderate size, no external dependencies spotted yet, similar shape to
   what's already ported. Good next candidates.
2. **Addendum handling** (`SaveAddendumProvider`, `GetAddendumInfo`,
   `LoadCodUserAndNotes`, `LoadRejectedCodes`, `LoadQuestions`) — needs a
   closer read first.
3. **The full read side of a claim** (`GetAHA`, `GetMemberAHATemplate`,
   `GetAHAShort`) — not blocked, just large (~5,000 lines combined);
   probably its own multi-batch effort the same way `SaveClaim` was.
4. **Submit/Update/PartialSave orchestrators** — the session/business-rule
   layer wrapped around the already-ported `SaveClaim` call.

Confirmed not implementable yet, without more source:
- **Login/SingleSignOn** — needs the external `AUS.Library`/
  `AUSAuthentication` assemblies.
- **`GetPRAIReport`** — needs the external `AUS.MMM.PRAI.Library` assembly.
- **All `GetAHAReport*`/report-generation methods (~40% of the file)** —
  needs the actual `.rdlc` report definitions, and RDLC itself
  (`Microsoft.Reporting.WebForms.LocalReport`) isn't natively usable in
  .NET Core the way it was in .NET Framework. Worth a dedicated
  conversation about the replacement approach (a different PDF library?
  keep report generation on the legacy service longer?) rather than solving
  it inline.

Two longer-standing open questions, still deliberately deferred:
- How the "3 different front-end forms sharing one database" reality
  (raised in chat — some sections legitimately don't apply to every form
  variant) should shape the new API's shape.
- The repository/EF question.

Also useful, if available: the `.asmx`/WSDL for the SOAP service itself, to
confirm which `AHAService1` methods are actually exposed as SOAP
operations (`IAHAService12` answers this now, since it was provided
alongside `AHAService1.vb`).

## Running locally

Requires the .NET 10 SDK and a SQL Server instance reachable via the
`ConnectionStrings:ChopperDb` setting (see `appsettings.Development.json`).

```
cd Chopper
dotnet run --project src/Chopper.Api
```
