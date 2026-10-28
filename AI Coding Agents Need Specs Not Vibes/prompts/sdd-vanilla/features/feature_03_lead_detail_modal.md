# Feature 03: Lead Detail Modal

## Goal

Allow users to open a lead, review its details, edit notes, update status, and save changes.

## Implementation Requirements

### Open Lead

Clicking a lead card opens a modal or side panel.

### Displayed Fields

The detail view must show:

- name
- company
- email
- status
- notes
- AI follow-up suggestion
- createdAt
- updatedAt

### Editable Fields

Users can edit:

- name
- company
- email
- status
- notes

### Save Behavior

When the user saves changes:

- call the lead update API
- validate inputs
- update the UI with the returned lead
- close the modal or show success state

### Cancel Behavior

Users can close the modal without saving.

## Acceptance Criteria

- User can open a lead detail modal.
- Modal displays the selected lead’s information.
- User can edit lead fields.
- User can save changes.
- Saved changes persist in SQLite.
- User can close modal without saving.
- Validation errors are shown clearly.
- UI updates after successful save.