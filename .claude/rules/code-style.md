# Code style

Match the code around you. Current conventions (see `src/Sferix.SPO.Core/Sql/SqlIdentifier.cs`):

- File-scoped namespaces equal to the folder path: `Sferix.SPO.<Project>.<Folder>`.
- One public type per file, file named after the type.
- `sealed` classes and `readonly record struct` value types by default; open types only by design.
- Nullable reference types on; no `!` suppression without a comment saying why it is safe.
- Guard clauses with the BCL helpers (`ArgumentException.ThrowIfNullOrWhiteSpace`, `ArgumentNullException.ThrowIfNull`).
- XML doc `<summary>` on every public type and member; `<remarks>` for limits and what it does **not** do.
- Comments explain why, not what. Keep them sparse.
- No warnings: `TreatWarningsAsErrors` is on for the whole solution. Fix the cause; suppress only with a
  justification in the same line or in `DECISIONS.md`.
- `Core` and `Abstractions` reference no provider, no UI and no application-specific code.
- No reflection on the default mapping path; metadata comes from the source generator.
- No `async void`, no blocking on async code (`.Result`, `.Wait()`) in library code.
