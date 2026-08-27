## Context

Boss1 and Boss2 each contain a copied, scene-authored `UI_BasePanel` Canvas. It includes
legacy result, HUD, weapon-tip, UI-sound, and `BattleHudContext` behaviours. The current
GF.UI `BattleHud` and `BattleResult` forms already replace most of this content, but a
static `BattleHudContext` component remains necessary only to pass scene actor references
to the UI service. `WeaponTip` is still wholly scene-owned.

Launch owns the framework UI component and `UICanvasRoot`. The requested `WorldUIRoot` is
a future attachment point, not a substitute for the framework's Screen Space UI groups.

## Goals / Non-Goals

**Goals:**

- Keep Boss scenes free of Canvas roots, scene-authored UI forms, and UI bridge behaviours.
- Dynamically compose battle HUD and tutorial presentation after actor initialization.
- Preserve the source weapon-tip rule: first non-normal weapon use per battle opens a
  matching visual tip, pauses game time, then accepts any press after two seconds.
- Add a named, persistent empty `WorldUIRoot` in Launch for future dynamic World Space UI.
- Make binding and cleanup explicit, testable, and independent of Unity script execution
  order.

**Non-Goals:**

- Do not convert Hero, Boss, map, or other gameplay objects into dynamically loaded
  entities in this change.
- Do not place current Screen Space GF.UI forms under `WorldUIRoot`.
- Do not redesign the tutorial artwork, timing, input rule, or weapon-tip data.
- Do not modify framework-core UI group ownership.

## Decisions

### 1. Use a scene-parented dynamic battle presentation installer

`ColorTimingCompositionRoot.BindScene` will create `BattlePresentationInstaller (Clone)`
for Boss1 and Boss2. The installer is parented to the loaded battle scene, waits one frame
for actor `Start` callbacks, validates the scene contract (one Hero and exactly one
supported Boss), then asks `IColorTimingUiService` to open the HUD and tutorial forms.
It is unloaded with the scene; the UI service closes tracked forms at transition start.

This retains a concrete lifecycle owner without serializing a bridge in every scene.

Alternatives considered:

- Keep `BattleHudContext` in the scene: rejected because it retains a UI bridge in authored
  content and depends on serialized UI composition.
- Let every actor open UI directly: rejected because gameplay actors would own presentation
  lifetime and duplicate transition cleanup.
- Use a permanent installer in Launch: rejected because it would retain stale actor
  references across scene transitions.

### 2. Make BattleTutorial an independent GF.UI form

`BattleTutorialForm` receives Hero, input, game-time, and settings explicitly when opened.
It owns weapon-event subscription, the per-battle seen-weapon set, the two-second
unscaled dismissal gate, and its pause lease. It is dynamically opened at battle start
with its visible tip panel hidden, then closes and unsubscribes on transition.

The tutorial is independent of `BattleHud` because it has a different visibility and
pause lifecycle. It is not parented below `WorldUIRoot` because it is a Screen Space
overlay.

### 3. Keep battle-result routing on the dynamic installer

The scene binder will bind battle result consumers to `BattlePresentationInstaller (Clone)`
rather than the legacy `UI_Game` component. The installer retains the existing
Boss1-to-Boss2 real-time delay through the explicit scene-flow contract, while the UI
service remains responsible only for opening and closing presentation forms. This removes
the last result bridge from `UI_BasePanel` without giving a process-lifetime service a
scene-specific coroutine or actor lifetime.

### 4. Reserve WorldUIRoot without coupling it to GF.UI groups

Launch will contain one persistent empty `WorldUIRoot` sibling of `UICanvasRoot`. It is a
plain transform attachment root, not a Canvas: each future world-space UI prefab owns the
Canvas settings required by its own rendering contract. It carries no product UI, input
receiver, or scene-specific reference. Future world UI services must dynamically attach
and detach their instances under it.

## Risks / Trade-offs

- [Actor initialization timing] → the dynamic installer waits one frame before resolving
  actors and opening forms; tests assert the HUD uses initialized HP.
- [Scene discovery becomes ambiguous] → validate exactly one Hero and exactly one of Boss1
  or Boss2; emit a clear error and do not open battle forms otherwise.
- [Tutorial leaks a time-scale lease] → release its lease on dismiss, close, and destroy.
- [Result routing changes scene progression] → retain a focused Boss1 defeated scenario
  that transitions to Boss2 only after the existing real-time delay.
- [WorldUIRoot implies current World Space support] → document it as an empty reserved
  attachment root and keep it out of current UI tests.

## Migration Plan

1. Create the BattleTutorial GF.UI prefab and register it in UITable.
2. Add tutorial form/service APIs and the dynamic installer; move battle-result routing to
   the dynamic installer.
3. Remove `UI_BasePanel` and all descendants from Boss1/Boss2 through an Editor migration.
4. Add `WorldUIRoot` to Launch through the same migration and validate scene contracts.
5. Run focused EditMode/PlayMode tests and manually verify Boss1, Boss2, tutorial pause,
   result progression, and no duplicate Canvas roots.

## Open Questions

None. Future World Space UI features will define their own presentation contracts before
using `WorldUIRoot`.
