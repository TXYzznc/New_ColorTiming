# GF.UI Main Menu Smoke Evidence

- Date: 2026-08-24
- Unity: 2022.3.62f3c1
- UnitySkills instance: `GameDesinger_189B1E1A` (`http://localhost:8093/`)
- Entry scene: `Assets/Game/Scene/Launch.unity`

## Runtime result

- `Launch` remained loaded and inactive as the framework bootstrap scene.
- `StartMenu` loaded and became the active product scene.
- `StartMenu` contained three authored roots; the former authored menu Canvas is no longer embedded in the scene.
- Exactly one `UI_ButtonAction` existed at runtime: `GameFramework/Builtin/UI/UICanvasRoot/UI Group - Default/MainMenu(Clone)`.
- The main Start button retained one `RuntimeOnly` persistent listener targeting `UI_ButtonAction.StartGameBtnDown`.
- Invoking the real `Button.onClick` disabled `StartButtonBox` and enabled `GoGameButtonBox`.
- Unity Console error count after startup and button invocation: `0`.

## Visual evidence

- `Assets/Screenshots/color-timing-main-menu-gfui.png` is a 1920x1080 live Game View capture of the GF.UI-hosted menu.
