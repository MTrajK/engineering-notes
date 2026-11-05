Implement the baseline participants feature across the existing application layers, including unit and HTTP integration tests.

**Goal**

Deliver working participant creation, detail, and listing endpoints that satisfy `docs/features/participants.md`. Reuse the existing domain model, persistence infrastructure, application outcomes, HTTP conventions, and test fixtures.

**Inspect before changing**

Read the root `AGENTS.md`, then review:

- `docs/features/participants.md`
- `docs/architecture.md`
- `docs/api-conventions.md`
- `docs/testing.md`
- `docs/coding-guidelines.md`

Inspect existing entities, normalization policies, database constraints, application services, tests, dependency registration, and Git status. Preserve existing work and workshop behavior.

Resolve genuine specification ambiguities explicitly. Do not silently change business requirements or acceptance IDs to fit an implementation.

**Implement a complete vertical slice**

Deliver these endpoints:

| Endpoint | Behavior |
| --- | --- |
| `POST /api/participants` | Create a participant |
| `GET /api/participants/{id}` | Retrieve a participant |
| `GET /api/participants` | List participants |

Use the documented create fields `name` and `email`, and response fields `id`, `name`, and `email`.

Keep responsibilities within the existing layers:

- API owns HTTP DTOs, binding, controllers, and HTTP mapping.
- Application owns use cases and focused persistence contracts.
- Domain owns local invariants and shared policies where already established.
- Persistence owns query execution, writes, and constraint interpretation.

Controllers must not access EF Core. Application must remain independent of HTTP and EF types.

**Participant creation**

Implement the documented requirements:

- Trim surrounding name and email whitespace before validation and storage.
- Enforce the name length and email length limits using UTF-16 code units.
- Preserve internal name whitespace, punctuation, Unicode characters, and casing.
- Allow duplicate names.
- Generate a non-empty participant GUID server-side.
- Create exactly one participant on success.
- Do not create registrations or modify workshop records.

Use the shared email policy already established during the persistence foundation. Confirm that it matches the feature specification, including:

- Exactly one `@` with nonempty text on both sides.
- No internal whitespace or control characters.
- No prohibited display-name or address-list delimiters.
- The documented `EmailAddressAttribute.IsValid` check.
- No additional requirement for a dot in the domain.

Do not add network calls, DNS validation, verification messages, provider-specific rewriting, or a competing controller-specific regex.

**Normalization and uniqueness**

Preserve the trimmed submitted email for display. Derive the uniqueness key using `ToUpperInvariant()` and use it consistently for lookup and database enforcement.

- Treat case variants and surrounding whitespace as the same identity.
- Preserve differences in dots and plus tags when they produce different normalized keys.
- Do not perform additional Unicode normalization or rewrite domains.
- Keep normalization keys out of HTTP input and output contracts.

Validate all independent fields before checking uniqueness. For example, an invalid name combined with an existing valid email returns validation failure rather than a conflict.

A duplicate returns `409` / `participant_email_conflict`. It must not overwrite the existing participant or return it as a successful creation.

Reuse the established database constraint and mutation strategy. Competing requests for the same normalized key must produce one successful creation, one documented conflict, and one stored row.

Translate only recognized uniqueness failures. Unexpected persistence failures must retain the shared unexpected-error behavior.

**Detail and listing**

For detail:

- Return `200` with the documented fields for an existing ID.
- Return the documented `400` problem for malformed IDs or an empty GUID.
- Return `404` / `participant_not_found` for a well-formed unknown ID.

For listing:

- Return every participant as a JSON array.
- Return `200` and `[]` when none exist.
- Order by normalized email using ordinal semantics, then ID, ascending.
- Preserve each participant’s display email casing.
- Do not depend on insertion order or the machine’s current culture.

Do not add filtering, search, pagination, navigation collections, update, or delete endpoints.

**Use shared HTTP conventions**

Reuse the existing outcome mapping, validation responses, and JSON configuration.

- Successful creation returns `201`, the created DTO, and a working `Location` header.
- Following `Location` returns the persisted participant.
- Validation keys use `name` and `email`.
- Problems follow the shared shape and stable codes.
- Conflict responses do not reveal the existing participant ID or echo submitted email values.
- Extra client-supplied identity or normalization properties cannot control server-owned state.
- Responses contain no persistence metadata or embedded registration/workshop graphs.

**Implement tests alongside behavior**

Use a test-first sequence for meaningful behavior changes: establish a failing assertion for absent behavior, implement it, then verify the result. Broken compilation or fixture setup is not behavioral red-stage evidence.

Add unit tests for application behavior and any domain policy changes. Use persistence test doubles and deterministic inputs.

Add HTTP integration tests using the existing isolated factory and real SQLite persistence. Cover PAR-001 through PAR-026, including:

- Creation with `Location` readback.
- Trimming and preservation of display values.
- Required fields and length boundaries.
- Valid and invalid email-policy examples.
- Exact, case-variant, and whitespace-variant duplicates.
- Distinct dot/plus-tag addresses.
- Duplicate names with different addresses.
- Validation precedence over uniqueness.
- Existing, missing, malformed, and empty-GUID IDs.
- Empty lists and ordinal normalized-email ordering.
- Competing duplicate creates.
- Client attempts to supply server-owned fields.
- No changes to workshops or registrations.

For culture-sensitive checks, restore any modified culture after the test and avoid process-wide changes that interfere with parallel tests. Prefer scoped or otherwise isolated verification.

For concurrency tests, send overlapping HTTP requests to the same test database. Use bounded coordination rather than sleeps or test-side locking that removes contention. Verify persisted effects through a fresh context or subsequent HTTP read.

Reference stable acceptance IDs where useful. Do not weaken assertions or bypass real persistence to make the concurrency scenario pass.

**Complete endpoint OpenAPI metadata**

Document participant request/response schemas, required fields, success statuses, applicable problems, and the creation `Location` header.

Confirm participant operations appear in Swagger UI and can execute against the Development host. Verify the existing workshop operations remain available.

**Boundaries**

Do not implement registration endpoints, production demo seeding, feedback, waiting-list functionality, authentication, or coverage enforcement.

Limit changes to shared infrastructure to demonstrated defects or capabilities needed for this feature. Explain those changes and preserve existing contracts.

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

Collect unit coverage if configured and address meaningful gaps in the introduced behavior. Final baseline coverage enforcement remains a later task.

Review the implementation against coding guidelines. Update relevant documentation to reflect delivered participant behavior while keeping pending baseline work explicit.

**Completion report**

Provide:

1. Endpoints and layer changes implemented.
2. Acceptance scenarios covered and any unresolved gaps.
3. Email-policy, normalization, and concurrent-uniqueness verification.
4. Workshop regression and Swagger verification results.
5. Commands actually executed and documentation updated.

Do not commit, push, or tag changes. Stop after the participants feature; registrations and cancellation are the next task.