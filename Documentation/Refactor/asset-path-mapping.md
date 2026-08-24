# ColorTiming asset path mapping

## Safety rules

- Preserve every migrated asset's source `.meta` GUID.
- Never overwrite a framework file when its content differs.
- Do not copy the four colliding framework folder metas: `Assets/Game.meta`, `Assets/Game/Scripts.meta`, `Assets/Game/Scripts/UI.meta`, and `Assets/Plugins.meta`.
- Keep source-authored `Assets`, `Packages`, and `ProjectSettings` read-only.
- Record every source-to-target file mapping and SHA-256 in `Documentation/Refactor/Baseline/migrated-assets.csv`.

## Mapping

| Source | Target | Purpose |
| --- | --- | --- |
| `Assets/Scenes/*.unity` | `Assets/Game/Scene/` | Framework scene-path convention; scene GUIDs retained |
| `Assets/Game/Prefba/UI/*` | `Assets/Game/Prefabs/UI/ColorTiming/` | Product UI prefabs |
| `Assets/Game/Prefba/Game/*` | `Assets/Game/Prefabs/UI/ColorTiming/Game/` | Pause/menu UI prefab |
| loose `Assets/Game/Prefba/*.prefab` | `Assets/Game/Prefabs/Entity/ColorTiming/` | Runtime combat entities |
| `Assets/Game/Prefba/Scene/*` | `Assets/Game/Prefabs/World/ColorTiming/` | World/scene prefabs |
| `Assets/Art/Sound/*` | `Assets/Game/Audio/ColorTiming/` | Framework sound-path convention |
| other `Assets/Art/*` | `Assets/Game/ColorTiming/Art/` | Images, animation, video, Spine data, materials |
| `Assets/Plugins/Spine/*` | `Assets/Plugins/Spine/` | Source-compatible Spine 3.8 runtime |
| `Assets/Editor/SpineSettings.asset*` | `Assets/Editor/` | Spine editor settings |
| active `Assets/Game/Scripts/*` | `Assets/Game/Scripts/ColorTiming/Legacy/` | GUID-preserving strangler compatibility layer |

The source `Prefba`, `UI`, `Game`, `Scene`, `Art`, `Spine`, and `Scripts` folder GUIDs are reused on the corresponding product-owned destination folders where doing so does not collide with the framework.

## Intentionally excluded legacy scripts

- `PlayerInput.cs`
- `Weapon_Hero.cs`
- `Skill/Skill_Jiandao.cs`
- `Anim/PlayAnimation.cs`
- `Anim/AnimStateMachine_DMD.cs`
- `ZZZZZZZZZZ.cs`

Each is listed in `deprecated-candidates.md` and had no authored code reference or serialized script reference in the baseline audit.
