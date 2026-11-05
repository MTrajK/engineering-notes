Implement the core domain model, SQLite persistence foundation, and isolated test infrastructure for the Workshop Booking API.

**Goal**

Establish the entities, EF Core mappings, database lifecycle, mutation coordination, and testing fixtures needed by the three baseline features. Verify this foundation without implementing feature controllers or application use cases.

**Inspect before changing**

Read the root `AGENTS.md`, then review:

- `docs/architecture.md`
- `docs/testing.md`
- `docs/api-conventions.md`
- `docs/coding-guidelines.md`
- All three specifications under `docs/features/`.

Inspect the current application outcomes, HTTP infrastructure, test host, package configuration, and Git status. Preserve existing work and build on the technical decisions recorded during repository review.

**Implement the core domain model**

Create Workshop, Participant, and Registration entities using the documented fields and relationships.

- Preserve entity identity and protect local invariants.
- Keep Domain independent of Application, EF Core, and ASP.NET Core.
- Use explicit instants for time-dependent domain behavior; do not read the system clock inside entities.
- Implement the shared participant text/email policy in an appropriate framework-independent location accessible to Application.
- Preserve display values separately from normalization keys.
- Keep available seats derived rather than storing a mutable seat counter.
- Do not introduce authentication accounts, soft deletion, historical registration statuses, or future-feature entities.

Implement local construction and normalization rules needed by these entities, with meaningful unit tests. Cross-record checks and feature orchestration belong to later application use cases.

Provide a controlled way to materialize valid persisted or seeded historical records without incorrectly reapplying creation-time future-date requirements. Do not expose a general public bypass for invalid state.

**Implement EF Core persistence**

In Persistence, add:

- A scoped context.
- Entity configurations discovered from the Persistence assembly.
- Primary keys and required field mappings.
- Registration foreign keys to workshop and participant.
- A unique constraint on participant normalized email.
- A unique constraint on the workshop/participant registration pair.
- Appropriate storage representations for timestamps and normalization keys.

Preserve the required timestamp precision, UTC instant comparisons, deterministic ordering, and non-ASCII topic matching. Verify provider behavior rather than assuming default SQLite comparison and ordering semantics match the specifications.

Do not expose EF types, `IQueryable`, SQL, connections, or transactions through Application contracts.

**Establish application persistence boundaries**

Introduce only the focused contracts needed for the selected persistence and mutation-coordination approach.

Explain how upcoming feature use cases will perform existence checks, apply business-failure precedence, and execute protected writes through those contracts.

Do not implement unused generic CRUD repositories, speculative service interfaces, or placeholder methods throwing `NotImplementedException`. Add feature-specific operations in later tasks when their implementation and tests can be delivered together.

**Implement database lifecycle**

Use EF Core with SQLite in-memory, following the concrete strategy already selected.

- Keep an owning connection open for the database lifetime.
- Use scoped contexts for request work.
- Create the schema before accepting requests using `EnsureCreated`.
- Dispose resources at their owning scopes.
- Keep initialization separate from request handlers.
- Do not add migrations, an on-disk database, or a reset endpoint.

Wire the foundation into the host so the application starts with an initialized database. Leave production demo data empty until the seeding task; do not create placeholder workshops or participants.

Keep initialization structured so deterministic seeding can be added later and tests can control initialization before startup.

**Implement mutation coordination**

Implement the agreed synchronization or transaction mechanism needed to protect future registration checks and writes.

It must support:

- Atomic capacity/duplicate decisions and registration insertion.
- Coordination between registration creation and cancellation.
- Participant-email uniqueness under competing creation requests.
- Cancellation-aware acquisition and reliable release.
- Scoped contexts without concurrent operations on one context or unsafe shared-connection use.

Account for reads and initialization as well as mutations if they share connection resources. Keep synchronization appropriately scoped to the database instance so isolated tests do not share unrelated mutable coordination state.

The authoritative registration clock instant must be captured after acquiring the relevant mutation gate. Design for that behavior without implementing registration use cases yet.

Use database constraints as well as application coordination. Translate only recognized constraint failures through the agreed persistence/application boundary; do not classify all database exceptions as conflicts.

**Build isolated test infrastructure**

Extend the existing `WebApplicationFactory` infrastructure rather than introducing a competing host setup.

Provide:

- A controlled `TimeProvider`.
- A fresh SQLite database per test by default.
- Database and clock replacement before host initialization.
- Explicit fixture-data setup through scoped persistence access.
- Fresh-context verification helpers where useful.
- Reliable disposal of factories, clients, scopes, contexts, and owned connections.

If named in-memory databases are used, generate unique database names per test. Keep fixture business IDs and timestamps deterministic.

Preserve normal application services and persistence in HTTP tests. Test configuration must not accidentally initialize or access the normal demo database.

Do not add production-only endpoints for fixture setup or test inspection.

**Verify the foundation**

Add focused tests for:

- Entity invariants and normalization introduced in this task.
- Historical materialization without creation-time clock rejection.
- Entity mapping and timestamp round trips.
- Foreign-key enforcement.
- Normalized-email and registration-pair uniqueness.
- Database isolation between factories.
- Schema availability after startup.
- Mutation coordination, including release after failure and cancellation while waiting.
- Correct lifetime ownership and clean shutdown.

Persistence tests may live in the integration-test project and inspect the database directly. Do not present these as completed HTTP endpoint coverage.

Verify synchronization under contention using bounded coordination rather than sleeps. Full booking, cancellation, and competing HTTP mutation scenarios remain for the feature tasks.

**Boundaries**

Do not implement feature controllers, complete application use cases, demo seeding, coverage enforcement, feedback, or waiting-list functionality.

Do not rewrite shared HTTP infrastructure unless integration exposes a concrete defect. Explain any necessary change.

**Verification and documentation**

Run formatting verification, build, and both test suites. Confirm the application starts, initializes its database, and still serves Swagger in Development.

Review the changes against coding guidelines. Update architecture and testing documentation with the implemented connection, synchronization, timestamp-storage, normalization, and fixture decisions.

Clearly distinguish verified persistence guarantees from future endpoint behavior. Final application coverage is not a completion gate for this partial foundation task; report any coverage measured without claiming baseline compliance.

**Completion report**

Summarize:

1. Domain entities and invariants implemented.
2. Mappings, constraints, and database lifecycle.
3. Persistence boundaries and mutation coordination.
4. Test fixtures and isolation.
5. Tests and commands actually executed.
6. Documentation changes and remaining feature work.

Do not commit, push, or tag changes. Stop after the persistence and test foundation; workshop use cases and endpoints are the next task.