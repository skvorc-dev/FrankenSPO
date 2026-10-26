# FrankenSPO

**FrankenSPO by Sferix** is an early-stage, modular, general-purpose object-relational mapping (ORM) project for .NET.

> **Status: planning and initial development.** No working ORM package or production compatibility is claimed yet.

## Intended design

- C# / .NET 10; generated metadata with a Native AOT-oriented design.
- Provider-independent persistence core, starting with SQLite and adding PostgreSQL.
- ObjectSpace, change tracking, queries, validation, transactions, audit fields and SoftDelete.
- Optional features and providers via separate packages; no required application-specific identity system.
- Safe SQL generation with bound values and metadata-validated SQL identifiers.

## Getting started

Implementation instructions will appear here as working, tested packages and examples become available. See [roadmap](docs/ROADMAP.md), [architecture](docs/architecture/OVERVIEW.md) and [capability manifest](docs/compatibility/features.json).

## Future companion projects

- [FrankenSPO.MiddleTier](https://github.com/skvorc-dev/FrankenSPO.MiddleTier) — planned remote ObjectSpace infrastructure.
- [FrankenSPO.WebApi](https://github.com/skvorc-dev/FrankenSPO.WebApi) — planned model-driven ASP.NET Core API server.

## Contributing and security

Please see [CONTRIBUTING.md](CONTRIBUTING.md) and [SECURITY.md](SECURITY.md). Licensed under [MIT](LICENSE).
