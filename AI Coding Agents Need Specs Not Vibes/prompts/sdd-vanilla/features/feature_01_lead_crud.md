# Feature 01: Lead CRUD

## Goal

Allow users to create, view, edit, and persist leads.

## Implementation Requirements

### Lead Fields

Each lead must include:

- id
- name
- company
- email
- status
- notes
- aiSuggestedNextAction
- createdAt
- updatedAt

### Create Lead

Users can create a lead with:

- name
- company
- email
- notes

Rules:

- name is required
- company is required
- email is optional
- notes are optional
- status defaults to "New"

### View Leads

The app must fetch leads from the API and display them on the main dashboard.

### Edit Lead

Users can edit:

- name
- company
- email
- status
- notes

### Persistence

All leads must be stored in SQLite through Prisma.

## Acceptance Criteria

- User can create a lead.
- Created lead is saved in SQLite.
- Created lead appears on the dashboard.
- User can edit lead details.
- Edited lead persists after page refresh.
- Name and company are required.
- Invalid email shows an error.
- Lead timestamps are created and updated correctly.