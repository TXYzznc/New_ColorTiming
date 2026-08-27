# Spine 3.8 / URP 14 migration audit

## Pinned upstream

- Runtime data/package: Spine 3.8, local package marker `spine-unity-3.8-2021-11-10.unitypackage`.
- Official repository: https://github.com/EsotericSoftware/spine-runtimes
- Pinned upstream commit: `baf30f0ff5fcdc5629323ea55f494e13ff2d5f27` (2021-11-10 18:47:37 UTC).
- Official URP module package version: `3.8.2`; declared minimum URP dependency `7.1.5`.
- Upstream module Git tree: `3d33ab6daff1d72f245bd9cce80bc50b09b58445`.
- License: Spine Runtimes License Agreement, vendored at `Assets/Plugins/SpineURP/LICENSE.md`; upstream reference: https://github.com/EsotericSoftware/spine-runtimes/blob/3.8/LICENSE
- Official rendering guidance: https://us.esotericsoftware.com/spine-unity-rendering

The complete upstream 3.8.2 package was first installed at the pinned commit. Under Unity 2022.3.62f3c1 + URP 14.0.12 its required 3D `Skeleton` shader compiled, but four unused 2D/Sprite/Outline shaders failed because of URP 7 APIs and hard-coded `Assets/Spine` include paths. The final project therefore vendors only the exact 3D Skeleton dependency closure plus its license. This removes unrelated failing shaders without changing the selected shader implementation.

The only intentional edit to the upstream shader is its menu name, from `Universal Render Pipeline/Spine/Skeleton` to `ColorTiming/URP/Spine Skeleton`, preventing accidental collision with another installed Spine URP module. Vendored file-set SHA-256 (sorted `path:fileHash` manifest): `8B4CBF18872F50DAB9C7D58B4001DC79C96458B4783098EE6787BE8ED5B7956A`.

## Material mapping

All source texture GUIDs and serialized Stencil values remain unchanged.

| Material | Source shader | Main texture GUID | Key source values | Target shader |
|---|---|---|---|---|
| `Boss/Spine/BOSS拆分_BOSS拆分.mat` | Spine/Skeleton Fill | `7be128b4a712111419382d0805db0736` | FillPhase 0; Stencil 1/Always | ColorTiming/URP/Spine Skeleton Fill |
| `Boss/Spine/BOSS拆分_BOSS拆分2.mat` | Spine/Skeleton | `a7e295fec558cac48bc2293760567ea5` | Stencil 1/Always | ColorTiming/URP/Spine Skeleton |
| `Boss/Spine/BOSS拆分_BOSS拆分3.mat` | Spine/Skeleton Fill | `04f9f3569efa39046a47cc4a4272589b` | FillPhase 0; Stencil 1/Always | ColorTiming/URP/Spine Skeleton Fill |
| `Boss/Spine2/BOSS拆分_BOSS拆分.mat` | Spine/Skeleton | `896612350a0c41840b8695371978e66d` | Stencil 1/Always | ColorTiming/URP/Spine Skeleton |
| `Boss/Spine2/BOSS拆分_BOSS拆分2.mat` | Spine/Skeleton | `938cd315239056140857e49e2d33f4fd` | Stencil 1/Always | ColorTiming/URP/Spine Skeleton |
| `Boss/tip/BOSS拆分_Material.mat` | Spine/Skeleton | `a4e67bc277ab3814fa0787a556a0829e` | Stencil 1/Always | ColorTiming/URP/Spine Skeleton |
| `Boss2/di/skeleton_Material.mat` | Spine/Skeleton | `99279c282c2811f44ad478dcc2dc6116` | Stencil 1/Always | ColorTiming/URP/Spine Skeleton |
| `Boss2/Shenti/第二章boss_Material_InsideMask.mat` | Spine/Skeleton | `f44a00597ce7d0242a6dea94ad8aad3f` | Stencil 1/Equal | ColorTiming/URP/Spine Skeleton |
| `Boss2/Shenti/第二章boss_Material_OutsideMask.mat` | Spine/Skeleton | `f44a00597ce7d0242a6dea94ad8aad3f` | Stencil 1/NotEqual | ColorTiming/URP/Spine Skeleton |
| `Boss2/Shenti/第二章boss_Material.mat` | Spine/Skeleton Fill | `f44a00597ce7d0242a6dea94ad8aad3f` | FillPhase 0; Stencil 1/Always | ColorTiming/URP/Spine Skeleton Fill |
| `Boss2/Weiba/第二章boss_Material.mat` | Spine/Skeleton | `b52cbbfa83ae5ad4187727450f2f3e42` | Stencil 1/Always | ColorTiming/URP/Spine Skeleton |

## Compatibility and visual evidence

- PMA is preserved by `Blend One OneMinusSrcAlpha` in both target shaders.
- Main texture alpha, Spine vertex color, transparent queue, double-sided rendering, no depth write, Stencil reference/comparison, and renderer sorting order are preserved.
- The custom URP Fill shader retains `_FillColor`, `_FillPhase`, `_StraightAlphaInput`, `_StencilRef`, and `_StencilComp`. Runtime `Boss2Anim_s.OnHit` evidence shows the complete transparent Boss silhouette flashing white.
- `validate_shader_errors`: 0 after removing the unused full-package shaders.
- Material inventory under `Assets/Game`: 8 vendored Spine URP Skeleton, 3 ColorTiming URP Fill, 1 `Sprites/Default`, 1 TMP SDF; missing shader GUIDs: 0.
- Scene validation: StartMenu 0 issues/0 missing references; Boss1 0 issues/0 missing references after FIX-011; Boss2 0 issues/0 missing references.
- Target screenshots (1920x1080):
  - `Assets/Screenshots/color-timing-urp-startmenu-final.png`
  - `Assets/Screenshots/color-timing-urp-boss1-stacked.png`
  - `Assets/Screenshots/color-timing-urp-boss2-stacked.png`
  - `Assets/Screenshots/color-timing-urp-boss2-onhit.png`

The source repository contained no authored pre-migration screenshots. A read-only isolated source copy was therefore used to capture 1920×1080 StartMenu, Boss1, and Boss2 baselines; see `visual-regression-comparison.md` and `VisualBaseline/Source/`. Paired target/source same-state visual checkpoints are complete and OpenSpec 10.8 is closed. Dynamic attacks, hits, media timing, input feel, and complete manual paths remain explicitly open under OpenSpec 12.1–12.4.
