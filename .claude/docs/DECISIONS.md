# Decisions

The source of truth for how this project works. Numbered, never renumbered; a changed decision gets a new entry
that names the one it replaces (`Replaces D-03`). Other files link here instead of repeating the rule.

| # | Date | Decision |
|---|------|----------|
| D-01 | 2026-10-27 | **One repository:** `skvorc-dev/FrankenSPO` on GitHub (public, MIT). The local main folder is a clone of it. Companion repos (MiddleTier, WebApi) are out of scope until their own M0 readiness milestones. |
| D-02 | 2026-10-27 | **Public-safe split:** project rules, plan and decisions are committed. Machine-local coordination (shared mailboxes, devices, backups, local paths) lives only in the gitignored `.claude/LOCAL.md`. |
| D-03 | 2026-10-27 | **Plan is the source of truth for tasks:** cards live in `.claude/plan/E*.md`, assignment in `BOARD.md`. GitHub issues mirror cards or epics, the GitHub board mirrors `BOARD.md`. On conflict the plan files win and the main agent syncs GitHub. |
| D-04 | 2026-10-27 | **Roadmap by gates, not dates:** phases P0–P5+ from `docs/ROADMAP.md`; one GitHub milestone per phase (P0–P3 created). A phase is done when its gate has evidence. |
| D-05 | 2026-10-27 | **Branches:** owner and main agent use `feature/`, `fix/`, `docs/<issue>-<desc>`; worker agents use `task/<Agent>_<TaskId>_<yyyyMMddTHHmm>` in their own worktree under `../FrankenSPO-wt/`. Only the owner and the main agent merge into `main`, by squash merge of a PR with green CI. |
| D-06 | 2026-10-27 | **Labels:** `type:*`, `priority:P0–P3`, `area:*` from `docs/labels.csv` (FrankenSPO repo only). No status labels; status lives on the board. GitHub default labels stay for Dependabot. |
| D-07 | 2026-10-27 | **Board:** one org project "FrankenSPO Roadmap" (`skvorc-dev` project 1) with fields Status (Backlog / Ready / In Progress / Review / Done) and Priority (P0–P3). No dates, no extra views until needed. |
| D-08 | 2026-10-27 | **`main` is protected** by a ruleset: no deletion, no force push, PR required, CI checks (`docs`, `dotnet (*)`) required. |
| D-09 | 2026-10-27 | **Commit identity:** `Andrej Skvorc <47588613+andrejskvorc@users.noreply.github.com>` in this repo, as in the existing history, so no private address enters public history. |
| D-10 | 2026-10-27 | **Commit messages:** Conventional Commits (`feat`, `fix`, `docs`, `test`, `ci`, `build`, `design`), referencing the issue. |
| D-11 | 2026-10-27 | **Stack:** C# / .NET 10, `TreatWarningsAsErrors`, xUnit, SQLite first (Microsoft.Data.Sqlite), PostgreSQL in P3, DocFX site on GitHub Pages. |
| D-12 | 2026-10-27 | **Dependency updates:** Dependabot weekly for GitHub Actions and NuGet. Known-vulnerable packages fail the build and are upgraded, not suppressed. |
| D-13 | 2026-10-27 | **Capability truth:** feature status only in `docs/compatibility/features.json`; "planned means unavailable"; status moves only with test evidence (from AGENTS.md). |
| D-14 | 2026-10-27 | **Card sizes:** S (≤ half a day of agent work), M (≤ 2 days), L (must be split before it is assigned). |
| D-15 | 2026-10-27 | **Licence holder:** LICENSE stays "Copyright (c) 2026 FrankenSPO contributors" (owner, closes #2). |
| D-16 | 2026-10-27 | **CodeQL:** GitHub default setup enabled for C# (part of F-04). |
| D-17 | 2026-10-27 | **Worker agents:** Claude side sessions, Codex and Gemini (Antigravity) are all allowed, as on the owner's other projects. None starts until the owner says when and how; until then the main agent works the cards itself. |
| D-18 | 2026-10-27 | **Companion repos** (MiddleTier, WebApi) get the same GitHub setup now: labels `type:*`/`priority:*`, milestone M0, the family board, CLAUDE.md, a `main` ruleset. They become separate local projects with their own main agents later. Amends D-01. |
| D-19 | 2026-10-27 | **NuGet:** reserve the `Sferix.SPO` package ID prefix before the P2 preview, not now. |
| D-20 | 2026-10-27 | **Brand:** "FrankenSPO by Sferix" stays, with the existing "no registered company" wording. |
| D-21 | 2026-10-27 | **Outside contributions:** issues welcome; pull requests only on a maintainer's request until P2 (CONTRIBUTING.md). |
| D-22 | 2026-10-27 | **DCO:** external contributors sign off commits (`git commit -s`); `.github/workflows/dco.yml` checks PRs from forks. No CLA. |
| D-23 | 2026-10-27 | **Releases (from P2):** tag `vX.Y.Z-preview.N` plus a GitHub Release with notes; the owner pushes NuGet packages by hand. No publishing automation or NuGet secrets in CI. |
| D-24 | 2026-10-27 | **No GitHub Discussions** yet; the "question" issue template stays. |

| D-25 | 2026-10-27 | **In sync with .NET LTS** (owner): build on the latest .NET 10 LTS SDK patch (`global.json`: 10.0, latest feature band, no previews); runtime packages (`Microsoft.Data.Sqlite`, `Microsoft.Extensions.*`, `System.*`) on the latest **10.0.x** patch, never the next non-LTS major. The source generator references the Roslyn version of the first LTS band (**5.0.0** for .NET 10, C# 14), because a generator built against a newer compiler is silently skipped on older SDK bands (CS9057, verified on SDK 10.0.113). Security fixes for the compiler reach users through their SDK patch; the reference is compile-time only. CI builds on the 10.0.1xx band to prove this. Revisit when .NET 12 LTS ships. Replaces nothing; implements F-08. |
| D-26 | 2026-10-27 | **LTS release rule** (owner). Until v1 is production: LTS only (D-25). After v1: development may move to a non-LTS (STS) .NET on a separate line, but anything built on a non-LTS .NET is released **only as a preview** (`vX.Y.Z-preview.N`, NuGet prerelease). **Only builds on an LTS .NET are production, signed-off releases**; non-LTS work becomes production when it ships on the next LTS. Amends D-23 and D-25. |
| D-27 | 2026-10-27 | **App IDs** (owner): Sferix is a brand of Skvorc Dev. Reverse-domain IDs of Sferix-family apps (Android package, Apple bundle ID, store IDs) use `dev.skvorc.sferix.<product>`, e.g. `dev.skvorc.sferix.frankenspo` for FrankenSPO sample apps. .NET packages and namespaces keep the `Sferix.<Product>` form (`Sferix.SPO`). Product domains are for websites only. FrankenSPO needs an app ID only from its first installable sample. |
