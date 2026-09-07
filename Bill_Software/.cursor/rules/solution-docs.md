# Solution documentation (no duplicate context)

When documenting, reviewing, or changing any `.aspx` page:

1. Follow `docs/page-catalog/RECURSIVE_INSTRUCTIONS.md` (self-recursive queue).
2. Read `docs/page-catalog/SHARED_CONTEXT.md` **once** per session. Do not paste AuthN, tenancy, Ponytail, or master-page behavior into a domain catalog.
3. Update only the matching `docs/page-catalog/DOMAIN_*.md` row plus `PAGE_INVENTORY.md` status.
4. Link existing `docs/01`–`docs/22` and `docs/sales-visit-workflow-audit/` instead of rewriting them.
5. Never create one markdown file per page. One table per domain.
6. Refresh the census with `python3 docs/page-catalog/_generate.py` then re-apply unique notes if the script overwrote a hand note — unique notes live in `_generate.py` (`UNIQUE_NOTES`) so they survive regeneration.
