# GitHub administration setup

This file records the GitHub settings that repository files cannot set, and what exists now. Some settings are
changed only through the GitHub UI or an authorized GitHub CLI.

## Labels and milestones

Labels come from `docs/labels.csv`. Milestones are P0 to P3, one per roadmap phase.

## One-time CLI bootstrap

Install GitHub CLI, run `gh auth login`, then from this repository:

```bash
bash scripts/bootstrap-github-admin.sh
```

The script creates labels and milestones idempotently in the three product repos. It does **not** create a board, branch rulesets, security settings or silently change visibility. Verify results in the GitHub UI.

## GitHub Project

Organization Project **FrankenSPO Roadmap** (project 1) has Status and Priority fields, and is connected to this repository.

## Repository rules and security

- Ruleset **Protect main**: no deletions and no force pushes; pull requests required (squash merge only); required status checks `docs` and `dotnet (ubuntu-latest|windows-latest|macos-latest)`.
- CodeQL: default setup for C#.
- Secret scanning and push protection: enabled (D-28).
- Private vulnerability reporting: enabled (see [SECURITY.md](../SECURITY.md)).
- Dependabot: version updates for GitHub Actions and NuGet, configured in `.github/dependabot.yml`; Dependabot alerts and the dependency graph are on (needed by the dependency review check).

Publication of NuGet packages and GitHub Releases is deferred until reproducible, tested code exists.
