# Workshops

## Status and purpose

Workshop creation, detail and listing are implemented with unit and SQLite HTTP integration tests. WKS-024 is implemented through real registration and cancellation endpoints.
Create workshops and let callers discover their schedule, capacity, and available
seats. The acceptance scenarios below define the delivered contract.

Field names and limits below are the selected baseline contract. Shared JSON,
date, validation, and problem-response conventions are defined in
[API conventions](../api-conventions.md). Use
[architecture](../architecture.md) for layer boundaries and
[testing](../testing.md) for verification guidance.

## Scope

- Create a workshop.
- Retrieve a workshop by ID.
- List all workshops, optionally filtered by topic.
- Report available seats from current registrations.

Updating, deleting, cancelling, rescheduling, recurring workshops, pagination,
and free-text search are outside this feature. Feedback and waiting-list behavior
are later features and must not be implemented as part of this baseline.

## Data contract

### Create input

| Field | Type | Requirement |
| --- | --- | --- |
| `title` | String | Required; 1–200 characters after trimming |
| `topic` | String | Required; 1–100 characters after trimming |
| `startsAt` | Timestamp with explicit offset | Required; strictly after the operation's captured current instant |
| `endsAt` | Timestamp with explicit offset | Required; strictly after `startsAt` |
| `capacity` | Integer | Required; 1 through 2,147,483,647 |

Text limits count .NET UTF-16 code units, matching `string.Length`. Trim
surrounding whitespace, preserve internal whitespace and display casing, then
validate length. There is no uniqueness requirement on workshop title, topic,
or schedule. Overlapping workshops are allowed.

### Response

Creation, detail, and list items use the same response fields:

| Field | Type | Meaning |
| --- | --- | --- |
| `id` | GUID | Server-generated workshop identity |
| `title` | String | Trimmed title |
| `topic` | String | Trimmed display topic |
| `startsAt` | UTC timestamp | Scheduled start instant |
| `endsAt` | UTC timestamp | Scheduled end instant |
| `capacity` | Integer | Maximum registrations |
| `availableSeats` | Integer | Capacity minus the count of current registrations |

Input offsets are converted to instants; responses use UTC with `Z`.
Clients cannot set `id` or `availableSeats`. Do not bind incoming JSON directly
to an entity. Neither field is part of the create DTO.

## Business rules

| ID | Rule |
| --- | --- |
| WKS-R01 | Required text is trimmed and validated using the limits above. |
| WKS-R02 | Capacity must be a positive 32-bit integer; it is not inferred when omitted. |
| WKS-R03 | Capture one instant from injected `TimeProvider` for creation. Require `startsAt > now` and `endsAt > startsAt`. |
| WKS-R04 | Successful creation persists exactly one workshop with a new non-empty ID and no registrations. |
| WKS-R05 | Rejected creation leaves persisted workshops and registrations unchanged. |
| WKS-R06 | Available seats are derived from current registrations; successful registration consumes a seat and cancellation frees it. |
| WKS-R07 | Listing and detail reads include upcoming, ongoing, and completed workshops. Creation-time future validation is not reapplied to reads. |
| WKS-R08 | Topic filtering trims its input and performs case-insensitive exact matching; it is not substring search. |
| WKS-R09 | Lists are ordered by `startsAt`, then ID, ascending. Filtering preserves that order. |

Use culture-independent topic comparison consistently in query execution and
tests. It must support case-insensitive matching beyond ASCII; do not assume
SQLite's default collation implements the application policy. A normalized key
or equivalent persistence implementation may support this without exposing it
in response DTOs. Identical stored topics differing only in casing need not be
merged into a separate topic resource.

Availability is not a promise that a later registration request will succeed:
registration must recheck its requirements within its protected write operation.
The registration feature owns those mutation rules and concurrency protection.
The workshop feature must never return a negative available-seat count for valid
stored state; do not hide overbooking by clamping an invalid count to zero.

Time passing does not consume seats, remove registrations, or hide workshops.
There is no stored workshop status or background task that transitions one.
Completed seed workshops may be initialized directly, as defined in architecture.

## Endpoints

| Endpoint | Success | Expected feature failures |
| --- | --- | --- |
| `POST /api/workshops` | `201`, created DTO, `Location: /api/workshops/{id}` | `400` / `validation_failed` |
| `GET /api/workshops/{id}` | `200`, workshop DTO | `400` / `validation_failed`; `404` / `workshop_not_found` |
| `GET /api/workshops` | `200`, workshop array | None for an omitted filter |
| `GET /api/workshops?topic={topic}` | `200`, matching workshop array | `400` / `validation_failed` for empty or whitespace-only filter |

A well-formed unknown ID is `404`. A malformed or empty GUID is `400`.
An empty list or a valid filter with no matches returns `200` and `[]`.
Use `title`, `topic`, `capacity`, `startsAt`, and `endsAt` as validation keys
for their respective fields. Report a non-future start under `startsAt` and an
end not after the start under `endsAt`. Shared transport errors and the problem
body shape follow API conventions.

## Acceptance scenarios

Scenario IDs are stable references for tests and task handoffs. Preserve IDs
when refining wording; add new IDs for additional behavior. These scenarios
describe outcomes without requiring a particular implementation or agent role.

| ID | Given / when | Expected outcome |
| --- | --- | --- |
| WKS-001 | Valid title/topic, future start, later end, and positive capacity are submitted | `201`; one workshop persisted; non-empty ID; available seats equals capacity; following `Location` returns matching data |
| WKS-002 | Title or topic has surrounding whitespace | Stored and returned value is trimmed; internal whitespace and casing remain intact |
| WKS-003 | Required text is missing, null, empty, or whitespace-only | `400` / `validation_failed` with the relevant field key; no write |
| WKS-004 | Trimmed title is 200 characters or topic is 100 characters | Accepted when all other fields are valid |
| WKS-005 | Trimmed title is 201 characters or topic is 101 characters | `400` with the relevant field key; no write |
| WKS-006 | Capacity is omitted, null, zero, negative, fractional, or outside Int32 range | `400`; no workshop created |
| WKS-007 | Capacity is 1 or Int32 maximum | Accepted when all other fields are valid; available seats equals capacity |
| WKS-008 | Start is before or exactly at fixed `now` | `400` with `startsAt`; no write |
| WKS-009 | Start is one representable .NET tick after fixed `now`, with a later end | Accepted |
| WKS-010 | End is before or exactly at start | `400` with `endsAt`; no write |
| WKS-011 | End is one representable .NET tick after a valid future start | Accepted; no additional minimum duration is imposed |
| WKS-012 | Either timestamp is missing, null, malformed, or lacks an explicit offset | `400`; no write |
| WKS-013 | Valid timestamps use non-UTC offsets | Returned UTC timestamps preserve the submitted instants and end/start comparisons use instants |
| WKS-014 | An existing workshop ID is retrieved | `200` with all documented response fields |
| WKS-015 | A well-formed unknown ID is retrieved | `404` / `workshop_not_found` |
| WKS-016 | A malformed ID or empty GUID is retrieved | `400` / `validation_failed` |
| WKS-017 | No workshops exist | Unfiltered list returns `200` and `[]` |
| WKS-018 | Workshops have different starts and two share the same start | All are returned by start ascending, with ID ascending for the tie; insertion order has no effect |
| WKS-019 | Filter matches a topic with different casing and surrounding whitespace | Only exact topic matches are returned in documented order |
| WKS-020 | Filter is only part of a stored topic, or has no match | `200` and `[]` |
| WKS-021 | Supplied filter is empty or whitespace-only | `400` with `topic`; omitted filter remains valid |
| WKS-022 | Upcoming, ongoing, and completed workshops exist | List and detail reads include them; no creation-time rejection on reads |
| WKS-023 | A workshop has zero, some, or capacity-many current registrations | Detail and list show capacity, capacity minus count, or zero available seats respectively |
| WKS-024 | A registration succeeds, then is cancelled | Subsequent workshop reads show a seat consumed, then restored; verify using the registration endpoints once implemented |
| WKS-025 | Create input includes extra `id` or `availableSeats` properties | Client values cannot control the generated identity or derived availability |
| WKS-026 | Two valid creates have identical title, topic, and schedule | Both succeed with distinct IDs |
| WKS-027 | Filter and stored topic differ only by a non-ASCII case pair, such as `É` and `é` | They match under the same culture-independent comparison policy |

## Dependencies and verification

Creation and basic reads can be implemented before registration endpoints.
Availability depends on persisted registrations, not the registration controller;
fixtures can establish that state for early tests. WKS-024 becomes an end-to-end
cross-feature check when registration and cancellation are available.

Unit tests cover normalization, field rules, time boundaries, and application
outcomes. HTTP integration tests cover all three endpoints, serialization,
filter/order behavior, `Location`, validation, and persisted effects. Use controlled
time and isolated SQLite data; follow `testing.md` for coverage requirements.

Keep the specification, tests, and delivered behavior aligned as requirements
change. Do not treat a passing happy-path demo as completion of these scenarios.

Local entity construction and normalization rules are implemented and unit tested. Persistence constraints are verified independently; workshop application use cases and HTTP acceptance scenarios are implemented, including WKS-024.
