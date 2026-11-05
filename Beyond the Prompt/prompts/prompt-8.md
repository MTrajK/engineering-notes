Implement deterministic startup seed data and complete the OpenAPI/Swagger documentation for the Workshop Booking API.

Work directly in the current repository using ordinary Codex capabilities. Do not invoke skills, spawn subagents, create custom-agent definitions, configure hooks, or introduce a multi-agent workflow.

**Goal**

Make the completed baseline easy to demonstrate through Swagger. Starting the application must create a predictable set of useful scenarios; restarting must restore that baseline.

Preserve the existing endpoint behavior, business rules, database lifecycle, and test isolation.

**Inspect before changing**

Read the root `AGENTS.md`, then review:

- `README.md`
- `docs/architecture.md`
- `docs/api-conventions.md`
- `docs/testing.md`
- Relevant feature specifications.
- `docs/coding-guidelines.md` when reviewing the implementation.

Inspect the database initializer, `TimeProvider` registration, domain construction/materialization paths, test fixtures, current OpenAPI configuration, and Git status.

Reuse the established infrastructure. Resolve gaps without weakening feature requirements.

**Implement startup seeding**

Keep seed data and seeding logic in Persistence. Invoke initialization from the existing application startup composition.

- Create the schema and seed the fresh database before accepting requests.
- Capture one startup instant from injected `TimeProvider`.
- Calculate every seed timestamp from that single instant.
- Assign explicit, stable GUIDs to seeded workshops, participants, and registrations.
- Use readable fictional names, topics, and email addresses.
- Maintain valid references, unique normalized emails, unique registration pairs, and capacity constraints.
- Avoid random data, fixed calendar dates that become stale, and independent clock reads.
- Keep the implementation small; use code-defined seed data or an explicit data file according to existing decisions.

The seeded state should include at least:

| Scenario | Required state |
| --- | --- |
| Upcoming workshop with free seats | Starts comfortably after startup; contains at least one registration and available capacity |
| Full upcoming workshop | Starts comfortably after startup; registration count equals capacity |
| Completed workshop | Ends before startup; contains registrations that were created before its start |
| Unregistered participant | Available for successful booking and full-workshop conflict demonstrations |

Use relative offsets suitable for a presentation rehearsal, such as upcoming workshops several days ahead and a completed workshop several days behind. Record the actual chosen offsets and capacities.

Seed historical records through the established valid initialization path. Do not call creation use cases that correctly reject past workshops, and do not weaken those use cases to accommodate seeding.

**Preserve initialization boundaries**

- Seed only during initialization, not inside request handlers.
- Do not reseed after a caller cancels or creates a registration.
- Preserve SQLite in-memory ownership and disposal.
- Do not add a reset endpoint, on-disk persistence, migrations, or automatic background resets.
- A process restart must discard changes and recreate the documented seed identities and scenarios with newly calculated relative dates.
- Handle initialization atomically where practical so the application does not start with partially seeded data.
- If initialization can be invoked repeatedly within one host, prevent duplicate seed insertion without restoring records deliberately changed after startup.

Keep normal integration tests on explicit isolated fixtures with demo seeding disabled. Add separate tests that enable the real seeder with a controlled clock.

**Complete OpenAPI documentation**

Review all nine implemented baseline operations:

- Workshop list, detail, and creation.
- Participant list, detail, and creation.
- Registration listing, creation, and cancellation.

Ensure the generated document accurately describes:

- Routes, methods, operation summaries, and resource grouping.
- HTTP request and response DTOs.
- Required fields, relevant limits, and parameter requirements.
- Optional workshop topic filtering.
- Required registration `workshopId` filtering.
- Success statuses and applicable problem responses.
- Validation problem fields.
- Creation `Location` headers where emitted.
- Empty `204` cancellation responses.
- Timestamp representation and explicit-offset input requirements.

Do not expose domain entities, normalization keys, or EF metadata as public schemas. Do not advertise a registration GET-by-ID route.

Swagger UI must load and execute the implemented operations in Development using the documented local startup command.

**Provide practical examples**

Add concise OpenAPI examples where the chosen tooling supports them without unnecessary packages or custom frameworks.

Examples must:

- Use valid JSON and documented field names.
- Use seeded identities for cross-resource requests where appropriate.
- Avoid hard-coded creation dates that will become invalid over time.
- Explain how to choose future dates for workshop creation if dynamic examples are not straightforward.
- Distinguish schema examples from data guaranteed to exist in the current host.

Do not overcomplicate OpenAPI generation solely to make every example dynamic.

**Document a repeatable Swagger walkthrough**

Update README with a short sequence that demonstrates:

1. Listing workshops and participants.
2. Inspecting availability in an upcoming workshop.
3. Creating a registration for an unregistered participant.
4. Observing reduced availability.
5. Repeating that request to demonstrate the duplicate conflict.
6. Attempting registration in the full upcoming workshop with an eligible participant.
7. Cancelling the successful registration and observing the restored seat.
8. Restarting the application to restore the seed baseline.

Identify the stable seed records needed for the sequence. Use the registration ID returned by creation for cancellation.

Clarify that restarting preserves seeded IDs but recalculates seed dates relative to the new startup instant. Do not imply arbitrary user-created records survive restart.

**Add meaningful verification**

Add tests covering:

- Stable seed identities across independent initialized databases.
- All relative dates derived from one controlled startup instant.
- Required scenario relationships and capacity counts.
- Valid historical registration timestamps.
- Normalized-email and registration-pair uniqueness.
- No duplicate initialization or request-triggered reseeding.
- Isolation between ordinary fixtures and seeded hosts.
- Fresh-host restoration after mutations.

Add focused OpenAPI checks for the documented paths, key schemas, and response contracts. Prefer targeted assertions over a brittle snapshot of the entire generated document.

Exercise the important seeded scenarios through real HTTP integration requests. Verify stored effects and availability, not only successful startup.

**Boundaries**

Do not implement feedback, waiting-list behavior, authentication, notifications, coverage enforcement, or unrelated feature changes.

Do not introduce production debug endpoints or a separate frontend. Preserve the baseline scope and existing API contracts.

**Verify and review**

Run formatting verification, build, both test suites, and all tests using the documented commands.

Start the Development host and verify Swagger UI and its OpenAPI document are reachable. Perform the walkthrough through available HTTP tools; inspect the interactive UI when the environment supports it. Clearly distinguish HTTP verification from actual browser/UI verification.

Restart the host and confirm the baseline data is restored.

Review changes against coding guidelines. Update documentation from planned to implemented only where the corresponding behavior has been verified.

**Completion report**

Provide:

1. Seed scenarios, stable identities, capacities, and relative-date choices.
2. Initialization and test-isolation behavior.
3. OpenAPI/Swagger improvements.
4. Walkthrough and restart verification results.
5. Tests and commands actually executed.
6. Documentation changes and unresolved limitations.

Do not commit, push, or tag changes. Stop after demo data and OpenAPI completion; acceptance-coverage auditing and coverage enforcement are next.