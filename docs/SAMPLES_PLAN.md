# Cross-platform and cross-language sample strategy

Use a small generic Sales & Inventory example domain. Define business objects and domain logic once in `SpoExample.Module`; keep database and UI configuration in hosts. Shared module convention is optional for minimal consumers.

## Progressive sample families

1. Core: `SpoExample.Console`, local SQLite and PostgreSQL worker as providers become available.
2. Desktop .NET: WinForms, Uno XAML and Uno C# Markup with shared ViewModels, Avalonia; supported desktop targets verified in CI.
3. Web/server .NET: Minimal API and server-hosted Blazor with direct SPO, not requiring the separate WebAPI framework.
4. MiddleTier: `SpoExample.Server` and remote ObjectSpace clients after its readiness gate; optionally Rust/native SDK for Swift, C++/Qt and Python.
5. WebAPI: `SpoExample.WebApi` plus language-neutral HTTP examples for Swift, Rust, Qt C++/Python, TypeScript and others once endpoints exist.
6. Optional Native: Native AOT C exports, stable C ABI and safe Rust wrappers for embedded local persistence; no duplicate ORM/SQL engine.

Every sample documents runtime requirements, exact tagged FrankenSPO package, providers, authentication trust boundaries, supported features, and tested platforms. Never generate unbuildable placeholder sample projects.
