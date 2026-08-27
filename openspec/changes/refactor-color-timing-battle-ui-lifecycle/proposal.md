## Why

Boss1 and Boss2 currently each serialize a complete `UI_BasePanel` Canvas and its UI bridge
components. This duplicates presentation assets, binds UI to scene lifetime, and leaves UI
event subscriptions and pause handles dependent on scene destruction instead of the project
UI service. The project requires battle presentation to follow the same GF.UI lifecycle as
the Loading, menu, result, and HUD forms.

## What Changes

- Remove the scene-authored `UI_BasePanel (1)` and `UI_BasePanel` Canvas roots from Boss1
  and Boss2 after their presentation responsibilities are migrated.
- Add a dynamic GF.UI `BattleTutorial` form that preserves the existing first-use weapon-tip
  behavior for the current battle.
- Create battle presentation installation dynamically after a battle scene is loaded; it
  resolves the scene's explicitly contracted Hero and supported Boss, opens the HUD and
  tutorial, and releases them on scene exit.
- Add an empty persistent `WorldUIRoot` in Launch for future World Space UI attachments.
  Current Screen Space GF.UI forms remain independent of this container.
- Keep Hero, Boss, map, and other authored gameplay content in the battle scenes; this change
  does not convert level actors to dynamically loaded entities.

## Capabilities

### New Capabilities

- `color-timing-battle-ui-lifecycle`: Dynamic battle HUD/tutorial ownership, scene cleanup,
  and the reserved World UI container for ColorTiming.

### Modified Capabilities

<!-- None. The ColorTiming runtime-presentation change remains an unarchived implementation change. -->

## Impact

- Product UI code, scene-flow presentation composition, and UI table/prefab registration.
- Boss1, Boss2, and Launch serialized scenes.
- New BattleTutorial GF.UI prefab and focused lifecycle tests.
