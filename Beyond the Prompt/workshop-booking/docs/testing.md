# Testing

## Status and scope

This document defines the testing approach for the Workshop Booking API. The
unit and integration suites now contain tests for Application outcomes and
shared HTTP infrastructure, domain invariants and SQLite persistence. Isolated
database fixtures, baseline feature acceptance tests and verified unit coverage
enforcement are implemented. See [the acceptance audit](acceptance-audit.md).

Use xUnit v3 with its VSTest adapter for both test projects and
`WebApplicationFactory<Program>` for HTTP integration tests. Keep `dotnet test`
on the VSTest runner because Coverlet's VSTest collector does not support
Microsoft Testing Platform v2. The package family is `xunit.v3.mtp-off`,
`xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`,
`Microsoft.AspNetCore.Mvc.Testing`, and `coverlet.collector`. As checked against
NuGet on 2026-11-05, compatible stable versions are xunit.v3.mtp-off 4.0.2,
xunit.runner.visualstudio 4.0.1, Microsoft.NET.Test.Sdk 18.11.0,
Microsoft.AspNetCore.Mvc.Testing 10.0.14, and coverlet.collector 10.1.0. Select
the VSTest runner in `global.json`; `xunit.v3.mtp-off` disables MTP while
retaining xUnit v3 support through the VSTest adapter. The plain xunit.v3 4
package selects MTP v2, which the .NET 10 SDK rejects under VSTest. Coverlet
documents that its [collector is incompatible with MTP v2](https://www.nuget.org/packages/coverlet.collector/10.1.0).
These package versions restore successfully. The unit project
also includes the Coverlet collector; coverage collection has not been run.

## Test boundaries

| Project | Verifies | Dependencies |
| --- | --- | --- |
| `WorkshopBooking.UnitTests` | Domain invariants and application use cases | Real domain/application code; controlled clock and persistence test doubles |
| `WorkshopBooking.IntegrationTests` | Entire HTTP endpoint behavior and stored effects | Real API pipeline, application services, EF Core, and SQLite in-memory |

Unit tests must run without an ASP.NET Core host, real database, network service,
or filesystem dependency. Use focused fakes or mocks for Application persistence
interfaces. Assert meaningful outcomes and interactions such as avoiding writes
after rejection; avoid coupling tests to private methods or incidental call order.

Integration tests send requests through `HttpClient` from the application factory.
Do not replace application services or persistence with mocks. Calling controller
methods directly is not endpoint integration coverage. Direct database access is
allowed for fixture setup and independent verification of stored effects.

## Deterministic fixtures

- Each test owns its state. Default to a fresh factory/database per test; a shared
  fixture is acceptable only with a documented reset and exclusive access.
- Open the SQLite owning connection before schema creation and keep it alive
  until that test's database is disposed. Dispose clients, scopes, contexts,
  factories, and owned connections reliably.
- A named in-memory database, if used, must have a unique name per test. Do not
  reuse the running demo database or share connections between independent tests.
- Replace the application database registration and `TimeProvider` before host
  initialization. Ensure schema creation and any startup seeding use the test
  configuration; the production database registration must not remain active.
- Use explicit fixture data instead of production demo seeds by default. Keep
  seeding tests separate and run them with the real seeder and controlled time.
- Use a fixed UTC instant, stable fixture IDs, and explicit timestamps. Do not
  use sleeps, wall-clock dates, random data, or execution order to obtain a pass.
- Do not assert database result order unless the query defines it. Include tied
  primary sort values to verify the documented ID tie-breaker.

Separate databases allow independent tests to run in parallel. A shared mutable
fixture requires local serialization, not disabling parallelism for every test.
Within one concurrency test, preserve simultaneous requests to the same database
so the test exercises the application's synchronization strategy.
For deterministic contention, use a test-controlled mutation coordinator with
signals for acquired/waiting leases. Hold the first lease, start all competing
requests, await confirmation that they are waiting, then release the lease.
Bound each wait with a timeout and release/dispose in `finally`; do not use
`Task.Delay`, sleeps, or an external test lock that prevents requests from
reaching the application coordinator. The production mutation coordinator and
shared-connection I/O guard are process-local; each test factory owns a separate
connection and coordinator.

## Behavior to cover

Feature acceptance criteria in `features/<feature>.md` determine the complete
scenario set. Use the following baseline to guide those specifications and tests:

| Area | Representative scenarios |
| --- | --- |
| Workshop creation | Valid input, invalid capacity, future start requirement, required text |
| Workshop reads | Existing/missing ID, derived available seats, topic filtering and ordering |
| Participants | Creation and reads, invalid email, trim/display behavior, case-insensitive uniqueness |
| Registration | Success, missing references, duplicate, full workshop, started workshop |
| Cancellation | Removal, freed seat, missing registration, repeated cancellation |
| Failure precedence | Invalid fields before lookup; workshop before participant; start before duplicate before capacity |
| Persistence integrity | Foreign keys, normalized-email uniqueness, registration-pair uniqueness, rejected writes leave state unchanged |
| Seeding | Stable IDs, dates based on one clock instant, internally consistent scenarios |

Test time-sensitive comparisons immediately before, exactly at, and immediately
after the relevant boundary. Creation requires a future start; registration is
rejected at or after workshop start. Future features define end-time behavior
in their acceptance criteria before implementation.

Use parameterized tests where inputs exercise the same rule. Keep distinct
business rules visible rather than hiding unrelated cases in one large theory.
Do not duplicate every unit scenario through HTTP; integration tests must cover
every endpoint, its applicable failure categories, and transport-specific behavior.

## HTTP assertions

Follow [API conventions](api-conventions.md). Verify:

- Status, response DTO fields, media type, and relevant headers. Compare media
  types without failing on a legitimate charset parameter.
- Workshop/participant creation returns `201` and a working `Location`; follow
  that URL to verify the stored resource. Registration creation returns its ID
  without advertising an unimplemented GET-by-ID route.
- Registration creation changes the persisted count and available seats.
  Cancellation returns an empty `204` response, removes the row, and frees a seat.
- Problems contain the agreed `status`, stable `code`, corresponding `type`,
  request-path `instance`, and a nonempty opaque `traceId`. Validate required
  fields without asserting exact prose or exact trace values.
- Validation includes public camelCase field keys or `$` for body-level errors.
  Cover missing/null fields, malformed JSON, incompatible types, malformed/empty
  GUIDs, and missing required query parameters.
- Well-formed missing resource IDs produce `404`; malformed IDs produce `400`.
  Existing collections with no matches return `200` and `[]`.
- Automatic validation, unmatched API routes, unsupported methods, and unsupported
  media types follow the same problem conventions. Verify `Allow` for `405`.
- UTC output uses `Z`; explicit input offsets represent equivalent instants;
  timestamps without an offset are rejected.

Use a narrow test-only failure injection to exercise unexpected-error handling,
while keeping normal endpoint tests on real services. Assert a generic `500`
problem without leaked internals. Do not add a production error-trigger endpoint.

Inspect mutation results with a fresh context or subsequent HTTP read to avoid
mistaking EF tracking state for saved data. Rejected mutations must leave counts
and relevant records unchanged.

## Concurrent mutations

Use HTTP integration tests for competing registrations against one remaining
seat. Both requests must reach the application without a test-side lock that
serializes them. Expect one success, one `workshop_full` conflict, and one new
registration. Use distinct unregistered participants and a future workshop.

Also verify competing duplicate registrations and case-variant participant
creation: one creation succeeds, the other returns the documented conflict, and
only one matching row exists. Use bounded coordination where necessary to make
the contested overlap reliable; do not rely on sleeps or repeated lucky runs.
These tests complement direct constraint checks and unit tests of business rules.

## Coverage requirement

Unit tests alone must produce **strictly greater than 90% line coverage for each
of Domain and Application separately**. A combined average, integration coverage,
or a rounded display value cannot establish compliance. Exactly 90% fails.

The gate must read the unit-test report, identify both assemblies explicitly,
and compare covered lines to valid lines without rounding. Fail on a missing or
empty report, a missing assembly, zero measurable lines, a failed test run, or
either assembly at or below the threshold. Never reuse stale coverage output.

Use PowerShell 7 with the pinned .NET 10 SDK, VSTest and coverlet.collector 10.1.0. Run from the repository root:

```powershell
pwsh -NoProfile -File scripts/Test-UnitCoverage.ps1
pwsh -NoProfile -File scripts/Test-CoverageGate.ps1
```

The first command is the enforcing gate; the second independently verifies its logic with 19 synthetic checks. scripts/Coverage.psm1 owns report validation and orchestration. The gate runs only tests/WorkshopBooking.UnitTests with the OpenCover collector. It uses a new GUID directory under artifacts/coverage/unit per invocation, prints that location and never reads sibling/stale directories or deletes unrelated artifacts. Integration tests remain a separate command and cannot improve these measurements.

Every coverage.opencover.xml under the current run is parsed with DTDs prohibited. Each expected assembly is explicitly matched by ModuleName. Sequence points are expanded from sl through el and deduplicated by full source path and line, across methods/modules/reports; a line is covered when any corresponding visit count is positive. File IDs are local to each module, not global source identities. The gate compares raw counts with decimal integer arithmetic: covered * 100 > valid * 90. Printed percentages do not decide compliance.

Missing/empty/malformed/unusable reports, malformed sequence points or file maps, missing assemblies and zero measurable lines fail. A failing unit-test process fails even if reports exist. Per-assembly counts and percentages are printed before threshold rejection. Multiple reports are merged by source line without double counting. Exactly 90% fails, including when the other assembly makes a combined average pass. PowerShell exits nonzero on every rejected run.

No source-code exclusions were added. Compiler-generated artifacts are governed by the collector defaults; ordinary entities, policies, services, validation and outcomes are measured. Both required assemblies are measurable. Inspect the printed run directory's XML (Files, Methods, SequencePoints and BranchPoints) to locate vc=0 points. Branch coverage is informative and has no numeric gate.

Exclude generated code only through explicit, justified configuration. Do not
exclude ordinary business code, remove tests, weaken assertions, or add tests
that merely execute lines to reach the target. Inspect uncovered branches and
missing acceptance scenarios; branch coverage is useful but has no separate
numeric threshold in the baseline.

## Verification commands

Run from the repository root:

```bash
dotnet restore
dotnet build --no-restore
dotnet test tests/WorkshopBooking.UnitTests
dotnet test tests/WorkshopBooking.IntegrationTests
dotnet test
dotnet format --verify-no-changes
pwsh -NoProfile -File scripts/Test-CoverageGate.ps1
pwsh -NoProfile -File scripts/Test-UnitCoverage.ps1
```

With `coverlet.collector` configured in the unit-test project, collect OpenCover
data with:

```bash
dotnet test tests/WorkshopBooking.UnitTests --collect:"XPlat Code Coverage" --results-directory artifacts/coverage/unit -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

This collector-only command is for inspection; it does not enforce the threshold. Use the enforcing command above for completion. All reports live under ignored artifacts.

## Test-first work and completion

When the assigned task or selected workflow requires test-first development,
write the acceptance tests, run them, and confirm they fail because the requested
behavior is absent. Build errors, missing packages, or broken fixture setup do
not establish that behavioral failure. Stabilize the agreed contracts and test
infrastructure before collecting red-stage evidence.

Implement the behavior, rerun affected tests, and preserve their behavioral
assertions during review. Resolve a test/specification disagreement explicitly;
do not weaken a test merely to obtain green results.

This document requires no specific roles, subagents, skills, or review pipeline.
Partial tasks report their scoped checks and remaining integration work. The
completed application requires formatting, build, both test suites, and the
coverage gate to pass. Report commands actually run and checks that remain
unverified. Rerun affected checks after fixes; run final full verification on the
combined result after integrating concurrent changes.

## Database fixtures and acceptance evidence

InfrastructureTestApplicationFactory creates an isolated SQLite owner and ControlledTimeProvider before startup. Normal fixtures disable seeding; dedicated DemoSeedTests opt in. WithDatabaseAsync creates a fresh scope/context and holds the database I/O guard for setup or readback. Do not nest it or invoke a guarded repository inside its callback. Dispose factories, scopes, contexts and clients; connection ownership is tested for both synchronous and asynchronous factory disposal.

The optional observing coordinator forwards acquisition to the real MutationCoordinator. Contention tests wait for both HTTP requests at the production gate using bounded TaskCompletionSource coordination, then release the held lease. No sleeps or test-side request serialization authorize writes. They verify final-seat, duplicate, cancellation/mixed outcomes, queued time capture, fresh persisted counts and HTTP availability.

See [acceptance-audit.md](acceptance-audit.md) for all 86 stable IDs and the reviewed assertions. PersistenceTests covers constraints, UTC ticks, Unicode keys, resource lifetimes and isolation. DemoSeedTests covers exact IDs/offsets, one startup instant, seed integrity, no reseeding after mutations, fresh-host restoration and OpenAPI contracts. HttpInfrastructureTests covers shared errors, JSON/date handling, Swagger environment and absence of test-only controllers in the normal host.

The verified gate measures Domain 91/91 lines (100%) and Application 200/202 (99.0099%). The remaining uncovered lines defend internal Outcome construction against states public factories cannot create. No coverage exclusions or artificial reflection-only tests were added. Final completion still requires rerunning the gate on the current tree.
