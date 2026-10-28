# Quality and Testing Standards

## Quality Goal

The project should produce reliable, maintainable software that can be changed safely.

Quality should be treated as part of implementation, not as a cleanup activity after implementation.

## Code Quality Principles

### 1. Readability

Code should be easy to read and understand.

Prefer:

- clear names
- small functions
- explicit behavior
- simple control flow
- minimal cleverness

Avoid code that is technically compact but difficult to understand.

### 2. Maintainability

Code should be organized so future changes are localized and predictable.

Avoid:

- duplicated business rules
- large files with mixed responsibilities
- hidden side effects
- fragile coupling between unrelated modules
- unexplained abstractions

### 3. Consistency

The codebase should follow consistent patterns.

This applies to:

- naming
- folder structure
- error handling
- validation
- API responses
- test structure
- logging

When a pattern exists, new code should follow it unless there is a clear reason to change it.

### 4. Type Safety

Where TypeScript or typed languages are used, types should be meaningful and strict.

Avoid bypassing type safety with:

- `any`
- unsafe casts
- loosely typed objects
- undocumented implicit shapes

Types should describe domain concepts clearly.

## Testing Principles

### 1. User-Visible Behavior Must Be Tested

Any behavior visible to users should have acceptance criteria and automated test coverage where practical.

Examples:

- creating a task
- changing task status
- assigning a user
- filtering tasks
- showing validation messages
- handling empty states
- enforcing permissions

### 2. Business Rules Must Be Tested

Business rules should not rely only on manual testing.

Examples:

- a task cannot be completed if required fields are missing
- a user cannot edit a project without permission
- overdue tasks are calculated consistently
- deleted or archived projects behave predictably

### 3. Regression Risk Determines Test Depth

Use the appropriate level of testing for the risk.

Possible levels:

- unit tests for isolated business rules
- integration tests for data and API behavior
- end-to-end tests for critical user flows
- manual exploratory testing for low-risk visual details

### 4. Tests Should Be Meaningful

Tests should verify behavior, not implementation details.

Avoid tests that only confirm that mocks were called unless that is the actual behavior being guaranteed.

Tests should be readable enough to explain what the system is expected to do.

## Acceptance Criteria Standards

Each feature spec should include clear acceptance criteria.

Acceptance criteria should be:

- observable
- testable
- unambiguous
- written from user or system behavior perspective

Preferred format:

```text
Given [context]
When [action]
Then [expected result]