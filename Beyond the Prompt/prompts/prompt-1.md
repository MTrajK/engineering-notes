We are preparing to implement the Workshop Booking API. This task is repository review and implementation planning only.

Work directly in the current repository using ordinary Codex capabilities. Do not invoke skills, spawn subagents, create custom-agent definitions, configure hooks, or introduce a multi-agent workflow.

**Goal**

Review the existing specifications and configuration, resolve the remaining technical decisions, and leave a concrete foundation plan for the next implementation task. Do not scaffold the solution or implement application code yet.

**Review the repository**

Read the root `AGENTS.md`, then inspect:

- `README.md`
- `docs/architecture.md`
- `docs/api-conventions.md`
- `docs/testing.md`
- `docs/coding-guidelines.md`
- `docs/features/workshops.md`
- `docs/features/participants.md`
- `docs/features/registrations.md`
- `.editorconfig`
- `Directory.Build.props`
- `.gitignore`

Check the actual repository contents and Git status before making changes. Preserve existing work. If a listed file is missing, report it rather than inventing its contents.

The repository root is the solution root. The intended structure has four application projects under `src/` and two test projects under `tests/`. Keep the existing documentation and shared configuration at the root.

**Inspect the development environment**

Determine the installed .NET SDKs, operating system, and available shell. Identify a compatible stable .NET 10 SDK to pin during scaffolding.

Do not install software or create `global.json` yet. If a prerequisite is unavailable, state the exact gap and its impact. Verify current package or SDK behavior against official documentation when necessary; distinguish verified facts from recommendations.

**Check consistency and feasibility**

Review the documents together for contradictions, missing contracts, and requirements that affect setup. Pay particular attention to:

- Layer dependencies and ownership of application versus HTTP contracts.
- Validation and business-failure precedence.
- UTC serialization, explicit-offset input validation, and precise time boundaries.
- SQLite ordering and case-insensitive topic matching, including non-ASCII cases.
- Email normalization and database uniqueness.
- Connection ownership, scoped contexts, concurrent requests, and disposal.
- Tests using controlled time and isolated databases.
- Unit-only coverage strictly greater than 90% for Domain and Application separately.
- Formatter/analyzer configuration and which checks actually run during builds.

Keep the existing business scope. Do not simplify acceptance criteria merely to make implementation easier.

**Resolve the remaining technical decisions**

Choose a straightforward approach for each item below. Give the decision, a brief rationale, and any important limitation.

1. **Solution and tooling:** solution format, SDK selection, test-runner approach, and required package families. Resolve compatible package versions where the environment and package sources allow; otherwise mark them for verification during scaffolding.

2. **Application outcomes:** the conceptual shape of successful results, field-validation failures, missing resources, and conflicts. Keep these independent of ASP.NET Core and avoid an elaborate result framework.

3. **Persistence contracts:** the minimum operation boundaries needed by the three features. Explain how registration can coordinate business decisions with an atomic persistence operation without exposing EF Core or SQL to Application. Do not design every method signature in advance.

4. **SQLite lifecycle and concurrency:** choose one concrete connection and synchronization strategy. Explain how it protects registration capacity, duplicate creation, cancellation, and shared-connection use while preserving scoped contexts and failure precedence. Include when the authoritative clock instant is captured.

5. **Test infrastructure:** database isolation, controlled time, startup initialization/seeding replacement, and deterministic concurrency-test coordination.

6. **Coverage tooling:** collection, report format, gate implementation runtime, and handling of missing assemblies, stale reports, zero measurable lines, and the exact 90% boundary. Prefer tooling compatible with the available environment and disclose additional prerequisites.

7. **HTTP infrastructure:** shared problem mapping, automatic validation, transport errors, timestamp handling, and OpenAPI/Swagger setup.

Make routine technical choices autonomously within the documented requirements. Ask only when an unresolved issue would materially change business behavior, scope, or an explicit requirement.

**Record the decisions**

Update the relevant existing documentation with the selected technical decisions and their rationale. Keep edits focused and preserve unrelated content.

Do not turn proposals into claims of implemented or tested behavior. Update feature specifications only if clarification is necessary, preserving acceptance IDs and business intent. Do not create extra guideline files or nested `AGENTS.md` files.

**Boundaries**

- Do not create solution/project files, application code, tests, or coverage scripts.
- Do not add packages or change analyzer strictness without an identified compatibility reason.
- Do not add authentication, caching, external services, feedback, or waiting-list functionality.
- Do not commit, push, or tag changes.
- Do not continue into scaffolding after finishing this task.

**Completion report**

Provide:

1. Repository and environment findings.
2. A decision table with rationale and verification status.
3. Contradictions or gaps found and how each was resolved.
4. Documentation changed.
5. The precise scope and completion checks for the next task: scaffolding the solution and development tooling.
6. Any remaining blockers.

Report only checks actually performed. No build, runtime, or coverage success should be claimed before the application exists.