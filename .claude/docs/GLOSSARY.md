# Glossary

| Term | Meaning |
|------|---------|
| **SPO** | The persistence library family name (`Sferix.SPO.*` assemblies). FrankenSPO is the product name. |
| **ObjectSpace** | The unit-of-work session: loads, tracks and saves persistent objects within one context. |
| **UnitOfWork** | Pattern behind ObjectSpace: collect changes, commit them atomically. |
| **Provider** | Database-specific adapter (SQLite, later PostgreSQL) implementing the provider contract. |
| **Capability / capability manifest** | What a provider or build actually supports. At runtime: the provider's `SpoCapability` set; for people: `docs/compatibility/features.json` (D-39). |
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
| **SPOBaseObject** | Simple base class for persistent entities with shared columns (UUIDv7 Id, timestamps, soft-delete flags, version); no inheritance mapping (D-35). |
| **[SpoEntity]** | Attribute on a `partial` class that gives its stable GUID entity ID; the GUID never changes on rename (D-37). |
| **[SpoField]** | Attribute on a persistent property with a positive field number, unique within its entity and never reused (D-37). |
| **ISpoModule / `<Name>Module`** | Interface and naming rule for modules, activated by explicit `AddModule<T>()` registration with no assembly scanning (D-36). |
| **Model activation** | Building the compiled model at startup; all capability and policy checks run then, and a model that fails them is rejected (D-33). |
| **Uncertain commit** | A commit whose outcome is unknown; it raises `SpoCommitUncertainException`, and that ObjectSpace can no longer be used (D-29). |
| **Optimistic concurrency (version column)** | An integer version column incremented on every update; a mismatch raises its own exception and writes nothing (D-31). |
