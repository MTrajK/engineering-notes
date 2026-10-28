---
name: workshop.validator
description: Use this skill when the user wants to validate the workshop solution by running build, lint, unit tests, and end-to-end tests.
---

# Workshop Validator

## Goal

Validate the current solution by running these four commands:

```bash
npm run build
npm run lint
npm run test
npm run test:e2e
```

## Rules

- Run the commands from the project root.
- Do not change files.
- Do not fix issues automatically.
- Stop only if a command blocks the next checks completely.
- Report the result clearly.

## Output format

```text
Solution validation: PASS / FAIL

Results:
✅ npm run build — passed
✅ npm run lint — passed
✅ npm run test — all unit tests passed
✅ npm run test:e2e — all 9 E2E tests passed

Notes:
- ...
```

## If something fails

Use this format:

```text
Solution validation: FAIL

Results:
✅ npm run build — passed
❌ npm run lint — failed
⏭️ npm run test — skipped
⏭️ npm run test:e2e — skipped

Failure details:
- Command:
- Error summary:
- File or test affected:

Recommended next action:
- ...
```
