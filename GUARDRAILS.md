# Guardrails for worker agents

You are a **worker agent** if you are not the main agent (the Claude Code session in the project's main folder).
These rules come on top of [AGENTS.md](AGENTS.md) and [CLAUDE.md](CLAUDE.md). When they conflict, the stricter rule wins.

## Before you start

1. Your task must be on `.claude/plan/BOARD.md` with **your agent name** and status `In Progress`.
   No assignment → no work. Do not pick up other cards, even if they look easy.
2. Read your card in its epic file (`.claude/plan/E*.md`): goal, acceptance criteria, files, size.
3. Read `.claude/rules/` and the decisions the card links to.

## Where you work

- **Your own git worktree**, never the main folder: a sibling folder `../FrankenSPO-wt/<branch-name>`.
  ```bash
  git -C <main-folder> fetch origin
  git -C <main-folder> worktree add ../FrankenSPO-wt/<branch> -b <branch> origin/main
  ```
- **Branch name:** `task/<Agent>_<TaskId>_<yyyyMMddTHHmm>` (local time when you start), e.g.
  `task/Codex_F-04_20261027T0915`. One task per branch.
- Change only the files your card names, or files plainly needed for it. Anything else: stop and ask.

## What you never do

- Merge into `main`, push to `main`, or approve your own PR. Only the owner and the main agent merge.
- Force-push, rebase or delete anyone else's branch; rewrite published history.
- Commit secrets, tokens, passwords, connection strings, personal data or private project names.
- Edit `.claude/plan/`, `.claude/docs/DECISIONS.md`, `CLAUDE.md`, `GUARDRAILS.md`, `.github/` or CI settings
  unless your card says so.
- Mark a feature `implemented`/`tested` in `features.json` without the tests that prove it.
- Add a dependency the card does not allow; if allowed, record it in `THIRD-PARTY-NOTICES.md`.
- Create GitHub issues, labels, milestones, releases or change repository settings.

## When you finish

1. `dotnet build` and `dotnet test` (see CLAUDE.md) and `python3 scripts/validate_repository.py` pass.
2. Commit with a Conventional Commit message referencing the issue (`Refs #N`).
3. Push your branch and open a PR to `main` using the template; fill **Task card** and **Verification** honestly,
   including what you could not check.
4. If your environment has a local coordination file (`.claude/LOCAL.md`), follow its reporting rules.

## Stop and ask the main agent when

- The card is unclear, too big, or conflicts with a decision or the code.
- A test outside your task fails, or CI is red on `main`.
- You would need anything listed under "What you never do".
