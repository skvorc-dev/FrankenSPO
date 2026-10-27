# Security policy

FrankenSPO has not yet published a production-ready release. Report suspected vulnerabilities privately through
[GitHub private vulnerability reporting](https://github.com/skvorc-dev/FrankenSPO/security/advisories/new); do not
expose them, credentials, or sensitive data in public issues.

Supported versions: none yet (pre-release); production releases only on an LTS .NET (see roadmap).

Security requirements include parameterized SQL values, validated identifiers, explicit raw SQL trust boundaries, mandatory enforcement of declared tenant/row/field restrictions, and fail-closed unsupported mappings. Requirements are not claims that the implementation exists.
