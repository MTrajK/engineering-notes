/speckit.constitution

Create a concise but enforceable project constitution for this greenfield project.

Use the following seed principles.

# Constitution Seed

## 1. Product-Level Principles

Durable user-facing principles only:
- Clear UX
- No silent failures
- Accessibility baseline
- Destructive actions require confirmation
- User-visible behavior requires acceptance criteria

## 2. Architectural Principles

- Simplicity first
- Explicit boundaries
- Avoid speculative abstraction
- Keep business logic testable
- Prefer modularity without over-engineering

## 3. Technical Constraints

- Local-first developer setup
- Dependency discipline
- Configuration rules
- Runtime/platform constraints, only if fixed
- No hardcoded secrets

## 4. Code Quality Standards

- Readability over cleverness
- Clear naming
- Small cohesive modules
- Consistent formatting/linting
- Documentation for non-obvious decisions

## 5. Testing Standards

- Acceptance criteria required
- Automated tests for critical behavior
- Regression tests for bug fixes
- Integration tests for external boundaries
- Tests verify behavior, not implementation details

## 6. Security and Privacy

- Secure defaults
- OWASP Top 10 considered where applicable
- Input validation
- Output encoding where relevant
- Least privilege
- Data minimization
- No sensitive data in logs

## 7. Logging and Observability

- Log meaningful operational events
- Avoid noisy logs
- Avoid secrets and unnecessary personal data
- Errors must include enough diagnostic context
- Services should define health checks where relevant

## 8. Delivery and Review Discipline

- Build incrementally
- Tasks must be small and testable
- Plans must document constitution violations
- Large scope must be split into phases