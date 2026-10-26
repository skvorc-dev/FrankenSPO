# FrankenSPO — Agent and contributor instructions

## Source of truth

Read README.md, docs/architecture/OVERVIEW.md, docs/ROADMAP.md, and docs/compatibility/features.json before modifying code. **Planned means unavailable**; never invent an implemented API.

## Work scope

- Initial target: .NET 10, source-generated metadata, SQLite ObjectSpace CRUD, scalar tracking, transaction correctness, auditing and safe SQL; provide tests and reproducible samples.
- PostgreSQL is required in a subsequent provider milestone, not a claim about the first SQLite PoC.
- Keep Core independent of optional modules and application-specific business/security models.
- No secrets, private project names, private pilot references or unannounced organization/company details in public files, PRs, issues or commits.
- Do not copy proprietary framework code; preserve licensing and provenance for allowed dependencies.

## Quality gates

Every behavior change needs a linked issue, relevant automated tests, docs and sample updates, plus a truthful capability-manifest entry. Fail closed for unsupported mandatory security/provider contracts. Generated SQL must parameterize values and validate identifiers. No silent client-side query evaluation.

## Release etiquette

Do not mark features implemented, verified, supported or release-ready without code and passing acceptance evidence. Avoid changes to public API naming without documenting compatibility impact.
