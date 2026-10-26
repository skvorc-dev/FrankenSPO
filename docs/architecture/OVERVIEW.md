# FrankenSPO architecture overview — design baseline

**Status: specified, not implemented.** This is a summary of approved direction, not a complete API or design specification.

FrankenSPO is an independent, platform-neutral .NET ORM with a source-generated metadata pipeline, an ObjectSpace/UnitOfWork persistence model and provider-neutral query compilation. SQLite is bundled in the intended convenience package; PostgreSQL and further providers are optional dependencies. The optional SPO.Security identity/roles module must not be required by applications that already have their own identity system.

## Core contracts

- Stable entity/property metadata IDs; immutable compiled model revisions; explicit compatibility across schema revisions.
- ObjectSpace identity mapping, scalar tracking, explicit partial-load semantics, transaction-aware working/flushed/committed baselines.
- Typed query AST; mandatory tenant/row/field policy injection when declared; SQL parameters for all values and metadata-validated identifiers.
- Atomic SaveChanges where supported; fail closed on mandatory capability deficits; clear uncertain-commit reporting.
- Auditing and SoftDelete defaults; separate advanced audit/event modules; post-commit callbacks are not durable delivery.
- Provider conformance against real SQLite and PostgreSQL; optional advanced modules follow verified activation and compatibility contracts.
- Explicit entity-aware and set-based bulk models; set-based writes preserve mandatory security/integrity guarantees and invalidate affected tracked/cache state.

## Development boundary

The first PoC proves the smallest correct SQLite vertical slice. The comprehensive feature roadmap is not the PoC's public capability list. Precise unresolved engineering contracts must be documented and tested before their individual features are exposed. See [feature manifest](../compatibility/features.json).
