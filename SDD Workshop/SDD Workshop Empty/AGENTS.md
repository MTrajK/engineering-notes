# AGENTS.md

## Purpose

This guide is for AI coding agents and new teammates working in this repository.
It describes the app, the current architecture, the commands that matter, and the
constraints that must stay true as the project evolves.

The product is a minimal personal Expense Tracker implemented as a frontend-only
React single-page app. Keep the app small, deterministic, browser-only, and easy
to maintain.

## Project Overview

- App type: frontend-only React SPA built with Vite and TypeScript.
- Product scope: single-user personal expense tracking.
- Persistence: `localStorage` only, seeded on first launch.
- Theme: light and dark modes with persisted manual preference.
- There is no backend, login, registration, sync, cloud storage, API
  integration.

## Tech Stack

Use the existing stack unless the user explicitly asks for a change:

- React 19
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

Do not add Redux, Zustand, MobX, TanStack Query, SWR, UI frameworks, Tailwind,
IndexedDB, backend services, authentication packages, i18n infrastructure, or
server-side rendering unless specifically requested.

## Repository Layout

```text
src/
  app/          App shell, router, layout, provider, reducer, app state
  domain/       Shared TypeScript domain models
  features/     Feature-specific UI, hooks, selectors, schemas, utils, tests
  shared/       Reusable cross-feature UI components
  storage/      localStorage wrappers, repositories, seed initialization
  styles/       Global styles, CSS variables, theme tokens
tests/
  e2e/          Playwright end-to-end tests
```

## Build And Test Commands

Install dependencies:

```bash
npm install
```

Start development server:

```bash
npm run dev
```

Create production build:

```bash
npm run build
```

Preview production build:

```bash
npm run preview
```

Run ESLint:

```bash
npm run lint
```

Run Vitest unit/component tests with coverage:

```bash
npm run test
```

Run Vitest in watch mode:

```bash
npm run test:watch
```

Run Playwright E2E tests:

```bash
npm run test:e2e
```

Playwright is configured to start Vite at `http://127.0.0.1:4173` via
`playwright.config.ts`. If browser binaries are missing, install Chromium with:

```bash
npx playwright install chromium
```

Before handing off feature work, run:

```bash
npm run lint
npm run build
npm run test
npm run test:e2e
```

For documentation-only changes, full test execution is optional, but say clearly
when tests were not run.

## Architecture Rules

- Keep page components thin. Put business logic in selectors, hooks, schemas, or
  utilities.
- Keep feature-specific code inside the relevant `src/features/<feature>/` folder.
- Put only genuinely reusable UI in `src/shared/`.
- Keep persistence behind `src/storage/`; page and feature components should not
  call `localStorage` directly.
- Keep domain models centralized in `src/domain/`.
- Use React Context + `useReducer` for app-wide state. Do not introduce another
  state library.
- Use namespaced reducer actions such as `expense/add`, `category/delete`,
  `dashboard/setMonth`, `theme/set`, and `expenseModal/openEdit`.
- Derive dashboard values through selectors, not inline JSX calculations.
- Keep TanStack Table setup encapsulated in `useExpensesTable` or related feature
  helpers.
- Do not add speculative abstractions. Prefer explicit, local, readable code.

## Code Style Guidelines

- Use TypeScript with strong explicit types. Avoid `any`.
- Prefer pure functions for calculations and business rules.
- Use Zod schemas for form validation; do not scatter validation checks across JSX.
- Use React Hook Form for forms.
- Use CSS Modules for component styles and CSS variables for tokens.
- Keep `data-theme="light"` / `data-theme="dark"` behavior intact.
- Use lucide-react icons for icon buttons when an icon exists.
- Keep components focused and accessible.
- Avoid dead code, placeholder TODOs, and unrelated refactors.
- Use Prettier-compatible formatting. The repo has `.prettierrc.json` and ESLint
  extends `eslint-config-prettier`.
- Keep files named consistently:
  - Components: `PascalCase.tsx`
  - Hooks: `useSomething.ts`
  - Utilities/selectors: `camelCase.ts`
  - Schemas: `somethingSchema.ts`
  - Reducers: `somethingReducer.ts`

## Accessibility Expectations

- Use semantic HTML for navigation, tables, headings, forms, and dialogs.
- Every input must have a label.
- Icon-only buttons must have accessible names.
- Dialogs and modals must support focus management and keyboard operation.
- Destructive actions must require confirmation.
- Do not rely on color alone to communicate important state.
- Preserve existing role/name patterns where tests depend on accessible queries.

## Testing Instructions

Testing is mandatory for behavior changes.

Use Vitest and React testing library for Unit / Component tests.

Use Playwright for critical user flows.

Test conventions:

- Put feature tests beside the code they cover when practical.
- Keep E2E tests in `tests/e2e/`.
- Prefer accessible queries (`getByRole`, `getByLabel`, `getByText`) over brittle
  selectors.
- Reset or isolate localStorage state in tests when needed.
- Do not weaken assertions just to make tests pass.

## Commit Messages

Use short, imperative commit messages. Conventional Commit style is preferred:

- `feat: add expense filtering`
- `fix: preserve theme after reload`
- `test: cover category deletion rules`
- `docs: update agent guide`
- `refactor: simplify dashboard selectors`

Keep each commit focused. Do not mix formatting-only churn with behavior changes
unless the formatting is required by the touched files.

## Pull Request Guidelines

PRs should include:

- What changed and why.
- Screenshots or short notes for visible UI changes.
- Tests run, including any skipped tests and the reason.
- Storage or migration impact, if any.
- Accessibility considerations for new UI.
- Any new dependency and why it is necessary.

Before opening a PR, verify:

- `npm run lint` passes.
- `npm run build` passes.
- `npm run test` passes.
- `npm run test:e2e` passes for user-flow changes.
- No generated artifacts are included.

## Agent Working Rules

- Inspect the current structure before adding files.
- Respect existing feature boundaries and naming conventions.
- Reuse current utilities, selectors, schemas, and components where appropriate.
- Keep changes scoped to the user request.
- Preserve product constraints: browser-only, single-user, localStorage-only.
- Do not introduce new dependencies without a clear, necessary reason.
- Do not overwrite user work or revert unrelated changes.
- If you cannot run a required command, report that clearly in the final response.
- In the end summarize changed files and the implementation.
- Update the README.md if needed.

## Definition Of Done

A change is complete when:

- The implementation matches the requested behavior and this guide.
- Types are correct.
- Relevant tests are added or updated.
- Lint, build, and applicable tests pass.
- UI changes preserve accessibility and theme behavior.
- Persistence behavior remains compatible with existing localStorage data.