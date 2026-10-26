# Complete GitHub administration setup

Some GitHub settings are **not set by repository files**. The connected repository tool can commit files and create issues, but does not expose writing organization Projects, repository milestones, labels, repo rulesets, CodeQL settings or metadata. These must be configured through GitHub UI or an authorized GitHub CLI.

## One-time CLI bootstrap

Install GitHub CLI, run `gh auth login`, then from this repository:

```bash
bash scripts/bootstrap-github-admin.sh
```

The script creates labels and milestones idempotently in the three product repos. It does **not** create a board, branch rulesets, security settings or silently change visibility. Verify results in the GitHub UI.

## GitHub Project

Create one organization Project named **FrankenSPO Roadmap**. Use views: Backlog, Active Development, Architecture, Providers, Documentation, Release Roadmap. Add custom fields Status (Backlog / Ready / In Progress / Review / Verification / Done), Priority P0-P3, Area, and Target Release; connect the main repository. No speculative deadlines.

## Assign issues

P0: FrankenSPO issues #2, #3, #4. P1: #5, #6, #7. The umbrella #1 spans P0-P1. #8 remains future work. MiddleTier issue #1 and WebApi issue #1 belong to their M0 readiness milestone.

## Repository rules / security

Protect `main`: prevent deletions and force pushes; require pull requests and passing status checks after the actual workflow check names appear. Ensure CodeQL is configured when source exists and GitHub's security features permit it. Verify dependabot, private vulnerability reporting, Actions permissions, and branch ruleset eligibility. Organization policies should be configured separately.

Do not enable branch checks that do not yet exist, or the repository can become impossible to merge. Publication of NuGet packages and GitHub Releases is deferred until reproducible, tested code exists.
