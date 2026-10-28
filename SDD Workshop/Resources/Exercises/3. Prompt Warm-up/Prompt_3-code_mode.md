Fix the failing tests by removing their dependency on shared seed data and making each failing test create and use isolated test-owned data.

Failing E2E tests:
- tests\e2e\app.spec.ts:44:1 › switches dashboard month and year and updates summary values
- tests\e2e\app.spec.ts:157:1 › edits an expense and updates the dashboard totals

Failing unit test:
- src/app/AppRouter.test.tsx > App shell navigation > renders the persistent navbar and switches routes

Problem:
These tests currently rely on seed data. That is fragile and incorrect. Tests should not depend on global/shared seed records because changes to seed data can break unrelated tests.

Requirements:
Make these tests deterministic, isolated, and self-contained by creating only the data they need inside the test or test setup, using unique values per test, and cleaning up test-created data where appropriate.

Validation:
- Run build.
- Run lint.
- Run all unit tests.
- Run all Playwright E2E tests.