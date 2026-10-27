from pathlib import Path
import json

root = Path(__file__).resolve().parents[1]
required = ["README.md", "LICENSE", "AGENTS.md", "CLAUDE.md", "GUARDRAILS.md", ".claude/docs/DECISIONS.md", ".claude/plan/BOARD.md", "SECURITY.md", "CONTRIBUTING.md", "docs/ROADMAP.md", "docs/compatibility/features.json"]
missing = [p for p in required if not (root / p).is_file()]
if missing:
    raise SystemExit("Missing files: " + ", ".join(missing))
capabilities = json.loads((root / "docs/compatibility/features.json").read_text(encoding="utf-8"))
assert capabilities["schema_version"] == 1
allowed = {"planned", "implemented", "tested", "provider-verified", "release-ready", "external-planned", "unsupported", "experimental"}
for field in ("providers", "features"):
    for name, status in capabilities[field].items():
        assert status in allowed, f"Unknown feature status: {field}.{name}={status}"
print("FrankenSPO repository validation passed")
