# Script layout pre-move baseline

- Captured: 2026-08-28 (Asia/Shanghai)
- Unity: 2022.3.62f3c1
- UnitySkills instance: `GameDesinger_189B1E1A`
- Working tree before this change: only `Assets/Game/ScriptsBuiltin/Editor/MigratedToolbox/ToolHubSettings.asset` was already modified and remains outside this change.
- Planned moved scripts: 28.
- Serialization contract: every destination must retain the source GUID recorded in `tools/validate_color_timing_script_layout.py`.
- Protected art baseline: reuse `../reimplement-color-timing-business-architecture/evidence/protected-assets-baseline.json`; this change does not authorize serialized art differences.

## Confirmed residue before migration

The following tracked directories contained no non-meta files:

- `Assets/Game/Scripts/ColorTiming/UI`
- `Assets/Game/Scripts/ColorTiming/Player`
- `Assets/Game/Scripts/ColorTiming/Bosses/Boss1`
- `Assets/Game/Scripts/ColorTiming/Bosses/Boss2`

`Presentation/UI/Views`, `Input/Adapters` and `Combat` are removed only after their listed scripts have moved successfully.
