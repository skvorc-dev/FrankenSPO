> **Historical:** completed during P0; current work is tracked in the roadmap and issues.

# Task 0001 — .NET foundation

**Status:** source files committed; CI verification required. No ORM feature is implemented.

The initial solution contains Abstractions, Core, a generator project placeholder, SQLite connection adapter and two xUnit projects.

The SQLite factory only opens connections. It does not initialize schema, map entities, implement ObjectSpace, track changes, enforce policies or generate safe query ASTs. The identifier validator is a conservative building block and must not be used as a substitute for metadata-based SQL identifier allowlisting.

## Build

```bash
dotnet restore FrankenSPO.slnx
dotnet build FrankenSPO.slnx -c Release --no-restore
dotnet test FrankenSPO.slnx -c Release --no-build
```

The tracked CI pipeline runs the solution on Linux, Windows and macOS. Tests currently exercise identifier primitives and real in-memory SQLite connectivity, **not** an ORM vertical slice.

## Next

Source-generator model declaration contracts, stable metadata IDs, and a real transaction-bound ObjectSpace SQLite CRUD test. Update feature statuses only when verified.
