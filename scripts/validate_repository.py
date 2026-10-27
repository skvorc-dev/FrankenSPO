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
import xml.etree.ElementTree as ET

# Every neutral .resx needs the same keys, with a non-empty value, in de, hr and es (D-41).
translations = ("de", "hr", "es")


def resource_keys(path):
    values = {}
    for data in ET.parse(path).getroot().iter("data"):
        values[data.get("name")] = (data.findtext("value") or "").strip()
    return values


for neutral in sorted((root / "src").rglob("*.resx")):
    if neutral.name.count(".") != 1:
        continue
    expected = resource_keys(neutral)
    for key, value in expected.items():
        if not value:
            raise SystemExit(f"{neutral.relative_to(root)}: neutral value for '{key}' is empty")
    for language in translations:
        culture = neutral.with_name(neutral.name.replace(".resx", f".{language}.resx"))
        if not culture.is_file():
            raise SystemExit(f"{neutral.relative_to(root)}: missing {culture.name}")
        actual = resource_keys(culture)
        for key in expected:
            if key not in actual:
                raise SystemExit(f"{culture.relative_to(root)}: missing key '{key}'")
            if not actual[key]:
                raise SystemExit(f"{culture.relative_to(root)}: empty value for key '{key}'")
        for key in actual:
            if key not in expected:
                raise SystemExit(f"{culture.relative_to(root)}: key '{key}' is not in {neutral.name}")

print("FrankenSPO repository validation passed")
