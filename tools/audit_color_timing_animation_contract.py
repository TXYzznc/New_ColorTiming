#!/usr/bin/env python3
"""Audit ColorTiming Animator and Animation Event compatibility contracts."""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path


REQUIRED_PARAMETERS = {
    "moveSpeed",
    "moveV",
    "weaponType",
    "switchWeapon",
    "Dash",
    "Atk",
    "Atk_x",
    "Hit",
    "Death",
}

RECEIVERS = {
    "Attack": "Assets/Game/Scripts/ColorTiming/Presentation/Actors/Player/PlayerAnimationEventRelay.cs",
    "PlayAuido": "Assets/Game/Scripts/ColorTiming/Presentation/Actors/Player/PlayerSoundView.cs",
    "PlayAuido_Random": "Assets/Game/Scripts/ColorTiming/Presentation/Actors/Player/PlayerSoundView.cs",
    "DashWD": "Assets/Game/Scripts/ColorTiming/Presentation/Actors/Player/PlayerAnimationEventRelay.cs",
    "DashEnd": "Assets/Game/Scripts/ColorTiming/Presentation/Actors/Player/PlayerAnimationEventRelay.cs",
    "SkillMove": "Assets/Game/Scripts/ColorTiming/Presentation/Actors/Player/PlayerAnimationEventRelay.cs",
    "Wudi": "Assets/Game/Scripts/ColorTiming/Presentation/Actors/Player/PlayerAnimationEventRelay.cs",
    "Hit": "Assets/Game/Scripts/ColorTiming/Presentation/Actors/Player/PlayerAnimationEventRelay.cs",
    "DeathOver": "Assets/Game/Scripts/ColorTiming/Presentation/Actors/Player/PlayerDeathSequenceView.cs",
    "EventEnd_Destroy": "Assets/Game/Scripts/ColorTiming/Presentation/Combat/Skills/Skill_base.cs",
    "OnFXEnd": "Assets/Game/Scripts/ColorTiming/Presentation/Combat/Skills/HitFX_Base.cs",
    "Cerate": "Assets/Game/Scripts/ColorTiming/Presentation/Combat/Skills/Skill_Bo1_Atk5_Item.cs",
    "End": "Assets/Game/Scripts/ColorTiming/Presentation/Combat/Skills/Skill_Bo1_Atk5_Item.cs",
}

BEHAVIOURS = {
    "EnterAnimStateEvent": (
        "Assets/Game/Scripts/ColorTiming/Presentation/Actors/Player/EnterAnimStateEvent.cs.meta",
        "Assets/Game/Sprites/ColorTiming/Hero/Animations/Hero.controller",
    ),
}


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8-sig", errors="replace")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path.cwd())
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    root = args.root.resolve()
    failures: list[str] = []

    controller_path = root / "Assets/Game/Sprites/ColorTiming/Hero/Animations/Hero.controller"
    controller = read(controller_path)
    parameters = set(re.findall(r"^\s*- m_Name: (\S+)\s*$", controller, re.MULTILINE))
    missing_parameters = sorted(REQUIRED_PARAMETERS - parameters)
    if missing_parameters:
        failures.append(f"Animator parameters missing: {', '.join(missing_parameters)}")

    held_thresholds = re.findall(
        r"m_ConditionEvent: Atk_x\s*\r?\n\s*m_EventTreshold: ([0-9.\-]+)", controller
    )
    if not held_thresholds or any(float(value) != 0.1 for value in held_thresholds):
        failures.append("Atk_x transitions do not consistently preserve the 0.1 threshold")

    behaviour_evidence = {}
    for name, (meta_relative, controller_relative) in BEHAVIOURS.items():
        meta = read(root / meta_relative)
        match = re.search(r"^guid: ([0-9a-f]{32})$", meta, re.MULTILINE)
        if not match:
            failures.append(f"{name} meta GUID is missing")
            continue
        guid = match.group(1)
        count = read(root / controller_relative).count(guid)
        behaviour_evidence[name] = {
            "guid": guid,
            "controller": controller_relative,
            "reference_count": count,
        }
        if count == 0:
            failures.append(f"{name} is not referenced by {controller_relative}")

    event_counts: dict[str, int] = {}
    for animation in (root / "Assets/Game").rglob("*.anim"):
        for event_name in re.findall(r"^\s*functionName: (\S+)\s*$", read(animation), re.MULTILINE):
            event_counts[event_name] = event_counts.get(event_name, 0) + 1

    receiver_evidence = {}
    for event_name, script_relative in RECEIVERS.items():
        script = read(root / script_relative)
        method_exists = re.search(rf"\b{re.escape(event_name)}\s*\(", script) is not None
        receiver_evidence[event_name] = {
            "animation_event_count": event_counts.get(event_name, 0),
            "receiver": script_relative,
            "receiver_method_exists": method_exists,
        }
        if event_counts.get(event_name, 0) == 0:
            failures.append(f"No animation clip emits {event_name}")
        if not method_exists:
            failures.append(f"Receiver method {event_name} missing from {script_relative}")

    report = {
        "status": "PASS" if not failures else "FAIL",
        "animator": {
            "controller": controller_path.relative_to(root).as_posix(),
            "required_parameters": sorted(REQUIRED_PARAMETERS),
            "missing_parameters": missing_parameters,
            "atk_x_thresholds": sorted(set(held_thresholds)),
        },
        "state_machine_behaviours": behaviour_evidence,
        "animation_events": receiver_evidence,
        "failures": failures,
    }
    rendered = json.dumps(report, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        output = args.output if args.output.is_absolute() else root / args.output
        output.parent.mkdir(parents=True, exist_ok=True)
        output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if not failures else 1


if __name__ == "__main__":
    raise SystemExit(main())
