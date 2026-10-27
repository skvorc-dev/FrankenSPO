# Query and security contract

**Status: specified, not implemented.**

## Purpose

States how queries become SQL safely: a typed AST, parameters for all values, metadata-validated identifiers, an explicit
raw SQL boundary, no hidden client-side evaluation, and fail-closed policies. Sources: [OVERVIEW](OVERVIEW.md),
[POC acceptance](../POC_ACCEPTANCE.md), `AGENTS.md`, `SECURITY.md`, issues #3, #5, #6. Today only a conservative
identifier check exists (`SqlIdentifier` in `src/Sferix.SPO.Core/Sql/`); it is a building block, not an allowlist.

## Contract

1. Queries are compiled from a typed query AST, not from string concatenation.
2. Every value (filter operand, key, paging value) reaches SQL as a parameter. User-supplied values are never concatenated or interpolated into SQL text.
3. Identifiers (tables, columns, sort keys) come from validated metadata. An identifier supplied as an input string is first resolved against metadata; a name not in metadata is rejected.
4. `SqlIdentifier` validation (ASCII letter or `_` first, then letters, digits or `_`, at most 63 characters) is necessary but not sufficient for rule 3.
5. The PoC supports an explicit, documented LINQ subset, including projections. An expression outside the subset throws a clear error; it is never approximated.
6. No silent client-side evaluation: if part of a query cannot be translated, the whole query is rejected rather than partly evaluated in memory.
7. Dynamic filtering and sorting accept only metadata-known property names; a malicious sort name is rejected.
8. `LIKE` patterns built from user text escape wildcards so the text matches literally.
9. Raw SQL is an explicit, separately named, documented trust boundary. Its values must still be passable as parameters. It never silently bypasses a declared policy (see rule 10 and open question 3).
10. A declared tenant, row or field restriction is injected into every affected query. If the provider or compiler cannot enforce it, model activation fails; the policy is never silently dropped.
11. Unsupported mandatory mappings and capability gaps (for example exact decimal query operations, complex associations) reject or remain absent; they do not degrade silently.
12. Regression cases that must exist before the feature is claimed (real SQLite): quotes, boolean tautologies, comments, semicolons, malicious sort names, bulk-like payloads, literal LIKE escaping.

## Not in scope

- A full SQL parser or a general-purpose SQL compiler.
- Set-based (bulk) write security: OVERVIEW requires set-based writes to preserve mandatory guarantees; the model is not yet specified.
- Identity and roles (optional `SPO.Security` module) and the form of authorization.

## Open questions

1. Which LINQ operators and types make up the PoC "explicitly supported subset"? The sources name none.
2. How are policies declared (attributes, a metadata API, a module) and what does a policy expression look like? Where does the tenant/actor value come from at runtime?
3. Is raw SQL allowed to bypass declared tenant/row/field policies, or is it rejected on entities that have a policy?
4. What does "model activation" mean in code: building the compiled model, or opening the first ObjectSpace?
5. What is the error contract for unsupported expressions (exception type, message content, whether the SQL text is exposed)?
6. Case sensitivity and collation of string comparison: provider-dependent, or normalised?
7. Should the 63-character `SqlIdentifier` limit stay for SQLite (no such limit there), or is it a deliberate PostgreSQL-compatible lowest common denominator?
