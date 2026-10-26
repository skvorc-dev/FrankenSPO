# Development workflow

1. Start with an issue and link any relevant architecture/acceptance contract.
2. Work on a short-lived branch (`feature/<issue>-description` or `fix/<issue>-description`).
3. Submit a PR with correctness tests, documentation and updated capability claims.
4. Verify CI, provider behavior, security boundaries, licensing and public-safe content.
5. Merge to protected `main` and release only verified features with tagged documentation.

## Definition of done

No feature is complete without implementation, tests, honest capability status, and compilable examples where relevant. Never publish confidential application details, secrets or claims about unannounced organizations.

## Versioning

Use tags like `v0.1.0-preview.1` when release criteria are satisfied. Planned milestones and speculative dates are not versions.
