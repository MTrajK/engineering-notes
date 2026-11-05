Scaffold the Workshop Booking API solution using the repository’s existing specifications and the technical decisions recorded during the previous review.

**Goal**

Create a minimal, buildable solution with the correct project boundaries and development tooling. Establish the structure for later implementation without implementing business features or shared application infrastructure yet.

**Inspect before changing**

Read the root `AGENTS.md`, then review:

- `README.md`
- `docs/architecture.md`
- `docs/testing.md`
- `.editorconfig`
- `Directory.Build.props`
- `.gitignore`

Consult API conventions and feature specifications only as needed for scaffolding decisions. Inspect Git status, existing files, installed SDKs, and decisions recorded by the previous task. Preserve existing work and make the task safe to resume if scaffolding is partially present.

If an expected technical decision is missing, resolve routine setup choices within the documented requirements. Report genuine blockers rather than silently changing scope.

**Create the solution structure**

Create one solution file at the repository root, using the format selected during review. Do not add another enclosing application directory.

Create these projects:

| Path | Project type |
| --- | --- |
| `src/WorkshopBooking.Domain` | Class library |
| `src/WorkshopBooking.Application` | Class library |
| `src/WorkshopBooking.Persistence` | Class library |
| `src/WorkshopBooking.Api` | ASP.NET Core controller-based API host |
| `tests/WorkshopBooking.UnitTests` | xUnit test project |
| `tests/WorkshopBooking.IntegrationTests` | xUnit test project |

Target `net10.0` and add all six projects to the solution.

Configure direct application project references as follows:

- Domain: no other application project.
- Application: Domain.
- Persistence: Application and Domain.
- API: Application and Persistence.
- UnitTests: Domain and Application.
- IntegrationTests: API and Persistence.

Keep Domain and Application free of ASP.NET Core and EF Core dependencies. Do not create additional projects.

**Configure SDK and packages**

- Create `global.json` using a compatible installed stable .NET 10 SDK and an intentional roll-forward policy. Do not select preview versions or silently change the target framework.
- Follow the package/version-management approach selected during review.
- Configure the agreed xUnit runner and compatible test SDK packages.
- Add the packages needed for the planned EF Core SQLite persistence, HTTP integration testing, Swagger/OpenAPI approach, and unit coverage collection in their owning projects.
- Use one coherent OpenAPI generation approach; avoid installing redundant generators.
- Do not install packages for speculative future features.
- If required SDKs or package sources are unavailable, report the concrete blocker. Do not claim verification succeeded.

Adding package references does not authorize implementing persistence, Swagger customization, or coverage enforcement in this task.

**Apply existing configuration**

Keep `.editorconfig`, `Directory.Build.props`, `.gitignore`, `AGENTS.md`, and `docs/` at the repository root.

Ensure projects inherit the shared build configuration. Avoid redundant project-level properties that override it.

Preserve nullable analysis, recommended analyzers, code-style enforcement, and warnings as errors. Fix scaffolded code to satisfy configured rules. Change configuration only to correct a demonstrated incompatibility, explaining the reason; do not weaken rules to obtain a successful build.

**Keep the scaffold minimal**

- Remove generated `Class1`, WeatherForecast, sample endpoints, and placeholder tests.
- Create a minimal host with controller registration and routing, ready for later endpoints.
- Make the API entry point accessible to the future integration-test factory.
- Retain secret-free development settings and a practical local launch profile.
- Do not add fake business services, empty controllers, `NotImplementedException` stubs, artificial production endpoints, or assertions that merely prove the test runner starts.
- Test projects may initially contain no tests. Explicitly report zero discovered tests rather than presenting that as behavioral verification.

Do not implement domain entities, application outcomes, HTTP error customization, database initialization, fixtures, seeding, feature behavior, or the coverage gate. Those belong to subsequent tasks.

**Verify the scaffold**

Run from the repository root:

```bash
dotnet restore
dotnet build --no-restore
dotnet format --verify-no-changes
dotnet test tests/WorkshopBooking.UnitTests
dotnet test tests/WorkshopBooking.IntegrationTests
dotnet test
```

Use the runner-compatible equivalent if the selected tooling requires different invocation, and document the verified commands.

Start the API and verify that the host listens successfully, then stop it cleanly. No business endpoints or working Swagger UI are required yet.

Inspect project references and package ownership. Do not introduce a separate architecture-testing framework for this check.

Coverage collection/enforcement and endpoint acceptance scenarios are not completion gates for this partial scaffolding task. Do not add dummy code or tests to manufacture coverage.

**Update documentation**

Update README setup instructions and bootstrap status to distinguish what now works from what remains planned. Record the selected SDK, package/test-runner setup, and verified commands where relevant.

Do not claim Swagger, persistence, tests, or coverage enforcement are implemented merely because their packages are installed. Preserve business specifications.

**Completion report**

Summarize:

1. Solution structure and project references.
2. Selected SDK, runner, and package versions.
3. Configuration changes, if any, and their reasons.
4. Commands actually executed, their outcomes, and discovered test counts.
5. Host-start verification.
6. Documentation updated and unresolved blockers.

Do not commit, push, or tag changes. Stop after scaffolding; do not proceed to shared application outcomes or API conventions implementation.