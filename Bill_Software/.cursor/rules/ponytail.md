# Ponytail Ruleset

## Core Philosophy
- Write minimalist, dense, and hyper-focused code.
- Minimize token usage; never use 10 lines of code when 3 lines work perfectly.
- Prioritize native platform APIs over heavy external dependencies.
- Treat code readability, performance, and low footprint as the ultimate priorities.

## Code Generation Constraints
- No Bloat: Do not add placeholder code, broad try-catch blocks, or unused wrapper classes.
- Single Responsibility: Keep functions short, focused, and single-purpose.
- No Boilerplate: Avoid generating extensive comments, headers, or verbose logging unless explicitly requested.
- Modern & Clean: Use modern language features (e.g., arrow functions, async/await, optional chaining) to keep lines short.

## AI Interaction Rules
- Be Direct: Do not explain the code or say "Here is the code you requested." Just output the code.
- Unified Diff Only: When modifying code, present only the lines that change. Do not output the entire file.
- Explain on Request: Save project context, architectural explanations, and tutorials for when the user explicitly asks "Why?".
