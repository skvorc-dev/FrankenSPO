# Glossary

| Term | Meaning |
|------|---------|
| **SPO** | The persistence library family name (`Sferix.SPO.*` assemblies). FrankenSPO is the product name. |
| **ObjectSpace** | The unit-of-work session: loads, tracks and saves persistent objects within one context. |
| **UnitOfWork** | Pattern behind ObjectSpace: collect changes, commit them atomically. |
| **Provider** | Database-specific adapter (SQLite, later PostgreSQL) implementing the provider contract. |
| **Capability / capability manifest** | What a provider or build actually supports; recorded in `docs/compatibility/features.json`. |
| **Fail closed** | Refuse to run when a mandatory guarantee cannot be enforced, instead of silently degrading. |
| **Baselines (working / flushed / committed)** | The three states of tracked values: in memory, written in the open transaction, durably committed. |
| **SoftDelete** | Deleting by marking a record as deleted instead of removing the row. |
| **Source-generated metadata** | Mapping information produced at build time by the Roslyn generator, not by runtime reflection. |
| **PoC** | The P1 SQLite proof of concept defined in `docs/POC_ACCEPTANCE.md`. |
| **MiddleTier / WebApi** | Separate companion repositories (remote ObjectSpace protocol; HTTP endpoint generation). Out of scope here. |
| **Owner** | Andrej Skvorc; decides product, money, legal, licence, visibility and accounts. |
| **Main agent** | The Claude Code session in the project's main folder; plans, assigns, reviews, merges. |
| **Worker agent** | Any other agent session; works only on its assigned card under `GUARDRAILS.md`. |
| **Card** | One task in an epic file: ID, goal, acceptance criteria, size, status. |
| **Epic** | A group of cards for one phase or theme, one file in `.claude/plan/`. |
| **Gate** | The evidence a roadmap phase needs before it counts as done. |
