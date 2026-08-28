#!/usr/bin/env python3
"""Create or compare the protected ColorTiming art and serialized-event manifest."""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import re
from collections import Counter
from pathlib import Path


RAW_EXTENSIONS = {
    ".anim", ".controller", ".mat", ".shader", ".shadergraph",
    ".png", ".jpg", ".jpeg", ".tga", ".psd", ".bmp",
    ".wav", ".mp3", ".ogg", ".aiff", ".mp4", ".mov",
    ".ttf", ".otf", ".asset", ".atlas", ".txt", ".json", ".bytes",
    ".playable", ".timeline",
}
SERIALIZED_EXTENSIONS = {".prefab", ".unity"}
GUID = re.compile(r"^guid:\s*([0-9a-f]{32})\s*$", re.MULTILINE)
ANIMATION_EVENT = re.compile(r"^\s*functionName:\s*(\S+)\s*$", re.MULTILINE)
UNITY_EVENT = re.compile(r"^\s*m_MethodName:\s*(\S+)\s*$", re.MULTILINE)
REFERENCE = re.compile(r"guid:\s*([0-9a-f]{32})")


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8-sig", errors="replace")


def migrated_targets(root: Path, migration_csv: Path) -> list[Path]:
    with migration_csv.open("r", encoding="utf-8-sig", newline="") as stream:
        rows = list(csv.DictReader(stream))
    values = []
    for row in rows:
        relative = row["Target"].replace("\\", "/")
        if relative.endswith(".meta"):
            continue
        path = root / relative
        if path.is_file() and path.suffix.lower() in RAW_EXTENSIONS | SERIALIZED_EXTENSIONS:
            values.append(path)
    # Runtime-created GF UI/Entity prefabs and rewritten scenes may not be source-path-identical.
    for pattern in ("Assets/Game/Prefabs/**/*.prefab", "Assets/Game/Scene/*.unity"):
        values.extend(root.glob(pattern))
    return sorted(set(values))


def record(root: Path, path: Path) -> dict[str, object]:
    relative = path.relative_to(root).as_posix()
    meta = Path(f"{path}.meta")
    meta_text = read_text(meta) if meta.is_file() else ""
    guid_match = GUID.search(meta_text)
    extension = path.suffix.lower()
    item: dict[str, object] = {
        "path": relative,
        "kind": "serialized" if extension in SERIALIZED_EXTENSIONS else "raw",
        "extension": extension,
        "guid": guid_match.group(1) if guid_match else "",
        "contentSha256": sha256(path),
        "metaSha256": sha256(meta) if meta.is_file() else "",
    }
    if extension in {".anim", ".controller", ".prefab", ".unity", ".asset", ".mat"}:
        text = read_text(path)
        item["referencedGuids"] = sorted(Counter(REFERENCE.findall(text)).items())
        item["animationEvents"] = sorted(Counter(ANIMATION_EVENT.findall(text)).items())
        item["unityEvents"] = sorted(Counter(UNITY_EVENT.findall(text)).items())
    return item


def build(root: Path, migration_csv: Path) -> dict[str, object]:
    assets = [record(root, path) for path in migrated_targets(root, migration_csv)]
    return {
        "schemaVersion": 1,
        "assetCount": len(assets),
        "rawCount": sum(item["kind"] == "raw" for item in assets),
        "serializedCount": sum(item["kind"] == "serialized" for item in assets),
        "animationEventCount": sum(sum(count for _, count in item.get("animationEvents", [])) for item in assets),
        "unityEventCount": sum(sum(count for _, count in item.get("unityEvents", [])) for item in assets),
        "assets": assets,
    }


def compare(baseline: dict[str, object], current: dict[str, object]) -> dict[str, object]:
    old = {item["path"]: item for item in baseline["assets"]}
    new = {item["path"]: item for item in current["assets"]}
    failures: list[str] = []
    allowed_serialized_changes: list[str] = []
    for path, before in old.items():
        after = new.get(path)
        if after is None:
            failures.append(f"protected asset missing: {path}")
            continue
        if before["guid"] != after["guid"]:
            failures.append(f"GUID changed: {path}")
        if before["kind"] == "raw":
            if before["contentSha256"] != after["contentSha256"]:
                failures.append(f"raw content changed: {path}")
            if before["metaSha256"] != after["metaSha256"]:
                failures.append(f"raw importer/meta changed: {path}")
        elif before["contentSha256"] != after["contentSha256"] or before["metaSha256"] != after["metaSha256"]:
            allowed_serialized_changes.append(path)
    return {
        "status": "PASS" if not failures else "FAIL",
        "failures": failures,
        "allowedSerializedChangesRequiringReview": allowed_serialized_changes,
        "baselineCounts": {key: baseline[key] for key in ("assetCount", "rawCount", "serializedCount", "animationEventCount", "unityEventCount")},
        "currentCounts": {key: current[key] for key in ("assetCount", "rawCount", "serializedCount", "animationEventCount", "unityEventCount")},
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path.cwd())
    parser.add_argument(
        "--migration-csv", type=Path,
        default=Path("openspec/changes/migrate-color-timing-to-ai-friendly-framework/evidence/inputs/migrated-assets.csv"),
    )
    parser.add_argument("--baseline", type=Path, required=True)
    parser.add_argument("--write-baseline", action="store_true")
    parser.add_argument("--report", type=Path)
    args = parser.parse_args()
    root = args.root.resolve()
    migration_csv = args.migration_csv if args.migration_csv.is_absolute() else root / args.migration_csv
    baseline_path = args.baseline if args.baseline.is_absolute() else root / args.baseline
    current = build(root, migration_csv)
    if args.write_baseline:
        baseline_path.parent.mkdir(parents=True, exist_ok=True)
        baseline_path.write_text(json.dumps(current, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(f"[BASELINE] assets={current['assetCount']} raw={current['rawCount']} serialized={current['serializedCount']} animationEvents={current['animationEventCount']} unityEvents={current['unityEventCount']}")
        return 0
    baseline = json.loads(baseline_path.read_text(encoding="utf-8"))
    result = compare(baseline, current)
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.report:
        report = args.report if args.report.is_absolute() else root / args.report
        report.parent.mkdir(parents=True, exist_ok=True)
        report.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if result["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
