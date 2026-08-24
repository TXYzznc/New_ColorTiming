#!/usr/bin/env python3
"""Fail when ColorTiming code bypasses the semantic input/camera adapters."""

from __future__ import annotations

import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SCRIPTS = ROOT / "Assets" / "Game" / "Scripts" / "ColorTiming"
ADAPTERS = SCRIPTS / "Input" / "Adapters"

DIRECT_INPUT = re.compile(
    r"(?:UnityEngine\.)?Input\."
    r"(?:GetAxis|GetAxisRaw|GetButton|GetButtonDown|GetKey|GetKeyDown|"
    r"GetMouseButton|GetMouseButtonDown|GetMouseButtonUp|mousePosition|anyKey|anyKeyDown)"
)
CAMERA_MAIN = re.compile(r"\bCamera\.main\b")


def main() -> int:
    findings: list[str] = []
    for path in sorted(SCRIPTS.rglob("*.cs")):
        if path.is_relative_to(ADAPTERS):
            continue
        for number, line in enumerate(path.read_text(encoding="utf-8-sig").splitlines(), 1):
            stripped = line.strip()
            if stripped.startswith("//"):
                continue
            if DIRECT_INPUT.search(line):
                findings.append(f"{path.relative_to(ROOT).as_posix()}:{number}: direct Unity Input")
            if CAMERA_MAIN.search(line):
                findings.append(f"{path.relative_to(ROOT).as_posix()}:{number}: Camera.main")

    if findings:
        print("ColorTiming input boundary audit failed:")
        print("\n".join(findings))
        return 1

    print("[OK] ColorTiming input boundary audit passed")
    return 0


if __name__ == "__main__":
    sys.exit(main())
