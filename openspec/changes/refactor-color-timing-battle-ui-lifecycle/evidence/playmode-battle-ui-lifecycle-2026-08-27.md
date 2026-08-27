# Battle UI Lifecycle PlayMode Evidence — 2026-08-27

## Automated result

- Unity 2022.3.62f3 PlayMode test:
  `ColorTiming.Tests.PlayMode.ColorTimingBattleHudPlayModeTests.BattleHud_IsRuntimeOwned_Unique_AndPlayerItemsUseExpectedLayout`
- Result: **Passed** (`1 / 1`, 0 failed, duration 5.852 s).
- Result file: `../../refactor-color-timing-runtime-presentation/evidence/TestResults/playmode-color-timing-latest.xml`

The test boots Launch, reaches StartMenu, enters Boss1, verifies the dynamic HUD and
`BattlePresentationInstaller (Clone)`, defeats Boss1, verifies the Boss2 HUD/tutorial,
and returns to StartMenu. It also asserts that each loaded Boss scene has no authored
Canvas or EventSystem, and that each boss HP controller owns exactly seven runtime items.

## Manual acceptance checklist

1. From Launch, enter Boss1 and confirm that only the GF.UI HUD and the first-use tutorial
   presentation appear; no `UI_BasePanel` exists in the Boss1 hierarchy.
2. Pick or switch to each non-normal weapon type once. Confirm its tutorial art appears,
   battle time pauses, and an arbitrary input dismisses it only after two unscaled seconds.
3. Defeat Boss1. Confirm the one-second real-time transition enters Boss2 without duplicate
   HUD, HP pips, EventSystem, or Canvas roots in the product scene.
4. Defeat Boss2 and confirm the result form appears once; return to StartMenu and confirm
   battle tutorial, pause lease, HUD, transient HP items, and battle installer are gone.
