Audit the baseline acceptance scenarios, address meaningful test gaps, and implement verified unit-coverage enforcement for the Workshop Booking API.

**Goal**

Establish evidence that the baseline behavior satisfies its specifications and that unit tests alone achieve strictly greater than 90% line coverage separately for Domain and Application.

Coverage collection is not enforcement. Deliver one documented command that runs unit tests, collects fresh coverage, validates both assemblies, and fails when the requirement is not met.

**Inspect before changing**

Read the root `AGENTS.md`, then review:

- `docs/testing.md`
- All three specifications under `docs/features/`.
- `docs/api-conventions.md`
- Relevant architecture decisions.
- Existing coverage configuration and scripts.
- `docs/coding-guidelines.md` when reviewing changes.

Inspect the actual test runner, collector versions, test projects, existing reports, and Git status. Preserve existing work and use the tooling approach already selected.

**Audit acceptance coverage**

Map the stable acceptance IDs to existing automated tests:

- WKS-001 through WKS-027.
- PAR-001 through PAR-026.
- REG-001 through REG-033.

For each scenario, identify whether it is:

- Covered by meaningful assertions.
- Partially covered.
- Missing.
- Blocked by an implementation defect or unresolved specification issue.

Keep the mapping concise in `docs/testing.md` or a focused linked document if the full mapping would overwhelm it. Reference real test names or locations. Do not create an inventory that merely claims coverage without checking assertions.

A single parameterized test may cover multiple inputs, and a scenario may require several tests. Do not force one test per acceptance ID.

Also inspect shared HTTP behavior, persistence integrity, seeding, isolation, and concurrency verification required by the supporting documents.

**Address meaningful gaps**

Add or improve tests for missing behavior and important branches, prioritizing:

- Validation boundaries and failure precedence.
- Exact time boundaries.
- Email normalization and topic matching.
- Persisted effects and unchanged state after rejection.
- Capacity protection and competing mutations.
- Correct HTTP status, problem codes, headers, and response shapes.
- Seed lifecycle and fixture isolation.

Use real SQLite and the existing HTTP host for endpoint integration tests. Keep unit tests independent of the database and ASP.NET Core host.

Fix implementation defects exposed by valid tests. Preserve existing business rules and API contracts. Do not change specifications merely to accommodate incorrect behavior.

Avoid redundant tests that mirror implementation details, brittle message assertions, sleeps, and concurrency tests that remove contention.

**Configure unit-only coverage collection**

Use the established compatible collector and runner.

- Collect coverage from the unit-test project only.
- Explicitly identify `WorkshopBooking.Domain` and `WorkshopBooking.Application`.
- Keep integration-test execution and results separate.
- Write reports under an ignored artifact directory.
- Ensure each gate invocation consumes only reports produced by that invocation.
- Remove stale gate-owned output or use an isolated run directory.
- Do not delete unrelated developer artifacts.

Account for multiple report files if the selected tooling produces them. Do not accidentally choose an arbitrary stale report or count the same executable lines twice.

**Implement the enforcing command**

Create a small repository-owned script or tool using the runtime selected during review. Document any additional prerequisite and an exact invocation from the repository root.

The command must:

1. Prepare fresh coverage output.
2. Run the unit tests with coverage.
3. Preserve test-run failures as command failures.
4. Parse the generated report.
5. Require both expected assemblies to be present.
6. Reject missing, empty, malformed, or unusable coverage data.
7. Reject assemblies with zero measurable lines.
8. Calculate coverage separately for each assembly from covered and valid line counts.
9. Fail if either assembly is at or below 90%.
10. Print readable per-assembly counts and percentages, then return the appropriate exit code.

Use raw counts rather than rounded report percentages to decide compliance. Exactly 90% fails; a value above 90% must not fail merely because its display rounds to 90%.

A combined solution average does not satisfy the requirement. Coverage from integration tests must never improve the unit-only result.

Keep the gate simple and understandable. Do not introduce a large coverage framework or an additional application layer.

**Verify the gate itself**

Add focused verification of threshold and report-validation logic using small synthetic report inputs that match the selected report format.

Cover at least:

- Below 90%.
- Exactly 90%.
- Strictly above 90%.
- One assembly passing while the other fails.
- A passing combined average with a failing individual assembly.
- Missing expected assembly.
- Zero measurable lines.
- Missing, empty, or malformed report.
- Stale output not being reused.
- Unit-test failure propagating as a nonzero command result.

Make these checks independent of real business coverage where practical. Do not temporarily weaken production coverage requirements or alter business code to manufacture gate failures.

**Review exclusions and uncovered code**

Allow only explicit, justified generated-code exclusions.

Do not exclude ordinary entities, application services, validation, error outcomes, or difficult branches. Do not broaden exclusions, delete assertions, or add tests that simply execute lines to reach the threshold.

Inspect remaining uncovered branches for meaningful missed behavior. Branch coverage is informative but has no separate numeric gate in the baseline.

Refactor unreachable or unnecessary code only when the change improves the implementation and preserves its contract.

**Update documentation**

Update `docs/testing.md` with:

- The exact verified coverage-gate command.
- Required runtime/tooling.
- Report location and unit-only scope.
- Threshold semantics and failure conditions.
- Justified exclusions.
- How to inspect coverage output.
- Acceptance audit results and remaining gaps.

Update README and `AGENTS.md` command references where necessary. Remove pending-enforcement statements only after the command has been implemented and verified.

Keep commands consistent across documents.

**Run final verification for this stage**

Run:

- Formatting verification.
- Build with the configured analyzer and warning requirements.
- Unit tests.
- Integration tests.
- The coverage-gate verification checks.
- The real unit-coverage gate.

Resolve failures caused by changes in this task, then rerun affected checks. If a requirement cannot be met, report the concrete blocker and actual measurements rather than claiming completion.

**Completion report**

Provide:

1. Acceptance audit summary and gaps resolved.
2. Tests and implementation fixes added.
3. Exact coverage-gate command and verified failure behavior.
4. Covered/valid line counts and percentages for each required assembly.
5. Commands actually executed and their results.
6. Exclusions, documentation changes, and unresolved issues.

Do not commit, push, or tag changes. Stop after acceptance auditing and coverage enforcement; the final combined review and presentation rehearsal are next.