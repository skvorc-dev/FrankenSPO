# Release acceptance checklist

The release rules (LTS-only production, preview tags, NuGet pushed by the owner) are in
[DECISIONS.md](../.claude/docs/DECISIONS.md).

- [ ] Built on an LTS .NET SDK (else mark as preview)
- [ ] Verified code, public API and tests correspond to the same commit
- [ ] Real-provider regression and SQL-injection tests pass
- [ ] Capability manifest accurately reflects supported behavior per provider
- [ ] README, samples, architecture and agent instructions match release
- [ ] Breaking changes and compatibility notes published
- [ ] Third-party dependency/provenance and license notices checked
- [ ] Security review, docs validation and CI checks pass
- [ ] No confidential application names, secrets or unannounced legal claims
- [ ] Git tag and any published package are reproducible and traceable
- [ ] GitHub Release notes written
- [ ] NuGet pushed manually by the owner
