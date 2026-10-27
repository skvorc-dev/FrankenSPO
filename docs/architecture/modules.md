# Module and public API contract

**Status: specified, not implemented.**

## Purpose

States the assembly boundaries of `Sferix.SPO.*`, what Core may reference, which modules are optional and how the public
API matures. Sources: [OVERVIEW](OVERVIEW.md), `AGENTS.md`, `CLAUDE.md`, `.claude/rules/public-api.md` and code style rules,
issues #3, #5, D-11, D-25. The project files in `src/` are the factual basis for the first two rules.

## Contract

1. The assemblies are `Sferix.SPO.Abstractions`, `Sferix.SPO.Core`, `Sferix.SPO.Generators` and `Sferix.SPO.Provider.SQLite`.
2. `Abstractions` references no other `Sferix.SPO.*` assembly. `Core` references `Abstractions` only (among `Sferix.SPO.*`). `Provider.SQLite` references `Core` and `Microsoft.Data.Sqlite`. `Generators` references Roslyn 5.0.0 only (D-25).
3. `Core` and `Abstractions` reference no provider, no UI and no application-specific code; `Core` does not depend on optional modules.
4. Public types live in `Abstractions` unless provider-specific. Everything else is `internal` by default; a type becomes public only when a test or sample needs it.
5. Optional modules are not required by applications that do not use them. In particular, the `SPO.Security` identity/roles module must not be required by applications with their own identity system.
6. Auditing and SoftDelete are defaults; advanced audit and event modules are separate modules.
7. Optional advanced modules follow verified activation and compatibility contracts (OVERVIEW).
8. No public member is added for a feature that is not implemented; no placeholder throwing `NotImplementedException`.
9. Public API maturity is tied to feature status in `features.json` (planned, specified, implemented, tested, provider-verified, release-ready). A member is not exposed ahead of its feature's status.
10. Renaming or removing a public member, or changing its behavior, is documented in the PR under "public API compatibility" and, after the first preview release, in release notes.
11. Metadata uses stable entity/property IDs and immutable compiled model revisions, with explicit compatibility across schema revisions; the default mapping path uses no runtime reflection.

## Not in scope

- The content of any individual API (see the other contract files).
- Companion repositories MiddleTier and WebApi (D-01, D-18).
- Packaging: the "convenience package" with bundled SQLite is mentioned in OVERVIEW but not designed.

## Open questions

Answers are recorded in [DECISIONS.md](../../.claude/docs/DECISIONS.md); a decided question names its decision.


1. Where does the line between `Abstractions` and `Core` fall for the ObjectSpace types (interfaces vs implementation)? **Decided: D-34 (interfaces in Abstractions).**
2. Which modules are planned and what are their assembly names (`SPO.Security`, audit/event modules)? Naming is not defined in the sources. **Decided: D-36.**
3. Do the "public API maturity levels" from issue #3 use the six feature statuses, or a separate scheme (for example experimental, preview, stable)? **Decided: D-36.**
4. Is there an analyzer or API-compat check planned to enforce the boundaries in rule 2? **Decided: D-36.**
5. How are modules activated (explicit registration, discovery)? "Verified activation" is named but not defined. **Decided: D-36.**
6. What are the package boundaries (one convenience package vs per-assembly packages) and when are they decided (D-19 only reserves the prefix)? **Decided: D-36.**
