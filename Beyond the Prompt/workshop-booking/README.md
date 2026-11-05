# Workshop Booking API

A small ASP.NET Core API for managing workshops, participants, and registrations.
Built as the demo application for **Beyond the Prompt: Engineering Your Codex
Workflow**, demonstrating approaches such as test-first implementation and
parallel coding agents. These workflows are optional.

## Project status

The solution, shared application outcomes, HTTP error handling, timestamp
serialization, and Development Swagger infrastructure are implemented. The
three baseline features, deterministic demo seeding, and OpenAPI documentation are implemented. Unit coverage enforcement is implemented; see docs/testing.md.
Domain entities, SQLite initialization, and isolated database fixtures are implemented.

The existing root configuration is now active as `.editorconfig` and
`.gitignore`; `Directory.Build.props`, `AGENTS.md`, and `docs/` remain at the
repository root.

## Scope

The baseline API supports:

- Creating and viewing workshops, including capacity and available seats.
- Creating and viewing participants with unique email addresses.
- Registering participants, listing registrations, and cancelling registrations.
- Validating capacity, duplicate registrations, and workshop start times.

Feedback and waiting-list features are reserved for later demo exercises. They
are not part of the initial implementation.

Authentication, caching, external services, and a frontend are outside the
baseline scope. Development Swagger UI provides the interactive interface; its
OpenAPI document includes all nine baseline operations.

## Prerequisites

- .NET 10 SDK `10.0.302` or a later patch in the same SDK feature band. The
  selected version is installed and pinned in `global.json`; prereleases are
  disabled.
- PowerShell 7 for the coverage gate and its verification checks. Final verification used 7.6.9.
- Git for version control; worktrees are used only in demos that need them.

The VSTest runner is selected in `global.json`. xUnit uses its MTP-off package
variant so the .NET 10 SDK runs tests through VSTest, which Coverlet's collector
requires. Package versions and ownership are recorded in
[architecture](docs/architecture.md) and [testing](docs/testing.md). No external
database server or Docker container is required.

## Codex project hooks

This project includes Codex hooks in `.codex/hooks.json` and `.codex/hooks/`.
Review and allow the project hook before using it:

1. Open `/hooks` in the Codex CLI, or go to **Settings → Hooks** in the desktop app.
2. Review and trust the project hook. Codex requires trust for project-local
   hooks, and new or changed hook definitions need review before they run.
3. Restart Codex if the hook does not appear.

## Run locally

Run commands from the repository root:

```bash
dotnet restore
dotnet build --no-restore
dotnet run --project src/WorkshopBooking.Api
```

The launch profile listens at `http://localhost:5080`. Swagger UI is available
at [http://localhost:5080/swagger](http://localhost:5080/swagger), and the
generated document is at
[http://localhost:5080/swagger/v1/swagger.json](http://localhost:5080/swagger/v1/swagger.json).
The document includes all nine baseline operations grouped by resource.
To start without a launch profile, use:

```bash
dotnet run --project src/WorkshopBooking.Api --no-launch-profile -- --environment Development --urls http://localhost:5080
```

Swagger UI and OpenAPI middleware are enabled only in Development.

## Baseline endpoints

These routes are implemented; OpenAPI describes their request and response schemas.

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

Startup creates and seeds an in-memory SQLite database before accepting requests. One captured startup instant defines every seed date. IDs are stable; dates move with each fresh process. User-created records do not survive restart. No endpoint or background job resets the database.

| Workshop | Stable ID | Start / end relative to startup | Capacity / registered / free |
| --- | --- | --- | --- |
| Practical API Design | `10000000-0000-0000-0000-000000000001` | +3 days / +3 days 2 hours | 4 / 1 / 3 |
| Testing Together | `10000000-0000-0000-0000-000000000002` | +5 days / +5 days 2 hours | 2 / 2 / 0 |
| Introduction to SQLite | `10000000-0000-0000-0000-000000000003` | -5 days / -5 days +2 hours | 3 / 1 / 2 |

Participants use IDs `20000000-0000-0000-0000-000000000001` (Alex River), `20000000-0000-0000-0000-000000000002` (Blair Meadow), and `20000000-0000-0000-0000-000000000003` (Casey Brook, initially unregistered). Display emails are Alex.River@example.test, Blair.Meadow@example.test and Casey.Brook@example.test.

Registration IDs `30000000-0000-0000-0000-000000000001` through `...0004` belong respectively to available/Alex, full/Alex, full/Blair, completed/Blair. Creation offsets are -1, -2, -1 and -6 days. Historical registrations precede workshop start.

### Swagger walkthrough

1. Run the local command above and open Swagger. Execute GET workshops and GET participants.
2. Execute GET workshop detail with the Practical API Design ID; observe 3 available seats.
3. Execute POST registrations with this JSON (also supplied as the schema example):

   ```json
   {"workshopId":"10000000-0000-0000-0000-000000000001","participantId":"20000000-0000-0000-0000-000000000003"}
   ```

4. Copy the returned registration ID. Read workshop detail/list again; availability is 2.
5. Repeat the POST; expect 409 / registration_duplicate.
6. Change workshopId to `10000000-0000-0000-0000-000000000002`, keeping Casey; expect 409 / workshop_full.
7. DELETE registrations using the ID returned in step 3. Expect empty 204; detail/list availability returns to 3.
8. Stop the process with Ctrl+C and rerun the local command. List records again: seed IDs and baseline counts are restored, and dates are recalculated from the new startup instant.

OpenAPI examples describe request shapes; they are not guarantees of current stored state after mutations. Workshop-create examples use dates seven days after document generation; replace them with future explicit-offset dates if the document has been open for a long time. Input timestamps require Z or a numeric offset; UTC output preserves seven fractional digits. Ordinary integration fixtures disable demo seeding; dedicated seeded tests opt in with a controlled clock.

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

# Enforce unit-only coverage for Domain and Application
pwsh -NoProfile -File scripts/Test-UnitCoverage.ps1

# Verify coverage-gate failure and threshold logic
pwsh -NoProfile -File scripts/Test-CoverageGate.ps1
```

Use `dotnet format` to apply supported automatic fixes.

The unit suite covers Domain and Application behavior. The integration suite uses
real SQLite and isolated test hosts for all baseline features, shared HTTP behavior,
persistence, seeding, OpenAPI and concurrency. Test-only failure controllers are
registered only by the integration fixture.

Unit tests alone must achieve **over 90% line coverage separately for Domain and
Application**. The verified unit-only enforcing command is `pwsh -NoProfile -File scripts/Test-UnitCoverage.ps1` (PowerShell 7 required). It collects fresh OpenCover data and rejects either assembly at or below 90%. See [testing](docs/testing.md) and the [acceptance audit](docs/acceptance-audit.md). `dotnet test` alone does not enforce coverage.

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

The final [baseline review and rehearsal](docs/baseline-review.md) records readiness evidence and later-demo coordination points.

## Supporting documentation

The following supporting documents are present at the repository root:

- `docs/architecture.md`: dependency boundaries, persistence lifecycle, design decisions.
- `docs/api-conventions.md`: validation, HTTP errors, dates, and ordering conventions.
- `docs/testing.md`: fixtures, test isolation, controlled time, and coverage enforcement.
- `docs/coding-guidelines.md`: guidance for code review and maintainability refactoring.
- `docs/features/<feature>.md`: business rules and acceptance scenarios for each feature.

## Troubleshooting

- **Project not found:** run from the repository root and confirm the project path.
- **SDK mismatch:** check `dotnet --info` against `global.json` once it is present.
- **Port already in use:** stop the other process or choose another port with `--urls`.
- **Swagger returns 404:** use Development and confirm Swagger UI has been configured.
- **Data disappeared:** restarting the process intentionally resets the in-memory database.
