---
applyTo: '**'
---
# Agent/Assistant Policy for this repository

This file documents a simple, enforceable policy for automated assistants (including GitHub Copilot, chat-based assistants, or CI bots) working in this repository.


Goal
----
Avoid surprise or destructive changes. Require explicit human approval before any schema-changing, file-deleting, or large refactor actions.

Modes and tokens
----------------
- STRICT mode (default for assistants working in this repo):
  - Behavior: "Do exactly what I say. Do NOT make assumptions. If the instruction is underspecified, list missing info and stop."
  - Required tokens to allow risky operations:
    - `APPROVE-MIGRATION` — allows creation or application of DB migrations or SQL schema changes.
    - `APPROVE-DELETE` — allows deleting files.
    - `APPROVE-REFACTOR` — allows large-scale refactors (moving many files, renames, public API changes).

How to give an instruction
--------------------------
Start prompts with one of the mode prefixes:

  STRICT: Do exactly what I say. Do not make assumptions. If anything is missing, list it and stop.

Then include explicit tokens if you want the assistant to perform risky actions.

Examples:

- Add safe scaffolding only (no migrations):

  `STRICT: Create region dict scaffolding (controller, repo, entity) for Features/Dict/Region. Do NOT run or create migrations.`

- Allow migrations and deletions:

  `STRICT: Implement Region feature and create migration. APPROVE-MIGRATION APPROVE-DELETE` 

Assistant responsibilities
-------------------------
- If a prompt does NOT include the needed APPROVE tokens the assistant must NOT perform the corresponding action.
- For any destructive action (delete files, change DB schema, or large refactors), the assistant must present a one-line checklist of intended changes and require an explicit `CONFIRM` reply from the user before proceeding.
- The assistant must not infer user priorities when explicit instructions are present. If uncertain, it must ask a clarifying question.

Repository recommendations (optional enforcement)
-----------------------------------------------
- Add branch protections and require PR reviews for `main`.
- Add CI checks that scan PR diffs for migration files, schema-change markers, or deleted files and fail the job unless the PR body contains the `APPROVE-*` tokens.
- Add a `CODEOWNERS` file for sensitive directories (Data/, Migrations/) so PRs modifying them require owner review.

Conclusion
----------
Follow this policy when making edits. If you are an automated assistant, require tokens or explicit confirmation before doing anything that could change the DB schema, delete files, or refactor large parts of the codebase.

If you want, I can also scaffold a simple CI job that enforces the tokens on PRs (GitHub Actions) — tell me if you want that added.

Additional: 
1. Tell it like it is; don't sugar-coat responses. Get right to the point. Be practical above all.
2. Use zsh as the shell environment for running commands.
3. Do not ever start the app, ask user instead to run `dotnet run` in the terminal.
