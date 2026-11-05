---
name: parallel-feature-workflow
description: Implement a substantial feature or cross-layer change through the project's planner, parallel implementer, and reviewer agents. Use when the user asks Codex to carry a feature request from planning through implementation and integration using separate Git worktrees.
---

# Parallel Feature Workflow

Treat the user's request accompanying this skill as the feature brief. Own the complete result and coordinate the three configured custom agents: `planner`, `implementer`, and `reviewer`.

## 1. Establish the task and repository state

1. Read applicable `AGENTS.md` files and inspect the repository root, current branch, `git status`, and relevant project documentation. Follow repository instructions and agent-specific requirements; use this workflow only because the user invoked it.
2. Read only the architecture, API, testing, coding, and feature documentation relevant to the request. For behavior changes, have the planner create or update the corresponding feature specification and stable acceptance scenarios before implementation.
3. Confirm the requested behavior is sufficiently clear. Ask the user about business decisions that would materially change behavior; do not invent answers.
4. Record the base commit and preserve any existing user changes. If the current checkout has changes that would make the integration target unsafe or ambiguous, stop before creating worktrees and ask how to handle them. Never stash, reset, discard, or overwrite user work.

## 2. Plan with the planner

Spawn the `planner` custom agent with the user's complete feature brief, repository state, and relevant project instructions. Ask it to:

- Inspect the code and relevant specifications.
- Update the feature specification and acceptance scenarios for new or changed behavior.
- Return a dependency-aware implementation plan and bounded implementer task cards.
- Identify shared contracts and any prerequisite that must be settled before parallel work.
- Assign each task an owner, acceptance IDs, exact file or area ownership, exclusions, required checks, and expected handoff.

Wait for the planner. Resolve necessary scope or contract decisions before implementation. Do not parallelize tasks with overlapping write ownership or unresolved dependencies. If the work is not meaningfully divisible, use one implementer rather than manufacturing parallel tasks.

## 3. Create isolated implementer worktrees

Use one Git worktree and unique branch per implementer, all based on the recorded base commit. Keep the user's original checkout as the integration target. Put temporary worktrees outside the repository's tracked tree, using a unique run-specific directory. Do not create a worktree from another implementer's worktree.

Before spawning implementers, verify each worktree exists and that `git -C <path> rev-parse --show-toplevel` returns its assigned root. Give each implementer the absolute path to its worktree and tell it to run all reads, edits, and commands against that path only. Require it to verify its root before editing. A parent chat being in a worktree does not establish that subagents have separate worktrees; create and assign the worker worktrees explicitly.

Keep planner-owned feature-spec edits in the integration checkout. Give implementers the relevant specification path and acceptance criteria as read-only context; tell them not to duplicate or edit planner-owned specification changes unless a task explicitly owns that documentation.

## 4. Run implementers in parallel

Spawn one `implementer` instance per independent task card, concurrently, up to the configured subagent limit. Each assignment must include:

- The user's goal and relevant acceptance IDs.
- The complete task card, dependencies, shared contracts, owned files, and exclusions.
- Its absolute worktree path and the requirement to edit only there.
- Relevant documentation and the test-first sequence: add focused tests, observe the expected failure, implement, then rerun affected checks.
- A request to stage only its own completed changes (no commit) after checks, and to return exact commands/results, changed paths, assumptions, blockers, and remaining coverage.

Wait for every implementer. If a task is blocked by a shared contract or a decision, stop dependent work and bring the issue to the parent instead of allowing incompatible guesses. Do not integrate incomplete or unreported work as complete.

## 5. Integrate and review with the reviewer

After all implementers finish, spawn the `reviewer` custom agent in the original integration checkout. Provide the base commit, planner-owned specification changes, each worker worktree path and task handoff, and the exact staged diff for each worker relative to the base. Ask the reviewer to apply the changes incrementally, preserve the planner's specification, resolve mechanical conflicts, review the combined result against the acceptance criteria and project guidance, fix straightforward in-scope issues, and run the required combined verification.

If worktree diffs conflict in behavior, contracts, or ownership, the reviewer must stop the affected integration and ask for a decision; never silently choose a business behavior. Let the reviewer distinguish directly verified results from worker reports and assumptions. Do not commit, push, publish, deploy, or remove worktrees.

## 6. Report the result

Summarize the implemented behavior, feature specification and acceptance IDs, which worktrees were integrated, important review fixes or unresolved findings, and the exact verification commands and outcomes. Call out any checks that could not run. Leave worktrees available for inspection and report their paths. Do not claim full completion if the reviewer reports unresolved blockers or failed required checks.

## Failure handling

- If a required custom agent is unavailable, report which role could not run and do not silently substitute a different workflow.
- If Git worktrees cannot be created or verified, do not launch parallel writers in the shared checkout. Return the planner's plan and explain the isolation blocker.
- If the base branch changes or the integration checkout is modified during the run, pause integration, preserve all worktrees, and ask for direction before reconciling.
