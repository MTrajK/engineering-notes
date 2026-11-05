# Workshop Booking API

A small ASP.NET Core API for managing workshops, participants, and registrations.
Built as the demo application for **Beyond the Prompt: Engineering Your Codex
Workflow**, demonstrating approaches such as test-first implementation and
parallel coding agents. These workflows are optional.

## Project status

The repository is being defined one file at a time. This README describes the
intended application; the application projects, configuration, seed data, tests,
and supporting documentation have not been scaffolded or verified yet.

Commands below become usable once their corresponding projects exist. Swagger
paths and the SDK selection must be confirmed during scaffolding.

## Scope

The baseline API supports:

- Creating and viewing workshops, including capacity and available seats.
- Creating and viewing participants with unique email addresses.
- Registering participants, listing registrations, and cancelling registrations.
- Validating capacity, duplicate registrations, and workshop start times.

Feedback and waiting-list features are reserved for later demo exercises. They
are not part of the initial implementation.

Authentication, caching, external services, and a frontend are outside the
baseline scope. Swagger UI provides the interactive interface for API requests.

## Prerequisites

- .NET 10 SDK: the proposed target for the application.
- Git for version control; worktrees are used only in demos that need them.

The SDK version will be pinned in `global.json` during scaffolding. No external
database server or Docker container is required.

## Run locally

Run commands from the repository root:

```bash
dotnet restore
dotnet build --no-restore
dotnet run --project src/WorkshopBooking.Api
```

Use the listening address printed in the startup output. For a predictable local
demo address, start the application with explicit development settings:

```bash
dotnet run --project src/WorkshopBooking.Api --no-launch-profile -- --environment Development --urls http://localhost:5080
```

The intended Swagger UI address for that command is
[http://localhost:5080/swagger](http://localhost:5080/swagger).
Open it, expand an endpoint, select **Try it out**, and execute a request.

## Baseline endpoints

These routes are the planned contract. The running application's OpenAPI document
will describe implemented request and response schemas.

| Method | Route | Purpose |
| --- | --- | --- |
| GET | `/api/workshops` | List workshops; optionally filter by topic |
| GET | `/api/workshops/{id}` | Get workshop details and available seats |
| POST | `/api/workshops` | Create a workshop |
| GET | `/api/participants` | List participants |
| GET | `/api/participants/{id}` | Get a participant |
| POST | `/api/participants` | Create a participant |
| GET | `/api/registrations?workshopId={id}` | List registrations for a workshop |
| POST | `/api/registrations` | Register a participant for a workshop |
| DELETE | `/api/registrations/{id}` | Cancel a registration |

## Demo data and reset

The application uses EF Core with SQLite in-memory. Its owning connection stays
open for the application's lifetime. Data is temporary: stopping the process
discards changes, and starting it again creates and seeds a fresh database.

The planned seed scenarios include an upcoming workshop with available seats,
a full upcoming workshop, and a completed workshop, together with participants
and registrations. Seed IDs remain stable. Workshop dates are calculated relative
to one captured startup clock instant so the scenarios remain useful over time.

To reset the demo, stop the process with `Ctrl+C` and run it again. No reset
endpoint, migration command, or database cleanup step is required for this setup.

## Tests and code checks

```bash
# Check formatting and supported style fixes without modifying files
dotnet format --verify-no-changes

# Compile and run configured analyzer checks
dotnet build

# Run each test suite separately
dotnet test tests/WorkshopBooking.UnitTests
dotnet test tests/WorkshopBooking.IntegrationTests

# Run all tests
dotnet test
```

Use `dotnet format` to apply supported automatic fixes.

Unit tests cover domain rules and application use cases. Integration tests call
the API through `WebApplicationFactory` and exercise the HTTP pipeline, business
logic, and real SQLite persistence with isolated test data.

Unit tests alone must achieve **over 90% line coverage separately for Domain and
Application**. Coverage tooling, exclusions, and the command that enforces this
threshold will be documented in `docs/testing.md` when configured. `dotnet test`
alone must not be treated as evidence that the coverage requirement was checked.

## Solution layout

| Project | Responsibility |
| --- | --- |
| `src/WorkshopBooking.Api` | API host, controllers, HTTP contracts, Swagger UI |
| `src/WorkshopBooking.Application` | Use cases, business orchestration, persistence interfaces |
| `src/WorkshopBooking.Domain` | Entities and domain rules |
| `src/WorkshopBooking.Persistence` | EF Core context, mappings, persistence, seeding |
| `tests/WorkshopBooking.UnitTests` | Isolated domain and application tests |
| `tests/WorkshopBooking.IntegrationTests` | HTTP-to-database integration tests |

## Working with coding agents

[AGENTS.md](AGENTS.md) contains shared project guidance and verification commands.
The project supports ordinary prompts, standalone agent sessions, custom agents,
and optional multi-agent workflows. No fixed set of roles or delegation process
is required.

Task prompts, custom-agent instructions, or a selected workflow define how work
is organized. Shared project guidance remains applicable when roles change.
Adding or changing agent roles does not require updating `AGENTS.md` unless
shared project constraints change.

Read supporting documentation when relevant to the current task. By default,
code changes include a final review using `docs/coding-guidelines.md`; explicitly
assigned code reviews also use that document. A selected workflow, skill, or
custom-agent definition may narrow documentation use and assign detailed review
to a separate stage. For example, an implementer can focus on tests and working
behavior while a reviewer applies the detailed coding guidelines.

This specializes documentation defaults and task responsibilities. Shared
architecture constraints, API contracts, and enforced formatter/analyzer rules
remain applicable to every contribution.

When working in parallel, agree shared contracts and file ownership, coordinate
shared changes, and verify the combined result. Worktrees are optional unless
required by the selected workflow. Complete each task's assigned scope without
assuming an unassigned later stage will finish it. Delegated tasks report their
scoped checks; the combined application still requires full verification.

## Supporting documentation

The following documents are planned and will be created separately:

- `docs/architecture.md`: dependency boundaries, persistence lifecycle, design decisions.
- `docs/api-conventions.md`: validation, HTTP errors, dates, and ordering conventions.
- `docs/testing.md`: fixtures, test isolation, controlled time, and coverage enforcement.
- `docs/coding-guidelines.md`: guidance for code review and maintainability refactoring.
- `docs/features/<feature>.md`: business rules and acceptance scenarios for each feature.

## Troubleshooting

- **Project not found:** confirm scaffolding is complete and run from the repository root.
- **SDK mismatch:** check `dotnet --info` against `global.json` once it is present.
- **Port already in use:** stop the other process or choose another port with `--urls`.
- **Swagger returns 404:** use Development and confirm Swagger UI has been configured.
- **Data disappeared:** restarting the process intentionally resets the in-memory database.
