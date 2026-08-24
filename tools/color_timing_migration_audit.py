#!/usr/bin/env python3
"""Generate repeatable, read-only source evidence for the ColorTiming migration."""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import re
import subprocess
from collections import Counter, defaultdict
from pathlib import Path
from typing import Iterable


TEXT_REFERENCE_SUFFIXES = {
    ".unity",
    ".prefab",
    ".asset",
    ".controller",
    ".anim",
    ".overridecontroller",
    ".playable",
    ".mat",
}
PRODUCT_SCRIPT_ROOTS = ("Assets/Game/Scripts/", "Assets/Editor/")
GUID_RE = re.compile(r"\bguid:\s*([0-9a-f]{32})\b")
CLASS_RE = re.compile(
    r"\b(?:class|interface|struct|enum)\s+([A-Za-z_][A-Za-z0-9_]*)"
)
METHOD_RE = re.compile(r"^\s*m_MethodName:\s*(.*?)\s*$", re.MULTILINE)
ANIMATION_EVENT_RE = re.compile(r"^\s*functionName:\s*(.*?)\s*$", re.MULTILINE)


def normalized(path: Path, root: Path) -> str:
    return path.relative_to(root).as_posix()


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8", errors="replace")


def meta_guid(asset: Path) -> str:
    meta = Path(f"{asset}.meta")
    if not meta.is_file():
        return ""
    match = GUID_RE.search(read_text(meta))
    return match.group(1) if match else ""


def git_output(root: Path, *args: str) -> str:
    result = subprocess.run(
        ["git", "-c", "core.fsmonitor=false", "-C", str(root), *args],
        check=False,
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    return (result.stdout + result.stderr).strip()


def write_csv(path: Path, fieldnames: list[str], rows: Iterable[dict]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def load_guid_index(assets: Path) -> dict[str, str]:
    result: dict[str, str] = {}
    for meta in assets.rglob("*.meta"):
        match = GUID_RE.search(read_text(meta))
        if match:
            result[match.group(1)] = normalized(Path(str(meta)[:-5]), assets.parent)
    return result


def collect_serialized_guid_references(assets: Path) -> tuple[Counter, dict[str, set[str]]]:
    counts: Counter = Counter()
    files: dict[str, set[str]] = defaultdict(set)
    for path in assets.rglob("*"):
        if not path.is_file() or path.suffix.lower() not in TEXT_REFERENCE_SUFFIXES:
            continue
        text = read_text(path)
        rel = normalized(path, assets.parent)
        for guid in GUID_RE.findall(text):
            counts[guid] += 1
            files[guid].add(rel)
    return counts, files


def asset_category(path: Path) -> str:
    suffix = path.suffix.lower()
    if suffix == ".cs":
        return "script"
    if suffix == ".unity":
        return "scene"
    if suffix == ".prefab":
        return "prefab"
    if suffix in {".controller", ".overridecontroller"}:
        return "animator-controller"
    if suffix == ".anim":
        return "animation-clip"
    if suffix in {".png", ".jpg", ".jpeg", ".tga", ".psd", ".exr"}:
        return "image"
    if suffix in {".wav", ".mp3", ".ogg", ".aiff"}:
        return "audio"
    if suffix in {".mp4", ".webm", ".mov"}:
        return "video"
    if suffix in {".ttf", ".otf"}:
        return "font"
    if suffix == ".mat":
        return "material"
    if suffix in {".shader", ".shadergraph", ".cginc", ".hlsl"}:
        return "shader"
    if suffix in {".json", ".skel", ".bytes", ".atlas", ".txt"} and "spine" in path.as_posix().lower():
        return "spine"
    return "other"


def project_snapshot(root: Path) -> dict:
    version_file = root / "ProjectSettings/ProjectVersion.txt"
    package_file = root / "Packages/manifest.json"
    build_file = root / "ProjectSettings/EditorBuildSettings.asset"
    graphics_file = root / "ProjectSettings/GraphicsSettings.asset"
    project_file = root / "ProjectSettings/ProjectSettings.asset"
    quality_file = root / "ProjectSettings/QualitySettings.asset"

    version_text = read_text(version_file) if version_file.is_file() else ""
    package_data = json.loads(read_text(package_file)) if package_file.is_file() else {}
    build_text = read_text(build_file) if build_file.is_file() else ""
    scene_rows = [
        {"enabled": int(enabled), "path": path, "guid": guid}
        for enabled, path, guid in re.findall(
            r"- enabled:\s*(\d+)\s+path:\s*(.*?)\s+guid:\s*([0-9a-f]{32})",
            build_text,
            re.DOTALL,
        )
    ]
    settings_hashes = {
        normalized(path, root): sha256(path)
        for path in sorted((root / "ProjectSettings").rglob("*"))
        if path.is_file()
    }

    def selected_lines(path: Path, patterns: list[str]) -> list[str]:
        if not path.is_file():
            return []
        result = []
        for line in read_text(path).splitlines():
            if any(pattern in line for pattern in patterns):
                result.append(line.strip())
        return result

    return {
        "path": str(root),
        "git_branch": git_output(root, "branch", "--show-current"),
        "git_head": git_output(root, "rev-parse", "HEAD"),
        "git_status": git_output(root, "status", "--short"),
        "unity_version_file": version_text.strip(),
        "packages": package_data.get("dependencies", {}),
        "build_scenes": scene_rows,
        "graphics_pipeline_lines": selected_lines(
            graphics_file,
            ["m_CustomRenderPipeline", "m_RenderPipelineGlobalSettingsMap"],
        ),
        "project_render_lines": selected_lines(
            project_file,
            ["colorSpace", "activeInputHandler", "defaultScreenWidth", "defaultScreenHeight"],
        ),
        "quality_pipeline_lines": selected_lines(quality_file, ["customRenderPipeline"]),
        "project_settings_sha256": settings_hashes,
    }


def scene_inventory(scene: Path, project: Path, guid_index: dict[str, str]) -> dict:
    text = read_text(scene)
    class_ids = Counter(re.findall(r"^--- !u!(\d+) &", text, re.MULTILINE))
    script_guids = re.findall(
        r"m_Script:\s*\{fileID:\s*11500000,\s*guid:\s*([0-9a-f]{32}),\s*type:\s*3\}",
        text,
    )
    names = re.findall(r"^\s*m_Name:\s*(.*?)\s*$", text, re.MULTILINE)
    methods = [item for item in METHOD_RE.findall(text) if item]
    return {
        "path": normalized(scene, project),
        "guid": meta_guid(scene),
        "sha256": sha256(scene),
        "game_objects": class_ids.get("1", 0),
        "mono_behaviours": class_ids.get("114", 0),
        "component_class_counts": dict(sorted(class_ids.items(), key=lambda item: int(item[0]))),
        "object_names": names,
        "script_guid_counts": dict(Counter(script_guids)),
        "script_paths": sorted({guid_index.get(guid, f"UNRESOLVED:{guid}") for guid in script_guids}),
        "persistent_event_methods": methods,
    }


def spine_events(path: Path) -> list[str]:
    if path.suffix.lower() != ".json":
        return []
    try:
        payload = json.loads(read_text(path))
    except json.JSONDecodeError:
        return []
    events = set(payload.get("events", {}).keys()) if isinstance(payload, dict) else set()
    return sorted(str(item) for item in events)


def generate(source: Path, target: Path, output: Path) -> None:
    source_assets = source / "Assets"
    target_assets = target / "Assets"
    output.mkdir(parents=True, exist_ok=True)

    source_guid_index = load_guid_index(source_assets)
    target_guid_index = load_guid_index(target_assets)
    serialized_counts, serialized_files = collect_serialized_guid_references(source_assets)

    snapshot = {
        "source": project_snapshot(source),
        "target": project_snapshot(target),
    }
    (output / "project-snapshot.json").write_text(
        json.dumps(snapshot, indent=2, ensure_ascii=False), encoding="utf-8"
    )

    source_files = [path for path in source_assets.rglob("*") if path.is_file() and path.suffix.lower() != ".meta"]
    asset_rows = []
    category_counts: Counter = Counter()
    for path in sorted(source_files):
        category = asset_category(path)
        category_counts[category] += 1
        asset_rows.append(
            {
                "path": normalized(path, source),
                "category": category,
                "extension": path.suffix.lower(),
                "bytes": path.stat().st_size,
                "sha256": sha256(path),
                "guid": meta_guid(path),
            }
        )
    write_csv(
        output / "source-assets.csv",
        ["path", "category", "extension", "bytes", "sha256", "guid"],
        asset_rows,
    )

    script_paths = [
        path
        for path in source_assets.rglob("*.cs")
        if normalized(path, source).startswith(PRODUCT_SCRIPT_ROOTS)
    ]
    all_script_text = {path: read_text(path) for path in script_paths}
    script_rows = []
    for path in sorted(script_paths):
        rel = normalized(path, source)
        text = all_script_text[path]
        guid = meta_guid(path)
        symbols = CLASS_RE.findall(text)
        other_code = "\n".join(value for key, value in all_script_text.items() if key != path)
        symbol_references = sum(
            len(re.findall(rf"\b{re.escape(symbol)}\b", other_code)) for symbol in symbols
        )
        ref_files = sorted(serialized_files.get(guid, set()))
        script_rows.append(
            {
                "path": rel,
                "guid": guid,
                "bytes": path.stat().st_size,
                "lines": len(text.splitlines()),
                "sha256": sha256(path),
                "symbols": ";".join(symbols),
                "code_symbol_references": symbol_references,
                "serialized_reference_count": serialized_counts.get(guid, 0),
                "serialized_reference_files": ";".join(ref_files),
            }
        )
    write_csv(
        output / "source-scripts.csv",
        [
            "path",
            "guid",
            "bytes",
            "lines",
            "sha256",
            "symbols",
            "code_symbol_references",
            "serialized_reference_count",
            "serialized_reference_files",
        ],
        script_rows,
    )

    scenes = [
        scene_inventory(path, source, source_guid_index)
        for path in sorted(source_assets.rglob("*.unity"))
    ]
    (output / "source-scenes.json").write_text(
        json.dumps(scenes, indent=2, ensure_ascii=False), encoding="utf-8"
    )

    event_rows = []
    for path in sorted(source_assets.rglob("*.anim")):
        text = read_text(path)
        for event in ANIMATION_EVENT_RE.findall(text):
            if event:
                event_rows.append(
                    {"clip": normalized(path, source), "clip_guid": meta_guid(path), "event": event}
                )
    write_csv(output / "animation-events.csv", ["clip", "clip_guid", "event"], event_rows)

    unity_event_rows = []
    for path in sorted(source_assets.rglob("*")):
        if not path.is_file() or path.suffix.lower() not in {".unity", ".prefab"}:
            continue
        for method in METHOD_RE.findall(read_text(path)):
            if method:
                unity_event_rows.append({"asset": normalized(path, source), "method": method})
    write_csv(output / "unityevent-methods.csv", ["asset", "method"], unity_event_rows)

    spine_rows = []
    for path in sorted(source_assets.rglob("*")):
        if not path.is_file():
            continue
        rel_lower = normalized(path, source).lower()
        if "spine" not in rel_lower and "boss" not in rel_lower:
            continue
        if path.suffix.lower() not in {".json", ".skel", ".atlas", ".txt", ".bytes"}:
            continue
        spine_rows.append(
            {
                "path": normalized(path, source),
                "guid": meta_guid(path),
                "bytes": path.stat().st_size,
                "sha256": sha256(path),
                "events": ";".join(spine_events(path)),
            }
        )
    write_csv(output / "spine-assets.csv", ["path", "guid", "bytes", "sha256", "events"], spine_rows)

    material_rows = []
    for path in sorted(source_assets.rglob("*.mat")):
        text = read_text(path)
        match = re.search(r"m_Shader:\s*\{[^}]*guid:\s*([0-9a-f]{32})", text)
        shader_guid = match.group(1) if match else ""
        material_rows.append(
            {
                "path": normalized(path, source),
                "guid": meta_guid(path),
                "shader_guid": shader_guid,
                "shader_path": source_guid_index.get(shader_guid, "BUILTIN_OR_UNRESOLVED"),
            }
        )
    write_csv(
        output / "materials.csv",
        ["path", "guid", "shader_guid", "shader_path"],
        material_rows,
    )

    collisions = []
    for guid in sorted(set(source_guid_index).intersection(target_guid_index)):
        collisions.append(
            {
                "guid": guid,
                "source_path": source_guid_index[guid],
                "target_path": target_guid_index[guid],
            }
        )
    write_csv(output / "pre-migration-guid-collisions.csv", ["guid", "source_path", "target_path"], collisions)

    summary_lines = [
        "# ColorTiming 源基线审计摘要",
        "",
        f"- 源项目：`{source}`",
        f"- 目标项目：`{target}`",
        f"- 源资产文件：{len(asset_rows)}",
        f"- 产品/编辑器自有脚本：{len(script_rows)}",
        f"- 场景：{len(scenes)}",
        f"- Prefab：{category_counts['prefab']}",
        f"- Animator Controller：{category_counts['animator-controller']}",
        f"- Animation Clip：{category_counts['animation-clip']}",
        f"- 图片：{category_counts['image']}",
        f"- 音频：{category_counts['audio']}",
        f"- 视频：{category_counts['video']}",
        f"- Animation Event 记录：{len(event_rows)}",
        f"- UnityEvent 持久方法绑定：{len(unity_event_rows)}",
        f"- 迁移前 GUID 交集：{len(collisions)}",
        "",
        "## 分类计数",
        "",
    ]
    summary_lines.extend(f"- {key}: {value}" for key, value in sorted(category_counts.items()))
    (output / "README.md").write_text("\n".join(summary_lines) + "\n", encoding="utf-8")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--target", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    generate(args.source.resolve(), args.target.resolve(), args.output.resolve())
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
