# E0 · Foundation (phase P0)

Milestone **P0 — Foundation**. Gate: clean baseline, green CI on three OSes, public-safe docs, self-organising repo.

### F-01 · Repository policies and contributor guidance
- Issue: #2 · Size: S · Status: Done (#2 closed, D-15)
- Goal: public-safe README, LICENSE, CONTRIBUTING, SECURITY, CODE_OF_CONDUCT, THIRD-PARTY-NOTICES, AGENTS.md.
- Acceptance:
  - [x] All files exist and are checked by `scripts/validate_repository.py`
  - [x] Owner confirms LICENSE copyright holder and that no text reveals private details
- Files: root `*.md`, `LICENSE`

### F-02 · Architecture contracts
- Issue: #3 · Size: M · Status: Done (PR #39, sonnet-1)
- Goal: the architecture docs state each core contract precisely enough that P1 cards can be written from them,
  without inventing design that the owner has not approved.
- Sources (the only ones): `docs/architecture/OVERVIEW.md`, `docs/POC_ACCEPTANCE.md`, `AGENTS.md`, `SECURITY.md`,
  issues #1, #3, #5, #6, `.claude/docs/DECISIONS.md`, and the existing code in `src/`.
- Acceptance:
  - [x] One file per contract in `docs/architecture/`: `objectspace.md` (identity map, unit of work, SaveChanges),
        `tracking.md` (scalar tracking, the three baselines working / flushed / committed, partial loads),
        `query-security.md` (typed query AST, parameters, metadata-validated identifiers, raw SQL boundary,
        no client-side evaluation, tenant/row/field policies fail closed), `providers.md` (provider contract,
        capability checks, fail closed, SQLite first, PostgreSQL in P3), `modules.md` (assembly boundaries of
        `Sferix.SPO.*`, what Core may reference, optional modules, public API maturity levels)
  - [x] Every file starts with `**Status: specified, not implemented.**` and has sections: Purpose, Contract
        (numbered, testable statements), Not in scope, Open questions
  - [x] Anything not in the sources goes under **Open questions** as a question for the owner, never as a decision
  - [x] `OVERVIEW.md` links the five files; no other doc claims anything is implemented
  - [x] `features.json`: only features whose contract is now written may move `planned` → `specified`; list them in the PR
  - [x] `scripts/validate_repository.py` and CI green
- Files: `docs/architecture/**`, `docs/compatibility/features.json`
- Notes: docs only, no code. D-13. Size M: one PR.

### F-03 · CI green again (NU1903)
- Issue: #4 · Size: S · Status: Done (PR #14)
- Goal: `main` builds again after `SQLitePCLRaw.lib.e_sqlite3 2.1.11` was flagged vulnerable.
- Acceptance:
  - [x] Microsoft.Data.Sqlite 10.0.14; CI green on Linux, Windows, macOS

### F-04 · CI security gates
- Issue: #4 · Size: S · Status: Done (PR #27, haiku-1)
- Goal: dependency problems are caught in PRs, not on `main`.
- Acceptance:
  - [x] New `.github/workflows/dependency-review.yml`: on `pull_request`, `permissions: contents: read`,
        steps `actions/checkout@v7` then `actions/dependency-review-action@v4` with `fail-on-severity: high`
  - [ ] Do **not** touch `ci.yml`, `pages.yml`, `dco.yml` or the ruleset
  - [x] CodeQL for C# is enabled (default setup, D-16)
  - [ ] CI green
- Files: `.github/workflows/**`
- Notes: Dependabot for NuGet is added by F-05. D-12.

### F-05 · Project organisation
- Issue: #15 · Size: M · Status: Done
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
- Issue: #4 · Size: S · Status: Done (D-25)
- Goal: decide which `Microsoft.CodeAnalysis.CSharp` version `Sferix.SPO.Generators` references, so the generator
  loads in every SDK we support; then accept or close Dependabot #19 (4.14 → 5.9).
- Acceptance:
  - [x] Decision in DECISIONS.md: lowest supported .NET SDK band and the matching Roslyn version
  - [x] Dependabot ignore rule for Roslyn versions above it in `.github/dependabot.yml`
  - [x] #19 merged or closed with a one-line reason
- Files: `src/Sferix.SPO.Generators/*.csproj`, `.github/dependabot.yml`, `.claude/docs/DECISIONS.md`
- Notes: a generator built against a newer Roslyn than the user's compiler fails with CS9057.

## Haiku chain 1 (cards F-09 … F-17, with F-04 first)

Small, exact cards for one worker in a chain (D-17). Each card: its own branch from `origin/main`, its own PR,
`Refs #<issue>`. Never invent features or facts; when a card needs a fact you cannot verify, write
`TODO(owner):` in the text and say so in the PR.

### F-09 · THIRD-PARTY-NOTICES: list current dependencies
- Issue: #4 · Size: S · Status: Done (PR #28, haiku-1)
- Goal: every NuGet package referenced in `src/` and `tests/` is listed with version, licence and use.
- Acceptance:
  - [ ] Table in `THIRD-PARTY-NOTICES.md`: package, version (from the csproj), SPDX licence (from nuget.org), "runtime" or "build/test only"
  - [ ] Transitive SQLite native bundle (`SQLitePCLRaw.*`) listed as runtime
  - [ ] Existing paragraph kept; no other file changed
- Files: `THIRD-PARTY-NOTICES.md`

### F-10 · SECURITY.md: private reporting is enabled
- Issue: #4 · Size: S · Status: Done (PR #29, haiku-1)
- Goal: SECURITY.md says how to report today.
- Acceptance:
  - [ ] Replace "when available" with a link to `https://github.com/skvorc-dev/FrankenSPO/security/advisories/new`
  - [ ] Add "Supported versions: none yet (pre-release); production releases only on an LTS .NET (see roadmap)"
  - [ ] Security requirements paragraph unchanged
- Files: `SECURITY.md`

### F-11 · Development workflow and release checklist match the decisions
- Issue: #15 · Size: S · Status: Done (PR #30, haiku-1)
- Goal: `docs/DEVELOPMENT_WORKFLOW.md` and `docs/RELEASE_CHECKLIST.md` agree with D-05, D-21, D-22, D-23, D-25, D-26.
- Acceptance:
  - [ ] Workflow: branch names `feature/`, `fix/`, `docs/<issue>-<desc>`; worker branches per GUARDRAILS.md; squash merge via PR with green CI; DCO sign-off for external contributors
  - [ ] Versioning: `vX.Y.Z-preview.N`; production releases only on an LTS .NET; non-LTS builds are previews
  - [ ] Checklist gains: "built on an LTS .NET SDK (else mark as preview)", "GitHub Release notes written", "NuGet pushed manually by the owner"
  - [ ] Link to `.claude/docs/DECISIONS.md` instead of copying decision text
- Files: `docs/DEVELOPMENT_WORKFLOW.md`, `docs/RELEASE_CHECKLIST.md`

### F-12 · GITHUB_SETUP.md describes the current setup
- Issue: #15 · Size: S · Status: Done (PR #31, haiku-1)
- Goal: the file documents what exists now instead of what still has to be done.
- Acceptance:
  - [ ] States: labels from `docs/labels.csv`; milestones P0–P3; org project "FrankenSPO Roadmap" (project 1) with Status and Priority; ruleset "Protect main" (no deletion/force push, PR + checks `docs`, `dotnet (ubuntu-latest|windows-latest|macos-latest)`, squash only); CodeQL default setup; private vulnerability reporting; Dependabot for Actions and NuGet
  - [ ] Keeps the bootstrap script instructions; removes the "Assign issues" section
- Files: `docs/GITHUB_SETUP.md`

### F-13 · `.editorconfig` from the code style rules
- Issue: #4 · Size: S · Status: Done (PR #32, haiku-1)
- Goal: editors and `dotnet format` follow `.claude/rules/code-style.md`.
- Acceptance:
  - [ ] Root `.editorconfig`: UTF-8, LF, final newline, 4 spaces for `*.cs`, 2 for `*.{json,yml,yaml,props,csproj,slnx}`; `csharp_style_namespace_declarations = file_scoped:warning`; `dotnet_style_qualification_for_*` false
  - [ ] `dotnet format FrankenSPO.slnx --verify-no-changes` passes **without changing any `.cs` file**; if it would change code, relax the rule instead and list it in the PR
  - [ ] Build has 0 warnings
- Files: `.editorconfig`

### F-14 · Pin the DocFX version in the Pages workflow
- Issue: #4 · Size: S · Status: Done (PR #33, haiku-1)
- Goal: reproducible docs builds.
- Acceptance:
  - [ ] `pages.yml`: `dotnet tool install --global docfx --version <latest stable from nuget.org>`
  - [ ] `docfx docfx.json` builds locally without errors with that version (say so in the PR)
- Files: `.github/workflows/pages.yml`

### F-15 · Bug report form asks for SDK and OS
- Issue: #4 · Size: S · Status: Done (PR #34, haiku-1)
- Goal: bug reports carry the environment.
- Acceptance:
  - [ ] `bug_report.yml`: required input ".NET SDK version (`dotnet --version`)", required dropdown OS (Linux / Windows / macOS / other), optional input "Database provider and version"
  - [ ] Existing fields unchanged; YAML valid (`python3 -c "import yaml,sys;yaml.safe_load(open(sys.argv[1]))" <file>`)
- Files: `.github/ISSUE_TEMPLATE/bug_report.yml`

### F-16 · Capability manifest: `specified` status
- Issue: #3 · Size: S · Status: Done (PR #35, haiku-1)
- Goal: first half of F-02 — the manifest can say "specified" (designed, not implemented).
- Acceptance:
  - [ ] `scripts/validate_repository.py` accepts `specified`
  - [ ] `features.json`: `note` explains the order planned → specified → implemented → tested → provider-verified → release-ready; **no feature status changed**
  - [ ] `site/status.md` gets a short list of those statuses with one line each
- Files: `scripts/validate_repository.py`, `docs/compatibility/features.json`, `site/status.md`

### F-17 · Edge-case tests for `SqlIdentifier` and `EntityId`
- Issue: #6 · Size: S · Status: Done (PR #36, haiku-1)
- Goal: lock in current behavior; no production code change.
- Acceptance:
  - [ ] New cases in `IdentifierTests`: 63-char identifier accepted, 64 rejected; null, empty and whitespace rejected; non-ASCII letter (`"KupacČ"`) rejected; `EntityId.ToString()` is the `D` format
  - [ ] If a test fails, do **not** change `src/`; stop and report it
  - [ ] `dotnet test` green
- Files: `tests/Sferix.SPO.Tests.Unit/IdentifierTests.cs`
