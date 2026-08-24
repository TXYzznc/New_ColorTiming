# Scene Lifecycle Smoke

Date: 2026-08-24

## Runtime path

Executed through the real serialized UI events from `Launch`:

1. `Launch -> StartMenu -> Boss1`
2. Boss1 pause Form `GoNext -> Boss2`
3. Boss2 pause Form `GoLast -> Boss1`
4. Boss1 pause Form `BackMenu -> StartMenu`
5. Repeated the fast `Boss1 -> StartMenu` path after the Entity recycle fix.

Temporary runtime-only `Button` components invoked the same public `UI_HeroInfo.TogglePause` callback; Play Mode was stopped without saving those validation objects.

## Evidence

- Every destination became the sole active product scene beside persistent `Launch`.
- Each transition completed with zero project console errors after the final fixes.
- Boss1 legacy BGM/environment AudioSources had `playOnAwake=false` and `isPlaying=false`; GF.Sound agents played `第一章bgm0526` and `amb_cave`.
- Boss2 GF.Sound BGM agent changed to `第二章bgm0526`; the outgoing Boss1 clips were absent.
- Returning to StartMenu changed the BGM agent to `菜单第三版`.
- After the final return: `ColorTimingTransientEntity` count was 0; cameras were exactly `Main Camera` and framework `UICamera`.
- The two remaining `VideoPlayer` objects were the expected active MainMenu media objects (`1开头`, `2循环`), not leaked Boss objects.
- Reopened pooled MainMenu had `StartButtonBox` active and `GoGameButtonBox` inactive.
- EditMode regression suite: 61/61 passed (job `2ada04ca`).
