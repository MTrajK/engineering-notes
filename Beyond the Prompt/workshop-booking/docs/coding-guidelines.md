# Coding guidelines

## Purpose and use

These are project-specific design and review guidelines for the Workshop Booking
API. They complement the shared constraints in [AGENTS.md](../AGENTS.md), the
[architecture](architecture.md), [API conventions](api-conventions.md), and
[testing guidance](testing.md). They describe intended implementation choices;
application code and automated configuration have not been scaffolded yet.

Use this document during final review of code changes by default, and when
explicitly assigned code review. A selected workflow, skill, or custom-agent
definition may assign detailed review to a separate stage and narrow which
documents an implementer reads. Follow that assigned scope. No particular roles
or delegation process are required by this document.

Shared architecture boundaries, API contracts, correctness, and enforced
formatter/analyzer rules remain applicable throughout implementation. Deferring
detailed review does not permit broken contracts or unsafe persistence behavior.

## Automated rules

Keep mechanical formatting, naming conventions, and supported style diagnostics
in `.editorconfig` and build configuration. Use `dotnet format` and the configured
analyzers rather than maintaining a second formatting rulebook here.

Review the remaining design decisions and readability issues that those tools
cannot establish. Fix warnings rather than suppressing them to make checks pass.
A necessary suppression must be narrowly scoped and explain the concrete reason.
Do not disable nullable analysis or weaken solution-wide checks for one feature.

## Design for the current use case

- Prefer a direct, readable implementation that satisfies the acceptance criteria.
  Add abstractions for a useful boundary or established variation, rather than
  hypothetical future features.
- Keep controllers focused on HTTP and application services focused on use cases.
  Keep EF configuration, query execution, and transaction details in Persistence.
- Use focused persistence interfaces owned by Application. Do not add a generic
  repository framework, mediator pipeline, or separate command/query framework
  without an assigned requirement.
- Constructor-inject required dependencies. Avoid service location, static
  mutable services, or dependencies hidden behind global accessors.
- Give each method one clear purpose. Split code when it makes a business step
  easier to understand or reuse; avoid arbitrary method/class length limits.
- Use names that describe the business concept or outcome. Avoid ambiguous
  helpers such as `Process`, `Manager`, or `Common` when a precise name fits.

## Types and domain state

- Use records for request/response DTOs and simple immutable application messages.
  Use classes for EF Core entities with identity and lifecycle.
- Model required data explicitly. Use nullable values only where absence has a
  meaning; do not use null-forgiving operators to hide missing initialization.
  A narrowly scoped EF materialization requirement may justify an exception.
- Do not return mutable tracked entities as HTTP responses. Map explicitly to
  DTOs, keeping persistence details and normalization keys private.
- Put local invariants in domain construction or behavior. Do not let public
  setters make invalid state the normal mutation path. EF materialization and
  seed setup must remain compatible with the selected entity design.
- Avoid introducing a value-object type for every primitive. Introduce one when
  it centralizes meaningful rules or prevents an actual category of mistakes.
- Keep available seats derived from capacity and registrations. Do not duplicate
  state whose consistency then depends on every caller updating both copies.

## Validation and outcomes

Use the agreed application outcome types for expected validation, missing-resource,
and conflict results. Keep HTTP status mapping in the API and database-specific
exception interpretation in Persistence. Do not add a separate result hierarchy
for each feature.

Transport validation and business validation serve different entry points.
Application behavior must not assume that every caller passed through an HTTP
controller. Share normalization and business validation policies where needed
without making Application depend on API DTOs or validation attributes.

Preserve the failure precedence and stable error codes in API conventions.
Do not use message parsing to identify failures or blanket exception handling
to turn every failure into a conflict. Allow unexpected exceptions to reach
central handling; preserve cancellation rather than reporting it as a business
rejection. Do not expose internal exception text in responses.

## Async work and resource ownership

- Use asynchronous database operations and propagate cancellation tokens through
  application and persistence contracts. Avoid `.Result`, `.Wait()`, and wrapping
  synchronous database work in `Task.Run`.
- Return `Task` or `Task<T>` for normal asynchronous application operations.
  Use `async void` only where a framework-required event signature demands it.
- Keep context, connection, and transaction ownership explicit. Dispose resources
  at the owning scope; do not dispose an injected dependency owned by the host.
- Do not share an EF context across requests or run parallel operations on the
  same context. Database lifetime does not determine context lifetime.
- Keep background/fire-and-forget work out of request handling. Background jobs
  are outside the baseline scope.

## Queries, writes, and time

Review queries for explicit filters, ordering, bounded work appropriate to the
requested feature, and unnecessary materialization. Apply filtering and projection
in persistence queries where practical. Use no-tracking reads when entities
will not be modified; avoid repeated per-row lookups when one query suffices.

Do not add pagination or other behavior solely as a review improvement when it
changes the agreed contract. The baseline intentionally has small demo datasets.
Do not replace clear code with pooling, unsafe code, spans, specialized collections,
or custom caching without measured need and authorization within the task.

Keep capacity checks and registration writes protected as one operation. Review
the actual concurrency strategy and database constraints; passing sequential
tests alone does not establish correctness under competing requests. Avoid
rechecking state outside the protected operation and trusting that stale result.

Use injected `TimeProvider` for business decisions and capture one instant per
operation. Pass that instant into domain behavior where appropriate. Respect
the documented UTC representation, email normalization, and exact time boundaries.
Do not introduce independent convenience implementations of those policies.

## Readability and duplication

Use early returns when they make failure paths clearer. Keep successful behavior
easy to follow and avoid deeply nested conditions. Name intermediate values
when they explain a business decision; avoid clever LINQ that obscures mutation
or important branching.

Remove duplicated business rules, normalization policies, and error mapping.
Similar-looking code with different semantics need not share a generic helper.
Prefer a small amount of explicit mapping over a new mapping framework.

Comments should explain intent, constraints, or a non-obvious decision. Remove
comments that restate the code, commented-out implementations, and obsolete
TODOs. Record durable cross-layer decisions in architecture documentation.

## Tests and refactoring

Review test quality against acceptance criteria, rather than test counts alone.
Assertions must distinguish correct behavior from plausible incorrect behavior.
Check deterministic time, isolation, meaningful boundary cases, stored effects,
and concurrency where relevant. Follow `testing.md` for coverage and commands.

Preserve behavioral assertions while simplifying production code. Updating a
test is appropriate when a documented contract changes or a test encoded an
implementation detail; explain that reason. Do not weaken assertions, alter
fixtures to avoid a failure, or broaden coverage exclusions to obtain green checks.

Keep review refactoring within the assigned scope. Separate unrelated cleanup
from a feature change. If an improvement would alter behavior, shared contracts,
or architecture, resolve it against the relevant specification before changing it.

## Review completion

- Check the combined change against acceptance criteria and shared contracts.
- Resolve correctness and integration issues before optional readability changes.
- When integrating parallel work, reconcile duplicated policies, inconsistent
  outcomes, and shared registration/configuration changes.
- Run the checks appropriate to the final changes. Completed application changes
  require format verification, build, both test suites, and the coverage gate.
- Report concrete unresolved issues and checks actually run. Distinguish verified
  results from planned tooling; coverage enforcement is pending until scaffolded.

Do not add features, packages, or layers merely to satisfy a personal style
preference. The review should leave the demo easier to understand and extend.
