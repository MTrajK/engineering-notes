$parallel-feature-workflow

Add exactly one read-only API method to each existing controller:

1. WorkshopsController: Get a workshop’s availability by ID. Return its capacity, the number of registrations currently occupying seats, and the number of seats remaining. Exclude cancelled registrations. Return 404 when the workshop doesn’t exist.

2. ParticipantsController: Get a participant’s registrations. Include each registration’s ID, workshop summary, registration date, and status, ordered newest first. Return 404 when the participant doesn’t exist; return 200 with an empty list when they have no registrations.

Follow the existing API conventions and feature specifications. Add no controllers or endpoints beyond these two methods.