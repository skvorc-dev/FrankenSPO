# FrankenSPO — instructions for Claude and other agents

FrankenSPO by Sferix is an early-stage, MIT-licensed, modular .NET object-relational mapper: source-generated
metadata, a provider-neutral ObjectSpace/UnitOfWork, SQLite first, PostgreSQL next. No ORM feature works yet;
see `docs/compatibility/features.json`. **Planned means unavailable.**

This repository is **public**. Everything committed here is visible to everyone (see [Public safety](#public-safety)).

## Read first

1. [AGENTS.md](AGENTS.md) — the public rulebook for any contributor or agent (scope, quality gates, release etiquette).
2. [GUARDRAILS.md](GUARDRAILS.md) — if you are **not** the main agent: what you may and may not do.
3. `.claude/rules/` — code style, testing, security, public API.
4. `.claude/docs/DECISIONS.md` — numbered decisions, the source of truth. `GLOSSARY.md` for terms.
5. `.claude/plan/` — roadmap, board and epic task cards.
6. `.claude/LOCAL.md` — machine-local coordination (not in git; only exists on the owner's machine).

## Tech stack

- C# / .NET 10 LTS, kept in sync with LTS patches; production releases only on LTS .NET (D-25, D-26; `global.json`, `Directory.Build.props`: nullable, implicit usings, `TreatWarningsAsErrors`, latest analyzers).
- Solution `FrankenSPO.slnx`; projects `src/Sferix.SPO.*`, tests `tests/Sferix.SPO.Tests.*` (xUnit).
- SQLite via `Microsoft.Data.Sqlite`; PostgreSQL from milestone P3.
- Roslyn incremental source generator (`Sferix.SPO.Generators`), referencing Roslyn 5.0.0 — the first .NET 10 band (D-25).
- Docs site: DocFX from `site/`, published to GitHub Pages by `.github/workflows/pages.yml`.
- CI: `.github/workflows/ci.yml` — `scripts/validate_repository.py` plus build and test on Linux, Windows and macOS.

```bash
dotnet restore FrankenSPO.slnx
dotnet build FrankenSPO.slnx -c Release --no-restore
dotnet test FrankenSPO.slnx -c Release --no-build
python3 scripts/validate_repository.py
```

## Conventions

- **Commits:** Conventional Commits as in the history — `feat:`, `fix:`, `docs:`, `test:`, `ci:`, `build:`,
  `design:`, optional scope `fix(ci):`. Reference the issue (`Refs #4`, `Closes #4`).
- **Branches** (D-05): owner and main agent `feature/<issue>-<desc>`, `fix/<issue>-<desc>`, `docs/<issue>-<desc>`;
  worker agents `task/<Agent>_<TaskId>_<yyyyMMddTHHmm>`. Short-lived, one task per branch.
- **Merging:** only the owner and the main agent merge into `main`, by squash merge of a PR with green CI.
- **Never** force-push, never rewrite `main`, never commit secrets.
- Every behavior change: linked issue, tests, docs, and a truthful `features.json` entry (AGENTS.md quality gates).
- Write a decision into `DECISIONS.md` when you choose something others must follow; do not repeat it elsewhere.

## How agents work here

| Role | Who | May |
|------|-----|-----|
| Owner | Andrej Skvorc | Everything. Decides product, money, legal, licence, visibility, accounts. |
| Main agent | The Claude Code session in the project's main folder (not a worktree) | Plan, assign, review, merge, keep plan and GitHub in sync |
| Worker agent | Any other session or tool (Claude side sessions, Codex, Gemini, …) | Only the task assigned to it on `.claude/plan/BOARD.md`, in its own worktree and branch; see GUARDRAILS.md |

Task flow: card in an epic file (`Backlog` → `Ready`) → main agent assigns it on BOARD.md (`In Progress`, with
agent and branch) → worker opens a PR (`Review`) → main agent or owner reviews and merges (`Done`).

## Main agent duties

At the start of every session:
1. `git pull` on `main`; check `gh pr list` and `gh run list --limit 5` (CI on `main` must be green).
2. Do the checks in `.claude/LOCAL.md` (mailbox, backup) if that file exists.
3. Reconcile `.claude/plan/BOARD.md` with the GitHub board and open PRs.

While working:
- Keep cards ready to assign: goal, acceptance criteria, size, files. Split anything sized `L` before assigning it.
- Assign tasks only on BOARD.md; never let two writers touch the same files at once.
- Review worker PRs against the card's acceptance criteria and GUARDRAILS; merge only with green CI.
- Mirror changes to GitHub: one issue per card or epic (labels `type:`/`priority:`/`area:`, milestone = phase),
  board Status matches BOARD.md. Plan files win on conflict.
- Record decisions in DECISIONS.md; keep ROADMAP, features.json and README telling the same story.
- Ask the owner before anything public, irreversible, or about product, money, legal, licence or accounts.

At the end: update BOARD.md, summarise for the owner what changed, what is open, and the questions.

## Public safety

No secrets, tokens or connection strings. No private project names, pilot customers, unannounced company details,
machine paths or shared-device details in committed files, issues, PRs or commit messages. Machine-local
coordination belongs in `.claude/LOCAL.md` (gitignored). Commits use the owner's GitHub noreply address (D-09).
