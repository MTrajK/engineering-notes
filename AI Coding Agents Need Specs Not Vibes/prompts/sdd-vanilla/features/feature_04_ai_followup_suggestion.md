# Feature 04: AI Follow-up Suggestion

## Goal

Generate and save a concise AI-powered follow-up suggestion for a lead.

## Implementation Requirements

### Generate Suggestion

Users can click a "Suggest follow-up" button from the lead detail modal.

The system must send the following lead context to the AI layer:

- name
- company
- status
- notes

### AI Output

The AI must return one practical next action under 40 words.

Example:

```text
Send a short follow-up email asking whether they reviewed the proposal and offer two time slots for a call next week.
```

### Save Suggestion

The generated suggestion must be saved to:

```ts
aiSuggestedNextAction
```

### Fallback Behavior

If the OpenAI API key is missing or the API call fails, use this fallback:

```text
Follow up with this lead, reference the latest notes, and ask for a clear next step.
```

### Loading and Error States

The UI must show:

- loading state while generating
- generated suggestion when complete
- readable error only if both AI and fallback fail

## Acceptance Criteria

- User can generate a follow-up suggestion.
- Suggestion uses lead context.
- Suggestion is under 40 words.
- Suggestion is saved in SQLite.
- Suggestion appears in the lead detail modal.
- Suggestion appears on the lead card.
- Missing OpenAI API key does not crash the app.
- Failed AI call returns the fallback suggestion.