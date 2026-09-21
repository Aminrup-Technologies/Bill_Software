# Cursor execution rules (Project_FLMX)

Permanent rulebook for agents working in this repository. Chat history is not architecture. If this file conflicts with a prior conversation, this file wins unless [`HANDOFF.md`](HANDOFF.md) records a newer, scoped exception.

Companion: [`ARCHITECTURE.md`](ARCHITECTURE.md), [`HANDOFF.md`](HANDOFF.md), [`SOLUTION_INDEX.md`](SOLUTION_INDEX.md) (documentation map), `Bill_Software/.cursor/rules/ponytail.md`, `Bill_Software/.cursor/rules/project.mdc`, [`22_Security_Baseline.md`](22_Security_Baseline.md).

---

## Repository-first workflow

1. Treat the repository as the only durable context.
2. Start every task by reading, in order:
   1. `docs/ARCHITECTURE.md`
   2. `docs/HANDOFF.md`
   3. `docs/CURSOR_RULES.md` (this file)
3. Then read **only** implementation files the user (or HANDOFF) explicitly referenced with `@filename`.
4. If required code is not referenced, **ask for additional `@filename` paths**. Do not scan the tree “to be thorough.”
5. Ignore previous conversational history unless it is summarized in repository docs (`HANDOFF.md`, ADRs, module docs).
6. Do not use memories of other products, Windows Services, or stacks that are not in `ARCHITECTURE.md`.

---

## Read scope

- Read only explicitly referenced `@filename` files (plus the three docs above when executing a change).
- Existing `docs/` are for naming/style and for files the task names. Do not rewrite them unless listed.
- Do not modify `README.md` or `CONTRIBUTING.md` unless the task lists them.

---

## Output: unified diffs only

- Present changes as **unified diffs only** (exact lines added/removed/changed).
- No conversational preamble, no “here is the code,” no post-explanation unless the user asked **why**.
- Diffs must apply to the referenced files. Do not emit a second copy of unchanged files.

---

## Modify only referenced files

- Create or edit only paths the task named.
- Preserve existing repository structure.
- Avoid unrelated edits (formatting-only, drive-by renames, comment noise).
- Do not add one markdown file per `.aspx` page. Page facts go in `DOMAIN_*.md` per `.cursor/rules/solution-docs.md`.

---

## Preserve architecture

- Keep ASP.NET Web Forms + IIS + ADO.NET + SQL Server.
- Do not add a Windows Service, background job host, new ORM, ASP.NET Identity, or JWT.
- Inherit `AuthGuard` / `SecurePage`. Do not implement parallel authentication or authorization.
- Keep kiosk (`tbl_card_login`) isolated from ERP auth.
- Honor Decision #8 and Decision #9 (`docs/22`).
- Do not rotate `machineKey` or production secrets in feature work.

---

## No placeholder implementations

- Ship complete, compiling behavior for the requested change.
- No `TODO`, `NotImplementedException`, empty stubs, or “fill in later” helpers.
- No invented fallbacks that fail open (missing table, missing membership, missing print map → deny).

---

## No unnecessary wrapper classes

- Do not add facades around existing `SqlConnection` / `DB_UTILITY` / page SQL.
- Do not extract “services” unless the task requires it and the type has a unique job (see `SHARED_RUNTIME.md`).
- Prefer native Web Forms controls. No new UI framework.

---

## No speculative refactoring

- Change only what the task requires.
- Do not “clean up” neighboring pages, SQL, or naming.
- Do not migrate concatenated historical SQL except in the files in scope — and then parameterize, do not rewrite the workflow.
- Do not bulk-populate grids. Keep manual selection workflows.

---

## SQL documentation metadata

When authoring or modifying `.sql` files, document **When**, **Why**, and **What** in the standard header. Prepend this exact block:

```
/* ============================================================================
   NAME:        <snake_case_name_under_six_words>
   WHEN:        <YYYY-MM-DD>
   WHY:         <Business or technical justification>
   WHAT:        <Specific table, index, or SP modifications>
   ============================================================================ */
```

- **WHEN:** change date (`YYYY-MM-DD`).
- **WHY:** business or technical justification (defect id / ADR / tenant isolation).
- **WHAT:** exact objects touched (table, index, procedure, grant).
- Parameterize all application SQL. Tenant filter: `CompanyID = @CompanyID` (or documented ownership predicate).
- `using` blocks for ADO.NET. No public static user state in `.aspx.cs`.

---

## Ponytail (condensed)

- Minimal, dense code. No verbose logging.
- VS 2015 / .NET Framework compatibility.
- `Session["USERID"]` validation across page lifecycles.
- Log `tbl_SystemNotification` before transactional commit where that pattern already exists.

---

## After a change

- Update `HANDOFF.md` Current Status / Current Task / Next Action if the work is ongoing.
- Do not paste secrets. Do not expand scope to “while we are here.”
- Git-affecting work follows **Controlled Execution** below and stops at the approval gate. Do not commit or push unless the user explicitly approved that step.

---

## Controlled Execution Workflow

Permanent Git-affecting operating model. **Enforcement:** `Bill_Software/.cursor/rules/project.mdc` (always-on). This section documents the architecture; `project.mdc` is what Cursor must follow without being re-prompted.

Cursor automatically:

1. **Inspect** — verifies branch, HEAD SHA, working tree, index; inventory stashes when the task is stash-related.
2. **Execute** — runs the approved scope in the agent environment (`git restore --source` preferred). Does not ask the user to paste Git commands unless execution fails. Does not apply/pop/drop a stash unless explicitly requested.
3. **Verify** — runs `git status`, `git diff --stat`, `git diff --cached --stat`; confirms expected files only and that secrets were not restored.
4. **Approval** — stops before commit, push, merge, or stash mutation; presents an execution summary and waits for explicit approval. Commit approval is not push approval.

### Execution modes

| Mode | Cursor may | Stops before |
|------|------------|--------------|
| **Analyze** | Read-only | Any write |
| **Restore** | Restore named paths | commit |
| **Implement** | Modify named files | commit |
| **Publish** | Push after a **separate** explicit approval | push until that approval |

Unnamed tasks: infer Analyze / Restore / Implement from the objective. Never Publish unless named or push is explicitly approved.

### Prompt contract

Implementation prompts should contain **only**:

- Objective
- Required files
- Task-specific constraints
- Acceptance criteria

Do not repeat Inspect → Execute → Verify → Approval, stash preservation, or secret-file exclusions in each prompt. Those are repository rules. Task-specific constraints hold only what is unique to that change (paths, SHAs, “do not edit X”).

### Git safeguards (summary)

Encoded in `project.mdc`: `git status` first; verify HEAD; preserve stashes; prefer `git restore`; never commit `Web.config`, `*.pubxml`, or MCP files; separate commit and push gates; never mutate stash state without explicit approval.
