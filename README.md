<p align="center"><img src="branding/banner.svg" alt="FrankenSPO — Persistence, Reimagined" width="100%"/></p>

# FrankenSPO

**FrankenSPO by Sferix** is an early-stage, MIT-licensed, modular, general-purpose .NET object-relational mapping project.

> **Early foundation:** .NET 10 project scaffolding and infrastructure tests exist; this repository still does not provide a functional ORM runtime or production-ready package. See [verified feature declarations](docs/compatibility/features.json).

## Planned architecture

C# / .NET 10, source-generated metadata, provider-neutral ObjectSpace/UnitOfWork, SQLite first, PostgreSQL next; secure query compilation with bound parameters, tracking, transaction semantics, SoftDelete and basic audit. Optional providers and extensions remain separate. Applications may supply their own security and identity systems.

## Development

Read [architecture](docs/architecture/OVERVIEW.md), [roadmap](docs/ROADMAP.md), [developer workflow](docs/DEVELOPMENT_WORKFLOW.md), [agent guidance](AGENTS.md) and [current issues](https://github.com/skvorc-dev/FrankenSPO/issues). CI builds and tests the committed .NET 10 solution. See [foundation task](docs/FOUNDATION_0001.md) for current scope and limitations.

## Companion projects

- [FrankenSPO.MiddleTier](https://github.com/skvorc-dev/FrankenSPO.MiddleTier) — planned remote ObjectSpace protocol and client/server integration.
- [FrankenSPO.WebApi](https://github.com/skvorc-dev/FrankenSPO.WebApi) — planned ASP.NET Core model-driven endpoint generation and OpenAPI/OData integration.

## Public contributions

See [CONTRIBUTING.md](CONTRIBUTING.md), [SECURITY.md](SECURITY.md), [MIT License](LICENSE) and [third-party notices](THIRD-PARTY-NOTICES.md). Nothing here implies a registered company or any unpublished customer/application integration.
