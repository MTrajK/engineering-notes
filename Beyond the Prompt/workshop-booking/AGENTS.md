# Project guidance

Workshop Booking is a small, layered ASP.NET Core API for coding-agent demos.
Keep changes focused on the requested task and its acceptance criteria.

## Read documentation when relevant

- For setup, Swagger access, or resetting demo data: use `README.md`.
- For service boundaries, shared contracts, or persistence changes: read
  `docs/architecture.md`.
- For endpoint behavior and HTTP error mapping: read `docs/api-conventions.md`.
- For tests, fixtures, or coverage: read `docs/testing.md`.
- For business behavior: read the relevant `docs/features/<feature>.md`.
- For code changes, use `docs/coding-guidelines.md` during final review by
  default; read it when explicitly assigned code review as well.
- Read only the documents needed for the task; follow relevant pointers further.
- An explicitly selected workflow, skill, or custom-agent definition may narrow
  which supporting documents to read and assign review to a separate stage.
  Follow that division of responsibilities within the assigned scope.
- These documentation defaults may be specialized; shared architecture
  constraints, API contracts, and enforced formatter/analyzer rules still apply.

## Shared constraints

- Preserve the API / Application / Domain / Persistence dependency boundaries.
- Use EF Core with SQLite in-memory; keep the owning connection open for the
  lifetime of its database. Restarting the app resets and reseeds demo data.
- Keep seed IDs stable. Calculate seed dates from one captured clock instant.
- Use injected `TimeProvider` for business time; tests use a controlled clock.
- Tests must not depend on execution order, shared mutable data, or external services.
- Preserve agreed API contracts and business rules during refactoring.
- Authentication, caching, and external infrastructure are outside the baseline scope.

## Working approach

- This guidance applies to ordinary prompts, standalone sessions, custom agents,
  and one-off subagents. No fixed roles or delegation process are required.
- Complete the requested task without assuming a particular agent role,
  delegation workflow, or later review stage.
- Use task-specific or custom-agent instructions for specialized workflows.
- Before implementing new or changed business behavior, create or update the
  relevant `docs/features/<feature>.md` with rules and acceptance scenarios.
  Keep it consistent with the delivered behavior and tests.
- Feature documentation is part of behavior-change tasks. Refactoring that
  preserves behavior does not require rewriting feature specifications.
- Adding or changing agent roles does not require updating this file unless
  shared project constraints change.
- Formatter and analyzer rules apply to all code changes.

## When working in parallel

- Agree shared contracts and file ownership before parallel implementation.
- If worktrees are assigned, work in the assigned worktree. Coordinate changes
  to shared files or contracts with collaborators; preserve their work.
- Hand off the worktree/branch, acceptance criteria covered, checks run, and any
  unresolved dependencies. Distinguish verified results from assumptions.
- Integrate completed worktrees incrementally and verify affected behavior.

## Commands

Run from the repository root. These are the intended project paths; during
initial scaffolding, establish them and verify the commands below.

```bash
dotnet restore
dotnet build --no-restore
dotnet run --project src/WorkshopBooking.Api
dotnet format --verify-no-changes
dotnet test tests/WorkshopBooking.UnitTests
dotnet test tests/WorkshopBooking.IntegrationTests
dotnet test
pwsh -NoProfile -File scripts/Test-UnitCoverage.ps1
pwsh -NoProfile -File scripts/Test-CoverageGate.ps1
```

Use `dotnet format` to apply supported fixes. The unit-only enforcing command and its verification checks are documented in
`docs/testing.md`; PowerShell 7 is required.

## Completion

- Complete the assigned scope and verify its acceptance criteria. Do not defer
  necessary work to an assumed later reviewer or integration stage.
- For behavior changes, add or update meaningful tests and run affected tests.
- For completed application changes, format verification, build, and both test
  suites must pass. Delegated partial tasks report their scoped checks; the
  combined result still requires full verification.
- Unit tests alone must achieve over 90% line coverage separately for Domain and
  Application. Do not weaken assertions or broaden exclusions to meet the target.
- Update relevant documentation when contracts, business rules, commands, or
  non-obvious architectural decisions change.
- Report checks actually run and any failures or checks that could not run.

## Bootstrap status

Supporting documents, custom-agent definitions, and application projects are
created separately. If a referenced file is missing, report that gap rather than
assuming its contents. Create it only when the assigned task includes that work.
