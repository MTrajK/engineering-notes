# Feature 02: Kanban Pipeline

## Goal

Display leads in a simple Kanban sales pipeline grouped by status.

## Implementation Requirements

### Pipeline Stages

The board must include five fixed columns:

- New
- Contacted
- Proposal
- Won
- Lost

### Lead Grouping

Each lead must appear in the column matching its status.

### Lead Card

Each card must display:

- lead name
- company
- email if available
- AI follow-up suggestion if available

### Status Updates

Users can change a lead's status from the lead detail modal.

After status update, the lead must move to the correct column.

### Empty States

Each empty column should show a simple empty-state message.

## Acceptance Criteria

- Dashboard displays five Kanban columns.
- Leads appear under the correct status.
- Empty columns remain visually clear.
- User can update a lead status.
- Lead moves to the correct column immediately after status change.
- Invalid statuses are rejected.
- Multiple leads can appear in the same column.