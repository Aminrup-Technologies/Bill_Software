# Cursor session bootstrap

Copy everything below the line into a **new** Composer/Agent chat. Do not paste prior conversation.

---

Objective: Start a new FLMX implementation session using the repository-first workflow. Ignore previous conversational history unless it is explicitly summarized in the repository documentation.

Purpose: Start every new Cursor session with **minimal context**. Repository documents take precedence over chat.

Required context (read in this order):

1. `@docs/ARCHITECTURE.md`
2. `@docs/HANDOFF.md`
3. `@docs/CURSOR_RULES.md`

Documentation map (page/UAT lookup only): `@docs/SOLUTION_INDEX.md`

Rules:

- Ignore previous conversational history.
- Read only implementation files explicitly referenced afterward (`@filename`).
- Request additional `@filename` files instead of scanning the repository.
- Preserve the existing ASP.NET Web Forms + .NET Framework architecture (`ARCHITECTURE.md`). No Windows Service host, no parallel AuthN, no JWT/Identity.
- Follow Ponytail (`Bill_Software/.cursor/rules/ponytail.md`): parameterized SQL, `CompanyID` tenant scope, no static user state in `.aspx.cs`.
- Cross-Tenant Duplication **v1.1** is closed at `88f9d0f`. Do not reopen it or Switch User runtime unless a new defect is named.
- Produce unified diffs only.
- Modify only referenced files. No speculative refactoring, wrappers, or placeholders.

Waiting for task-specific `@filename` references.
