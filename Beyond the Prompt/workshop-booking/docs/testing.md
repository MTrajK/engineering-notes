# Testing

## Status and scope

This document defines the intended testing approach for the Workshop Booking API.
Application projects and test infrastructure have not been scaffolded or verified
yet. Commands below require the corresponding projects and packages.

Use xUnit for both test projects and `WebApplicationFactory<Program>` for HTTP
integration tests. Configure unit coverage with Coverlet's collector and a
repository-owned coverage gate during scaffolding. Pin compatible package
versions then; this document does not claim those tools are already installed.

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

Exclude generated code only through explicit, justified configuration. Do not
exclude ordinary business code, remove tests, weaken assertions, or add tests
that merely execute lines to reach the target. Inspect uncovered branches and
missing acceptance scenarios; branch coverage is useful but has no separate
numeric threshold in the baseline.

## Commands and scaffolding checklist

Run from the repository root after scaffolding:

```bash
dotnet restore
dotnet build --no-restore
dotnet test tests/WorkshopBooking.UnitTests
dotnet test tests/WorkshopBooking.IntegrationTests
dotnet test
dotnet format --verify-no-changes
```

With `coverlet.collector` configured in the unit-test project, the intended
collection command is:

```bash
dotnet test tests/WorkshopBooking.UnitTests --collect:"XPlat Code Coverage" --results-directory artifacts/coverage/unit
```

This command collects coverage; it does **not** enforce the threshold. During
scaffolding, implement and record one exact gate command here that clears stale
output, runs unit tests with coverage, checks both assemblies, and returns a
nonzero exit code on failure. Verify the gate with below-threshold, exactly-90%,
missing-report, and passing inputs. Coverage enforcement remains pending until
that command exists and has been verified; `dotnet test` alone is insufficient.

Keep artifact output outside source control. Record the chosen test runner,
collector compatibility, coverage report location, and any reset-fixture procedure
here when configured. Do not invent a working command for an absent script.

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
