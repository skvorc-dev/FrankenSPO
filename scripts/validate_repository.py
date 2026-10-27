from pathlib import Path
import json

root = Path(__file__).resolve().parents[1]
required = ["README.md", "LICENSE", "AGENTS.md", "CLAUDE.md", "GUARDRAILS.md", ".claude/docs/DECISIONS.md", ".claude/plan/BOARD.md", "SECURITY.md", "CONTRIBUTING.md", "docs/ROADMAP.md", "docs/compatibility/features.json"]
missing = [p for p in required if not (root / p).is_file()]
if missing:
    raise SystemExit("Missing files: " + ", ".join(missing))
capabilities = json.loads((root / "docs/compatibility/features.json").read_text(encoding="utf-8"))
assert capabilities["schema_version"] == 1
allowed = {"planned", "specified", "implemented", "tested", "provider-verified", "release-ready", "external-planned", "unsupported", "experimental"}
for field in ("providers", "features"):
    for name, status in capabilities[field].items():
        assert status in allowed, f"Unknown feature status: {field}.{name}={status}"
import re

architecture = root / "docs/architecture"
overview = (architecture / "OVERVIEW.md").read_text(encoding="utf-8")
status_line = re.compile(r"^\*\*Status: (specified|implemented|tested|provider-verified|release-ready)\b")
for doc in sorted(architecture.glob("*.md")):
    if doc.name == "OVERVIEW.md":
        continue
    head = doc.read_text(encoding="utf-8").splitlines()[:5]
    if not any(status_line.match(line) for line in head):
        raise SystemExit(f"{doc.relative_to(root)}: missing '**Status: specified, not implemented.**' in its first 5 lines")
    if f"]({doc.name})" not in overview and f"](./{doc.name})" not in overview:
        raise SystemExit(f"{doc.relative_to(root)}: not linked from docs/architecture/OVERVIEW.md")
print("FrankenSPO repository validation passed")
