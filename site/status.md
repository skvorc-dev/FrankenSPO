# Development status

**Pre-implementation: no usable ORM runtime is released.**

This documentation site does not imply a released package, server, SDK or usable API.

Read the repository's [README](https://github.com/skvorc-dev/FrankenSPO#readme) and [tracked issues](https://github.com/skvorc-dev/FrankenSPO/issues) for current implementation progress.

When new features are implemented, verified examples and provider/version capability documentation will be published here along with release notes.

## Feature status values

Each feature and provider in the [capability manifest](https://github.com/skvorc-dev/FrankenSPO/blob/main/docs/compatibility/features.json) has one of these statuses, in this order. The manifest also uses `external-planned`, `unsupported` and `experimental`:

- `planned`: on the roadmap; no design or code yet.
- `specified`: designed in the architecture docs; not implemented.
- `implemented`: code exists; not yet proven by tests.
- `tested`: code exists and tests prove the behavior.
- `provider-verified`: tested against a real database provider.
- `release-ready`: verified and included in a release candidate.
