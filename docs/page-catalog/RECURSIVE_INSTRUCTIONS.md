# Self-recursive page review instructions

Use this playbook to document **every** `.aspx` page in `Bill_Software/` without duplicating shared architecture.

## Goal

Maintain a complete, non-duplicative catalog:

| Artifact | Role |
|----------|------|
| [SHARED_CONTEXT.md](SHARED_CONTEXT.md) | Cross-cutting facts — **read once per session** |
| [SHARED_RUNTIME.md](SHARED_RUNTIME.md) | Handlers, ASCX, helpers, SQL scripts, SP call sites — **not** page census |
| [SHARED_SCHEMA.md](SHARED_SCHEMA.md) | Live UAT tables, FKs, missing objects — **not** page census |
| [PAGE_INVENTORY.md](PAGE_INVENTORY.md) | Census of all pages + domain + review status |
| `DOMAIN_*.md` | Unique facts per page, grouped by business domain |
| Existing `docs/01`–`docs/22` and `sales-visit-workflow-audit/` | Leftover pointers + specialist narrative — **link, never copy** |

## Recursion (do this until the queue is empty)

```
REVIEW_NEXT_PAGE:
  1. Open PAGE_INVENTORY.md. If every row is Reviewed, STOP and run CONSISTENCY_PASS.
  2. Take the first row whose status is Pending (or Stale if the .aspx/.cs hash changed).
  3. Load SHARED_CONTEXT.md if not already loaded this session. Load SHARED_RUNTIME.md
     only for .ashx / .ascx / helper / SP work. Do not reload README or security docs
     unless the page is auth, print, or card-kiosk.
  4. Read only:
       a. the .aspx Page directive (Title, MasterPageFile, Inherits)
       b. the .aspx.cs class declaration, RequiredPermissionKey / EnsurePrint,
          Page_Load, WebMethods, QueryString keys, SQL table names, CRUD verbs
       c. the matching DOMAIN_*.md row
       d. the linked module narrative ONLY if this page is named in that narrative
          and the unique facts disagree
  5. WRITE unique facts into the domain file row. Do not paste AuthGuard, CompanyContext,
     Ponytail, or session-validation prose.
  6. Mark the inventory row Reviewed. If the page was deleted, mark Removed and drop
     the domain row.
  7. GOTO REVIEW_NEXT_PAGE.
```

```
CONSISTENCY_PASS:
  1. Re-scan all *.aspx (excluding packages/). Count must match PAGE_INVENTORY.md.
  2. Every page belongs to exactly one domain file.
  3. No domain file restates SHARED_CONTEXT.md.
  4. Menu ids in Bill.Master that point at an .aspx must appear as menu_id on that page.
  5. Pages inheriting SecurePage must show a permission key or any-keys.
  6. Print pages must show EnsurePrint resource (including unmapped/fail-closed).
  7. docs/01–docs/13 must not contradict DOMAIN_*.md (wrong table names, admin/ vs app/ paths).
  8. STOP.
```

## Classification (first fit)

Apply `DOMAIN_RULES` in [generate script comments](#domain-keys) / PAGE_INVENTORY domain column.
Do not put the same page in two domain files. Print layouts stay in `DOMAIN_print.md`
even when they belong commercially to invoice/PO — the app page is documented in the
commercial domain; the print page is documented once under print with a resource key.

## Dedup rules (mandatory)

- **One source of truth per fact.** If README or docs/22 already states it, link.
- **No copy-forward.** Do not clone a neighbor page's paragraph and change the filename.
- **Shared SQL tables** (e.g. `tbl_login`, `ActiveSessions`) belong in SHARED_CONTEXT
  unless this page's use is unusual (writes password hash, bypasses CompanyID, etc.).
- **Existing module docs** are leftover-fact pointers (01–13) or specialist history (14–22).
  Catalog rows hold file-level facts (class name, permission key, QS, WebMethods).
- **Do not create one markdown file per page.** That duplicates context. One table per domain.

## When source changes

If you edit an `.aspx` / `.aspx.cs`:

1. Update **only** that page's domain row (permission, tables, ops, notes).
2. Flip inventory status to Reviewed and set "last verified" to the change date.
3. Do not rewrite sibling rows.

## Domain keys

`auth` `dashboard` `sales-visit` `attendance-hr` `users-rbac` `masters`
`customer` `vendor-purchase` `pr-po` `quotation` `dpcc-challan` `proforma`
`invoice` `hydrant` `payments` `expenses-made` `reports` `print`
`card-kiosk` `other`

## Agent batching

Review in domain batches (one domain per pass) so SHARED_CONTEXT stays in cache
and table names are compared for uniqueness inside the domain. Never start a new
session by re-summarizing the whole ERP.

## Regeneration

Refresh the **census tables** with:

```
python3 docs/page-catalog/_generate.py
```

Everything after `<!-- NARRATIVE:BEGIN -->` in each `DOMAIN_*.md` is preserved.
Unique one-liners in `_generate.py` (`UNIQUE_NOTES`) are regenerated into “Page-unique notes”.
The generator writes **only** `PAGE_INVENTORY.md` and `DOMAIN_*.md` census tables. It must not overwrite `SHARED_CONTEXT.md`, `SHARED_RUNTIME.md`, `SHARED_SCHEMA.md`, `RECURSIVE_INSTRUCTIONS.md`, or `SOLUTION_INDEX.md`.
Do not copy SHARED_CONTEXT into domain files.
