# Weapon Presentation Mapping Audit

Date: 2026-08-24

## Result

- Source and target `Boss1` serialize the same 24 weapon sprite GUIDs and 7 cursor texture GUIDs in the same order.
- Target `Boss1` and `Boss2` serialize identical weapon/cursor arrays.
- The three slots at indices 18–20 intentionally reference the same `无武器.png`; the legacy normal-state selection of index 18 is therefore preserved.
- `WeaponPresentationState` now owns the authored index contract instead of `UI_HeroInfo` repeating magic numbers.
- Every domain color/type combination is covered by `WeaponPresentationMapsEveryColorAndWeaponToAuthoredSlots`.
- Unity EditMode result after the change: 60/60 passed (job `3b6bf517`); Unity console errors: 0.

## Authored contract

| Presentation | Mapping |
| --- | --- |
| Weapon icon | non-normal: `legacyAnimatorIndex - 1`; normal: slot 18 |
| Held cursor | normal: `手1`; colored weapons: red/green/purple/orange sword cursor |
| Normal attack-held cursor | `手2` (slot 5) |
| Pause cursor | `手3` (slot 6) |
| Charge hint | alternate hint only for hammer and axe |

`UI_HeroInfo` restores the default cursor on scene destruction, so colored/attack/pause cursors do not leak into the next scene.
