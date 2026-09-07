#!/usr/bin/env python3
"""Extract unique facts from every .aspx page and generate solution docs."""
from __future__ import annotations

import json
import os
import re
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
APP = ROOT / "Bill_Software"
DOCS = ROOT / "docs"
CATALOG = DOCS / "page-catalog"
OUT_JSON = Path("/tmp/page_inventory.json")

EXISTING_MODULE_DOCS = {
    "attendance": "docs/01_Attendance_Clock.md",
    "employee": "docs/02_Employee_Admin.md",
    "role": "docs/03_Role_Permissions.md",
    "dept": "docs/04_Department_Designation.md",
    "customer": "docs/05_Customer_Vendor.md",
    "vendor": "docs/05_Customer_Vendor.md",
    "visit": "docs/06_Sales_Visit_Planner.md",
    "daily_rpt": "docs/07_Sales_Visit_Reporting.md",
    "expense_entry": "docs/08_Expense_Management.md",
    "quotation": "docs/09_Quotation_Generation.md",
    "purchase_order": "docs/10_Purchase_Order.md",
    "po": "docs/10_Purchase_Order.md",
    "mail": "docs/11_Communications.md",
    "home": "docs/12_Home_Dashboard.md",
    "invoice": "docs/13_Invoice_Search_View_PO_Discovery.md",
}

KNOWN_TABLES = {
    "Roles", "Permissions", "UserRoles", "RolePermissions",
    "ActiveSessions", "UserCompanyAccess",
}
TABLE_RE = re.compile(
    r"\b(?:dbo\.)?(tbl_[A-Za-z0-9_]+|qs[A-Za-z0-9_]+|UserCompanyAccess|ActiveSessions|UserRoles|RolePermissions|Permissions|Roles)\b",
    re.I,
)
SP_RE = re.compile(r"\b(sp_[A-Za-z0-9_]+)\b")
QS_RE = re.compile(r'Request\.QueryString\["([^"]+)"\]')
WM_RE = re.compile(r"\[WebMethod[^\]]*\]\s*(?:public\s+)?static\s+\w+\s+(\w+)", re.M)
PERM_RE = re.compile(
    r'RequiredPermissionKey\s*\{\s*get\s*\{\s*return\s*"([^"]+)"', re.M
)
ANY_PERM_RE = re.compile(
    r'RequiredAnyPermissionKeys\s*\{\s*get\s*\{\s*return\s*new\s+string\[\]\s*\{([^}]+)\}',
    re.M,
)
BASE_RE = re.compile(r"class\s+(\w+)\s*:\s*([\w.]+)")
PAGE_DIR_RE = re.compile(
    r'<%@\s*Page\b([^%]*)%>', re.I | re.S
)
ATTR_RE = re.compile(r'(\w+)\s*=\s*"([^"]*)"')
MENU_LI_RE = re.compile(
    r'<li[^>]*id="([^"]+)"[^>]*>\s*(?:<a[^>]*href="([^"]+)"[^>]*>([^<]*)</a>)?',
    re.I,
)
INSERT_RE = re.compile(r"\bINSERT\s+INTO\b", re.I)
UPDATE_RE = re.compile(r"\bUPDATE\s+", re.I)
DELETE_RE = re.compile(r"\bDELETE\s+FROM\b|\bDELETE\s+\w+", re.I)
COMPANY_RE = re.compile(r"CompanyID|CompanyId|CurrentCompanyID", re.I)
SESSION_RE = re.compile(r'Session\["USERID"\]')
AUTH_PRINT_RE = re.compile(
    r'AuthGuard\.EnsurePrint\(\s*this\s*,\s*"([^"]+)"\s*,\s*Request\.QueryString\["([^"]+)"\]'
)
AUTH_PAGE_RE = re.compile(r"AuthGuard\.(EnsurePage|EnsurePrint|TryValidateSession)")


def parse_page_directive(text: str) -> dict:
    m = PAGE_DIR_RE.search(text)
    if not m:
        return {}
    attrs = {k: v for k, v in ATTR_RE.findall(m.group(1))}
    return attrs


def extract_tables(cs: str) -> list[str]:
    found = []
    seen = set()
    for m in TABLE_RE.finditer(cs):
        t = m.group(1)
        key = t.lower()
        if key in seen:
            continue
        seen.add(key)
        found.append(t)
    for m in SP_RE.finditer(cs):
        t = m.group(1)
        key = t.lower()
        if key in seen:
            continue
        seen.add(key)
        found.append(t + "()")
    return found[:12]


def crud_ops(cs: str) -> list[str]:
    ops = []
    if INSERT_RE.search(cs):
        ops.append("INSERT")
    if UPDATE_RE.search(cs):
        ops.append("UPDATE")
    if DELETE_RE.search(cs):
        ops.append("DELETE")
    if not ops and re.search(r"\bSELECT\b", cs, re.I):
        ops.append("SELECT")
    return ops


def read(p: Path) -> str:
    try:
        return p.read_text(encoding="utf-8", errors="replace")
    except Exception:
        return ""


def parse_menu(master_text: str) -> dict[str, dict]:
    """Map aspx filename -> primary menu item + extra ids from Bill.Master."""
    mapping = {}
    for mid, href, label in MENU_LI_RE.findall(master_text):
        if not href or href == "#":
            continue
        fname = href.split("?")[0].split("/")[-1].lower()
        item = {"menu_id": mid, "label": (label or mid).strip(), "href": href}
        if fname not in mapping:
            mapping[fname] = item
            mapping[fname]["extra_ids"] = []
        else:
            mapping[fname]["extra_ids"].append(mid)
    return mapping


def _has(p: str, *needles: str) -> bool:
    return any(n in p for n in needles)


def _stem(p: str) -> str:
    name = p.replace("\\", "/").rsplit("/", 1)[-1].lower()
    return name[:-5] if name.endswith(".aspx") else name


def _stem_match(p: str, *needles: str) -> bool:
    """True if the aspx stem equals needle or is bounded by '_' on both sides of needle."""
    s = _stem(p)
    for n in needles:
        n = n.lower()
        if s == n or s.startswith(n + "_") or s.endswith("_" + n) or f"_{n}_" in s:
            return True
    return False


DOMAIN_RULES = [
    ("auth", lambda p: p in {
        "index.aspx", "index_start.aspx", "reset_password.aspx", "sessionkeepalive.aspx",
    }),
    ("card-kiosk", lambda p: p.startswith("admin/") or p.startswith("print/")
     or p in {"index_card.aspx", "sessionkeepalive1.aspx"}),
    ("print", lambda p: p.startswith("corporate/business/print/")),
    ("dashboard", lambda p: p.endswith("app/home.aspx") or "quickaction" in p),
    ("sales-visit", lambda p: _has(p,
        "visit_planner", "daily_rpt", "vw_dailyrpts", "srch_dailyrpts", "expense_entry")),
    ("attendance-hr", lambda p: _has(p,
        "attendance", "myleaves", "adminleave", "adminshift", "adminoverride",
        "adminapprovaldashboard")),
    ("users-rbac", lambda p: _has(p,
        "adduser", "viewuser", "manageroles", "managepermissions", "update_designation",
        "settings.aspx") or "/update/" in p),
    ("reports", lambda p: _has(p,
        "rpts_", "product_stock", "service_stock", "payment_due.aspx", "purchess_due")),
    ("customer", lambda p: _has(p,
        "client", "representative", "addfactory", "showfactory", "update_factory")),
    ("pr-po", lambda p: "requisition" in p or _stem_match(p,
        "view_pr", "approve_pr", "generate_po", "view_po",
        "search_purchaseorder", "edit_purchaseorder", "delete_purchaseorder",
        "view_purchaseorder")),
    ("payments", lambda p: _stem_match(p,
        "add_payment", "view_payment", "seartch_payment", "delete_payment",
        "paymentmail", "paymentsdue", "paymentsreceived", "finalpaymentinvoice",
        "add_payment_purchess", "view_purchess_payment", "seartch_purchess_payments",
        "delete_purches_payment")),
    ("vendor-purchase", lambda p: (
        _has(p, "purtch") or _stem_match(p, "vendor", "purches", "search_products", "editpurchase")
    ) and "quotation" not in p),
    ("quotation", lambda p: "quotat" in p or "quatation" in p),
    ("dpcc-challan", lambda p: "chalan" in p or "chhalan" in p),
    ("proforma", lambda p: "proforma" in p),
    ("hydrant", lambda p: "hydrent" in p or "hydrant" in p),
    ("invoice", lambda p: "invoice" in p),
    ("expenses-made", lambda p: _has(p, "expenc", "expens", "patty_cash", "petty")),
    ("masters", lambda p: _stem_match(p,
        "product", "service", "vat_master", "master_state", "master_city",
        "addindustry", "paymentphase", "addprimaryservice", "importproducts",
        "primaryserviceterms", "expenses_head", "hydrantproduct") or _has(p,
        "productparent", "product_master", "newproduct")),
]


def classify(rel: str) -> str:
    p = rel.replace("\\", "/").lower()
    for name, fn in DOMAIN_RULES:
        if fn(p):
            return name
    return "other"


def existing_doc_for(rel: str, domain: str) -> str:
    low = rel.lower()
    if "visit_planner" in low:
        return "docs/06_Sales_Visit_Planner.md"
    if any(x in low for x in ["daily_rpt", "vw_dailyrpts", "srch_dailyrpts"]):
        return "docs/07_Sales_Visit_Reporting.md"
    if "expense_entry" in low:
        return "docs/08_Expense_Management.md"
    if "attendance" in low:
        return "docs/01_Attendance_Clock.md"
    if any(x in low for x in ["adduser", "viewuser"]):
        return "docs/02_Employee_Admin.md"
    if any(x in low for x in ["manageroles", "managepermissions"]):
        return "docs/03_Role_Permissions.md"
    if "update_designation" in low:
        return "docs/04_Department_Designation.md"
    if any(x in low for x in ["client", "vendor"]):
        return "docs/05_Customer_Vendor.md"
    if "quotat" in low or "quatation" in low:
        return "docs/09_Quotation_Generation.md"
    if any(x in low for x in ["purchaseorder", "view_po", "generate_po"]):
        return "docs/10_Purchase_Order.md"
    if any(x in low for x in ["invoicemail", "proformamail", "paymentmail"]):
        return "docs/11_Communications.md"
    if rel.endswith("app/home.aspx"):
        return "docs/12_Home_Dashboard.md"
    if any(x in low for x in ["seartch_invoice", "view_invoice"]):
        return "docs/13_Invoice_Search_View_PO_Discovery.md"
    if any(x in low for x in ["index.aspx", "reset_password"]):
        return "docs/14_Authentication_Authorization_Architecture.md"
    return ""


def purpose_from(page: dict) -> str:
    title = (page.get("title") or "").strip()
    title = re.sub(r"^Flame-Ex\s*\|\s*", "", title).strip()
    if page["path"].endswith("app/home.aspx"):
        return "Dashboard / My Profile"
    if title and title not in {"Home"}:
        return title
    if page.get("menu_label") and page["menu_label"] not in {"Create", "View", "Search", "Delete"}:
        return page["menu_label"]
    if page.get("menu_label"):
        return page["menu_label"]
    stem = Path(page["path"]).stem
    pretty = re.sub(r"[_\-]+", " ", stem)
    pretty = re.sub(r"(?<=[a-z])(?=[A-Z])", " ", pretty)
    return pretty.strip()


def inventory_pages(menu_map: dict) -> list[dict]:
    pages = []
    for aspx in sorted(APP.rglob("*.aspx")):
        if "packages" in aspx.parts:
            continue
        rel = str(aspx.relative_to(APP)).replace("\\", "/")
        markup = read(aspx)
        attrs = parse_page_directive(markup)
        cs_path = aspx.with_suffix(".aspx.cs")
        cs = read(cs_path) if cs_path.exists() else ""
        base_m = BASE_RE.search(cs)
        cls, base = (base_m.group(1), base_m.group(2)) if base_m else ("", "")
        perms = PERM_RE.findall(cs)
        any_raw = ANY_PERM_RE.search(cs)
        any_perms = []
        if any_raw:
            any_perms = re.findall(r'"([^"]+)"', any_raw.group(1))
        print_auth = AUTH_PRINT_RE.findall(cs)
        fname = aspx.name.lower()
        menu = menu_map.get(fname)
        domain = classify(rel)
        webmethods = WM_RE.findall(cs)
        qs = sorted(set(QS_RE.findall(cs)))
        tables = extract_tables(cs)
        ops = crud_ops(cs)
        pages.append({
            "path": rel,
            "title": attrs.get("Title", ""),
            "master": attrs.get("MasterPageFile", ""),
            "inherits": attrs.get("Inherits", ""),
            "class": cls,
            "base": base,
            "secure_page": base.endswith("SecurePage") or base == "SecurePage",
            "permission_key": perms[0] if perms else "",
            "any_permission_keys": any_perms,
            "print_resource": print_auth[0][0] if print_auth else "",
            "print_qs": print_auth[0][1] if print_auth else "",
            "menu_id": (menu or {}).get("menu_id", ""),
            "menu_extra": (menu or {}).get("extra_ids", []),
            "menu_label": (menu or {}).get("label", ""),
            "querystring": qs,
            "webmethods": webmethods,
            "tables": tables,
            "ops": ops,
            "company_id": bool(COMPANY_RE.search(cs)),
            "session_userid": bool(SESSION_RE.search(cs)),
            "authguard": bool(AUTH_PAGE_RE.search(cs) or perms or any_perms or print_auth),
            "domain": domain,
            "existing_doc": existing_doc_for(rel, domain),
            "has_codebehind": bool(cs),
            "lines_cs": cs.count("\n") + 1 if cs else 0,
        })
    for p in pages:
        p["purpose"] = purpose_from(p)
    return pages


DOMAIN_META = {
    "auth": ("Authentication & session", "Public and keep-alive pages. Shared AuthN lives in docs/14 and docs/22."),
    "card-kiosk": ("ID-card kiosk (isolated)", "Separate `tbl_card_login` app under `/admin` + `card.Master`. Not ERP RBAC. See docs/21."),
    "dashboard": ("Home dashboard", "Landing KPIs. Narrative: docs/12_Home_Dashboard.md."),
    "sales-visit": ("Sales visit lifecycle", "Planner, daily report, manager search, visit expenses. Narrative: docs/06, docs/07, docs/08 + sales-visit-workflow-audit/."),
    "attendance-hr": ("Attendance, leaves, shifts", "Clock-in, leave, shift setup, admin override. Narrative: docs/01."),
    "users-rbac": ("Users, roles, permissions", "Provisioning and RBAC admin. Narrative: docs/02, docs/03, docs/04, docs/16–18."),
    "masters": ("Master data", "Products, services, tax, geography, expense heads, employee config."),
    "customer": ("Customers & factories", "Customer CRUD, SPOC, factory sites. Narrative: docs/05."),
    "vendor-purchase": ("Principles & purchases", "Vendor (Principle) master and purchase-from-vendor. Narrative: docs/05, docs/10."),
    "pr-po": ("PR / PO", "Purchase requisition → purchase order. Narrative: docs/10."),
    "quotation": ("Quotations", "Create/view/search/edit/delete + hydrant quotations. Narrative: docs/09."),
    "dpcc-challan": ("DPCC / challan", "Delivery challan (DPCC) create/view/search/delete + prints."),
    "proforma": ("Proforma invoices", "Create/view/search/delete/mailer + prints."),
    "invoice": ("Tax invoices", "Create/view/search/delete/block/mailer + prints. Discovery: docs/13."),
    "hydrant": ("Hydrant invoices & quotations", "Hydrant-product commercial documents (parallel to tax invoice)."),
    "payments": ("Payments received & purchase payments", "Collections, due lists, mailers, purchase-side payments."),
    "expenses-made": ("Payments made (GL expenses)", "General and petty-cash expense vouchers — not field-visit expenses."),
    "reports": ("Stock & due reports", "Product/service stock and due summaries."),
    "print": ("Print layouts", "Standalone (no Bill.Master). Auth via AuthGuard.EnsurePrint. Shared print gate: docs/16, docs/22."),
    "other": ("Uncategorized / leftover", "Pages that do not match a domain rule. Keep this list empty; if a page lands here, extend DOMAIN_RULES instead of duplicating a catalog."),
}

DOMAIN_ORDER = [
    "auth", "dashboard", "sales-visit", "attendance-hr", "users-rbac", "masters",
    "customer", "vendor-purchase", "pr-po", "quotation", "dpcc-challan",
    "proforma", "invoice", "hydrant", "payments", "expenses-made", "reports",
    "print", "card-kiosk", "other",
]


def md_escape(s: str) -> str:
    return s.replace("|", "\\|").replace("\n", " ")


UNIQUE_NOTES = {
    "index_start.aspx": "Empty launcher. ImageButton1 redirects to missing `index_quotation.aspx`; ImageButton2 → `index_card.aspx`.",
    "index.aspx": "ERP login / OTP / session issue. Canonical write-up: docs/14 + docs/22. Do not restate PBKDF2 or ActiveSessions here.",
    "index_card.aspx": "Isolated kiosk login against `tbl_card_login` (plaintext). Not ERP RBAC. See docs/21 (A-02).",
    "SessionKeepAlive1.aspx": "Keep-alive for the card kiosk session (`tbl_card_login`), not ERP `ActiveSessions`.",
    "reset_password.aspx": "Public token page (`token`, `uid`). Must not inherit Bill.Master. See docs/14 / Phase 0B.",
    "corporate/business/app/QuickAction.aspx": "Unauthenticated email action: QS `t` decrypts to ReqID/Type/Action/ManagerID/CompanyID for leave or regularization approve/reject.",
    "corporate/business/app/home.aspx": "Menu id `home1` label is My Profile; page title is Dashboard. Same file.",
    "corporate/business/app/rpts_vw_qtn_po_counts.aspx": "Stub: empty `Page_Load`, no SQL. Menu still lists it as QTN/PO Added.",
    "corporate/business/app/Edit_quatation.aspx": "Leftover v1 editor. Live menu `Edit_quatation` points at `Edit_quatation_v2.aspx`.",
    "corporate/business/app/Create_quotation.aspx": "Also linked from menu id `Li2` as Create Purchase Order (same page, second entry).",
    "corporate/business/app/expense_entry.aspx": "Child of two menus: `RequiredAnyPermissionKeys` = visit_planner OR vw_dailyrpts.",
    "corporate/business/app/AdminApprovalDashboard.aspx": "HR approvals (leave + regularization), not sales-visit approval. Sales-visit approval is `srch_dailyrpts`.",
    "corporate/business/app/seartch_invoice.aspx": "Filename misspelled `seartch`. Discovery: docs/13.",
    "corporate/business/app/seartch_purtch.aspx": "Filename misspelled `seartch_purtch` (purchase search).",
    "corporate/business/print/NewInvoice_v2.aspx": "CompanyID-aware print variant; live search popups still open `NewInvoice.aspx` / `NewInvoiceDuplicate.aspx`.",
    "corporate/business/print/NewPurchaseOrder.aspx": "Prints from `tbl_Quotation.ID` (quotation-backed PO), not `tbl_PO_Header`.",
    "corporate/business/app/Update/contactno.aspx": "Legacy profile popup (no master). Same pattern as `Update/emailid`, `Update/name`, `Update/password`. Forced lockout uses `settings.aspx`.",
    "corporate/business/app/Update/password.aspx": "Profile popup (no master). Forced-password flow redirects to `settings.aspx`, not these Update/* pages.",
    "corporate/business/app/View_PO_Details.aspx": "PO line/header detail + `sp_ReleasePO_Final`. Opened from `View_PO.aspx` with QS `poId`.",
    "corporate/business/app/settings.aspx": "Lockout landing when `MustUpdateUserId` or `MustVerifyContact` is set (Bill.Master).",
}


def unique_notes_for(pages: list[dict]) -> list[str]:
    notes = []
    for p in pages:
        if p["path"] in UNIQUE_NOTES:
            notes.append(f"- `{p['path']}`: {UNIQUE_NOTES[p['path']]}")
        extras = p.get("menu_extra") or []
        if extras and p["path"] not in UNIQUE_NOTES:
            notes.append(
                f"- `{p['path']}`: additional Bill.Master ids: "
                + ", ".join(f"`{x}`" for x in extras)
            )
        if p.get("print_resource") == "unmapped":
            notes.append(
                f"- `{p['path']}`: `EnsurePrint` resource `unmapped` — **fails closed** (Phase 0A)."
            )
        if not p["has_codebehind"]:
            notes.append(f"- `{p['path']}`: markup only (no `.aspx.cs`).")
        if p["lines_cs"] and p["lines_cs"] < 25 and "stub" not in (UNIQUE_NOTES.get(p["path"]) or "").lower():
            if p["path"] not in UNIQUE_NOTES and not p["tables"] and not p["ops"]:
                notes.append(f"- `{p['path']}`: code-behind is effectively empty ({p['lines_cs']} lines).")
    # dedupe while preserving order
    seen = set()
    out = []
    for n in notes:
        if n in seen:
            continue
        seen.add(n)
        out.append(n)
    return out


def flags(p: dict) -> str:
    bits = []
    master = (p.get("master") or "").lower()
    if p["secure_page"]:
        bits.append("SecurePage")
    elif p["print_resource"]:
        bits.append("EnsurePrint")
    elif "quickaction" in p["path"].lower():
        bits.append("token-link")
    elif p["path"].split("/")[-1].lower() in {
        "index.aspx", "reset_password.aspx", "index_card.aspx", "index_start.aspx",
    }:
        bits.append("public")
    elif "bill.master" in master:
        bits.append("Bill.Master")
    elif "card.master" in master:
        bits.append("card.Master")
    elif p["session_userid"] or p["authguard"]:
        bits.append("Session")
    else:
        bits.append("no-page-gate")
    if p["company_id"]:
        bits.append("CompanyID")
    if p["webmethods"]:
        bits.append("WebMethod")
    return ", ".join(bits)


def perm_cell(p: dict) -> str:
    if p["any_permission_keys"]:
        return "any: " + ", ".join(p["any_permission_keys"])
    if p["permission_key"]:
        return "`" + p["permission_key"] + "`"
    if p["print_resource"]:
        res = p["print_resource"]
        if res == "unmapped":
            return "print **fail-closed** (`unmapped`)"
        return f"print `{res}`"
    if p["menu_id"]:
        return f"menu `{p['menu_id']}`"
    return "—"


def write_domain_file(domain: str, pages: list[dict]) -> str:
    title, blurb = DOMAIN_META[domain]
    lines = [
        f"# {title}",
        "",
        f"> Domain key: `{domain}` · {len(pages)} page(s)",
        "",
        blurb,
        "",
        "**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in "
        "[SHARED_CONTEXT](SHARED_CONTEXT.md).",
        "",
        "| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |",
        "|------|---------|----------------|------|-----------------|-----|-----------|",
    ]
    for p in sorted(pages, key=lambda x: x["path"]):
        master = Path(p["master"]).name if p["master"] else "none"
        cls = p["class"] or "—"
        tables = ", ".join(f"`{t}`" for t in p["tables"][:8]) or "—"
        ops = ", ".join(p["ops"]) or "—"
        narr = p["existing_doc"]
        narr = f"[doc](../{Path(narr).name})" if narr else "this catalog"
        # existing docs are in docs/ not page-catalog; fix relative
        if p["existing_doc"]:
            narr = f"[module](../../{p['existing_doc']})" if False else f"[module](../{Path(p['existing_doc']).name})"
            # sales-visit-workflow-audit is nested — keep simple
            if "/" in Path(p["existing_doc"]).name:
                narr = f"[module](../{p['existing_doc'].replace('docs/', '')})"
            else:
                narr = f"[module](../{Path(p['existing_doc']).name})"
        qs = ""
        if p["querystring"]:
            qs = "<br>QS: " + ", ".join(f"`{q}`" for q in p["querystring"][:6])
        wm = ""
        if p["webmethods"]:
            wm = "<br>WM: " + ", ".join(f"`{w}`" for w in p["webmethods"][:6])
        purpose = md_escape(p["purpose"]) + qs + wm
        lines.append(
            "| `{path}` | {purpose} | {master} / `{cls}` | {gate}<br>{perm} | {tables} | {ops} | {narr} |".format(
                path=p["path"],
                purpose=purpose,
                master=master,
                cls=cls,
                gate=flags(p),
                perm=perm_cell(p),
                tables=tables,
                ops=ops,
                narr=narr,
            )
        )
    notes = unique_notes_for(pages)
    if notes:
        lines += ["", "## Page-unique notes", ""]
        lines.extend(notes)
    lines.append("")
    return "\n".join(lines)


SHARED = r'''# Shared context (read once)

This file is the **only** place page-catalog documents may point for cross-cutting behavior.
Do **not** restate these rules inside domain catalogs or per-page notes.

## Where the canonical text lives

| Topic | Canonical document | What it covers |
|-------|--------------------|----------------|
| Solution overview, stack, repo layout | [README.md](../../README.md) | Product identity, Ponytail standards, setup |
| Ponytail coding rules | [Bill_Software/.cursor/rules/ponytail.md](../../Bill_Software/.cursor/rules/ponytail.md) | No static leaks, parameterized SQL, tenant scope |
| AuthN / AuthZ contract | [22_Security_Baseline.md](../22_Security_Baseline.md) | Engineering contract for every future change |
| AuthN / AuthZ review | [14_Authentication_Authorization_Architecture.md](../14_Authentication_Authorization_Architecture.md) | Defects A-01…A-31, dual role systems |
| Phase history 0B–3 | [15](../15_Phase0B_Secrets_Deployment.md)–[21](../21_Phase3_Infrastructure_Hardening.md) | Implementation notes — not page catalogs |
| Sales-visit deep dive | [sales-visit-workflow-audit/](../sales-visit-workflow-audit/) | State machine, tables, defects D-* |

## Runtime facts used by every ERP page (do not copy)

1. **Master:** `corporate/business/app/Bill.Master` validates `AuthGuard.TryValidateSession`, binds `UserCompanyAccess` companies, renders menu from `Permissions` ∩ `UserRoles` ∩ `RolePermissions`.
2. **Page gate:** pages that inherit `SecurePage` call `AuthGuard.EnsurePage` / `EnsurePageAny` in `OnInit` using `RequiredPermissionKey` (must match the `id` on the menu `<li>` in Bill.Master).
3. **Print gate:** `corporate/business/print/*.aspx` have **no master**. They call `AuthGuard.EnsurePrint(this, resourceKey, queryValue)`. Resource `"unmapped"` **fails closed**.
4. **Tenant:** `CompanyContext.CurrentCompanyID` from `Session["CompanyID"]`. Queries on tenant tables must use `@CompanyID`.
5. **Identity:** `Session["USERID"]` (business key) + `Session["SessionToken"]` (`dbo.ActiveSessions`).
6. **Card kiosk** (`index_card.aspx`, `/admin/*`, `Print/id_card1.aspx`) is a **separate** plaintext `tbl_card_login` app. Do not document it as ERP RBAC.

## What a page entry may contain

Only facts that differ from the rows above:

- Purpose / menu label
- Permission key or print resource
- QueryString / WebMethods unique to the page
- Tables and CRUD verbs **this file** uses
- Links to an existing narrative module doc
- Defects or quirks **unique** to this page (misspelled filename, fail-closed print, no CompanyID, leftover page)

If a sentence would be true for most pages in the domain, put it in the domain blurb (one sentence) or leave it here.
'''

INSTRUCTIONS = r'''# Self-recursive page review instructions

Use this playbook to document **every** `.aspx` page in `Bill_Software/` without duplicating shared architecture.

## Goal

Maintain a complete, non-duplicative catalog:

| Artifact | Role |
|----------|------|
| [SHARED_CONTEXT.md](SHARED_CONTEXT.md) | Cross-cutting facts — **read once per session** |
| [PAGE_INVENTORY.md](PAGE_INVENTORY.md) | Census of all pages + domain + review status |
| `DOMAIN_*.md` | Unique facts per page, grouped by business domain |
| Existing `docs/01`–`docs/22` and `sales-visit-workflow-audit/` | Deep narrative — **link, never copy** |

## Recursion (do this until the queue is empty)

```
REVIEW_NEXT_PAGE:
  1. Open PAGE_INVENTORY.md. If every row is Reviewed, STOP and run CONSISTENCY_PASS.
  2. Take the first row whose status is Pending (or Stale if the .aspx/.cs hash changed).
  3. Load SHARED_CONTEXT.md if not already loaded this session. Do not reload README or
     security docs unless the page is auth, print, or card-kiosk.
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
  7. STOP.
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
- **Existing module docs** remain the narrative. Catalog rows only add file-level facts
  those narratives omitted (class name, permission key, QS, WebMethods).
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

The census can be refreshed by running a workspace extractor against `Bill_Software/**/*.aspx`.
After regeneration, re-run REVIEW_NEXT_PAGE only for rows whose unique facts changed
(permission key, tables, ops, master, SecurePage). Leave human notes intact.
'''

HUB = r'''# Solution documentation map

AminrupERP / Flame-ex (`Bill_Software`) documentation is split so **shared architecture is written once** and **every `.aspx` page is listed once**.

## Start here

| If you need… | Open |
|--------------|------|
| How to review the next undocumented/stale page | [page-catalog/RECURSIVE_INSTRUCTIONS.md](page-catalog/RECURSIVE_INSTRUCTIONS.md) |
| Cross-cutting AuthN, tenancy, master page, print gate | [page-catalog/SHARED_CONTEXT.md](page-catalog/SHARED_CONTEXT.md) |
| Census of all pages | [page-catalog/PAGE_INVENTORY.md](page-catalog/PAGE_INVENTORY.md) |
| Product + Ponytail + setup | [README.md](../README.md) |
| Security contract | [22_Security_Baseline.md](22_Security_Baseline.md) |

## Page catalogs (unique facts only)

<!-- DOMAIN_LINKS -->

## Module narratives (do not duplicate into catalogs)

| Doc | Topic |
|-----|--------|
| [01_Attendance_Clock.md](01_Attendance_Clock.md) | Attendance |
| [02_Employee_Admin.md](02_Employee_Admin.md) | User provisioning |
| [03_Role_Permissions.md](03_Role_Permissions.md) | Roles / permissions |
| [04_Department_Designation.md](04_Department_Designation.md) | Dept / designation |
| [05_Customer_Vendor.md](05_Customer_Vendor.md) | Customer / vendor directory |
| [06_Sales_Visit_Planner.md](06_Sales_Visit_Planner.md) | Visit calendar |
| [07_Sales_Visit_Reporting.md](07_Sales_Visit_Reporting.md) | Daily reports / approval |
| [08_Expense_Management.md](08_Expense_Management.md) | Visit expenses |
| [09_Quotation_Generation.md](09_Quotation_Generation.md) | Quotations |
| [10_Purchase_Order.md](10_Purchase_Order.md) | Purchase orders |
| [11_Communications.md](11_Communications.md) | Email / SMS |
| [12_Home_Dashboard.md](12_Home_Dashboard.md) | Home KPIs |
| [13_Invoice_Search_View_PO_Discovery.md](13_Invoice_Search_View_PO_Discovery.md) | Invoice search/view discovery |
| [sales-visit-workflow-audit/](sales-visit-workflow-audit/) | Visit architecture audit |
| [14](14_Authentication_Authorization_Architecture.md)–[21](21_Phase3_Infrastructure_Hardening.md) | AuthZ phases (history) |
| [22_Security_Baseline.md](22_Security_Baseline.md) | Canonical security contract |

## Invoice lineage (specialist, not page catalogs)

- [invoice_export_data_inventory.md](invoice_export_data_inventory.md)
- [invoice_insert_data_lineage.md](invoice_insert_data_lineage.md)
'''


def write_inventory(pages: list[dict]) -> str:
    lines = [
        "# Page inventory",
        "",
        f"Total `.aspx` pages: **{len(pages)}**. Status is the recursive queue: "
        "work top-to-bottom on `Pending`/`Stale` using "
        "[RECURSIVE_INSTRUCTIONS.md](RECURSIVE_INSTRUCTIONS.md).",
        "",
        "Shared architecture is not recorded here. See [SHARED_CONTEXT.md](SHARED_CONTEXT.md).",
        "",
        "| # | Page | Domain | Menu | Gate | Status |",
        "|---|------|--------|------|------|--------|",
    ]
    for i, p in enumerate(pages, 1):
        menu = p["menu_id"] or "—"
        lines.append(
            f"| {i} | `{p['path']}` | `{p['domain']}` | `{menu}` | {flags(p)} | Reviewed |"
        )
    # counts
    by = defaultdict(int)
    for p in pages:
        by[p["domain"]] += 1
    lines += ["", "## Counts by domain", "", "| Domain | Pages | Catalog |", "|--------|------:|---------|"]
    for d in DOMAIN_ORDER:
        if by[d]:
            lines.append(f"| `{d}` | {by[d]} | [DOMAIN_{d}.md](DOMAIN_{d}.md) |")
    # coverage vs menu
    menu_pages = [p for p in pages if p["menu_id"]]
    unlisted = [p for p in pages if not p["menu_id"] and p["domain"] not in ("print", "auth", "card-kiosk")]
    lines += [
        "",
        f"Pages with a Bill.Master menu id: **{len(menu_pages)}**.",
        "",
        "## ERP app pages not on the primary menu",
        "",
        "These are reachable by redirect, popup, leftover workflow, or secondary links. "
        "They still need a catalog row.",
        "",
    ]
    if unlisted:
        lines.append("| Page | Domain | Notes |")
        lines.append("|------|--------|-------|")
        for p in unlisted:
            note = p["purpose"]
            if p["querystring"]:
                note += " · QS " + ", ".join(p["querystring"][:4])
            lines.append(f"| `{p['path']}` | `{p['domain']}` | {md_escape(note)} |")
    else:
        lines.append("_None._")
    lines.append("")
    return "\n".join(lines)


def main():
    CATALOG.mkdir(parents=True, exist_ok=True)
    master = read(APP / "corporate/business/app/Bill.Master")
    menu_map = parse_menu(master)
    pages = inventory_pages(menu_map)

    OUT_JSON.write_text(json.dumps(pages, indent=2), encoding="utf-8")

    (CATALOG / "SHARED_CONTEXT.md").write_text(SHARED, encoding="utf-8")
    (CATALOG / "RECURSIVE_INSTRUCTIONS.md").write_text(INSTRUCTIONS, encoding="utf-8")
    (CATALOG / "PAGE_INVENTORY.md").write_text(write_inventory(pages), encoding="utf-8")

    by_domain = defaultdict(list)
    for p in pages:
        by_domain[p["domain"]].append(p)

    domain_links = ["| Domain | Pages | File |", "|--------|------:|------|"]
    for d in DOMAIN_ORDER:
        dest = CATALOG / f"DOMAIN_{d}.md"
        if d not in by_domain:
            if dest.exists():
                dest.unlink()
            continue
        title = DOMAIN_META[d][0]
        dest.write_text(
            write_domain_file(d, by_domain[d]), encoding="utf-8"
        )
        domain_links.append(
            f"| {title} | {len(by_domain[d])} | [page-catalog/DOMAIN_{d}.md](page-catalog/DOMAIN_{d}.md) |"
        )

    hub = HUB.replace("<!-- DOMAIN_LINKS -->", "\n".join(domain_links))
    (DOCS / "SOLUTION_INDEX.md").write_text(hub, encoding="utf-8")

    print(f"pages={len(pages)} domains={len(by_domain)} menu_mapped={sum(1 for p in pages if p['menu_id'])}")
    for d in DOMAIN_ORDER:
        if d in by_domain:
            print(f"  {d:20s} {len(by_domain[d]):3d}")


if __name__ == "__main__":
    main()
