# E0 · Foundation (phase P0)

Milestone **P0 — Foundation**. Gate: clean baseline, green CI on three OSes, public-safe docs, self-organising repo.

### F-01 · Repository policies and contributor guidance
- Issue: #2 · Size: S · Status: Done (#2 closed, D-15)
- Goal: public-safe README, LICENSE, CONTRIBUTING, SECURITY, CODE_OF_CONDUCT, THIRD-PARTY-NOTICES, AGENTS.md.
- Acceptance:
  - [x] All files exist and are checked by `scripts/validate_repository.py`
  - [x] Owner confirms LICENSE copyright holder and that no text reveals private details
- Files: root `*.md`, `LICENSE`

### F-02 · Architecture contracts and capability statuses
- Issue: #3 · Size: M · Status: Ready
- Goal: architecture docs cover ObjectSpace, three baselines, secured query AST, provider capability checks,
  module boundaries; the manifest distinguishes specified / implemented / tested / provider-verified.
- Acceptance:
  - [ ] `docs/architecture/` has one short section per contract named in issue #3, each marked `specified`
  - [ ] `features.json` allows `specified`; `scripts/validate_repository.py` accepts it; CI green
  - [ ] README, AGENTS.md and the manifest agree that no ORM feature is implemented
- Files: `docs/architecture/**`, `docs/compatibility/features.json`, `scripts/validate_repository.py`
- Notes: docs only, no code. D-13.

### F-03 · CI green again (NU1903)
- Issue: #4 · Size: S · Status: Done (PR #14)
- Goal: `main` builds again after `SQLitePCLRaw.lib.e_sqlite3 2.1.11` was flagged vulnerable.
- Acceptance:
  - [x] Microsoft.Data.Sqlite 10.0.14; CI green on Linux, Windows, macOS

### F-04 · CI security gates
- Issue: #4 · Size: S · Status: Ready
- Goal: dependency problems are caught in PRs, not on `main`.
- Acceptance:
  - [ ] `.github/workflows/dependency-review.yml` runs `actions/dependency-review-action` on PRs and fails on high severity
  - [x] CodeQL for C# is enabled (default setup, D-16)
  - [ ] CI green
- Files: `.github/workflows/**`
- Notes: Dependabot for NuGet is added by F-05. D-12.

### F-05 · Project organisation
- Issue: #15 · Size: M · Status: In Progress (main agent)
- Goal: CLAUDE.md, `.claude/` rules, decisions, glossary, plan, GUARDRAILS, labels, milestones, board.
- Acceptance: see issue #15.

### F-06 · Protect `main`
- Issue: #15 · Size: S · Status: Done (ruleset "Protect main", 2026-10-27)
- Goal: ruleset per D-08 once CI is green.
- Acceptance:
  - [x] Ruleset active: no deletion, no force push, PR required (0 approvals, squash only), required checks `docs` and `dotnet (ubuntu|windows|macos-latest)`, no bypass
  - [x] Owner and main agent can still squash-merge a green PR (checked with PR #16)

### F-07 · Merge Dependabot Action bumps
- Issue: #4 · Size: S · Status: Done
- Goal: Dependabot PRs merged once green on current `main`.
- Done: #9, #10, #12, #13, #17, #18, #20, #21; #11 superseded by #23. Companion repos: all Action bumps merged.

### F-08 · Roslyn version policy for the source generator
- Issue: #4 · Size: S · Status: Ready
- Goal: decide which `Microsoft.CodeAnalysis.CSharp` version `Sferix.SPO.Generators` references, so the generator
  loads in every SDK we support; then accept or close Dependabot #19 (4.14 → 5.9).
- Acceptance:
  - [ ] Decision in DECISIONS.md: lowest supported .NET SDK band and the matching Roslyn version
  - [ ] Dependabot ignore rule for Roslyn versions above it in `.github/dependabot.yml`
  - [ ] #19 merged or closed with a one-line reason
- Files: `src/Sferix.SPO.Generators/*.csproj`, `.github/dependabot.yml`, `.claude/docs/DECISIONS.md`
- Notes: a generator built against a newer Roslyn than the user's compiler fails with CS9057.
