#!/usr/bin/env python3
"""Reconcile every source ColorTiming asset with one target disposition and GUID."""

from __future__ import annotations

import argparse
import csv
import json
import re
from collections import Counter, defaultdict
from datetime import datetime, timezone
from pathlib import Path


GUID_RE = re.compile(r"^guid:\s*([0-9a-f]{32})\s*$", re.MULTILINE)
REMOVED_ASSETS = {
    "Assets/Game/Scripts/PlayerInput.cs": "empty prototype; replaced by IGameInput",
    "Assets/Game/Scripts/Weapon_Hero.cs": "empty prototype",
    "Assets/Game/Scripts/Skill/Skill_Jiandao.cs": "empty prototype; formal attack path retained elsewhere",
    "Assets/Game/Scripts/Anim/PlayAnimation.cs": "unreferenced no-op prototype",
    "Assets/Game/Scripts/Anim/AnimStateMachine_DMD.cs": "unreferenced animation prototype",
    "Assets/Game/Scripts/ZZZZZZZZZZ.cs": "unreferenced Spine bone debug probe",
    "Assets/Game/Scripts/GameManager.cs": "replaced by persistent IColorTimingSettings/GF.Setting",
}


def norm(value: str) -> str:
    return value.replace("\\", "/")


def read_csv(path: Path) -> list[dict[str, str]]:
    with path.open("r", encoding="utf-8-sig", newline="") as stream:
        return list(csv.DictReader(stream))


def meta_guid(asset_path: Path) -> str:
    meta = Path(f"{asset_path}.meta")
    if not meta.is_file():
        return ""
    match = GUID_RE.search(meta.read_text(encoding="utf-8", errors="replace"))
    return match.group(1) if match else ""


def audit(source_manifest: Path, migration_manifest: Path, target: Path) -> dict:
    source_rows = read_csv(source_manifest)
    migration_rows = read_csv(migration_manifest)
    mappings: dict[str, list[dict[str, str]]] = defaultdict(list)
    for row in migration_rows:
        mappings[norm(row["Source"])].append(row)

    failures: list[str] = []
    dispositions: list[dict[str, str]] = []
    for source in source_rows:
        source_path = norm(source["path"])
        expected_guid = source.get("guid", "").lower()
        rows = mappings.get(source_path, [])

        if source_path in REMOVED_ASSETS:
            if any((target / norm(row["Target"])).is_file() for row in rows):
                failures.append(f"removed asset still exists at mapped target: {source_path}")
            dispositions.append({
                "source": source_path,
                "guid": expected_guid,
                "disposition": "removed",
                "target": "",
                "reason": REMOVED_ASSETS[source_path],
            })
            continue

        if len(rows) != 1:
            failures.append(f"source asset has {len(rows)} migration mappings: {source_path}")
            continue

        target_relative = norm(rows[0]["Target"])
        target_path = target / target_relative
        if not target_path.is_file():
            failures.append(f"mapped target is missing: {source_path} -> {target_relative}")
            continue

        actual_guid = meta_guid(target_path)
        if expected_guid and actual_guid != expected_guid:
            failures.append(
                f"GUID mismatch: {source_path} ({expected_guid}) -> {target_relative} ({actual_guid or 'missing'})"
            )
        dispositions.append({
            "source": source_path,
            "guid": expected_guid,
            "disposition": "migrated",
            "target": target_relative,
            "reason": "GUID-preserving migration",
        })

    mapped_targets = [norm(row["Target"]) for row in migration_rows]
    duplicate_mapping_targets = sorted(
        value for value, count in Counter(mapped_targets).items() if count > 1
    )
    for value in duplicate_mapping_targets:
        failures.append(f"duplicate migration target: {value}")

    guid_paths: dict[str, list[str]] = defaultdict(list)
    for meta in (target / "Assets").rglob("*.meta"):
        match = GUID_RE.search(meta.read_text(encoding="utf-8", errors="replace"))
        if match:
            guid_paths[match.group(1)].append(meta.relative_to(target).as_posix())
    duplicate_guids = {
        guid: paths for guid, paths in sorted(guid_paths.items()) if len(paths) > 1
    }
    for guid, paths in duplicate_guids.items():
        failures.append(f"duplicate target GUID {guid}: {', '.join(paths)}")

    removed_guid_leaks = []
    for item in dispositions:
        if item["disposition"] == "removed" and item["guid"] in guid_paths:
            removed_guid_leaks.append({"guid": item["guid"], "paths": guid_paths[item["guid"]]})
            failures.append(
                f"removed GUID still exists in target {item['guid']}: {', '.join(guid_paths[item['guid']])}"
            )

    disposition_counts = Counter(item["disposition"] for item in dispositions)
    if len(dispositions) != len(source_rows):
        failures.append(
            f"only {len(dispositions)} of {len(source_rows)} source assets received a disposition"
        )

    return {
        "status": "PASS" if not failures else "FAIL",
        "generatedAtUtc": datetime.now(timezone.utc).isoformat(),
        "sourceAssetCount": len(source_rows),
        "dispositionCount": len(dispositions),
        "migratedCount": disposition_counts["migrated"],
        "removedCount": disposition_counts["removed"],
        "migrationRecordCount": len(migration_rows),
        "targetGuidCount": len(guid_paths),
        "duplicateMappingTargets": duplicate_mapping_targets,
        "duplicateTargetGuids": duplicate_guids,
        "removedGuidLeaks": removed_guid_leaks,
        "failures": failures,
        "removedAssets": [item for item in dispositions if item["disposition"] == "removed"],
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--source-manifest", type=Path,
        default=Path("Documentation/Refactor/Baseline/source-assets.csv"),
    )
    parser.add_argument(
        "--migration-manifest", type=Path,
        default=Path("Documentation/Refactor/Baseline/migrated-assets.csv"),
    )
    parser.add_argument("--target", type=Path, default=Path("."))
    parser.add_argument(
        "--output", type=Path,
        default=Path("Documentation/Refactor/asset-reconciliation.json"),
    )
    args = parser.parse_args()

    result = audit(args.source_manifest, args.migration_manifest, args.target.resolve())
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
    print(
        f"[{result['status']}] {result['dispositionCount']}/{result['sourceAssetCount']} dispositions; "
        f"migrated={result['migratedCount']}, removed={result['removedCount']}, "
        f"duplicateTargetGuids={len(result['duplicateTargetGuids'])}"
    )
    for failure in result["failures"]:
        print(f"- {failure}")
    return 0 if result["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
