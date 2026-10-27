# Security

See also `SECURITY.md` (public policy) and the security section of `docs/POC_ACCEPTANCE.md`.

- **SQL values are always parameters.** Never concatenate or interpolate user-supplied values into SQL.
- **Identifiers** (tables, columns, sort keys) come from validated metadata, never from input strings directly;
  `SqlIdentifier` is a building block, not an allowlist.
- Raw SQL is an explicit, documented trust boundary in the API.
- Fail closed: a declared tenant/row/field restriction or a mandatory capability the provider cannot enforce
  is an error at model activation, never a silent no-op.
- No silent client-side query evaluation.
- No secrets in code, tests, samples, logs, issues or commits; test connection strings use `:memory:` or temp files.
- Dependencies: only with a reason, recorded in `THIRD-PARTY-NOTICES.md`; known vulnerabilities fail the build
  (NU1903 under `TreatWarningsAsErrors`) and are fixed, not suppressed.
- Never copy proprietary ORM source code.
- Vulnerabilities are reported privately (GitHub private vulnerability reporting), never in public issues.
