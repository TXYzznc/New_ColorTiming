# GF.UI Battle Result Smoke Evidence

- Date: 2026-08-24
- Entry path: `Launch → StartMenu → Boss2`
- Navigation used the real serialized StartMenu `Button.onClick` listeners.

## Runtime result

- Boss2 loaded as the active product scene while framework `Launch` remained loaded and inactive.
- Boss2 scene binding completed with an enabled gameplay camera and zero Console errors.
- A runtime-only validation button invoked the preserved compatibility method `UI_Game.ShowRus(false)` without modifying saved scene assets.
- Exactly one result form opened at `GameFramework/Builtin/UI/UICanvasRoot/UI Group - Dialog/BattleResult(Clone)`.
- The result form used the Dialog UI group and the UI service held the result pause lease.
- Console error count after result-form creation: `0`.

## Visual evidence

- `Assets/Screenshots/color-timing-battle-result-gfui.png` is a 1920x1080 live Game View capture after the authored final white-result fade completed.
