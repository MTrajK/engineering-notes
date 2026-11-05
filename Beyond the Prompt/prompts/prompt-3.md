Implement the shared application outcomes and HTTP infrastructure for the Workshop Booking API, building on the existing scaffold.

**Goal**

Establish reusable application outcomes, consistent HTTP error handling, JSON/date conventions, and working Swagger UI. Verify this infrastructure without implementing business features or persistence.

**Inspect before changing**

Read the root `AGENTS.md`, then review:

- `docs/architecture.md`
- `docs/api-conventions.md`
- `docs/testing.md`
- `docs/coding-guidelines.md`
- Relevant setup and decisions recorded in `README.md`.

Inspect the scaffold, package versions, project references, and Git status. Preserve existing work. Use the technical decisions already recorded; do not introduce a competing approach.

**Implement application outcomes**

In Application, implement the agreed small outcome model for:

- Successful operations, including operations returning a value.
- Field-validation failures.
- Missing resources.
- Business conflicts.

Keep the model independent of HTTP, ASP.NET Core, EF Core, and database exceptions.

Support stable application error codes and field-level validation information. Prevent contradictory states such as an outcome simultaneously indicating success and failure.

Keep the design straightforward. Do not introduce a generic workflow engine, mediator pipeline, exception-per-error hierarchy, or separate outcome framework for each feature.

**Implement shared HTTP mapping**

In API, provide one reusable mechanism for translating application outcomes into the documented HTTP responses.

Follow `docs/api-conventions.md` for:

- Status codes and stable error codes.
- Problem response fields and content type.
- Validation `errors` keys.
- Request-path `instance` without the query string.
- Opaque request-correlated `traceId`.
- Generic unexpected-error responses without leaked internals.

Configure automatic model validation to use the same public contract. Handle malformed JSON, incompatible types, and binding errors consistently, including mapping framework member paths to public camelCase names and `$` where appropriate.

Configure unmatched `/api` routes, unsupported methods, and unsupported media types to follow the documented problem conventions. Preserve framework headers such as `Allow` for `405`.

Scope this behavior appropriately so it does not replace Swagger pages or their assets with API problem responses.

**Implement exception handling**

Use centralized handling for unexpected exceptions.

- Log unexpected failures server-side and return the generic `internal_error` problem.
- Do not expose exception messages, stack traces, SQL, or submitted email values.
- Preserve cancellation semantics rather than translating request cancellation into a business failure.
- Do not create production endpoints for deliberately triggering errors.

Do not implement database-specific exception translation yet.

**Configure JSON and timestamps**

Implement the shared serialization and input behavior required by API conventions:

- camelCase DTO properties.
- String enums when applicable.
- Explicitly represented meaningful null response fields.
- ISO 8601 timestamp inputs requiring `Z` or an explicit numeric offset.
- UTC timestamp output using `Z`.
- Preservation of timestamp precision needed by the documented boundary scenarios.

Do not rely on server-local time when interpreting timestamps without offsets. Keep serialization infrastructure in API; Application may use `DateTimeOffset` without depending on HTTP.

Feature-specific required fields, normalization, and business rules remain for later tasks.

**Configure OpenAPI and Swagger UI**

Use the generation approach selected during scaffolding.

- Make Swagger UI available in Development at the documented path.
- Ensure it loads the generated OpenAPI document.
- Establish reusable problem schemas and response metadata support where useful.
- Keep package registration and middleware configuration coherent.
- Do not add dummy business endpoints to populate the document.

The document may contain no business operations until feature controllers are implemented. Do not claim endpoint schemas are complete at this stage.

**Add meaningful tests**

Add unit tests for application outcome behavior, including valid states and prevention of contradictory states. Tests must verify useful behavior rather than incidental implementation details.

For HTTP infrastructure tests, establish only the minimum `WebApplicationFactory` setup needed now. Database isolation, seeding replacement, and the full fixture infrastructure belong to the next task.

Use test-assembly-only controllers or equivalent test-host endpoints to exercise:

- Outcome-to-HTTP mapping.
- Automatic validation and binding failures.
- Shared problem fields and content types.
- Unexpected exceptions without information leakage.
- Unknown API routes, `405` with `Allow`, and `415`.
- Timestamp input/output behavior.
- Swagger UI and OpenAPI availability in Development.

Ensure test-only endpoints cannot be discovered in the normal application host. Keep normal shared middleware and serialization active; do not replace the components being verified with mocks.

Avoid asserting exact human-readable messages or exact trace identifiers.

**Boundaries**

Do not implement:

- Workshop, participant, or registration endpoints.
- Domain entities or business services.
- EF mappings, database initialization, persistence operations, or seed data.
- Full database test fixtures.
- Coverage enforcement.
- Authentication, caching, feedback, or waiting-list functionality.

Do not change feature requirements to fit the infrastructure. Resolve genuine contract ambiguities explicitly and document the resolution.

**Verify and review**

Run formatting verification, build, and both available test suites. Start the Development host and verify Swagger UI can load its OpenAPI document.

Review the changes against coding guidelines. Preserve the layer boundaries and fix analyzer findings without broadly suppressing rules.

Final business coverage requirements do not apply as a completion gate to this partial infrastructure task. Do not manufacture measurable Domain code or tests to satisfy them.

Update relevant documentation to distinguish implemented infrastructure from pending feature behavior and persistence.

**Completion report**

Provide:

1. Outcome model and HTTP mapping implemented.
2. JSON, timestamp, validation, and exception-handling decisions.
3. Swagger/OpenAPI verification results.
4. Tests added and commands actually executed.
5. Confirmation that test-only endpoints are absent from the normal host.
6. Documentation changed and remaining gaps.

Do not commit, push, or tag changes. Stop after shared application and HTTP infrastructure; do not proceed to persistence implementation.