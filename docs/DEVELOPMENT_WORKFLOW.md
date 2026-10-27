# Development workflow

The numbered decisions in [DECISIONS.md](../.claude/docs/DECISIONS.md) are the source of truth for branches, merging,
contributions and releases.

1. Start with an issue and link any relevant architecture/acceptance contract.
2. Work on a short-lived branch: `feature/<issue>-<desc>`, `fix/<issue>-<desc>` or `docs/<issue>-<desc>`. Worker
   agents use their own branch and worktree as described in [GUARDRAILS.md](../GUARDRAILS.md).
3. Submit a PR with correctness tests, documentation and updated capability claims.
4. Verify CI, provider behavior, security boundaries, licensing and public-safe content.
5. Squash-merge the PR into protected `main` once CI is green. External contributors sign off their commits (DCO).
6. Release only verified features, with tagged documentation.

## Definition of done

No feature is complete without implementation, tests, honest capability status, and compilable examples where relevant. Never publish confidential application details, secrets or claims about unannounced organizations.

## Versioning

Use tags like `vX.Y.Z-preview.N` when release criteria are satisfied. Production releases are built only on an LTS
.NET; a build on a non-LTS .NET is released only as a preview. Planned milestones and speculative dates are not versions.
