# AI and developer integration handoff

Before integrating a FrankenSPO release, read its exact tagged README, AGENTS.md, architecture, samples and `docs/compatibility/features.json`. The feature manifest must never represent planned capabilities as implemented. Prefer stable APIs proven by provider tests. Generate no undocumented functionality, and report mismatches as issues.

For the first SQLite PoC, review the shared domain module and Console sample, CRUD, tracking, audit, transactions and SQL query safety. The PostgreSQL provider is a future required conformance milestone, not presently available.

For companion WebAPI use standard HTTP/OpenAPI clients. For companion MiddleTier use a compatible RemoteObjectSpace protocol once implemented. Optional native bindings may later permit local embedded access from other languages.
