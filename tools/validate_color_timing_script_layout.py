#!/usr/bin/env python3
"""Validate the ColorTiming product script layout and moved MonoScript GUIDs."""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path


PROJECT_ROOT = Path(__file__).resolve().parents[1]
SCRIPT_ROOT = PROJECT_ROOT / "Assets/Game/Scripts/ColorTiming"

MOVED_SCRIPTS = {
    "Presentation/UI/Forms/BattleHudForm.cs": ("2c65aad324ef23745a46fc57e6b9c791", "ColorTiming.Presentation.UI.Forms"),
    "Presentation/UI/Forms/BattleTutorialForm.cs": ("7e701e516cd388044937dfe7e8293255", "ColorTiming.Presentation.UI.Forms"),
    "Presentation/UI/Forms/ColorTimingLoadingForm.cs": ("d40a06f13dae1694a9e06ce3d971b930", "ColorTiming.Presentation.UI.Forms"),
    "Presentation/UI/Forms/MainMenuForm.cs": ("b9cd36b0d383e7f4487ecb2a57b8b5a2", "ColorTiming.Presentation.UI.Forms"),
    "Presentation/UI/Forms/PauseMenuForm.cs": ("83e88180789b9614699de8934fb8964d", "ColorTiming.Presentation.UI.Forms"),
    "Presentation/UI/Forms/BattleResultForm.cs": ("5089c843456c0e64697c28d4a4f662c6", "ColorTiming.Presentation.UI.Forms"),
    "Presentation/UI/Components/BattlePlayerInfoView.cs": ("10b5e2d63d3a11d4ab1a741699aa88d1", "ColorTiming.Presentation.UI.Components"),
    "Presentation/UI/Components/Boss1HealthView.cs": ("f1f2a4ac3fd25d54ab848adfe84f581d", "ColorTiming.Presentation.UI.Components"),
    "Presentation/UI/Components/Boss2HealthView.cs": ("fd7a939aa0a05004ab3d85017ad27c17", "ColorTiming.Presentation.UI.Components"),
    "Presentation/UI/Components/BossWeaknessPipView.cs": ("3b1b4e0ce86445441b3a77202a892f6e", "ColorTiming.Presentation.UI.Components"),
    "Presentation/UI/Components/PlayerHealthPipsView.cs": ("96bff7c2db6b2e643a2c57af27ea8def", "ColorTiming.Presentation.UI.Components"),
    "Presentation/UI/Components/PlayerHealthPipView.cs": ("b6a708d0191e2d143af4c0dcac0a152b", "ColorTiming.Presentation.UI.Components"),
    "Presentation/UI/Components/UiButtonSoundView.cs": ("3d7b885f386c53c41bf1c8fcdd697dd7", "ColorTiming.Presentation.UI.Components"),
    "Presentation/UI/Components/UiSoundView.cs": ("934366cbb0b605a4e8d82eae91179bfb", "ColorTiming.Presentation.UI.Components"),
    "Presentation/UI/Components/MainMenuIntroSequence.cs": ("d43edb29e88b0f544833b5bbb54d0669", "ColorTiming.Presentation.UI.Components"),
    "Presentation/UI/Presenters/BattleHudPresentation.cs": ("79705b77bc0647746a2da8f0c44c5cf0", "ColorTiming.Presentation.UI.Presenters"),
    "Presentation/UI/Models/BattlePresentationResult.cs": ("c434c9df05de411cbb5021d6198c4d2d", "ColorTiming.Presentation.UI.Models"),
    "Presentation/UI/Models/WeaponPresentationState.cs": ("f4b3bfdfb33d54e47a39f5966990a51f", "ColorTiming.Presentation.UI.Models"),
    "Presentation/UI/Contracts/IColorTimingUiService.cs": ("7642d8b4c74a42a6ac17560b9adab0d9", "ColorTiming.Presentation.UI.Contracts"),
    "Presentation/UI/Contracts/IPlayerDamageSignal.cs": ("d265887703884d25a75dbac00d0a94cc", "ColorTiming.Presentation.UI.Contracts"),
    "Infrastructure/GF/UI/GfColorTimingUiService.cs": ("d008de7b02b840bc84abb66e36deec8e", "ColorTiming.Infrastructure.GF.UI"),
    "Infrastructure/GF/Audio/GfColorTimingSoundService.cs": ("9012c53b53c545f599fbd641550a64d4", "ColorTiming.Infrastructure.GF.Audio"),
    "Infrastructure/GF/Entity/GfTransientEntityService.cs": ("120e3a578f9543599f911411f4ec67ef", "ColorTiming.Infrastructure.GF.Entity"),
    "Infrastructure/GF/Settings/GfColorTimingSettings.cs": ("3fcb46c7e3a546c59d35521144086294", "ColorTiming.Infrastructure.GF.Settings"),
    "Infrastructure/Unity/Input/LegacyGameInputAdapter.cs": ("900540d9b9ac9a84aaf7183e438a1603", "ColorTiming.Infrastructure.Unity.Input"),
    "Infrastructure/Unity/Input/GameplayPointerWorldAdapter.cs": ("e5409a4cb1f5a1a4d9cadb105d260137", "ColorTiming.Infrastructure.Unity.Input"),
    "Infrastructure/Unity/Time/UnityGameTimeAdapter.cs": ("69f4ff2e114f448f9530e47fa3d45d27", "ColorTiming.Infrastructure.Unity.Time"),
    "Presentation/Camera/ColorTimingUrpCameraStack.cs": ("271cbebbd1c179f40a612ffe0c3ae61b", "ColorTiming.Presentation.Camera"),
}

FORBIDDEN_DIRECTORIES = (
    "UI",
    "Player",
    "Bosses",
    "Bosses/Boss1",
    "Bosses/Boss2",
    "Presentation/UI/Views",
    "Input/Adapters",
    "Combat",
)


def read_guid(meta_path: Path) -> str | None:
    if not meta_path.is_file():
        return None
    match = re.search(r"^guid:\s*(\S+)", meta_path.read_text(encoding="utf-8"), re.MULTILINE)
    return match.group(1) if match else None


def validate() -> dict[str, object]:
    issues: list[str] = []
    checked: list[dict[str, str]] = []

    for relative_path, (expected_guid, expected_namespace) in MOVED_SCRIPTS.items():
        script_path = SCRIPT_ROOT / relative_path
        actual_guid = read_guid(Path(f"{script_path}.meta"))
        if not script_path.is_file():
            issues.append(f"missing script: {relative_path}")
            continue
        if actual_guid != expected_guid:
            issues.append(f"GUID mismatch: {relative_path}: expected {expected_guid}, got {actual_guid}")
        source = script_path.read_text(encoding="utf-8-sig")
        if not re.search(rf"^namespace\s+{re.escape(expected_namespace)}(?:\s*\{{|\s*;)", source, re.MULTILINE):
            issues.append(f"namespace mismatch: {relative_path}: expected {expected_namespace}")
        checked.append({"path": relative_path, "guid": actual_guid or "", "namespace": expected_namespace})

    for relative_path in FORBIDDEN_DIRECTORIES:
        directory = SCRIPT_ROOT / relative_path
        if directory.exists():
            issues.append(f"obsolete directory remains: {relative_path}")

    ui_root = SCRIPT_ROOT / "Presentation/UI"
    direct_scripts = sorted(path.name for path in ui_root.glob("*.cs")) if ui_root.is_dir() else []
    if direct_scripts:
        issues.append(f"unclassified UI scripts remain: {', '.join(direct_scripts)}")

    return {
        "passed": not issues,
        "scriptRoot": str(SCRIPT_ROOT.relative_to(PROJECT_ROOT)).replace("\\", "/"),
        "checkedScriptCount": len(checked),
        "issues": issues,
        "scripts": checked,
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--write", type=Path, help="Optional JSON evidence output path relative to the project root")
    args = parser.parse_args()

    report = validate()
    payload = json.dumps(report, ensure_ascii=False, indent=2) + "\n"
    print(payload, end="")
    if args.write:
        output = args.write if args.write.is_absolute() else PROJECT_ROOT / args.write
        output.parent.mkdir(parents=True, exist_ok=True)
        output.write_text(payload, encoding="utf-8")
    return 0 if report["passed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
