# API conventions

## Status and scope

This document defines the HTTP conventions for the Workshop Booking API.
Shared problem handling, JSON settings, and timestamp serialization are
implemented and verified by HTTP infrastructure tests. Business endpoint
contracts apply to the implemented baseline and later extensions.

Feature documents in `features/<feature>.md` define business rules and acceptance
scenarios. This file defines how those outcomes are represented over HTTP.
The running OpenAPI document describes implemented routes and DTO schemas.

## Routes and identifiers

- Use controller-based endpoints under `/api` with lowercase resource names.
- Keep the baseline routes in [README](../README.md). Do not introduce URL
  versioning, nested route alternatives, or extra endpoints without a requirement.
- Use server-generated, non-empty GUID identifiers. Seed identifiers remain
  stable across restarts; newly created resources receive new identifiers.
- A malformed identifier or an empty GUID is `400`; a well-formed identifier
  that does not identify the required resource is `404`.
- Preserve that distinction during routing and model binding. Do not use a
  route constraint that turns malformed resource identifiers into route misses.

## JSON contracts

- Use JSON request and response DTOs with camelCase property names.
- Accept `application/json` for JSON request bodies. Return successful JSON
  bodies as `application/json` and problem bodies as `application/problem+json`.
- Use strings for enum values if a feature introduces an enum; document the
  allowed values. Do not expose internal enum ordinals as API contracts.
- Expose only explicit DTO fields, including resource IDs and agreed derived
  values. Do not expose EF navigation graphs, persistence metadata, or email
  normalization keys.
- Missing required fields, explicit nulls for required values, malformed JSON,
  and incompatible JSON types produce `400`.
- Use nullable request properties or equivalent validation where needed to
  distinguish a missing required value from a CLR default.
- Unknown JSON properties may be ignored; never map them into entities or
  allow clients to set server-owned fields.
- Return DTOs directly. Collections are JSON arrays; do not add a universal
  success envelope. Empty collections are `[]`.
- Return meaningful nullable response fields explicitly as `null`; do not use
  zero or an empty string to represent an unavailable value.

## Success responses

| Operation | Status | Body and headers |
| --- | --- | --- |
| Get a resource | `200 OK` | Resource response DTO |
| List or filter resources | `200 OK` | Array, including an empty array |
| Create a resource | `201 Created` | Created resource response DTO |
| Cancel a registration | `204 No Content` | No response body |

For workshop and participant creation, return a `Location` header pointing to
the corresponding implemented GET-by-ID endpoint. Registration creation returns
its created DTO, including its ID. The baseline has no registration GET-by-ID
route, so omit `Location` rather than advertise a nonexistent endpoint.

DELETE removes the registration. A repeated DELETE returns `404` because the
registration no longer exists. Do not add a soft-delete or historical status
model without an explicit feature requirement.

## Failure mapping

| Condition | Status | Stable `code` |
| --- | --- | --- |
| Binding, field validation, or invalid creation values | `400 Bad Request` | `validation_failed` |
| Required workshop does not exist | `404 Not Found` | `workshop_not_found` |
| Required participant does not exist | `404 Not Found` | `participant_not_found` |
| Registration being cancelled does not exist | `404 Not Found` | `registration_not_found` |
| Participant email is already used | `409 Conflict` | `participant_email_conflict` |
| Participant already registered for the workshop | `409 Conflict` | `registration_duplicate` |
| Workshop has no available seats | `409 Conflict` | `workshop_full` |
| Registration targets a workshop that has started | `409 Conflict` | `workshop_started` |
| Unexpected server failure | `500 Internal Server Error` | `internal_error` |

For example, a nonpositive capacity or a creation start time that is not in the
future is validation failure. A valid registration request rejected because of
the existing workshop state is a conflict. Do not introduce `422` for this API.

Use `405` for an unsupported method on a recognized route and `415` for an
unsupported request content type. These are transport failures, distinct from
application outcomes. Unmatched routes return `404` with `route_not_found`.
Use `method_not_allowed` and `unsupported_media_type` for their respective codes.
Preserve required framework headers, such as `Allow` on a `405` response.

### Problem response shape

Use one ProblemDetails-compatible shape for API failures, including automatic
validation and framework-generated API errors. A representative conflict is:

```json
{
  "type": "urn:workshop-booking:problem:workshop_full",
  "title": "Workshop is full",
  "status": 409,
  "detail": "The workshop has no available seats.",
  "instance": "/api/registrations",
  "code": "workshop_full",
  "traceId": "example-trace-id"
}
```

`type` follows `urn:workshop-booking:problem:<code>`. `instance` is the request
path without its query string. `traceId` correlates with server diagnostics and
is opaque to clients. Titles and details are readable messages; clients and
tests identify failure categories using `status` and `code`.

Validation problems also include `errors`, a map of field names to arrays of
messages. Use camelCase DTO or query names, such as `capacity` or `workshopId`.
Use `$` for body-level JSON failures and request-level validation. Translate
framework member paths into these public names before returning them.

Do not return raw exceptions, SQL, stack traces, or submitted email values in
problem messages. Log unexpected exceptions server-side and return a generic
`internal_error` problem. Client disconnection or request cancellation does not
require a fabricated business error or a guaranteed response after disconnection.

### Multiple failures

Collect field validation failures before accessing persistence. When several
business failures apply, return one outcome with a deterministic precedence:

1. Validate the request shape and independent field values.
2. Resolve required resources: workshop first, then participant where applicable.
3. For registration, check workshop start, duplicate registration, then capacity.
4. Perform the protected write and map recognized concurrent conflicts using
   the same codes.

Future feature specifications define their own business-check ordering while
preserving validation and missing-resource conventions. Rejected mutations must
leave stored data unchanged.

## Text, email, and time

Trim surrounding whitespace from user-facing text before validation and storage.
Required text cannot be empty or whitespace-only after trimming. Feature
specifications define length limits and additional field constraints.

For participant email, retain the trimmed submitted value for display. Validate
its format using one shared application policy, then derive a lookup key with
`ToUpperInvariant()`. Apply the same normalization to uniqueness checks and the
database constraint. This API intentionally treats the whole address as
case-insensitive. Do not apply provider-specific dot or plus-address rewriting.

Workshop topic filtering follows the same invariant-key approach: trim the
filter, derive an uppercase-invariant exact-match key, and compare with the
persisted key using binary equality. Do not rely on SQLite `NOCASE`, which does
not provide the required non-ASCII case folding. Return the original trimmed
topic value. Keep normalized topic and email keys out of response DTOs.

Represent instants with `DateTimeOffset` in application contracts. Accept ISO
8601 timestamps with an explicit `Z` or numeric offset and normalize responses
to UTC with `Z`. Reject timestamps without an offset; do not infer server-local
time. Configure binding and serialization explicitly to enforce this contract.
Use a dedicated JSON converter or equivalent strict input parser so values
without an explicit offset cannot silently inherit a default offset. Keep output
serialization normalized to UTC.

Business checks obtain time from injected `TimeProvider`, capturing one instant
per operation. Feature documents define exact start/end comparisons; tests use
a controlled clock. Dates remain instants even when a caller supplies a local
offset. Display-time-zone conversion is outside the baseline scope.

## Filtering and ordering

- `GET /api/workshops` optionally accepts `topic`. Trim the filter and use a
  case-insensitive exact match. An omitted filter lists all workshops; an empty
  or whitespace-only supplied filter is `400`. No matches returns `200` and `[]`.
- `GET /api/registrations` requires a non-empty GUID `workshopId`. A missing or
  malformed filter is `400`; a nonexistent workshop is `404`; an existing
  workshop without registrations returns `200` and `[]`.
- Order workshops by start time, then ID; participants by normalized email,
  then ID; and registrations by creation time, then ID, all ascending.
- Apply ordering explicitly. Never rely on database insertion order. Preserve
  the same ordering semantics in integration tests.
- Pagination and free-text search are outside the baseline. Feature additions
  must document any new query parameters and ordering guarantees.

## OpenAPI and verification

Configure interactive Swagger UI for local development. Describe request and
response DTOs, required parameters, success statuses, and applicable problem
responses. The public `ApiProblemDetails` type is the reusable response schema;
feature actions can reference it with `ProducesResponseType` metadata. The
production document includes workshop create, detail and list operations. Test-only controllers
exercise problem schema/response metadata in the integration host. Document
`Location` where emitted. Avoid exposing EF entities as OpenAPI schemas. Use
controller-based API conventions and one shared problem
mapping path: `[ApiController]` automatic model-state failures are converted by
the invalid-model-state response factory; application outcomes map centrally;
unexpected exceptions map through exception middleware; status-code pages cover
unmatched routes and framework transport failures. Preserve `Allow` on `405`
and use `415` for unsupported media types. The API owns error DTOs and field-name
translation. Enable Swagger/OpenAPI and Swagger UI only in Development; do not
add an external API-versioning package or custom success envelope.

Integration tests verify HTTP status, content type, relevant headers, response
shape, and stored effects for every endpoint. Cover automatic validation and
recognized database conflicts as well as controller-generated outcomes. Avoid
asserting exact human-readable messages, generated IDs, or trace identifiers.

See [architecture](architecture.md) for dependency and persistence boundaries.
`testing.md` and `features/<feature>.md` provide test guidance and business
acceptance scenarios for the implementation.

BaselineOpenApiFilter and DemoSchemaFilter document the nine operations, controller resource tags, trimmed field limits, required filters, Location headers and application/problem+json. Registration has no GET-by-ID route. Schema examples are illustrative; registration examples refer to fresh demo seed IDs. Workshop example dates are seven days after document generation and must remain future dates when executed.
