# Mini CRM Product Specification

## Product Overview

Mini CRM is a lightweight lead tracking application for solo consultants, freelancers, and small startup teams.

The application helps users track sales opportunities through a simple Kanban workflow and use AI to suggest the next follow-up action for each lead.

This is not a full CRM. It is intentionally small, focused, and easy to understand.

## Target User

The primary user is a solo consultant or small team member who needs to track early sales conversations without using a heavyweight CRM.

The user wants to know:
- who they are talking to
- what stage each lead is in
- what notes have been captured
- what they should do next

## Core Workflow

1. User opens the CRM dashboard.
2. User sees leads grouped by sales stage.
3. User clicks "New lead".
4. User enters lead details.
5. The lead appears in the "New" column.
6. User can move the lead between stages.
7. User can open a lead detail panel.
8. User can edit notes.
9. User can request an AI-generated follow-up suggestion.
10. The system generates and displays the suggested next action.

## Sales Stages

The product uses five fixed sales stages:

1. New
2. Contacted
3. Proposal
4. Won
5. Lost

Stages are not user-configurable in this version.

## Lead Fields

Each lead has:

- id
- name
- company
- email
- status
- notes
- aiSuggestedNextAction
- createdAt
- updatedAt

## Validation Rules

When creating a lead:

- name is required
- company is required
- email is optional
- notes are optional
- status defaults to New

Email validation should be lightweight. If an email is provided, it should contain "@".

## AI Follow-up Suggestion

The AI suggestion should be generated from:

- lead name
- company
- current status
- notes

The output should be short and practical.

Example output:

"Send a short follow-up email asking whether they reviewed the proposal and offer two time slots for a call next week."

The suggestion should be saved to the lead after generation.

## Non-Goals

This version does not include:

- authentication
- multiple users
- payments
- real CRM integrations
- email sending
- calendar integration
- drag and drop
- advanced permissions
- activity timeline
- file uploads

## Success Criteria

The app is successful if a user can:

- create a lead
- see the lead in the correct Kanban column
- update the lead status
- edit notes
- generate and save an AI follow-up suggestion
- understand the interface without explanation