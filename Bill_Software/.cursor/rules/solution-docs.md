# Solution documentation (no duplicate context)

When documenting, reviewing, or changing any `.aspx` page:

1. Follow `docs/page-catalog/RECURSIVE_INSTRUCTIONS.md` (self-recursive queue).
2. Read `docs/page-catalog/SHARED_CONTEXT.md` **once** per session. Read `SHARED_RUNTIME.md` only for handlers/helpers/SPs. Read `SHARED_SCHEMA.md` for live UAT table/FK/gap facts. Read `DATA_DICTIONARY.md` for page↔object verbs. Do not paste AuthN, tenancy, Ponytail, or master-page behavior into a domain catalog.
3. Update only the matching `docs/page-catalog/DOMAIN_*.md` row plus `PAGE_INVENTORY.md` status.
4. Link existing `docs/01`–`docs/22` and `docs/sales-visit-workflow-audit/` instead of rewriting them. Module docs 01–13 are leftover pointers, not a second census.
5. Never create one markdown file per page. One table per domain.
6. Refresh the census **and** the page↔object dictionary with `python3 docs/page-catalog/_generate.py`. Text after `<!-- NARRATIVE:BEGIN -->` in each `DOMAIN_*.md` is kept. One-line quirks live in `_generate.py` (`UNIQUE_NOTES`). Do not hand-edit `DATA_DICTIONARY.md`.
