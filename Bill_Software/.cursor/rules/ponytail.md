# Ponytail Ruleset: Enterprise ASP.NET & SQL

## Core Philosophy
- Write minimalist, dense, and hyper-focused code.
- Minimize token usage; never use 10 lines of code when 3 lines work.
- Prioritize native ASP.NET Web Forms controls over heavy external dependencies.
- Zero bloat: no broad wrapper classes, no placeholder code, no verbose logging.

## ASP.NET Web Forms & C# Constraints
- Target Environment: .NET Framework / Visual Studio 2015 compatibility.
- Zero Static Leaks: NEVER declare static DataTables, collections, or user objects in `.aspx.cs` code-behinds (prevents cross-session IIS data contamination).
- Control & Event Integrity: Preserve existing server control IDs (e.g., DataList1, txtParentProducts), event signatures, and postback lifecycle logic.
- Workflow Preservation: Keep item selection workflows manual in selection grids; do not auto-populate grids with bulk records unless explicitly requested.

## Multi-Tenant Security & Authentication
- Multi-Tenant Isolation: Enforce strict tenant scoping (`AND CompanyID = @CompanyID` or `CompanyContext.CurrentCompanyID`) on all queries, Stored Procedures, and data-access methods.
- Session Security: Enforce `Session["USERID"]` validation checks across page lifecycles.
- Parameterization: Always use parameterized queries or Stored Procedures; never generate inline SQL string concatenations.

## MS-SQL Script Documentation Standard
- When authoring or modifying `.sql` files, prepend this exact header:
  /* ============================================================================
     NAME:        <snake_case_name_under_six_words>
     WHEN:        <YYYY-MM-DD>
     WHY:         <Business or technical justification>
     WHAT:        <Specific table, index, or SP modifications>
     ============================================================================ */

## AI Interaction & Diff Rules
- Unified Diffs Only: Present only the exact lines being changed or added.
- Direct Execution: Output the code directly without conversational preambles ("Here is the code...") or post-explanations unless explicitly asked "Why?".