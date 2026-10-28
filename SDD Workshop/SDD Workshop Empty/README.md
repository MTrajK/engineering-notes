# Expense Tracker

A minimal personal expense tracker built as a frontend-only React SPA. The app stores all data in the browser, ships with seeded sample data, and focuses on a small set of reliable flows: dashboard summaries, expense management, category management, and theme persistence.

## Features

- Month-based dashboard with total, daily average, and category breakdown
- Expense list with filtering, pagination, edit, and delete flows
- Category management with uniqueness rules and protected deletion for used categories
- Shared add/edit expense modal available from every page
- Light and dark theme support with persisted preference
- Seeded local data for immediate first-run use

## Tech Stack

- React
- Vite
- TypeScript
- React Router
- Recharts
- TanStack Table
- React Hook Form
- Zod
- React Context + `useReducer`
- `localStorage`
- CSS Modules + CSS variables
- date-fns
- lucide-react
- Vitest + React Testing Library
- Playwright
- ESLint + Prettier

## Project Structure

```text
src/
  app/          App shell, routing, provider, reducer, shared app state
  domain/       Core TypeScript models and shared domain types
  features/     Feature-specific UI, hooks, schemas, selectors, and tests
  shared/       Reusable cross-feature UI components
  storage/      Browser persistence helpers, repositories, seed initialization
  styles/       Global CSS variables and theme tokens
tests/
  e2e/          Playwright end-to-end coverage
```

## Setup

### Prerequisites

- Node.js 20+ recommended
- npm
- playwright install chromium browser

### Install

```bash
npm install
npx playwright install chromium
```

### Start the App

```bash
npm run dev
```

The Vite dev server will print the local URL in the terminal.

## Available Scripts

- `npm run dev` starts the development server
- `npm run build` creates the production build
- `npm run preview` serves the production build locally
- `npm run lint` runs ESLint
- `npm run test` runs the Vitest suite with coverage
- `npm run test:watch` runs Vitest in watch mode
- `npm run test:e2e` runs the Playwright suite

## Testing

### Unit and Component Tests

```bash
npm run test
```

### End-to-End Tests

```bash
npm run test:e2e
```

### Full Local Verification

```bash
npm run lint
npm run build
npm run test
npm run test:e2e
```

## Lint and Formatting

- Lint with `npm run lint`
- Prettier is configured in the repository for consistent formatting

If you want to format the codebase locally, run Prettier with your preferred editor integration or CLI setup.

## Browser Storage and Seed Data

- The app uses `localStorage` only
- On first launch, seeded categories and expenses are written into browser storage
- After initialization, all mutations are persisted locally in the browser
- Clearing site storage resets the app back to the seeded state on the next load

## Notes

- The app is intentionally frontend-only and single-user
- There is no backend, authentication, sync