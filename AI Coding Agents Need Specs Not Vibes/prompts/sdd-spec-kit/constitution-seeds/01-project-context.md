# Project Context

## Project Name

ProjectFlow

## Project Type

Greenfield web application for project and task management.

## Product Context

ProjectFlow is a lightweight project management application for small teams that need to organize projects, tasks, responsibilities, statuses, and deadlines.

The application should help users answer:

- What projects are currently active?
- What tasks belong to each project?
- Who is responsible for each task?
- What is blocked, overdue, or completed?
- What needs attention next?

## Target Users

Primary users:

- Project managers
- Scrum masters
- Product owners
- Business analysts
- Team leads
- Delivery managers

Secondary users:

- Developers
- QA engineers
- Designers
- Stakeholders who need visibility into project progress

## Product Principles

The application should prioritize:

1. Clarity over complexity
2. Fast task entry and updates
3. Easy visibility into project progress
4. Predictable workflows
5. Low cognitive load for non-technical users

## Product Non-Goals

The project should not try to become a full enterprise project management suite.

The following are out of scope for the initial project direction:

- Advanced portfolio management
- Financial planning
- Resource capacity planning
- Complex dependency graphs
- Enterprise reporting
- AI-based task estimation
- Native mobile applications
- Deep integrations with Jira, Azure DevOps, or Microsoft Project

These may be considered later, but they should not influence the initial architecture unless explicitly specified in a feature spec.

## Expected User Experience

The application should feel simple, structured, and predictable.

Users should be able to:

- Create and manage projects
- Create and manage tasks
- Assign ownership
- Track status
- Filter and search information
- Understand what requires attention

The interface should avoid unnecessary configuration and should guide users toward standard workflows.

## Long-Lived Product Assumptions

- Most users are business or delivery users, not technical administrators.
- The application should support multiple projects.
- Tasks belong to projects.
- Tasks may have status, priority, assignee, due date, and description.
- The application should be usable with minimal onboarding.
- Data integrity is more important than visual experimentation.
- User-visible behavior must be covered by acceptance criteria in feature specs.

## Constitution Guidance

Use this file only as high-level product context.

Do not put detailed feature requirements into the constitution.

The constitution should define stable rules for how the project is built, tested, secured, and maintained.