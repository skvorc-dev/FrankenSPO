# Public API

FrankenSPO is a library, so its public surface is a contract.

- Public types live in `Sferix.SPO.Abstractions` unless they are provider-specific.
- Keep the surface small: `internal` by default; make something public only when a test or sample needs it.
- Never expose an API for a feature that is not implemented. No placeholder members that throw
  `NotImplementedException` in public types.
- Renaming or removing a public member, or changing its behavior, needs a note in the PR under
  "public API compatibility" and, after the first preview release, a release-notes entry.
- Names follow .NET design guidelines: PascalCase, no abbreviations except well-known ones (SQL, ID as `Id`).
- Async APIs end in `Async`, take a `CancellationToken` last, and return `Task`/`ValueTask`.
