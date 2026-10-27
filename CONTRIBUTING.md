# Contributing to FrankenSPO

FrankenSPO is in initial development.

- **Issues are welcome:** bugs, questions, documentation gaps and feature ideas, using the issue forms.
- **Pull requests from outside contributors are accepted only on a maintainer's request** until the first developer
  preview (milestone P2). Please open an issue first and wait until a maintainer asks for a PR.
- **Sign off your commits** (`git commit -s`) to certify the [Developer Certificate of Origin](https://developercertificate.org/):
  you wrote the change or have the right to submit it under the MIT licence. PRs from forks are checked for it.
- Link your pull request to its issue. Maintain tests, public documentation, reproducible examples and the capability
  manifest alongside code changes.
- Respect MIT and record third-party dependency and source provenance in `THIRD-PARTY-NOTICES.md`.
- Do not submit private data, secret material or proprietary framework implementation code.
- **Four languages:** every user-visible string lives in a `.resx` resource in four languages: `X.resx` (English, neutral),
  `X.de.resx`, `X.hr.resx` and `X.es.resx`, with the same keys and no empty values (D-40, D-41).
- **Checked in CI:** `scripts/validate_repository.py` fails when a language file or key is missing; keep placeholders such as `{0}` identical in every language.
- Messages never contain SQL text or values.
