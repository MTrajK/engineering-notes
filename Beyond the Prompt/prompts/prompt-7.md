Implement the baseline registrations feature across the existing application layers, including unit and HTTP integration tests.

Work directly in the current repository using ordinary Codex capabilities. Do not invoke skills, spawn subagents, create custom-agent definitions, configure hooks, or introduce a multi-agent workflow.

**Goal**

Deliver registration creation, workshop-filtered listing, and cancellation according to `docs/features/registrations.md`. Preserve capacity, uniqueness, failure precedence, and consistency under competing requests.

Reuse the existing domain model, application outcomes, persistence coordination, HTTP conventions, and isolated test infrastructure.

**Inspect before changing**

Read the root `AGENTS.md`, then review:

- `docs/features/registrations.md`
- `docs/features/workshops.md`
- `docs/features/participants.md`
- `docs/architecture.md`
- `docs/api-conventions.md`
- `docs/testing.md`
- `docs/coding-guidelines.md`

Inspect current persistence contracts, registration mappings, synchronization, clock usage, tests, dependency registration, and Git status. Preserve workshop and participant behavior.

Resolve genuine ambiguities explicitly without weakening requirements or changing acceptance scenario IDs.

**Implement a complete vertical slice**

Deliver these endpoints:

| Endpoint | Behavior |
| --- | --- |
| `POST /api/registrations` | Register an existing participant for an existing workshop |
| `GET /api/registrations?workshopId={id}` | List current registrations for one workshop |
| `DELETE /api/registrations/{id}` | Cancel a registration |

Creation accepts only `workshopId` and `participantId`. Creation and list responses contain `id`, `workshopId`, `participantId`, and `createdAt`.

Keep HTTP concerns in API, use-case orchestration in Application, local invariants in Domain, and database operations in Persistence. Do not call other controllers or make internal HTTP requests.

**Registration creation**

Implement validation and business decisions in this order:

1. Validate both input IDs and collect applicable field errors.
2. Resolve the workshop, then the participant.
3. Reject a workshop that has started.
4. Reject an existing registration for the pair.
5. Reject a workshop with no available capacity.
6. Persist the registration within the protected mutation operation.

Apply the documented outcomes:

- Invalid IDs: `400` / `validation_failed`.
- Missing workshop: `404` / `workshop_not_found`.
- Missing participant: `404` / `participant_not_found`.
- Started workshop: `409` / `workshop_started`.
- Duplicate pair: `409` / `registration_duplicate`.
- Full workshop: `409` / `workshop_full`.

Missing resources take precedence over state conflicts. A duplicate in a full upcoming workshop returns `registration_duplicate`; a duplicate in a started workshop returns `workshop_started`.

Generate the registration ID server-side. Capture one instant from injected `TimeProvider` after acquiring the relevant mutation gate, and use it for the authoritative start comparison and `createdAt`.

Registration is allowed strictly before workshop start, including the documented one-tick boundary. Do not impose a minimum lead time, schedule-overlap restriction, or additional end-time restriction.

Return `201` and the created DTO. Omit `Location` because the baseline has no registration GET-by-ID route.

**Atomicity and concurrency**

Reuse the established persistence and coordination strategy. Extend it only where necessary to deliver the specified behavior.

- Protect authoritative state checks and insertion as one operation.
- Keep foreign keys and the registration-pair unique constraint.
- Coordinate creation and cancellation under the same consistency strategy.
- Do not authorize writes using stale preflight checks.
- Preserve scoped-context and connection-safety requirements.
- Release synchronization and dispose transactions reliably on success, rejection, exception, or cancellation.
- Do not translate unrelated database failures or lock errors into business conflicts.

Required competing-request outcomes include:

- Distinct participants competing for one seat: one creation and one `workshop_full`.
- The same pair competing for registration: one creation and one `registration_duplicate`, including when the winner consumes the last seat.
- Competing cancellations of one registration: one success and one `registration_not_found`.
- Cancellation competing with creation: an outcome consistent with one valid mutation order, without overbooking or deferred writes after rejection.

Keep business policy in Application/Domain and transaction mechanics in Persistence. Do not move the entire use case into a controller or EF repository merely to obtain atomicity.

**Registration listing**

Require a non-empty, well-formed `workshopId` query parameter.

- Missing, empty, malformed, or empty-GUID input returns `400` with `workshopId`.
- A well-formed unknown workshop returns `404` / `workshop_not_found`.
- An existing workshop with no registrations returns `200` and `[]`.
- Return only registrations belonging to the selected workshop.
- Order by `createdAt`, then ID, ascending.
- Include registrations for upcoming, ongoing, and completed workshops.

Do not add an unfiltered list, participant-wide list, pagination, or additional routes.

**Cancellation**

Remove the registration identified by its registration ID.

- Malformed IDs or empty GUIDs return the documented `400`.
- A well-formed missing ID returns `404` / `registration_not_found`.
- Successful cancellation returns `204` with no body.
- Cancellation is allowed before, at, and after workshop start/end.
- Free exactly one seat without changing capacity, schedule, or participant details.
- Repeated cancellation must not free another seat.
- A participant may register again after cancellation when the normal creation rules permit it, receiving a new registration ID and timestamp.

Do not introduce soft deletion, cancellation history, notifications, waiting-list promotion, or background work.

**Use shared HTTP conventions**

Reuse existing DTO serialization, outcome mapping, validation, and problem responses.

- Use public validation keys `workshopId` and `participantId`.
- Serialize `createdAt` in UTC with `Z`.
- Prevent extra submitted fields from controlling identity or timestamps.
- Return no embedded entity graphs or persistence metadata.
- Rejected mutations leave stored state unchanged.

**Implement tests alongside behavior**

Use meaningful test-first steps, confirming behavioral failure before implementing the corresponding behavior.

Add unit tests for application orchestration, precedence, controlled time, and avoiding writes on rejection. Reuse existing domain tests rather than duplicating them mechanically.

Add HTTP integration tests against real SQLite using the existing isolated factory. Cover REG-001 through REG-033, including:

- Success and response fields.
- Input validation and missing-resource precedence.
- Exact time boundaries and time capture after gate acquisition.
- Started, duplicate, and full-workshop conflicts.
- Listing filters, missing workshop, empty arrays, and tied-timestamp ordering.
- Cancellation, repeated cancellation, and re-registration.
- Competing creates, duplicate creates, cancellations, and mixed mutations.
- Client attempts to supply server-owned fields.
- Unchanged stored state after rejection.

Use bounded coordination for contested scenarios. Do not use sleeps, probabilistic retry loops, or test-side serialization that removes contention. Do not let application test doubles replace the production coordination or persistence under test.

Verify mutations using fresh-context reads or subsequent HTTP requests.

Complete the previously pending workshop scenario WKS-024 through the real registration and cancellation endpoints: confirm a seat is consumed and then restored in workshop detail and list responses.

Verify participant records and workshop schedule/capacity remain unchanged.

**OpenAPI and documentation**

Document all three operations, required parameters, response DTOs, and applicable problem responses. Ensure Swagger can execute them and does not advertise an unimplemented registration GET-by-ID route.

Update relevant documentation to reflect implemented registration behavior, synchronization guarantees, and completion of the cross-feature workshop scenario. Preserve future demo features as out of scope.

**Verify and review**

Run formatting verification, build, both test suites, and all tests using the repository’s documented commands. Collect unit coverage if configured and inspect meaningful gaps.

Review against coding guidelines. Do not weaken assertions, broaden exclusions, or suppress diagnostics to obtain passing checks.

Full coverage enforcement and deterministic production demo seeding remain later tasks.

**Completion report**

Provide:

1. Endpoints and layer changes implemented.
2. REG acceptance coverage and WKS-024 verification.
3. Concurrency strategy and observed competing-request outcomes.
4. Regression, Swagger, and persisted-state verification.
5. Commands actually executed, documentation changes, and unresolved issues.

Do not commit, push, or tag changes. Stop after registrations and cancellation; deterministic demo data and final OpenAPI documentation are next.