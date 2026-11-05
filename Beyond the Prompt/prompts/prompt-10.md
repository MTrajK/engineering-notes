Perform the final combined review and presentation rehearsal for the Workshop Booking API. Fix issues within the agreed baseline scope and leave a verified application ready for a baseline commit or tag.

Work directly in the current repository using ordinary Codex capabilities. Do not invoke skills, spawn subagents, create custom-agent definitions, configure hooks, or introduce a multi-agent workflow.

**Goal**

Verify the completed application as a whole: behavior, architecture, maintainability, tests, tooling, documentation, and demo operation.

Do not assume earlier task reports prove the current repository is correct. Inspect the delivered code and run the required checks.

**Inspect before changing**

Read the root `AGENTS.md`, then review:

- `README.md`
- `docs/architecture.md`
- `docs/api-conventions.md`
- `docs/testing.md`
- `docs/coding-guidelines.md`
- All three baseline feature specifications.
- Shared configuration, SDK/package configuration, and verification scripts.

Inspect Git status and preserve existing work. Distinguish pre-existing changes from changes made during this task.

**Review the combined implementation**

Check the application against its documented contracts, prioritizing correctness and integration issues.

Verify:

- Project dependencies and layer responsibilities.
- Controllers using application contracts rather than EF Core.
- Consistent validation, application outcomes, error codes, and HTTP mapping.
- Protected domain state and explicit DTO mapping.
- Email normalization and uniqueness.
- Workshop filtering, ordering, timestamps, and derived availability.
- Registration failure precedence, atomic capacity protection, and cancellation.
- Connection, context, transaction, and synchronization lifetimes.
- Cancellation propagation and resource disposal.
- Deterministic seeding and isolated tests.
- Absence of test-only endpoints from the normal application host.

Fix demonstrated defects within scope. Do not replace the established architecture or introduce new frameworks to satisfy personal preferences.

**Apply coding guidelines**

Simplify code where there is a concrete benefit:

- Consolidate duplicated business policies, normalization, and error mapping.
- Remove obsolete code, unused dependencies, misleading comments, and abandoned placeholders.
- Improve unclear names and unnecessarily complicated control flow.
- Remove speculative abstractions without useful responsibilities.
- Preserve clear persistence boundaries and explicit ownership.

Keep refactoring focused and preserve observable behavior. Do not weaken tests, change acceptance criteria, broaden coverage exclusions, or suppress diagnostics to make verification pass.

If a proposed improvement would materially change business behavior or scope, report it separately rather than implementing it as cleanup.

**Audit test quality and coverage enforcement**

Review the acceptance mapping and representative assertions rather than relying solely on test counts.

Confirm that:

- All baseline acceptance scenarios have meaningful coverage.
- WKS-024 exercises actual registration and cancellation endpoints.
- Concurrency tests exercise overlapping requests and real persistence.
- Time-boundary tests use a controlled clock.
- Tests do not depend on execution order, shared mutable state, or wall-clock timing.
- The coverage command uses fresh unit-only reports.
- Domain and Application each exceed 90% line coverage independently.
- Gate failures remain detectable and propagate nonzero exit codes.

Add tests only for demonstrated gaps or defects. Avoid duplicating already adequate coverage.

**Rehearse the documented demo**

Start the application from the repository root using the README command.

Verify Swagger UI and its OpenAPI document, then perform the documented workflow:

1. List seeded workshops and participants.
2. Inspect an upcoming workshop’s availability.
3. Register an eligible participant.
4. Confirm the seat count decreases.
5. Repeat the request and confirm the duplicate conflict.
6. Attempt registration in a full upcoming workshop and confirm the full-workshop conflict.
7. Cancel the successful registration.
8. Confirm the seat is restored.
9. Restart the host and confirm the seed baseline returns.

Verify all nine baseline endpoints, including workshop and participant creation with working `Location` readback.

Use available HTTP tools for request verification. Inspect the interactive Swagger UI when the environment supports browser access. Clearly distinguish HTTP/document checks from actual UI interaction.

Stop any application processes started for verification.

**Check readiness for later feature demos**

Confirm that the existing structure can accommodate feedback and waiting-list features through the established layers and extension points.

Review EF configuration discovery, dependency registration, test fixtures, and existing workshop/participant/registration contracts.

Do not implement future features, introduce speculative interfaces, or create placeholder controllers. Record concrete coordination points that later work will need to address.

**Reconcile documentation**

Update documentation to describe the application that actually exists.

- Verify setup prerequisites and SDK selection.
- Confirm run, build, format, test, and coverage commands.
- Confirm Swagger paths and environment requirements.
- Document actual seed identities, relative dates, and reset behavior.
- Replace obsolete “planned” or “pending” statements only where implementation has been verified.
- Keep architecture decisions consistent with code.
- Preserve feature acceptance IDs and business intent.
- Keep future feedback and waiting-list functionality explicitly outside the baseline.

Keep `AGENTS.md` concise and workflow-neutral. Do not add task history, lengthy review findings, or role-specific procedures to it.

**Run final verification**

After the final code or configuration changes, run the documented:

- Restore and build.
- Formatting verification.
- Unit and integration test suites.
- Coverage-gate verification checks.
- Real unit-coverage enforcement.

Also verify application startup, Swagger/OpenAPI availability, and restart/reset behavior.

Check that generated outputs and local settings are ignored while source, documentation, shared configuration, and required scripts remain trackable.

If failures occur, fix them and rerun the affected checks. Perform final combined verification after the last relevant change.

**Boundaries**

Do not add authentication, caching, external infrastructure, notifications, feedback, waiting-list functionality, CI workflows, deployment configuration, or advanced Codex workflow files.

Do not commit, push, tag, publish, or deploy. Leave the repository ready for the user to create the presentation baseline checkpoint.

**Completion report**

Provide:

1. A clear readiness verdict, qualified by any remaining blockers.
2. Findings fixed and any unresolved issues, with file references.
3. Final commands and observed results.
4. Unit-coverage counts and percentages for Domain and Application.
5. Demo rehearsal and restart/reset results.
6. Documentation updated.
7. Concrete extension points relevant to later demos.
8. Any manual verification still required.

Only claim presentation readiness when the required checks pass. Do not present assumptions or unperformed checks as verified results.