#!/usr/bin/env python3
"""Reproducible static guardrails for the ColorTiming product runtime."""

from __future__ import annotations

import json
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
RUNTIME = ROOT / "Assets/Game/Scripts/ColorTiming"
OUTPUT = ROOT / "openspec/changes/migrate-color-timing-to-ai-friendly-framework/evidence/runtime-risk-audit.json"

FORBIDDEN_PATTERNS = {
    "GameObject.Find": re.compile(r"\bGameObject\s*\.\s*Find\s*\("),
    "FindObjectOfType": re.compile(r"\bFindObjectOfType(?:s)?\s*<"),
    "Object.FindObjectOfType": re.compile(r"\b(?:UnityEngine\.)?Object\s*\.\s*FindObjectOfType(?:s)?\s*<"),
    "Resources.Load": re.compile(r"\bResources\s*\.\s*Load\s*(?:<[^>]+>)?\s*\("),
}

DEBUG_HOTKEY = re.compile(r"\bKeyCode\s*\.\s*[IOPT]\b")
MUTABLE_STATIC = re.compile(
    r"^\s*(?:(?:public|private|protected|internal)\s+)?static\s+(?!readonly\b|const\b)"
)
ALLOWED_MUTABLE_STATICS = {
    ("Legacy/UI/LoadScenes.cs", "static LoadScenes persistentView;"):
        "Bounded pooled loading-view identity; cleared by OnDestroy and not used as a service locator.",
}


def rel(path: Path) -> str:
    return path.relative_to(RUNTIME).as_posix()


def occurrence(path: Path, line_number: int, line: str) -> dict[str, object]:
    return {"file": rel(path), "line": line_number, "text": line.strip()}


def main() -> int:
    forbidden: list[dict[str, object]] = []
    debug_hotkeys: list[dict[str, object]] = []
    mutable_statics: list[dict[str, object]] = []
    allowed_statics: list[dict[str, object]] = []
    add_listener: list[dict[str, object]] = []
    remove_listener: list[dict[str, object]] = []
    event_add: list[dict[str, object]] = []
    event_remove: list[dict[str, object]] = []
    update_methods: list[dict[str, object]] = []

    for path in sorted(RUNTIME.rglob("*.cs")):
        lines = path.read_text(encoding="utf-8-sig", errors="replace").splitlines()
        for number, line in enumerate(lines, 1):
            code = line.split("//", 1)[0]
            if not code.strip():
                continue
            for name, pattern in FORBIDDEN_PATTERNS.items():
                if pattern.search(code):
                    item = occurrence(path, number, line)
                    item["api"] = name
                    forbidden.append(item)
            if DEBUG_HOTKEY.search(code):
                debug_hotkeys.append(occurrence(path, number, line))
            if MUTABLE_STATIC.search(code) and code.strip().endswith(";") and "(" not in code:
                item = occurrence(path, number, line)
                key = (rel(path), line.strip())
                if key in ALLOWED_MUTABLE_STATICS:
                    item["rationale"] = ALLOWED_MUTABLE_STATICS[key]
                    allowed_statics.append(item)
                else:
                    mutable_statics.append(item)
            if ".AddListener(" in code:
                add_listener.append(occurrence(path, number, line))
            if ".RemoveListener(" in code:
                remove_listener.append(occurrence(path, number, line))
            if re.search(r"\w+(?:\.\w+)*\s*\+=", code):
                event_add.append(occurrence(path, number, line))
            if re.search(r"\w+(?:\.\w+)*\s*-=", code):
                event_remove.append(occurrence(path, number, line))
            if re.search(r"\b(?:private|protected|public|internal)?\s*void\s+(?:Update|LateUpdate|FixedUpdate)\s*\(", code):
                update_methods.append(occurrence(path, number, line))

    passed = not forbidden and not debug_hotkeys and not mutable_statics
    report = {
        "status": "pass" if passed else "fail",
        "runtimeRoot": RUNTIME.relative_to(ROOT).as_posix(),
        "forbiddenApiOccurrences": forbidden,
        "removedDebugHotkeyOccurrences": debug_hotkeys,
        "unapprovedMutableStatics": mutable_statics,
        "approvedMutableStatics": allowed_statics,
        "lifecycleInventory": {
            "addListener": add_listener,
            "removeListener": remove_listener,
            "eventSubscribe": event_add,
            "eventUnsubscribe": event_remove,
            "updateMethods": update_methods,
        },
        "notes": [
            "Lifecycle inventories are review inputs, not a proof based on raw counts.",
            "Update allocation and subscription symmetry conclusions are recorded in runtime-risk-and-lifecycle-audit.md.",
        ],
    }
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text(json.dumps(report, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    print(
        f"[{report['status'].upper()}] forbidden={len(forbidden)}, "
        f"debugHotkeys={len(debug_hotkeys)}, unapprovedMutableStatics={len(mutable_statics)}, "
        f"approvedMutableStatics={len(allowed_statics)}, updateMethods={len(update_methods)}"
    )
    return 0 if passed else 1


if __name__ == "__main__":
    raise SystemExit(main())
