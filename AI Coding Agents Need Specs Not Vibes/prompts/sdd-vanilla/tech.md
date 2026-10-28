# Mini CRM Technical Specification

## Tech Stack

Use a simple full-stack TypeScript implementation.

Preferred stack:

- Next.js
- TypeScript
- React
- Tailwind CSS
- SQLite
- Prisma ORM
- OpenAI API

## Architecture

The application should include:

- a main dashboard page
- reusable UI components
- API routes for lead operations
- API route for AI suggestion generation
- Prisma-based database access
- shared TypeScript types
- lightweight server-side validation

Keep the architecture intentionally simple and readable.

## Suggested Folder Structure

```text
/src
  /app
    /page.tsx

    /api
      /leads
        route.ts

      /leads/[id]
        route.ts

      /leads/[id]/suggest-follow-up
        route.ts

  /components
    LeadBoard.tsx
    LeadColumn.tsx
    LeadCard.tsx
    LeadForm.tsx
    LeadDetailModal.tsx

  /lib
    prisma.ts
    ai.ts
    validation.ts

  /types
    lead.ts

/prisma
  schema.prisma

/database
  dev.db
```

## Database

Use SQLite for local development simplicity.

Database file:

```text
/database/dev.db
```

Use Prisma ORM for database access.

## Prisma Schema

```prisma
datasource db {
  provider = "sqlite"
  url      = "file:./database/dev.db"
}

generator client {
  provider = "prisma-client-js"
}

model Lead {
  id                      String   @id @default(cuid())
  name                    String
  company                 String
  email                   String?
  status                  String
  notes                   String?
  aiSuggestedNextAction   String?
  createdAt               DateTime @default(now())
  updatedAt               DateTime @updatedAt
}
```

## Data Model

```ts
export type LeadStatus =
  | "New"
  | "Contacted"
  | "Proposal"
  | "Won"
  | "Lost";

export type Lead = {
  id: string;
  name: string;
  company: string;
  email?: string;
  status: LeadStatus;
  notes?: string;
  aiSuggestedNextAction?: string;
  createdAt: string;
  updatedAt: string;
};
```

## API Routes

### GET /api/leads

Returns all leads ordered by created date descending.

Response:

```ts
Lead[]
```

---

### POST /api/leads

Creates a new lead.

Request body:

```ts
{
  name: string;
  company: string;
  email?: string;
  notes?: string;
}
```

Behavior:

- validate required fields
- validate email format if email exists
- default status to "New"
- create timestamps automatically
- return created lead

---

### PATCH /api/leads/:id

Updates an existing lead.

Allowed fields:

- name
- company
- email
- status
- notes

Behavior:

- validate status if provided
- validate email if provided
- update updatedAt automatically
- return updated lead

---

### POST /api/leads/:id/suggest-follow-up

Generates an AI follow-up suggestion for a lead.

Behavior:

- find the lead
- call OpenAI with lead context
- save result to aiSuggestedNextAction
- update updatedAt
- return updated lead

## OpenAI Integration

Use environment variable:

```text
OPENAI_API_KEY
```

Use a lightweight prompt.

Suggested system prompt:

```text
You are a practical sales assistant helping a consultant manage leads.

Generate one concise next follow-up action based on the lead information provided.

Keep the response under 40 words.
```

## Fallback AI Behavior

If:

- OPENAI_API_KEY is missing
- OpenAI request fails
- timeout occurs

Return this fallback:

```text
Follow up with this lead, reference the latest notes, and ask for a clear next step.
```

The app should never crash because of AI failures.

## UI Requirements

The UI should include:

- page title: "Mini CRM"
- short subtitle
- Kanban board with five columns
- "New Lead" button
- modal or side panel for lead creation
- editable lead detail modal
- status selector
- notes textarea
- AI suggestion section
- button to generate AI follow-up suggestion

## UI Constraints

Keep the UI intentionally simple.

Avoid:

- drag and drop
- authentication
- complex animations
- Redux or complex state libraries
- unnecessary abstractions
- enterprise patterns

This is a teaching demo.

## Validation Rules

### Lead Creation

Required:

- name
- company

Optional:

- email
- notes

### Email Validation

If email exists:

- must contain "@"

Lightweight validation only.

## Error Handling

Handle:

- missing required fields
- invalid email format
- invalid status
- lead not found
- AI API failure
- database failure

Errors should return:

- clear API responses
- readable UI messages

## Suggested Commands

Install dependencies:

```bash
npm install next react react-dom typescript tailwindcss prisma @prisma/client openai
```

Initialize Prisma:

```bash
npx prisma init
```

Create database:

```bash
npx prisma migrate dev --name init
```

Run development server:

```bash
npm run dev
```

## Production Notes

This is a teaching/demo application.

Known limitations:

- SQLite is not intended for production scaling
- no authentication
- no permissions
- no audit trail
- minimal validation
- single-user assumptions
- no automated tests