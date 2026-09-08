#!/usr/bin/env python3
"""Refresh UAT_CATALOG.md from flamex_uat (columns + SP signatures). No SP bodies. No secrets."""
from __future__ import annotations

import os
import re
import sys
from collections import defaultdict
from pathlib import Path

try:
    import pymssql
except ImportError:
    sys.exit("pymssql is required")

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
APP = ROOT / "Bill_Software"
OUT = HERE / "UAT_CATALOG.md"

SP_NAME_RE = re.compile(
    r"\b(sp_[A-Za-z0-9_]+|usp_[A-Za-z0-9_]+|"
    r"InsertOrGetProduct|Get_Quotation_Details_With_Counts|"
    r"CompareTablesAndGenerateAlter|CompareTablesFull)\b"
)

SP_PURPOSE = {
    "sp_RunAttendanceRulesEngine": "Recalc attendance after punch / shift map.",
    "sp_AllocateEmployeeLeaves": "Initial leave balances on new ERP user.",
    "sp_GetActiveNotifications": "Toast feed (GlobalNotification OnInit currently throws).",
    "sp_MarkNotificationRead": "Dismiss one notification.",
    "sp_Requisition_CreateDraft": "Modern PR draft header.",
    "sp_SubmitRequisition": "Submit PR.",
    "sp_RequisitionItem_BulkUpsert": "PR line upsert (TVP).",
    "sp_RequisitionItem_Upsert": "PR single-line upsert (UAT; C# uses bulk).",
    "sp_CancelRequisition": "Cancel PR.",
    "sp_Requisition_Approve": "Approve submitted PR (C# path).",
    "sp_ApproveRequisition": "Alternate PR approve (UAT; not called from C#).",
    "sp_RejectRequisition": "Reject PR (UAT; C# uses details-page path).",
    "sp_GeneratePO_FromReqNo": "Vendor PO from approved PR.",
    "sp_ReleasePO_Final": "Final-release vendor PO.",
    "sp_ReleasePO": "Alternate PO release (UAT; app uses sp_ReleasePO_Final).",
    "sp_GetReleasedPO_Details": "Vendor PO print payload.",
    "sp_getapplock": "SQL applock around quote/client-PO save (system proc).",
    "sp_SearchProductsFast": "Stock page typeahead.",
    "sp_GetProductStockByStore": "Stock by store.",
    "sp_GetProductCategories": "Category list.",
    "sp_CreatePO_Amendment": "PO amendment header (UAT; no C# caller).",
    "sp_AddPO_AmendmentItem": "PO amendment line (UAT; no C# caller).",
    "sp_ApprovePO_Amendment": "Approve PO amendment (UAT; no C# caller).",
    "sp_RequestPOCancellation": "Request PO cancel (UAT; no C# caller).",
    "sp_ApprovePOCancellation": "Approve PO cancel (UAT; no C# caller).",
    "sp_RequestPOItemCancellation": "Request PO line cancel (UAT; no C# caller).",
    "sp_ApprovePOItemCancellation": "Approve PO line cancel (UAT; no C# caller).",
    "sp_CancelPO_Draft": "Cancel draft PO (UAT; no C# caller).",
    "sp_CancelPOItem_Draft": "Cancel draft PO line (UAT; no C# caller).",
    "sp_SearchStockGrouped": "Grouped stock search (UAT; no C# caller).",
    "InsertOrGetProduct": "Insert or reuse product (UAT; no C# caller).",
    "Get_Quotation_Details_With_Counts": "Quotation details + counts (UAT; no C# caller).",
    "sp_ManagePayrollStatus": "Payroll status (UAT; no C# caller).",
    "sp_WriteProcessLog": "Process log write (UAT; no C# caller).",
    "usp_CreateMissingMastersFromStock": "Stock→master backfill (UAT; no C# caller).",
    "usp_Reconcile_MasterToStock": "Master vs stock reconcile (UAT; no C# caller).",
    "usp_UpdateZeroMastersFromStock": "Zero-master update from stock (UAT; no C# caller).",
    "CompareTablesAndGenerateAlter": "DDL compare helper (UAT; no C# caller).",
    "CompareTablesFull": "Table compare helper (UAT; no C# caller).",
    "sp_GetProductCategories": "Product category list.",
}


def connect():
    host = os.environ.get("UAT_HOST")
    user = os.environ.get("UAT_USER")
    password = os.environ.get("UAT_PASS")
    db = os.environ.get("UAT_DB", "flamex_uat")
    if not host or not user or not password:
        sys.exit("Set UAT_HOST, UAT_USER, UAT_PASS (and optional UAT_DB). Do not put secrets in git.")
    return pymssql.connect(
        server=host, user=user, password=password, database=db, login_timeout=20
    )


def q(cur, sql, params=None):
    cur.execute(sql, params or ())
    return cur.fetchall()


def sql_type(ty: str, max_len, prec, scale, is_identity: bool) -> str:
    t = ty.lower()
    if t in {"nvarchar", "nchar", "varchar", "char", "binary", "varbinary"}:
        if max_len == -1:
            inner = "max"
        elif t in {"nvarchar", "nchar"}:
            inner = str(max_len // 2)
        else:
            inner = str(max_len)
        out = f"{ty}({inner})"
    elif t in {"decimal", "numeric"}:
        out = f"{ty}({prec},{scale})"
    elif t in {"time", "datetime2", "datetimeoffset"}:
        out = f"{ty}({scale})"
    elif t == "float" and prec:
        out = f"{ty}({prec})"
    else:
        out = ty
    if is_identity:
        out += " IDENTITY"
    return out


def scan_sp_callers() -> dict[str, list[str]]:
    found: dict[str, set[str]] = defaultdict(set)
    for src in APP.rglob("*.cs"):
        if "packages" in src.parts or src.name.endswith(".designer.cs"):
            continue
        text = src.read_text(encoding="utf-8", errors="replace")
        rel = str(src.relative_to(APP)).replace("\\", "/")
        if rel.endswith(".aspx.cs"):
            rel = rel[:-3]
        elif rel.endswith(".ashx.cs") or rel.endswith(".ascx.cs"):
            rel = rel[:-3]
        for m in SP_NAME_RE.finditer(text):
            found[m.group(1)].add(rel)
    return {k: sorted(v, key=str.lower) for k, v in found.items()}


def md_escape(s: str) -> str:
    return (s or "").replace("|", "\\|").replace("\n", " ").strip()


def main() -> None:
    callers = scan_sp_callers()
    cn = connect()
    cur = cn.cursor()

    server = q(cur, "SELECT @@SERVERNAME")[0][0]
    ver = q(cur, "SELECT CAST(SERVERPROPERTY('ProductVersion') AS varchar(32))")[0][0]
    db = q(cur, "SELECT DB_NAME()")[0][0]

    tables = q(
        cur,
        """
        SELECT SCHEMA_NAME(t.schema_id), t.name, t.object_id
        FROM sys.tables t
        WHERE t.is_ms_shipped = 0
        ORDER BY 1, 2
        """,
    )
    views = q(
        cur,
        """
        SELECT SCHEMA_NAME(v.schema_id), v.name, v.object_id
        FROM sys.views v
        WHERE v.is_ms_shipped = 0
        ORDER BY 1, 2
        """,
    )
    procs = q(
        cur,
        """
        SELECT SCHEMA_NAME(p.schema_id), p.name, p.object_id
        FROM sys.procedures p
        WHERE p.is_ms_shipped = 0
        ORDER BY 1, 2
        """,
    )
    types = q(
        cur,
        """
        SELECT SCHEMA_NAME(tt.schema_id), tt.name, tt.type_table_object_id
        FROM sys.table_types tt
        ORDER BY 1, 2
        """,
    )

    def columns_for(object_id: int):
        return q(
            cur,
            """
            SELECT c.column_id, c.name, ty.name, c.max_length, c.precision, c.scale,
                   c.is_nullable, c.is_identity, dc.definition
            FROM sys.columns c
            JOIN sys.types ty ON ty.user_type_id = c.user_type_id
            LEFT JOIN sys.default_constraints dc
              ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
            WHERE c.object_id = %s
            ORDER BY c.column_id
            """,
            (object_id,),
        )

    def keys_for(object_id: int):
        rows = q(
            cur,
            """
            SELECT i.name, i.is_primary_key, i.is_unique, i.has_filter, i.filter_definition,
                   STUFF((
                     SELECT ',' + c.name
                     FROM sys.index_columns ic
                     JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                     WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id
                       AND ic.is_included_column = 0
                     ORDER BY ic.key_ordinal
                     FOR XML PATH('')
                   ), 1, 1, '')
            FROM sys.indexes i
            WHERE i.object_id = %s AND i.index_id > 0 AND (i.is_primary_key = 1 OR i.is_unique = 1)
            ORDER BY i.is_primary_key DESC, i.name
            """,
            (object_id,),
        )
        return rows

    def params_for(object_id: int):
        return q(
            cur,
            """
            SELECT pr.parameter_id, pr.name, ty.name, pr.max_length, pr.precision, pr.scale,
                   pr.is_output
            FROM sys.parameters pr
            JOIN sys.types ty ON ty.user_type_id = pr.user_type_id
            WHERE pr.object_id = %s
            ORDER BY pr.parameter_id
            """,
            (object_id,),
        )

    def write_cols(lines: list[str], cols) -> None:
        lines += ["| # | Column | Type | Null | Default |", "|--:|--------|------|------|---------|"]
        for col_id, name, ty, max_len, prec, scale, nullable, ident, default in cols:
            nulls = "NULL" if nullable else "NOT NULL"
            dflt = md_escape(default) if default else ""
            lines.append(
                f"| {col_id} | `{name}` | `{sql_type(ty, max_len, prec, scale, bool(ident))}` | {nulls} | {dflt} |"
            )
        lines.append("")

    def write_keys(lines: list[str], keys) -> None:
        if not keys:
            lines.append("_No primary or unique key._")
            lines.append("")
            return
        bits = []
        for name, is_pk, is_uq, has_filter, filt, cols in keys:
            kind = "PK" if is_pk else "UNIQUE"
            extra = f" WHERE {filt}" if has_filter and filt else ""
            bits.append(f"- **{kind}** `{name}` ({cols or '—'}){extra}")
        lines.extend(bits)
        lines.append("")

    lines = [
        "# UAT object catalog",
        "",
        f"Live **`{db}`** snapshot. Server `{server}`, SQL Server `{ver}`.",
        "Generated by [`_catalog.py`](_catalog.py). **Do not hand-edit. Do not paste secrets.**",
        "**No stored-procedure bodies** — names, parameters, and C# callers only.",
        "",
        f"**{len(tables)}** tables, **{len(views)}** views, **{len(procs)}** user procedures, "
        f"**{len(types)}** table type(s).",
        "Facts / gaps / FKs: [`SHARED_SCHEMA.md`](SHARED_SCHEMA.md). "
        "Page verbs: [`DATA_DICTIONARY.md`](DATA_DICTIONARY.md).",
        "",
        "## Contents",
        "",
        "- [Tables](#tables)",
        "- [Views](#views)",
        "- [Table types](#table-types)",
        "- [Stored procedures](#stored-procedures)",
        "",
        "## Tables",
        "",
        "| # | Table | Columns | Keys |",
        "|--:|-------|--------:|------|",
    ]

    table_blocks: list[str] = []
    for i, (schema, name, oid) in enumerate(tables, 1):
        cols = columns_for(oid)
        keys = keys_for(oid)
        key_short = ", ".join(
            ("PK" if r[1] else "UQ") + " " + (r[5] or r[0] or "") for r in keys
        ) or "—"
        fq = f"{schema}.{name}"
        lines.append(f"| {i} | [`{fq}`](#{fq.replace('.', '').replace('$', '').lower()}) | {len(cols)} | {md_escape(key_short)} |")
        block = [f"### `{fq}`", ""]
        write_keys(block, keys)
        write_cols(block, cols)
        table_blocks.extend(block)

    lines += ["", *table_blocks]

    lines += ["## Views", ""]
    for schema, name, oid in views:
        fq = f"{schema}.{name}"
        lines += [f"### `{fq}`", ""]
        write_cols(lines, columns_for(oid))

    lines += ["## Table types", ""]
    for schema, name, oid in types:
        fq = f"{schema}.{name}"
        lines += [f"### `{fq}`", "", "Used by `sp_RequisitionItem_BulkUpsert` `@Items`.", ""]
        write_cols(lines, columns_for(oid))

    lines += [
        "## Stored procedures",
        "",
        "User procedures on UAT, plus system `sp_getapplock` (called from C#, not in `sys.procedures` here).",
        "Callers are `.cs` files that mention the name. **Bodies are not listed.**",
        "",
        "| Procedure | Parameters | C# callers | Purpose |",
        "|-----------|------------|------------|---------|",
    ]

    proc_blocks: list[str] = []
    proc_names = [(schema, name, oid) for schema, name, oid in procs]
    # system applock
    extra = [("sys", "sp_getapplock", None)]

    def sig_from_params(params) -> str:
        if not params:
            return "—"
        parts = []
        for _pid, pname, ty, max_len, prec, scale, is_out in params:
            t = sql_type(ty, max_len, prec, scale, False)
            d = " OUTPUT" if is_out else ""
            parts.append(f"{pname} {t}{d}".strip())
        return ", ".join(parts)

    for schema, name, oid in proc_names + extra:
        params = params_for(oid) if oid else [
            (1, "@Resource", "nvarchar", 510, 0, 0, False),
            (2, "@LockMode", "nvarchar", 64, 0, 0, False),
            (3, "@LockOwner", "nvarchar", 64, 0, 0, False),
            (4, "@LockTimeout", "int", 4, 10, 0, False),
        ]
        # simplify applock params if we queried none - keep placeholder short
        if name == "sp_getapplock" and oid is None:
            sig = "@Resource, @LockMode, … (system)"
        else:
            sig = sig_from_params(params)
        cs = callers.get(name, [])
        caller_txt = ", ".join(f"`{c}`" for c in cs) if cs else "_none_"
        purpose = SP_PURPOSE.get(name, "—")
        fq = f"{schema}.{name}"
        lines.append(
            f"| `{fq}` | {md_escape(sig)[:180]}{'…' if len(sig) > 180 else ''} | {caller_txt} | {purpose} |"
        )
        if oid is None:
            continue
        proc_blocks += [f"### `{fq}`", ""]
        if cs:
            proc_blocks.append("C# callers: " + ", ".join(f"`{c}`" for c in cs) + ".")
        else:
            proc_blocks.append("C# callers: **none** (UAT-only or unused from this tree).")
        proc_blocks.append("")
        proc_blocks.append(purpose)
        proc_blocks.append("")
        if params:
            proc_blocks += [
                "| Param | Type | Direction |",
                "|-------|------|-----------|",
            ]
            for _pid, pname, ty, max_len, prec, scale, is_out in params:
                direc = "OUTPUT" if is_out else "IN"
                label = pname or "(return)"
                proc_blocks.append(
                    f"| `{label}` | `{sql_type(ty, max_len, prec, scale, False)}` | {direc} |"
                )
            proc_blocks.append("")
        else:
            proc_blocks.append("_No parameters._")
            proc_blocks.append("")

    lines += ["", *proc_blocks]
    lines.append("Refresh: `UAT_HOST` / `UAT_USER` / `UAT_PASS` / `UAT_DB` then `python3 docs/page-catalog/_catalog.py`.")
    lines.append("")

    cn.close()
    OUT.write_text("\n".join(lines), encoding="utf-8")
    print(f"wrote {OUT} tables={len(tables)} views={len(views)} procs={len(procs)} types={len(types)} lines={len(lines)}")


if __name__ == "__main__":
    main()
