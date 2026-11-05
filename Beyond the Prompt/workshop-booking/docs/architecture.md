# Architecture

## Status and purpose

This document defines the intended architecture of the Workshop Booking API.
The application has not been scaffolded or verified yet. Establish these
boundaries during scaffolding and update this document when a decision changes.

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
- Use a small, typed application outcome for expected failures: validation,
  missing resources, and business conflicts. Agree its shape during scaffolding
  before implementing endpoints; reuse it across features.
- API maps those outcomes consistently. Central exception handling handles
  unexpected failures; do not expose stack traces or database details.
- Persistence translates recognized constraint failures into the agreed
  application outcomes. Do not classify every database exception as a conflict.

`api-conventions.md` will define precise status codes, error bodies, validation
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
3. Seed demo data through an initialization scope.
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
Feature and API documentation will define normalization semantics.

Availability is derived from workshop capacity and current registrations. Do
not maintain an independent mutable available-seat count.

A capacity check followed by an unprotected insert can overbook a workshop.
Persistence must protect capacity-dependent registration checks and writes as
one atomic operation. During scaffolding, select and document a SQLite-compatible
transaction or serialization strategy; verify it with competing requests before
claiming concurrent booking correctness. A unique constraint prevents duplicate
registrations but does not enforce workshop capacity.

Do not concurrently execute commands on one shared connection. The chosen
connection and synchronization strategy must account for independent request
contexts, keep transaction ownership clear, and avoid partial writes.

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

Supporting documents are being created separately. Missing documents are gaps
to report, rather than evidence of an established contract.
