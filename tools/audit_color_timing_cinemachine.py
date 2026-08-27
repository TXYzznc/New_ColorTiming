#!/usr/bin/env python3
"""Compare source/target Cinemachine serialization and required runtime bridges."""

from __future__ import annotations

import json
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SOURCE = Path(r"D:\unity\UnityProject\ColorTimeing\ColorTimeing")
OUTPUT = ROOT / "openspec/changes/migrate-color-timing-to-ai-friendly-framework/evidence/cinemachine-audit.json"
GUIDS = {
    "VirtualCamera": "45e653bab7fb20e499bda25e1b646fea",
    "Confiner2D": "f453f694addf4275988fac205bc91968",
    "ImpulseSource": "180ecf9b41d478f468eb3e9083753217",
    "ImpulseListener": "00b2d199b96b516448144ab30fb26aed",
    "FramingTransposer": "6ad980451443d70438faac0bc6c235a0",
    "Brain": "72ece51f2901e7445ab60da3685d6b5f",
}


def package_version(project: Path) -> str | None:
    manifest = json.loads((project / "Packages/manifest.json").read_text(encoding="utf-8-sig"))
    return manifest.get("dependencies", {}).get("com.unity.cinemachine")


def blocks(path: Path, guid: str) -> list[str]:
    text = path.read_text(encoding="utf-8-sig", errors="replace").replace("\r\n", "\n")
    return [block.strip() for block in re.split(r"(?=^--- !u!)", text, flags=re.MULTILINE) if guid in block]


def semantic_blocks(path: Path, guid: str) -> list[str]:
    values = blocks(path, guid)
    if guid == GUIDS["Confiner2D"]:
        # Cinemachine 2.10.3 writes its newly serialized default padding field when
        # Unity resaves an older scene. Missing in the source and explicit zero are equivalent.
        values = [re.sub(r"^  m_Padding: 0\n?", "", value, flags=re.MULTILINE).strip() for value in values]
    return values


def main() -> int:
    comparisons = []
    failures = []
    for scene in ("Boss1", "Boss2"):
        source_scene = SOURCE / f"Assets/Scenes/{scene}.unity"
        target_scene = ROOT / f"Assets/Game/Scene/{scene}.unity"
        for component, guid in GUIDS.items():
            source_blocks = semantic_blocks(source_scene, guid)
            target_blocks = semantic_blocks(target_scene, guid)
            exact = source_blocks == target_blocks
            item = {
                "scene": scene,
                "component": component,
                "guid": guid,
                "sourceCount": len(source_blocks),
                "targetCount": len(target_blocks),
                "serializedBlockExactMatch": exact,
            }
            comparisons.append(item)
            if len(source_blocks) != 1 or len(target_blocks) != 1 or not exact:
                failures.append(item)

    source_version = package_version(SOURCE)
    target_version = package_version(ROOT)
    if source_version != target_version or target_version != "2.10.3":
        failures.append({"packageVersion": {"source": source_version, "target": target_version}})

    runtime_checks = {
        "deathCameraDisablesConfiner": "GetComponentInParent<CinemachineConfiner2D>().enabled = false;",
        "deathCameraDisablesImpulseListener": "GetComponentInParent<CinemachineImpulseListener>().enabled = false;",
        "heroHitGeneratesImpulse": "impulseSource?.GenerateImpulse();",
    }
    runtime_text = (
        (ROOT / "Assets/Game/Scripts/ColorTiming/Legacy/Death_sc_Over.cs").read_text(encoding="utf-8-sig")
        + (ROOT / "Assets/Game/Scripts/ColorTiming/Legacy/HeroAnimStae.cs").read_text(encoding="utf-8-sig")
        + (ROOT / "Assets/Game/Scripts/ColorTiming/Legacy/Skill/Skill_Zhadan.cs").read_text(encoding="utf-8-sig")
    )
    runtime_results = {name: snippet in runtime_text for name, snippet in runtime_checks.items()}
    for name, passed in runtime_results.items():
        if not passed:
            failures.append({"runtimeBridge": name})

    report = {
        "status": "pass" if not failures else "fail",
        "packageVersion": {"source": source_version, "target": target_version},
        "components": comparisons,
        "runtimeBridges": runtime_results,
        "failures": failures,
    }
    OUTPUT.write_text(json.dumps(report, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(
        f"[{report['status'].upper()}] components={len(comparisons)}, "
        f"exact={sum(item['serializedBlockExactMatch'] for item in comparisons)}, "
        f"runtimeBridges={sum(runtime_results.values())}/{len(runtime_results)}"
    )
    return 0 if not failures else 1


if __name__ == "__main__":
    raise SystemExit(main())
