# ColorTiming validation gate — 2026-08-24

Unity instance: `GameDesinger_189B1E1A`  
Unity: `2022.3.62f3c1`  
UnitySkills: `http://localhost:8093/`

## Unity scene/project validation

Each product scene was loaded as the actual active scene before validation. Launch was restored afterward.

| Scene | Missing scripts/prefabs | Missing serialized references | Error/warning issues | Informational duplicate names |
|---|---:|---:|---:|---:|
| StartMenu | 0 | 0 | 0 | 0 |
| Boss1 | 0 | 0 | 0 | 9 groups |
| Boss2 | 0 | 0 | 0 | 3 groups |

The duplicate-name entries are authored repeated sprites, images and environment pieces. They are `Info` only and no runtime path relies on name lookup. Project-wide prefab/scene missing scripts: 0. Shader compile errors: 0.

## Serialized and contract audits

- Animation/Animator contract: PASS. All 9 required Animator parameters exist; EnterAnimStateEvent, RestXuli and Xuli references exist; all 13 Animation Event method families have receivers.
- Spine TrackEntry listener audit: PASS. 10 subscription sites across Event/Complete/End have 14 explicit removal sites; missing removal paths: 0.
- Asset/GUID reconciliation: PASS. 3575/3575 source assets have a unique disposition; 3568 migrated, 7 removed with evidence; duplicate target GUIDs: 0.
- Framework purity: PASS. Only the four formal ColorTiming build scenes are enabled and no undeclared framework/sample product content is present.
- Input boundary: PASS. Product gameplay input enters through the framework adapter; removed debug injection hotkeys do not occur.
- Runtime risk/lifecycle audit: PASS. Forbidden lookup/resource APIs 0, unapproved mutable statics 0, cleanup symmetry reviewed.
- Source-method reverse audit: PASS. 64 source product scripts have a disposition; 57 retained/refactored scripts have 0 missing target paths, and all method-signature changes in 21 scripts have an explicit framework/domain/dead-code disposition.
- Serialized-field reverse audit: PASS. 54 source Unity component classes expose 241 inherited field contracts; missing target classes 0, missing field names 0, serialized field type changes 0.

## Test evidence after lifecycle fixes

- Final full EditMode rerun after all runtime fixes: 203/203 passed, 0 failed, 0 skipped, 0 inconclusive (16.478 s). Evidence: `editmode-color-timing-203.xml` and `.log`.
- Full ColorTiming PlayMode direct-runtime suite: 14/14 passed, 0 failed, 0 skipped, 0 inconclusive (88.613 s). It covers Launch/StartMenu, both Boss scenes, all authored Boss attacks and Spine events, same-scene death reload, player movement/Dash/hit/death, all weapon Animation Events, grass/audio, camera/Cinemachine, UI/settings/pause, video transition, GF.Entity spawn/recycle and final result. Evidence: `playmode-color-timing-14.xml` and `.log`.
- Formal boss progression PlayMode: Boss1 all 11 segments/three colors, automatic Boss2 transition, Boss2 all 15 segments/four colors, tail threshold and final result passed.
- Feature traceability row audit: 57/57 reviewed; 30 direct, 19 partial and 8 static/manual-only. All rows remain manual-pending until the full three-scene regression is recorded.
- Fresh-Library batch import/compile: return code 0, compiler errors 0, package-resolution failures 0.
- Unity compile/console health immediately after changes: 0 errors, 0 warnings.
- Latest Test Runner discovery: 63 ColorTiming EditMode and 14 ColorTiming PlayMode tests are all `Runnable`. The complete isolated project copy subsequently executed the full 14-test PlayMode suite with 14/14 passing. The full 203-test EditMode suite was rerun after the final runtime fixes; its final evidence is recorded alongside this gate.

This gate establishes automated integrity and lifecycle evidence. It does not replace the visual/manual acceptance checklist in OpenSpec section 12.
