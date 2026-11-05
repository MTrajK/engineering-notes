# Baseline review and rehearsal

Verified on 2026-11-05 with .NET SDK 10.0.302 and PowerShell 7.6.9. The working tree was clean at the start of this review. All changes in this review are limited to documentation/configuration comments, naming the baseline-wide OpenAPI filter accurately, formatting its registration, and testing both factory disposal paths. No business behavior, acceptance IDs, package versions or coverage exclusions changed.

## Readiness

The baseline is ready for the user to create a commit/tag. No unresolved baseline blockers were found. Nothing was committed, pushed, tagged, deployed or published. Feedback and waiting-list functionality remain outside this baseline.

## Review findings and changes

- Renamed WorkshopOpenApiFilter to BaselineOpenApiFilter because it documents all three resources. Runtime schemas/routes and DTO contracts are unchanged.
- Added a synchronous-disposal case to PersistenceTests.HostOwnsConnectionAndScopesOwnContexts. Both existing disposal paths close the owner connection; no lifecycle implementation fix was required. Existing HttpInfrastructureTests continue proving test-only controllers are absent from the normal host.
- Reconciled obsolete scaffold notes, the missing PowerShell prerequisite, duplicated feature summaries, SDK/package status and current persistence/test ownership documentation. AGENTS.md remains workflow-neutral.
- Inspected project references, controllers, Application contracts/services, private domain state, SQL projections, time converters, unique-constraint mapping, connection/gate lifetimes, seeding, acceptance evidence and coverage scripts. Domain/Application remain independent of EF and HTTP. All request database operations use the shared I/O gate, while mutation leases cover authoritative checks and writes. Single-row registration insert/delete remains the serialization point. Seed insertion uses its own startup transaction.

## Final verification

All commands completed successfully after the code/configuration changes:

| Command | Observed result |
| --- | --- |
| dotnet restore | Packages up to date |
| dotnet build --no-restore | Zero warnings/errors |
| dotnet format --verify-no-changes | Passed |
| dotnet test tests/WorkshopBooking.UnitTests --no-build | 34 passed |
| dotnet test tests/WorkshopBooking.IntegrationTests --no-build | 135 passed |
| pwsh -NoProfile -File scripts/Test-CoverageGate.ps1 | 19 synthetic checks passed; deliberately failing cases were rejected |
| pwsh -NoProfile -File scripts/Test-UnitCoverage.ps1 | Domain 91/91 lines (100%); Application 200/202 (99.0099%) |
| git diff --check | Passed |

Coverage uses fresh unit-only reports; integration results cannot improve it. The two uncovered Application lines guard invalid internal Outcome construction, unreachable through public factories. No exclusions were added. Review [the 86-scenario audit](acceptance-audit.md) and [testing](testing.md) for representative assertions and threshold/failure semantics.

## Live rehearsal

Started from the repository root with dotnet run --project src/WorkshopBooking.Api in Development. Swagger UI and /swagger/v1/swagger.json returned 200 with nine baseline operations. The in-app browser displayed Swagger and executed GET /api/workshops through Try it out, showing 200 and the three seeded scenarios. Native Chrome control was unavailable; this browser check used the in-app browser.

The complete workflow was also asserted through HTTP: list seed workshops/participants; detail availability 3; book Casey into Practical API Design (201 without Location); list the new registration; observe 2 seats; repeat for registration_duplicate (409); attempt the full workshop for workshop_full (409); cancel (empty 204); verify detail and list return 3 seats. Created a workshop and participant and followed each 201 Location to matching detail data. A test-only failure route returned 404 on the normal host.

For reset proof, deleted one seed registration and created additional workshop/participant records before stopping. A fresh process restored exactly three workshops, three participants, the deleted stable seed registration and three free seats; seed dates advanced relative to the new startup instant. The rehearsal-created records returned 404. Every host started for verification was stopped; port 5080 had no listener afterward. Temporary HTTP evidence is ignored under artifacts/rehearsal.

PowerShell's HTTP client returned application/problem+json content as UTF-8 bytes in this environment. The rehearsal decoded those responses before asserting codes; no API change was needed.

## Later demo coordination points

- Add local entities/rules in Domain, focused contracts/orchestration in Application, EF configurations and implementations in Persistence, and HTTP DTOs/controllers in API.
- EF configuration discovery already scans the Persistence assembly. Register implemented services in PersistenceServices and update BaselineOpenApiFilter/DTO schemas for new operations.
- All new writes must use the existing mutation lease and separate I/O guard. Define explicit transaction ownership for multi-row work such as a future promotion flow; preserve failure precedence and captured-time rules.
- Revisit ConstraintFailures when adding unique indexes: its current extended-UNIQUE classification assumes one non-primary unique index per mapped participant/registration entity.
- Coordinate context/service-registration/seed changes and use isolated factories with controlled clocks. Add tests and update feature specifications before new behavior. No speculative contracts or future-feature placeholders were introduced.

No additional manual correctness checks are required for the baseline. Rehearse on the actual presentation machine and choose a future workshop date if using an old Swagger document. The user remains responsible for choosing and creating the baseline commit/tag.
