#!/usr/bin/env python3
"""Check that every product-owned Spine TrackEntry listener has a removal path."""

from __future__ import annotations

import json
import re
from collections import Counter
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
RUNTIME = ROOT / "Assets/Game/Scripts/ColorTiming"
OUTPUT = ROOT / "openspec/changes/migrate-color-timing-to-ai-friendly-framework/evidence/spine-listener-audit.json"
PATTERN = re.compile(r"\b\w+\.(Event|Complete|End)\s*([+-]=)\s*(\w+)")


def main() -> int:
    additions: Counter[tuple[str, str]] = Counter()
    removals: Counter[tuple[str, str]] = Counter()
    occurrences: list[dict[str, object]] = []

    for path in sorted(RUNTIME.rglob("*.cs")):
        for number, line in enumerate(path.read_text(encoding="utf-8-sig", errors="replace").splitlines(), 1):
            code = line.split("//", 1)[0]
            for event_name, operation, handler in PATTERN.findall(code):
                key = (event_name, handler)
                (additions if operation == "+=" else removals)[key] += 1
                occurrences.append({
                    "file": path.relative_to(ROOT).as_posix(),
                    "line": number,
                    "event": event_name,
                    "operation": operation,
                    "handler": handler,
                })

    missing = []
    for key, count in sorted(additions.items()):
        if removals[key] == 0:
            missing.append({"event": key[0], "handler": key[1], "subscriptionSites": count})

    report = {
        "status": "pass" if not missing else "fail",
        "subscriptionPairs": [
            {
                "event": event,
                "handler": handler,
                "addSites": additions[(event, handler)],
                "removeSites": removals[(event, handler)],
            }
            for event, handler in sorted(additions)
        ],
        "missingRemovalPaths": missing,
        "occurrences": occurrences,
        "note": "Counts are source sites, not runtime invocation counts; each removal path is also covered by lifecycle source review.",
    }
    OUTPUT.write_text(json.dumps(report, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(
        f"[{report['status'].upper()}] pairs={len(additions)}, "
        f"subscriptionSites={sum(additions.values())}, removalSites={sum(removals.values())}, "
        f"missing={len(missing)}"
    )
    return 0 if not missing else 1


if __name__ == "__main__":
    raise SystemExit(main())
