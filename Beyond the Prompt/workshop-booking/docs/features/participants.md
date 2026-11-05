# Participants

## Status and purpose

Participant creation, detail and listing are implemented and verified against PAR-001 through PAR-026.
Create participants and retrieve their identity and contact details so other
features can refer to them by ID. A participant is a stored domain record,
not an authenticated account.

The fields and limits below are the selected baseline contract. Follow
[API conventions](../api-conventions.md) for JSON and problem responses,
[architecture](../architecture.md) for layer boundaries and persistence, and
[testing](../testing.md) for verification guidance.

## Scope

- Create a participant with a name and unique email address.
- Retrieve a participant by ID.
- List all participants in a deterministic order.

Updating or deleting participants, authentication, email ownership verification,
email delivery, search, filtering, and pagination are outside this feature.
Registration, feedback, and waiting-list operations belong to their own features.
Creating a participant does not register them for any workshop.

## Data contract

### Create input

| Field | Type | Requirement |
| --- | --- | --- |
| `name` | String | Required; 1–100 characters after trimming |
| `email` | String | Required; at most 254 characters after trimming; valid under the policy below; unique after normalization |

Text limits count .NET UTF-16 code units, matching `string.Length`. Trim
surrounding whitespace before validation and storage. Preserve internal name
whitespace, punctuation, Unicode characters, and casing. Do not split names into
first/last components or require names to be unique.

### Response

Creation, detail, and list items use the same response fields:

| Field | Type | Meaning |
| --- | --- | --- |
| `id` | GUID | Server-generated participant identity |
| `name` | String | Trimmed display name |
| `email` | String | Trimmed submitted email, preserving its display casing |

Clients cannot set the ID or normalization key. Neither belongs in the create
DTO, and the normalization key must never appear in a response. Do not include
registration collections or workshop navigation graphs in participant responses.

## Email policy

Use one shared application policy for all participant creation paths. For this
demo, a valid email must satisfy all of the following after trimming:

- It is nonempty and no longer than 254 UTF-16 code units.
- It contains exactly one `@`, with nonempty text on both sides.
- It contains no whitespace or control characters internally.
- It contains no display-name/address-list delimiters: `<`, `>`, `,`, or `;`.
- It passes `System.ComponentModel.DataAnnotations.EmailAddressAttribute.IsValid`.

This is a pragmatic application validation policy, not proof of deliverability.
Do not add DNS checks, network calls, verification messages, a custom comprehensive
email parser, or a second controller-specific regex. A dot in the domain is not
an additional requirement; `alex@example` is valid under this baseline policy.

After validation, derive the uniqueness key from the trimmed address with
`ToUpperInvariant()`. Use the same key for application lookups and the database
unique constraint. The whole address is intentionally case-insensitive for this
API. Do not remove dots, strip plus tags, rewrite domains, or perform additional
Unicode normalization. Different keys represent different participant emails.

For example, ` Alex.Example+demo@Example.com ` is displayed as
`Alex.Example+demo@Example.com` and has key `ALEX.EXAMPLE+DEMO@EXAMPLE.COM`.
`AlexExample@Example.com` and `Alex.Example@Example.com` remain different keys.

## Business rules

| ID | Rule |
| --- | --- |
| PAR-R01 | Name is required and must satisfy the trimmed text limits. Duplicate names are allowed. |
| PAR-R02 | Email is required and must satisfy the shared email policy. |
| PAR-R03 | Only one participant may exist per normalized email key. |
| PAR-R04 | Successful creation persists exactly one participant with a new non-empty GUID and no registrations. |
| PAR-R05 | Validate fields before checking uniqueness. Invalid requests return validation failure even if their email also belongs to an existing participant. |
| PAR-R06 | A duplicate email returns a conflict without overwriting or returning the existing participant as a successful creation. |
| PAR-R07 | Rejected creation leaves participants and registrations unchanged. |
| PAR-R08 | Listing returns every participant ordered by normalized email, then ID, ascending. |

Use ordinal ordering of normalized email keys; do not rely on the machine's
current culture or default database row order. The ID tie-breaker follows the
shared API convention. Although the unique email constraint prevents equal keys
in valid stored state, retain the complete documented sort contract.

Enforce normalized-email uniqueness in the database as well as application
behavior. Concurrent attempts with the same key must produce one creation and
one `participant_email_conflict`, with one row persisted. Translate the recognized
unique constraint failure; unrelated database failures remain unexpected errors.

## Endpoints

| Endpoint | Success | Expected feature failures |
| --- | --- | --- |
| `POST /api/participants` | `201`, created DTO, `Location: /api/participants/{id}` | `400` / `validation_failed`; `409` / `participant_email_conflict` |
| `GET /api/participants/{id}` | `200`, participant DTO | `400` / `validation_failed`; `404` / `participant_not_found` |
| `GET /api/participants` | `200`, participant array | None for a valid unfiltered request |

A well-formed unknown ID is `404`. A malformed ID or empty GUID is `400`.
An empty database returns `200` and `[]` for the list. Use `name` and `email`
as validation keys for their respective input fields. Shared transport errors
and problem shape follow API conventions. Conflict responses must not reveal
the existing participant's ID or echo the submitted email.

## Acceptance scenarios

Scenario IDs are stable references for tests and task handoffs. Keep their IDs
when refining wording and add new IDs for additional behavior.

| ID | Given / when | Expected outcome |
| --- | --- | --- |
| PAR-001 | A valid name and unused valid email are submitted | `201`; one participant persisted with a non-empty ID and documented fields; following `Location` returns matching data |
| PAR-002 | Name and email contain surrounding whitespace | Both are trimmed before validation and storage; email casing and internal name whitespace remain intact |
| PAR-003 | Name is missing, null, empty, or whitespace-only | `400` / `validation_failed` with `name`; no write |
| PAR-004 | Trimmed name is 1 or 100 characters | Accepted when email is valid and unused |
| PAR-005 | Trimmed name is 101 characters | `400` with `name`; no write |
| PAR-006 | Name includes Unicode characters, punctuation, or multiple words | Accepted within the length limit and returned without internal rewriting |
| PAR-007 | Email is missing, null, empty, or whitespace-only | `400` with `email`; no write |
| PAR-008 | Email lacks `@`, contains multiple `@` characters, or has an empty side | `400` with `email`; no write |
| PAR-009 | Email contains internal whitespace/control characters or display-name/list delimiters | `400` with `email`; no write |
| PAR-010 | A policy-valid email is exactly 254 characters after trimming | Accepted when unused and the name is valid |
| PAR-011 | Email is 255 characters after trimming | `400` with `email`; no write |
| PAR-012 | Email is `alex@example` | Accepted when unused; no extra dotted-domain rule |
| PAR-013 | Email exactly matches an existing participant's address | `409` / `participant_email_conflict`; existing record unchanged and no extra row |
| PAR-014 | Email differs from an existing address only by casing or surrounding whitespace | Same conflict as an exact duplicate |
| PAR-015 | Emails differ by a plus tag or dots and produce different normalized keys | Separate participants may be created; no provider-specific rewriting |
| PAR-016 | Two valid creates use the same name but different normalized email keys | Both succeed with distinct IDs |
| PAR-017 | Invalid name accompanies an existing valid email | `400` with `name`, rather than `409`; no write |
| PAR-018 | An existing participant ID is retrieved | `200` with only the documented participant fields |
| PAR-019 | A well-formed unknown ID is retrieved | `404` / `participant_not_found` |
| PAR-020 | A malformed ID or empty GUID is retrieved | `400` / `validation_failed` |
| PAR-021 | No participants exist | List returns `200` and `[]` |
| PAR-022 | Participants are inserted out of normalized-email order, with mixed display casing | List follows ordinal normalized-email order and preserves each displayed email |
| PAR-023 | Concurrent valid creates use case variants of one address | Exactly one `201`, one `409` / `participant_email_conflict`, and one matching stored row |
| PAR-024 | Create input includes extra `id` or normalization-key properties | Client values cannot control identity or uniqueness; response does not expose internal keys |
| PAR-025 | Any participant creation succeeds or is rejected | No workshop or registration records are added or modified |
| PAR-026 | Server culture changes while the same case-variant addresses are checked | The invariant normalization policy and ordinal ordering remain unchanged |

## Dependencies and verification

This feature can be implemented independently of workshop and registration
controllers. Other features depend on participant existence by ID and must use
the agreed application/persistence contracts, not another controller's methods.

Unit tests cover name validation, the email policy, normalization, expected
outcomes, and avoiding writes on rejection. HTTP integration tests cover all
three endpoints, JSON/binding failures, `Location`, list ordering, database
uniqueness, and competing creates. Include fresh-context or HTTP readback of
stored effects, not only response status assertions.

Keep this specification, tests, and delivered behavior aligned as requirements
change. Use the shared testing document for isolation and coverage requirements.

Local entity construction and normalization rules are implemented and unit tested. Persistence constraints are verified independently; participant use cases and HTTP acceptance scenarios are implemented.
