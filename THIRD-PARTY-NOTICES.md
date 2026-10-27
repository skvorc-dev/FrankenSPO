# Third-party notices and provenance

No third-party source-code vendoring is declared in this initial documentation-only repository. Before adding dependencies or vendoring permitted source code, verify current licenses, record provenance, preserve required notices and document any changes. Never copy proprietary ORM implementations.

## NuGet packages

Licences were read from the `.nuspec` of each package on nuget.org. Versions are the ones in the project files;
the `SQLitePCLRaw.*` packages are transitive through `Microsoft.Data.Sqlite` and are listed with the version
restored into the build.

| Package | Version | Licence (SPDX) | Use |
|---------|---------|----------------|-----|
| `Microsoft.Data.Sqlite` | 10.0.14 | MIT | runtime (SQLite provider) |
| `SQLitePCLRaw.bundle_e_sqlite3` | 2.1.13 | Apache-2.0 | runtime (transitive, SQLite native bundle) |
| `SQLitePCLRaw.core` | 2.1.13 | Apache-2.0 | runtime (transitive, SQLite native bundle) |
| `SQLitePCLRaw.lib.e_sqlite3` | 2.1.13 | Apache-2.0 | runtime (transitive, SQLite native bundle) |
| `SQLitePCLRaw.provider.e_sqlite3` | 2.1.13 | Apache-2.0 | runtime (transitive, SQLite native bundle) |
| `Microsoft.CodeAnalysis.CSharp` | 5.0.0 | MIT | build only (source generator, `PrivateAssets="all"`) |
| `Microsoft.NET.Test.Sdk` | 18.11.0 | MIT | build/test only |
| `xunit` | 2.9.3 | Apache-2.0 | build/test only |
| `xunit.runner.visualstudio` | 4.0.1 | Apache-2.0 | build/test only |
