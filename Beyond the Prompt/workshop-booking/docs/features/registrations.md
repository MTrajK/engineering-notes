# Registrations

## Status and purpose

Baseline feature specification; implementation and tests are pending.
Register an existing participant for an existing workshop, list its registrations,
and cancel a registration while preserving workshop capacity and uniqueness.

Follow [API conventions](../api-conventions.md) for JSON, errors, and HTTP
behavior, [architecture](../architecture.md) for persistence boundaries, and
[testing](../testing.md) for test isolation and verification. Related feature
contracts are [workshops](workshops.md) and [participants](participants.md).

## Scope

- Register one participant for one workshop.
- List registrations for one required workshop ID.
- Cancel a registration by registration ID and free its seat.

No registration updates, GET-by-ID endpoint, participant-wide registration list,
bulk operations, payment, notifications, cancellation history, authentication,
or automatic waiting-list promotion are included. Workshop end time and schedule
overlaps do not add registration restrictions beyond the rules below.

## Data contract

### Create input

| Field | Type | Requirement |
| --- | --- | --- |
| `workshopId` | GUID | Required, non-empty, and identifies an existing workshop |
| `participantId` | GUID | Required, non-empty, and identifies an existing participant |

The caller provides identities only. Do not create missing participants or
workshops implicitly. IDs must bind as GUIDs; missing, null, malformed, empty,
and incompatible JSON values are validation failures before resource lookup.

### Response

Creation and list items use the same response fields:

| Field | Type | Meaning |
| --- | --- | --- |
| `id` | GUID | Server-generated registration identity |
| `workshopId` | GUID | Referenced workshop |
| `participantId` | GUID | Referenced participant |
| `createdAt` | UTC timestamp | Captured business instant for successful registration |

Clients cannot set `id` or `createdAt`; neither is part of the create DTO.
Responses contain no embedded participant/workshop objects or internal state.
Cancel returns no DTO. No soft-delete flag or mutable registration status is
part of the baseline model.

## Business rules

| ID | Rule |
| --- | --- |
| REG-R01 | The workshop and participant must both exist before registration can succeed. |
| REG-R02 | Registration is allowed only when the captured current instant is strictly before workshop `startsAt`. |
| REG-R03 | At most one current registration may exist for a participant/workshop pair. |
| REG-R04 | Registration requires current registration count to be strictly less than capacity. |
| REG-R05 | Successful creation persists exactly one registration with a new non-empty ID and the captured instant as `createdAt`. |
| REG-R06 | Rejected registration leaves workshops, participants, and registrations unchanged. |
| REG-R07 | Cancellation removes an existing registration and frees exactly one seat without deleting its participant or workshop. |
| REG-R08 | Cancellation is allowed before, at, and after workshop start/end; there is no cancellation cutoff in this baseline. |
| REG-R09 | Cancelling a nonexistent registration returns `registration_not_found`; repeated cancellation does not free an additional seat. |
| REG-R10 | After cancellation, the same participant may register again if the workshop has not started and a seat remains. This creates a new registration ID and timestamp. |
| REG-R11 | Listing requires an existing workshop and returns only its current registrations, ordered by `createdAt`, then ID, ascending. |

Listing is allowed for upcoming, ongoing, and completed workshops. Time passing
does not delete registrations or hide them. An existing workshop with no current
registrations returns an empty array.

### Failure precedence and time

For creation, use this order when several failures apply:

1. Validate both input IDs; collect applicable field errors without database writes.
2. Resolve the workshop, then the participant.
3. Reject a workshop that has started.
4. Reject an existing participant/workshop registration.
5. Reject a workshop with no remaining capacity.
6. Persist the registration within the protected mutation operation.

Use `workshopId` and `participantId` as field validation keys. Missing-resource
responses take precedence over workshop-state conflicts. A duplicate in a full
upcoming workshop returns `registration_duplicate`; a duplicate in a started
workshop returns `workshop_started`.

Capture one instant from injected `TimeProvider` for the authoritative registration
decision and use it for both the start comparison and `createdAt`. Capture it
after acquiring any mutation serialization gate, rather than before an unbounded
wait for that gate. Compare instants, not local date components or formatted
strings. Do not impose a separate minimum lead time.

### Integrity and competing mutations

Protect authoritative capacity/duplicate checks and the insert as one atomic
operation using the SQLite-compatible strategy established during scaffolding.
Preflight reads outside that operation cannot authorize the write. Keep database
foreign keys and a unique constraint on `(workshopId, participantId)`.

Cancellation must coordinate with creation under the same consistency strategy.
Each successful mutation must preserve uniqueness and keep registration count
between zero and capacity. Do not store or independently decrement an available
seat counter, clamp invalid counts, or translate ordinary lock failures into a
misleading `workshop_full` result.

For two distinct participants competing for one remaining seat, exactly one
registration succeeds and the other receives `workshop_full`. For two requests
for the same pair, the losing request receives `registration_duplicate` while
the workshop remains upcoming, including when the winner consumes the final seat.

For competing cancellation and creation, either serialized order is valid.
Creation may succeed after the seat is freed or return `workshop_full` before
cancellation completes. A client can retry a rejected creation; the server must
not silently create a registration later. No retry-idempotency mechanism is
part of this feature.

## Endpoints

| Endpoint | Success | Expected feature failures |
| --- | --- | --- |
| `POST /api/registrations` | `201`, created DTO; omit `Location` because no GET-by-ID route exists | `400` / `validation_failed`; `404` / `workshop_not_found` or `participant_not_found`; `409` / `workshop_started`, `registration_duplicate`, or `workshop_full` |
| `GET /api/registrations?workshopId={id}` | `200`, registration array | `400` / `validation_failed`; `404` / `workshop_not_found` |
| `DELETE /api/registrations/{id}` | `204`, empty body | `400` / `validation_failed`; `404` / `registration_not_found` |

The list query requires exactly the workshop selection described above; there
is no unfiltered registration list. A missing, malformed, or empty-GUID filter
is `400`, not an empty result. A well-formed missing workshop is `404`.
Shared transport errors and problem response shapes follow API conventions.

## Acceptance scenarios

Scenario IDs are stable references for tests and task handoffs. Keep their IDs
when refining wording; add new IDs for additional behavior.

| ID | Given / when | Expected outcome |
| --- | --- | --- |
| REG-001 | Existing participant registers for an upcoming workshop with a free seat | `201`; response has the referenced IDs, new non-empty ID, and fixed-clock `createdAt` in UTC; exactly one row added; available seats decreases by one |
| REG-002 | Either create ID is missing, null, malformed, empty GUID, or an incompatible JSON type | `400` / `validation_failed`; field errors use public names; no write |
| REG-003 | Both input IDs are invalid | `400` includes applicable validation errors before resource lookup |
| REG-004 | Workshop ID is valid but unknown | `404` / `workshop_not_found`; no write |
| REG-005 | Workshop exists but participant ID is unknown | `404` / `participant_not_found`; no write |
| REG-006 | Both well-formed IDs are unknown | `404` / `workshop_not_found` takes precedence |
| REG-007 | Participant is missing and the existing workshop is started or full | `404` / `participant_not_found` takes precedence |
| REG-008 | Fixed `now` is one .NET tick before workshop start and a seat is free | Registration succeeds |
| REG-009 | Fixed `now` equals workshop start | `409` / `workshop_started`; no write |
| REG-010 | Fixed `now` is after workshop start, including after workshop end | `409` / `workshop_started`; no write |
| REG-011 | Participant is already registered in an upcoming workshop | `409` / `registration_duplicate`; existing registration unchanged |
| REG-012 | A distinct unregistered participant targets a full upcoming workshop | `409` / `workshop_full`; no write |
| REG-013 | Existing count is capacity minus one | Creation succeeds and leaves zero available seats |
| REG-014 | Existing pair targets a full upcoming workshop | `registration_duplicate` takes precedence over `workshop_full` |
| REG-015 | Existing pair targets a started, full workshop | `workshop_started` takes precedence over duplicate and capacity conflicts |
| REG-016 | One participant registers for two different valid workshops | Both succeed, including if their schedules overlap |
| REG-017 | Existing workshop has no registrations | Filtered list returns `200` and `[]` |
| REG-018 | Registrations exist for several workshops | Filtered list returns only the selected workshop's records and all documented fields |
| REG-019 | Selected workshop registrations have different creation times and a tied timestamp | List orders by creation time, then ID, ascending, independent of insertion order |
| REG-020 | List filter is missing, malformed, empty, or empty GUID | `400` / `validation_failed` with `workshopId` |
| REG-021 | List filter identifies a well-formed unknown workshop | `404` / `workshop_not_found` |
| REG-022 | Workshop has started or ended and has stored registrations | Listing still returns its current records |
| REG-023 | Existing registration is cancelled | `204` with no body; row removed; workshop availability increases by one; participant/workshop unchanged |
| REG-024 | Existing registration is cancelled at or after workshop start/end | Same successful cancellation behavior; no time cutoff |
| REG-025 | Cancellation ID is well-formed but unknown, including a previously cancelled ID | `404` / `registration_not_found`; no additional seat freed |
| REG-026 | Cancellation ID is malformed or empty GUID | `400` / `validation_failed`; no mutation |
| REG-027 | Participant cancels then registers again before start with a free seat | New registration succeeds with a different ID and the new captured timestamp |
| REG-028 | Two distinct participants concurrently compete for one free seat | One `201`, one `409` / `workshop_full`; one new row and zero available seats |
| REG-029 | Two concurrent requests target the same previously unregistered pair | One `201`, one `409` / `registration_duplicate`; exactly one matching row, including with only one initial free seat |
| REG-030 | Two requests concurrently cancel the same registration | One `204`, one `404` / `registration_not_found`; exactly one row removed and one seat freed |
| REG-031 | A full workshop receives cancellation and a distinct participant's creation concurrently | Outcome matches one valid mutation order; no overbooking or negative count; rejected creation makes no later write |
| REG-032 | Create input supplies extra `id` or `createdAt` fields | Client values cannot control generated identity or the captured timestamp |
| REG-033 | Registration start check waits behind the mutation gate until the workshop has started | The instant captured after gate acquisition rejects creation with `workshop_started` |

## Dependencies and verification

This feature depends on workshop schedule/capacity and participant existence.
Use Application persistence contracts; do not call workshop/participant controllers
or make internal HTTP requests. Registering must not alter participant details,
workshop schedule, or configured capacity.

Unit tests cover field validation, existence, rule precedence, controlled-time
boundaries, expected outcomes, and avoiding writes on rejection. HTTP integration
tests cover all endpoints, fresh-context/readback effects, relational constraints,
and competing mutations on real SQLite persistence. Use bounded coordination for
concurrency tests; no sleeps or test-side serialization that removes contention.

Cross-feature checks verify workshop availability after creation/cancellation
and that participant records remain unchanged. Relevant workshop acceptance
scenario WKS-024 is satisfied by this integrated flow.

The final combined implementation must satisfy the shared test and coverage
requirements. Keep specification, tests, and delivered behavior aligned when
requirements change; this document assigns no particular agent roles or workflow.
