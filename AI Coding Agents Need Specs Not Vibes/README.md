# AI Coding Agents Need Specs, Not Vibes

Companion repo for the **"AI Coding Agents Need Specs, Not Vibes"** presentation (`AI Coding Agents Need Specs Not Vibes.pptx`).

It demonstrates a Spec-Driven Development (SDD) workflow using [Spec Kit](https://github.com/spec-kit) with [Windsurf](https://windsurf.com/).

## Repo Structure

```
├── sample-project/          # Spec Kit project template ready for Windsurf
│   ├── .windsurf/           # Spec Kit slash-command workflows (/speckit.*)
│   └── .specify/            # Spec Kit output directory
│
├── prompts/                 # Prompt templates & examples
│   ├── sdd-spec-kit/        # Prompts for the Spec Kit workflow
│   │   ├── constitution.md  # /speckit.constitution prompt example
│   │   ├── specify.md       # /speckit.specify prompt template
│   │   ├── clarify.md       # /speckit.clarify prompt example
│   │   └── constitution-seeds/  # Seed principles for the constitution
│   │
│   └── sdd-vanilla/         # Prompts for SDD without a framework
│       ├── product.md       # Product spec (Mini CRM example)
│       ├── tech.md          # Technical spec
│       └── features/        # Per-feature spec prompts
```

## Getting Started

### Use the sample project as a template

1. Copy the `sample-project/` folder into your local path where the new project should be located.
2. Open it in **Windsurf**.
3. Create the constitution using the constitution-seed from prompts (`/speckit.constitution`).
3. The Spec Kit workflows are available as slash commands (e.g. `/speckit.specify`, `/speckit.plan`, `/speckit.implement`).

### Use the prompt templates

Browse the `prompts/` folder for ready-to-use prompt templates and examples. Copy and adapt them for your own features.

## Presentation

See `AI Coding Agents Need Specs Not Vibes.pptx` for the full context and walkthrough.
