# GitHub organization and repository configuration

**Tracked files do not automatically create GitHub Projects, repository milestones, custom labels, GitHub organization topics, branch protection, security toggles or a published package.** Configure those separately in repository settings or via an authenticated GitHub CLI.

## Main project

Create organizational GitHub Project **FrankenSPO Roadmap**, with views Backlog, Development, Architecture, Provider Matrix, Documentation, Release Roadmap. Fields: Status (Backlog, Ready, In Progress, Review, Verification, Done), Priority (P0–P3), Area, Iteration, Target Release.

## Repository milestones

FrankenSPO phases: P0 Foundation, P1 SQLite PoC, P2 Developer Preview, P3 PostgreSQL Conformance, P4 General-purpose query/relationship growth. No speculative calendar deadlines.

For each of MiddleTier and WebAPI, create only **M0 Development Readiness**, linking its existing issue #1. Do not create fake implementation schedules.

## Branch and security settings

Protect `main` from force pushes and deletion; require pull requests and passing checks once workflows report their actual names. Start with CI checks rather than imposing nonexistent status checks. Verify Actions permissions, Dependabot, private vulnerability reporting and CodeQL availability/eligibility through GitHub settings. CodeQL source scanning should activate once eligible source exists.

Use `gh auth login` locally, then a GitHub CLI or browser session for settings not provided by the connected tools. Do not post credentials into issues or chats.
