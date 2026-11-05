Implement the baseline workshops feature across the existing application layers, including unit and HTTP integration tests.

Work directly in the current repository using ordinary Codex capabilities. Do not invoke skills, spawn subagents, create custom-agent definitions, configure hooks, or introduce a multi-agent workflow.

**Goal**

Deliver working workshop creation, detail, and listing endpoints that satisfy `docs/features/workshops.md`. Build on the existing domain model, persistence foundation, application outcomes, HTTP conventions, and test infrastructure.

**Inspect before changing**

Read the root `AGENTS.md`, then review:

- `docs/features/workshops.md`
- `docs/architecture.md`
- `docs/api-conventions.md`
- `docs/testing.md`
- `docs/coding-guidelines.md`

Inspect the existing code, tests, dependency registration, and Git status. Preserve existing work and reuse established infrastructure rather than introducing competing abstractions.

The feature specification defines business behavior. If implementation reveals a genuine ambiguity or contradiction, resolve it explicitly without silently weakening acceptance criteria. Preserve acceptance scenario IDs.

**Implement a complete vertical slice**

Deliver these endpoints:

| Endpoint | Behavior |
| --- | --- |
| `POST /api/workshops` | Create a workshop |
| `GET /api/workshops/{id}` | Retrieve workshop details and availability |
| `GET /api/workshops` | List workshops, optionally filtered by topic |

Use the documented request and response fields.

Keep responsibilities within their existing boundaries:

- API: HTTP DTOs, binding, controller actions, response mapping.
- Application: use cases, orchestration, focused persistence contracts.
- Domain: local invariants and domain behavior.
- Persistence: queries, writes, mapping, and database-specific behavior.

Controllers must not use EF Core directly. Application contracts must not expose HTTP or EF types.

**Workshop creation**

Implement all documented creation requirements:

- Trim title and topic before validation and storage.
- Enforce required fields and documented UTF-16 length limits.
- Require a positive 32-bit integer capacity.
- Capture one instant from injected `TimeProvider`.
- Require start strictly after that instant and end strictly after start.
- Preserve the precision needed by the documented time-boundary scenarios.
- Allow duplicate titles, topics, schedules, and overlapping workshops.
- Generate the workshop ID server-side.
- Persist exactly one workshop with no registrations on success.
- Leave persisted state unchanged on rejection.

Return `201` with the created DTO and a `Location` header pointing to the implemented detail endpoint. Following that URL must return the created resource.

Do not duplicate existing domain validation or normalization policies. Extend them only where required by the specification.

**Workshop detail and availability**

Return the documented DTO for an existing workshop.

- Malformed IDs and empty GUIDs produce the documented `400` problem.
- Well-formed unknown IDs produce `404` / `workshop_not_found`.
- Available seats are capacity minus current registration count.
- Reads include upcoming, ongoing, and completed workshops.
- Do not reapply creation-time future-date validation during reads.
- Do not store a separate mutable availability counter or hide overbooking by clamping invalid counts.

Availability must reflect persisted registration state. Registration endpoints are not required yet; integration fixtures can establish registrations directly.

**Workshop listing and filtering**

Implement the documented behavior:

- No filter returns all workshops.
- A supplied topic filter is trimmed and uses culture-independent, case-insensitive exact matching.
- Support the documented non-ASCII case scenario.
- An empty or whitespace-only supplied filter produces `400` with `topic`.
- No matches returns `200` and `[]`.
- Order by start instant, then ID, ascending.
- Return the same documented fields as the detail endpoint.

Use the persistence strategy already selected for SQLite-compatible filtering and ordering. Avoid per-workshop registration queries when one composed query or a small fixed number of queries can provide availability.

Do not add pagination, free-text search, extra filters, or new routes.

**Use shared HTTP conventions**

Reuse the existing outcome mapping, validation problem responses, JSON serialization, and timestamp handling.

Verify that:

- Validation errors use public field names.
- Non-future start errors identify `startsAt`.
- Invalid end ordering identifies `endsAt`.
- Unknown request properties cannot control server-owned identity or availability.
- Responses contain only the documented DTO fields.
- Timestamps represent the correct instants and serialize in UTC with `Z`.

Do not introduce feature-specific error envelopes or expose persistence details.

**Implement tests alongside behavior**

Use a test-first sequence for meaningful behavior changes: establish a failing assertion for missing behavior, implement the behavior, then rerun the affected tests. Compilation failures or broken fixtures do not establish the expected behavioral failure.

Add unit tests for application use cases and any domain behavior introduced or changed. Use controlled time and persistence test doubles.

Add HTTP integration tests using the existing isolated factory and real SQLite persistence. Cover the applicable scenarios WKS-001 through WKS-027, including:

- Creation and `Location` readback.
- Text and capacity boundaries.
- Exact start/end-time boundaries.
- Timestamp binding and offset normalization.
- Missing and malformed IDs.
- Empty collections.
- Topic matching, including non-ASCII casing.
- Deterministic ordering with tied start times.
- Availability for zero, some, and capacity-many registrations.
- Read access to ongoing and completed workshops.
- Rejected writes leaving state unchanged.

WKS-024 requires registration creation and cancellation endpoints, which belong to a later task. Explicitly mark that cross-feature HTTP scenario as pending. Verify availability using direct fixtures now, without adding skipped placeholder tests or implementing registration endpoints.

Reference stable acceptance IDs in test names or lightweight test metadata where useful. Do not force one test per scenario when parameterized cases provide clearer coverage.

**Complete endpoint OpenAPI metadata**

Document workshop request/response schemas, optional topic filtering, required fields, success responses, applicable problems, and the creation `Location` header.

Confirm the workshop operations appear in Swagger UI and can execute against the Development host. Do not complete unrelated feature documentation or add demo seed data.

**Boundaries**

Do not implement participant or registration endpoints, production seeding, feedback, waiting-list behavior, or coverage enforcement.

Keep changes to shared infrastructure limited to defects or capabilities genuinely required by this feature. Preserve existing behavior and explain any shared changes.

**Verify and review**

Run:

```bash
dotnet format --verify-no-changes
dotnet build
dotnet test tests/WorkshopBooking.UnitTests
dotnet test tests/WorkshopBooking.IntegrationTests
dotnet test
```

Use the documented runner-compatible commands if they differ.

Collect unit coverage if tooling is already available and inspect meaningful gaps in the introduced behavior. Do not claim complete baseline coverage while later features or the gate remain pending.

Review against coding guidelines. Update relevant documentation to distinguish delivered workshop behavior from remaining baseline work.

**Completion report**

Provide:

1. Endpoints and layer changes implemented.
2. Acceptance scenarios covered and the explicitly pending WKS-024 dependency.
3. Unit and HTTP integration verification results.
4. Swagger verification.
5. Commands actually executed, documentation changes, and unresolved issues.

Do not commit, push, or tag changes. Stop after the workshops feature; participants are the next task.