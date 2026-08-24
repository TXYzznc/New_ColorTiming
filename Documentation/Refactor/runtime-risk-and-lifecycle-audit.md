# ColorTiming runtime risk and lifecycle audit

Date: 2026-08-24  
Scope: `Assets/Game/Scripts/ColorTiming/**/*.cs`

## Reproducible static result

Command: `python tools/audit_color_timing_runtime_risks.py`

Result: PASS — forbidden APIs 0, removed debug hotkeys 0, unapproved mutable statics 0, approved bounded mutable statics 1, Update-family methods inventoried 29. Machine-readable evidence is `runtime-risk-audit.json`.

- No `GameObject.Find`, `FindObjectOfType(s)` or `Resources.Load` occurs in product runtime code.
- No `KeyCode.I/O/P/T` debug injection path remains.
- The only mutable static field is `LoadScenes.persistentView`. It is a bounded identity guard for the pooled loading view, is cleared in `OnDestroy`, and exposes no global service or mutable gameplay state.

## Update and allocation review

All 29 `Update`/`LateUpdate`/`FixedUpdate` declarations were reviewed against the allocation scan.

- Weapon spawning reuses pre-sized `activeColors` and `availablePositions` lists; no per-frame list construction remains.
- Per-frame movement, state, fade, animation and camera paths allocate only value types (`Vector2`, `Vector3`, `Color`) or reuse serialized/field collections.
- LINQ `ToArray`/`ToList` sites are restricted to scene binding, framework entity show/hide, queue projections, boss initialization/phase changes and scene teardown—not steady-state Update loops.
- `HeroSoundManager` constructs a temporary clip candidate list per discrete footstep request, not per frame. Boss reposition/attack helpers allocate only when that discrete action begins.
- `GfTransientEntityService.ReleaseAll().ToArray()` intentionally snapshots IDs during scene teardown so callbacks may safely mutate tracking.

## Subscription and async symmetry

The generated JSON inventories every runtime `AddListener`/`RemoveListener` and C# event `+=`/`-=` site. Source review confirms:

| Owner | Subscribe/acquire | Release boundary |
|---|---|---|
| `ColorTimingCompositionRoot` | scene transition | `Dispose` unsubscribes, releases entities, UI, sound, time and input host |
| `GfColorTimingUiService` | transition + pause/result leases | transition/dispose closes tracked forms and disposes both leases |
| `GfColorTimingSoundService` | game-time scale | `OnDestroy` unsubscribes; transition reset stops tracked scene/gameplay sounds |
| Boss controllers/anim adapters | UnityEvent and Spine Event/Complete/End | matching `OnDestroy`/track-end removal |
| Hero controller/views/HUD | animation, damage, weapon and pickup UnityEvents | matching `OnDestroy`, `OnDisable`, trigger-exit or framework-despawn removal |
| `StartVido`, `UI_Game` | coroutines | disable/destroy stops retained routine handles |
| `LoadScenes` | transition/progress | rebind first unsubscribes; `OnDestroy` unsubscribes and clears static guard |
| `Hero_XuliTip` | weapon/state events | `OnDestroy` removes both listeners (FIX-015) |

The product runtime does not create `CancellationTokenSource` instances. Its asynchronous work is framework scene loading, GF callbacks, or retained Unity coroutine handles with explicit stop/destruction boundaries.

## Framework-owned resource cleanup

- World weapons, skills, projectiles and HitFX use `ITransientEntityService`/GF.Entity. `ReleaseAll` runs before outgoing scenes unload, reparents scene children to the persistent Entity root, hides each tracked entity and clears tracking.
- `ColorTimingTransientEntity.OnHide` notifies every `IFrameworkEntityParticipant`, removes tracking and clears release delegates.
- UI forms are GF.UI pooled forms. The UI service tracks serial IDs, closes pause/result/scene forms at transition, and releases all time-scale leases.
- Sound serials are split into scene and gameplay sets, pruned when complete, stopped at reset/scene exit and disconnected from time changes on destruction.
- Normal runtime has one persistent Launch EventSystem; Boss1/Boss2 authored EventSystems remain serialized but inactive (FIX-013).

## Runtime evidence

- Unity compile health after lifecycle fixes: 0 errors, 0 warnings.
- Full EditMode run: 203/203 passed after the final sound-mapping additions.
- ColorTiming PlayMode lifecycle run: 5/5 passed, covering Launch, StartMenu, Boss1, Boss2, pause reopen/scene exit, GF.UI forms, GF.Entity reset and GF.Sound persistence/reset.
- `scene-lifecycle-smoke.md` records repeated Boss1/Boss2/menu transitions with no retained forms, transient entities or console errors.

Conclusion: the static risk and cleanup-symmetry gates pass. Remaining visual/manual acceptance work is tracked separately and is not inferred from this audit.
