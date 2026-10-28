
---

# 04-security-privacy-and-operations.md

```md
# Security, Privacy, and Operations

## Security Goal

The project should be secure by default.

Security should be considered during specification, planning, implementation, and review.

The application may handle project information, user data, assignments, deadlines, comments, and potentially sensitive internal team data. This information must be protected.

## Authentication Principles

Authentication must be implemented using secure, standard mechanisms.

The application should avoid custom authentication unless explicitly justified.

Authentication behavior should be clear for:

- login
- logout
- session expiration
- password reset, if applicable
- account creation, if applicable

Authentication state should not be trusted only on the client side.

## Authorization Principles

Authorization must be enforced server-side.

The system should clearly define who can:

- view a project
- create a project
- edit a project
- archive or delete a project
- create tasks
- edit tasks
- assign users
- change task status
- view reports or dashboards

Permissions should be explicit, tested, and not hidden in UI-only checks.

## Data Protection Principles

Sensitive data must not be exposed unnecessarily.

The application should follow these rules:

- store only necessary data
- avoid logging sensitive data
- avoid exposing internal identifiers unnecessarily
- validate inputs before persistence
- protect against unauthorized data access
- use secure transport in production environments

## Input Validation Principles

All user input must be validated.

Validation should happen at appropriate layers:

- UI validation for immediate feedback
- server-side validation for enforcement
- database constraints for integrity where appropriate

Validation errors should be clear and actionable.

## OWASP-Aware Development

The project should consider common web application risks, including:

- broken access control
- injection
- authentication/session issues
- insecure design
- security misconfiguration
- vulnerable dependencies
- logging and monitoring gaps

Implementation plans should explicitly address relevant security risks for each feature.

## Privacy Principles

The project should collect the minimum data necessary.

For user and project data:

- do not collect unused personal information
- do not expose private project data across tenants or teams
- document retention or deletion behavior where relevant
- avoid using production user data in local development or tests

## Accessibility Principles

User-facing features should follow basic accessibility expectations.

The application should support:

- keyboard navigation for main workflows
- visible focus states
- semantic HTML where applicable
- readable contrast
- labels for form fields
- clear error messages
- meaningful button and link text

Accessibility should not be postponed until the end of the project.

## Logging Principles

The system should produce useful logs for debugging and operations.

Logs should include:

- meaningful error context
- request or operation identifiers where appropriate
- security-relevant events
- failed authorization attempts where appropriate

Logs must not include:

- passwords
- tokens
- secrets
- unnecessary personal data
- full sensitive payloads

## Monitoring and Observability Principles

Production-like environments should make failures diagnosable.

The project should support:

- error tracking
- structured logs
- basic health checks
- visibility into failed requests
- visibility into critical background operations, if any

## Performance Principles

Performance should be considered where it affects user experience.

For a project management app, key performance areas include:

- loading project lists
- loading task lists
- filtering and searching tasks
- saving task updates
- switching between projects

Avoid premature optimization, but do not ignore obvious performance risks.

## Operational Principles

The project should be easy to run, test, and deploy.

The repository should include clear instructions for:

- local setup
- environment variables
- database setup
- running tests
- building the app
- deploying or packaging the app

Operational complexity should remain proportional to the project size.

## Constitution Guidance

The constitution should require secure-by-default implementation, server-side authorization, responsible data handling, accessibility basics, logging discipline, and operational clarity.

Security and privacy should be treated as core engineering constraints, not optional later improvements.