# Architecture

## Status and purpose

This document defines the intended architecture of the Workshop Booking API.
The baseline API, domain model, persistence, demo seeding and verification tooling
are implemented. Preserve these boundaries and update this document when a decision changes.

The application is one ASP.NET Core process with four application projects and
an ephemeral relational database. Keep the design small enough to explain during
the presentation and extend with independent features.

## Project boundaries

References below are direct project references. Framework and package references
must also respect the responsibilities of each layer.

| Project | May reference | Owns |
| --- | --- | --- |
| `WorkshopBooking.Domain` | No other application project | Entities and rules that depend only on domain state |
| `WorkshopBooking.Application` | Domain | Use cases, application contracts, persistence interfaces, business orchestration |
| `WorkshopBooking.Persistence` | Application, Domain | EF Core context, mappings, queries, writes, schema initialization, seeding |
| `WorkshopBooking.Api` | Application, Persistence | HTTP controllers and contracts, HTTP error mapping, application composition |

The API references Persistence to register its implementations at startup.
Controllers depend on Application contracts; they do not use `DbContext`, EF Core,
SQL, or concrete persistence implementations.

Domain and Application must not depend on ASP.NET Core or EF Core. Persistence
must not depend on the API or expose HTTP results. Domain must not depend on
Application. Keep these constraints even when a selected agent workflow defers
detailed coding review.

Unit tests reference Domain and Application. Integration tests reference the API
and may reference Persistence to configure and inspect the test database.

## SDK and package selections

Target `net10.0` and pin the SDK to installed stable SDK `10.0.302` in root
`global.json`, with `latestPatch` roll-forward restricted to that feature band
and prereleases disabled. Final verification used SDK 10.0.302.
Configure `global.json` test runner as `VSTest` because the selected Coverlet
collector uses VSTest. Use a classic `.sln` for compatibility with the
documented `dotnet sln` and `dotnet test` commands.

Selected package versions (restored and built during final verification):

| Project | Package | Version | Purpose |
| --- | --- | --- | --- |
| Persistence | [`Microsoft.EntityFrameworkCore.Sqlite`](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite/10.0.14) | `10.0.14` | EF Core and SQLite provider, aligned with .NET 10 servicing |
| API | [`Swashbuckle.AspNetCore`](https://www.nuget.org/packages/Swashbuckle.AspNetCore/10.3.1) | `10.3.1` | OpenAPI generation and Swagger UI |
| Both test projects | [`xunit.v3.mtp-off`](https://www.nuget.org/packages/xunit.v3.mtp-off/4.0.2) | `4.0.2` | xUnit v3 without MTP, for VSTest/Coverlet compatibility |
| Both test projects | [`xunit.runner.visualstudio`](https://www.nuget.org/packages/xunit.runner.visualstudio/4.0.1) | `4.0.1` | xUnit VSTest adapter |
| Both test projects | [`Microsoft.NET.Test.Sdk`](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/18.11.0) | `18.11.0` | `dotnet test` / VSTest host |
| Integration tests | [`Microsoft.AspNetCore.Mvc.Testing`](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Testing/10.0.14) | `10.0.14` | `WebApplicationFactory<Program>` |
| Unit tests | [`coverlet.collector`](https://www.nuget.org/packages/coverlet.collector/10.1.0) | `10.1.0` | OpenCover line-coverage collection |

Domain and Application need no third-party packages for the baseline. Do not
add both a separate OpenAPI generator and Swashbuckle without a demonstrated
need. xUnit v3's default package enables MTP v2, which the .NET 10 SDK refuses
to run under VSTest. The scaffold therefore uses xunit.v3.mtp-off alongside
`xunit.runner.visualstudio`; this demonstrated incompatibility is why the
package differs from the earlier generic xunit.v3 candidate.

The root `Directory.Build.props` enables .NET analyzers, `AnalysisLevel` 10
recommended, build-time code-style analysis, nullable annotations, deterministic
builds, and warnings as errors. Preserve those settings. The existing formatter
configuration is active as root `.editorconfig`, and `.gitignore` is active at
the standard root path. Use `dotnet build` for compiler/analyzer diagnostics
that run during builds and `dotnet format --verify-no-changes` for formatting
and supported style checks. `IDE0055` is covered by format verification;
`IDE1006` naming analysis is explicitly not guaranteed as a command-line build
check. The scaffold and infrastructure were built/formatted with SDK 10.0.302;
do not weaken severities to accommodate configuration issues.

## Request handling

Controllers bind requests, perform transport-level validation, invoke an
application use case, and translate its outcome into the HTTP contract.
Keep business decisions out of controllers.

Application services coordinate loading state, checking business requirements,
applying domain behavior, and persisting changes. Domain entities enforce local
invariants; requirements involving stored records belong in application use cases
with persistence support.

For example, registering a participant requires checking that the participant
and workshop exist, that the workshop has not started, that a seat is available,
and that no duplicate registration exists. These checks and the write form one
use-case operation. Feature documents define the exact rules and boundaries.

Use asynchronous persistence operations and pass cancellation tokens through
the request path. Read-only operations must not mutate stored state.

## Contracts and failure handling

- API owns HTTP request and response DTOs. Do not serialize EF entities directly.
- Application contracts describe use-case inputs, outputs, and expected failures
  without referencing HTTP status codes or ASP.NET Core types.
- Use the small generic `Outcome<T>` and `ApplicationFailure` model in
  Application for success, field validation, missing resources, and conflicts.
  Use `Outcome<Unit>` for successful operations with no response value. The
  factories prevent callers from constructing contradictory success/failure
  states; reuse these types across features.
- API maps those outcomes consistently. Central exception handling handles
  unexpected failures; do not expose stack traces or database details.
- Persistence translates recognized constraint failures into the agreed
  application outcomes. Do not classify every database exception as a conflict.

### Application outcome shape

Use a small generic outcome with either a value or one typed failure. Failures
carry a stable application code and, for field validation, a collection keyed by
application field name. The failure categories are validation (possibly several
field errors), missing resource, and business conflict. Avoid exceptions for
expected outcomes, HTTP status codes in Application, and a hierarchy of result
wrappers. API DTOs and HTTP problem bodies remain API-owned. Persistence may
return recognized uniqueness/capacity conflicts through the same outcome
vocabulary; unknown database errors propagate to the API exception handler.

`api-conventions.md` defines precise status codes, error bodies, validation
format, and date representation. Do not invent separate conventions per feature.

## Persistence boundaries

Define focused persistence interfaces in Application and implement them in
Persistence. Expose the operations needed by use cases; avoid a generic repository
framework or an extra unit-of-work wrapper without a concrete need.

Do not expose `DbContext`, `DbSet`, `IQueryable`, EF-specific types, or an open
connection through application interfaces. Persistence owns query execution,
entity mappings, and database transaction details.

Use a scoped EF Core context for request work. The connection owner and the
context have different lifetimes: keeping the database alive does not justify
making `DbContext` a singleton or sharing it across requests.

### Database lifecycle

Use EF Core with SQLite in-memory for the demo and HTTP integration tests.
Do not substitute EF Core's InMemory provider.

At startup, initialize the database before accepting API requests:

1. Create and open the connection that owns the in-memory database lifetime.
2. Create the schema for this fresh database.
3. Capture one startup instant and seed the documented demo scenarios transactionally.
4. Keep the owning connection open until the application shuts down.

Use EF Core `EnsureCreated` for this disposable baseline. Do not mix schema
creation with a migration workflow. Persistent storage and migrations require a
separate architecture decision if they become part of the scope.

Stopping the application discards all data. A restart creates a fresh seeded
database. Do not add a reset endpoint or persist data to disk for convenience.

### Integrity and concurrent requests

Configure foreign keys and database uniqueness constraints for normalized
participant email and the participant/workshop registration pair. Keep original
email display values separate from their consistently normalized lookup keys.
For both email and topic, derive keys through the shared Domain ParticipantPolicy accessible to Application using `ToUpperInvariant()`
and persist them with SQLite's default binary equality/unique index. Do not use
SQLite `NOCASE`, whose built-in case folding is ASCII-only. Topic filtering is
an exact lookup against that key; responses retain the submitted display topic.
This defines culture-independent case-insensitivity without provider-specific
collations. Keep ordering explicit and use ordinal application ordering for
normalized keys where SQLite's text ordering is not the stated contract.

Availability is derived from workshop capacity and current registrations. Do
not maintain an independent mutable available-seat count.

A capacity check followed by an unprotected insert can overbook a workshop.
Use one application-wide `SemaphoreSlim` mutation coordinator for every
operation that can change workshop, participant, or registration state. A
registration holds it across authoritative workshop-then-participant lookup,
start/duplicate/capacity decisions, and insertion; cancellation and participant
or workshop creation use the same coordinator. Capture `TimeProvider.GetUtcNow()`
once only after acquiring it, and use that instant for both the start comparison
and `createdAt`. This preserves failure precedence and makes the serialized
operation's state authoritative.

The in-memory SQLite connection is one singleton owner kept open until host
shutdown. Each request still gets its own scoped `DbContext`, configured with
that connection. A second singleton async database-I/O semaphore guards every
database operation so commands never overlap on the shared connection; the
mutation coordinator is held across a multi-operation mutation, while the I/O
semaphore is acquired and released around each individual EF operation. Keep
these gates distinct to avoid reentrant-lock deadlocks. All application database
access must go through the guarded persistence implementation. This deliberately
serializes database I/O within the single-process demo; it is not a multi-process
or horizontally scaled locking scheme. Database uniqueness and foreign keys
remain defense in depth. Map only recognized unique constraints; unexpected
SQLite lock or I/O failures are not `workshop_full` or another business conflict.
The registration/cancellation writes each change one row, so the serialized
check-then-single-write use case is atomic to other application writers without
an explicit SQLite transaction. Its write is the operation's linearization
point. If a future mutation spans multiple row writes, revisit transaction
ownership in Persistence rather than extending this assumption.

Expose focused Application persistence boundaries: workshop create/list/get;
participant create/list/get and normalized-email existence; registration
lookup/list/insert/delete. Application owns the registration decision flow and
acquires an Application-level mutation lease before authoritative lookups; it
holds that lease through persistence of the resulting mutation. Persistence
implements the lease with the singleton async mutation coordinator registered
at the composition root, while guarding each individual database operation with
the separate shared-connection I/O semaphore. This keeps business precedence in
Application, the process-local consistency boundary explicit, and EF Core/SQL
out of Application. Tests can replace the coordinator with a controlled
implementation to coordinate waiting operations without sleeps.

## Time and seed data

Register `TimeProvider` through dependency injection. Business use cases obtain
current time from it; domain methods may receive the captured instant explicitly.
Do not read the system clock directly inside business rules.

Capture one current instant per time-sensitive operation so its decisions use
the same reference point. Tests supply controlled time. Feature specifications
define behavior exactly at workshop start and end boundaries.

Seed IDs are stable across restarts. Capture one startup instant and calculate
all seed dates from it. Seed upcoming workshops with free seats, a full upcoming
workshop, and a completed workshop, with consistent participants and registrations.
Completed workshops may be seeded directly without invoking creation rules that
require a future start time.

Seeding belongs to Persistence and runs during initialization. Request handlers
must not initialize or reseed the database. Integration tests control their own
fixtures rather than relying on a changing startup date or another test's writes.

## Feature extensions and shared files

Feedback and waiting-list controllers are later exercises, outside the baseline.
Their eventual implementations use the same layer boundaries and existing
workshop, participant, and registration contracts. Automatic waiting-list
promotion is outside the planned initial extension.

Keep feature-specific services and persistence mappings in their owning projects.
Discover EF entity configurations from the Persistence assembly so adding a
feature does not require editing a central mapping list. Each layer can expose
a small service-registration entry point; the API remains the composition root.

Before concurrent work, agree shared contracts and coordinate edits to startup,
service registration, context configuration, and seed data. These are ordinary
shared integration points. The architecture requires no particular agents,
roles, skills, worktrees, or delegation workflow.

## Scope and related documents

Authentication, caching, external infrastructure, a frontend, and background
processing are outside the baseline. Use direct application services; add extra
frameworks or architectural layers only when an assigned requirement needs them.

- [README](../README.md): setup, Swagger, commands, and demo operation.
- [Project guidance](../AGENTS.md): shared requirements and documentation defaults.
- `api-conventions.md`: HTTP behavior and error mapping.
- `testing.md`: fixtures, isolation, coverage, and test commands.
- `coding-guidelines.md`: detailed review guidance.
- `features/<feature>.md`: business rules and acceptance scenarios.

Supporting documents are present in this repository. Report missing referenced
documents rather than assuming their contents.

## Persistence representation and initialization

DateTimeOffset values use INTEGER UTC ticks (100 ns), preserving instant comparisons and ordering. Topic and email keys use trimmed ToUpperInvariant with BINARY equality. ParticipantStore orders normalized email in managed code with StringComparer.Ordinal because SQLite UTF-8 text order differs from .NET UTF-16 ordinal order for some Unicode values. GUID tie-breaks use canonical text order. Domain entities have private constructors/setters; internal Restore factories validate durable invariants while allowing historical schedules for seeding.

DemoSeed owns the code-defined baseline in README. BookingDatabase opens its connection and runs EnsureCreated inside the I/O gate. It captures one TimeProvider instant and inserts seed rows in one SQLite transaction before requests are accepted. Its completion flag is set only after commit, and later initialization is a no-op even after cancellation. Schema creation is separate from the seed transaction; failed seeding aborts startup. Ordinary test factories disable demo seeding and own their replacement connection; seeded tests opt in explicitly.

ConstraintFailures recognizes SQLite extended UNIQUE code 2067 by affected entity type. Participant and Registration each currently have one non-primary unique index. Primary-key, foreign-key, lock and unrelated errors propagate. Adding another unique index requires revisiting this classification; it does not parse exception messages.

## Coordination points for later demos

Feedback and waiting-list work belongs in the same Domain/Application/Persistence/API layers. Existing focused IWorkshopStore, IParticipantStore and IRegistrationStore contracts provide current lookup and mutation needs. Extend a contract only with an implemented and tested operation. EF configurations are discovered from the Persistence assembly; add each new configuration there, plus scoped registrations in PersistenceServices and controller metadata in BaselineOpenApiFilter.

All new writers must acquire IMutationCoordinator before authoritative reads and use the separate BookingDatabase I/O guard around EF calls. Waiting-list promotion could require multiple writes, so define transaction ownership and failure precedence before implementing it. Update constraint classification if new unique indexes are introduced. Use isolated fixtures and controlled clocks for new tests; coordinate changes to the context, service registration, seed relationships and baseline OpenAPI filter. No future entities, routes or interfaces are included in this baseline.
