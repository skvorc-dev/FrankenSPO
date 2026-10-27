# Testing

- Framework: xUnit. Unit tests in `tests/Sferix.SPO.Tests.Unit`, provider tests in `tests/Sferix.SPO.Tests.<Provider>`.
- Test classes `public sealed class <Subject>Tests`; test names state the behavior:
  `UserInputRemainsDataWhenParameterized`, not `Test1`.
- Provider behavior is tested against a **real** database (SQLite in-memory or file); no mocks of the database.
- Every bug fix comes with a test that fails without the fix.
- Every SQL-producing feature needs injection regression cases (quotes, tautologies, comments, semicolons,
  malicious sort names, LIKE escaping); see `docs/POC_ACCEPTANCE.md`.
- Transactions: test commit, rollback, and the tracking state after a failure.
- Tests must be deterministic and independent: no shared files, clocks or ordering between tests.
- A feature moves past `planned` in `features.json` only with tests that prove it.
- Before a PR: `dotnet build FrankenSPO.slnx -c Release` and `dotnet test FrankenSPO.slnx -c Release --no-build`
  pass locally; CI must pass on Linux, Windows and macOS.
