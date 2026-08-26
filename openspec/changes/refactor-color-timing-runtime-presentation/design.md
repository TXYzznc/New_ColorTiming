## Context

`LoadScenes` is a legacy scene component in StartMenu. It survives scene loads, subscribes to the product scene flow, and owns a separate Canvas, progress bar, and unscaled fade. `ColorTimingStartupProcedure` simultaneously calls `BuiltinView.ShowLoadingProgress`, so every product transition after StartMenu can display both loading presentations.

The user has created `Assets/Game/Prefabs/UI/ColorTiming/Game/Loading.prefab` from the legacy visual hierarchy. It currently retains the legacy component and must be converted to a GF.UI form. Launch `BuiltinView` remains necessary for error dialogs and future startup presentation, but no current startup art or progress UI is desired.

## Goals / Non-Goals

**Goals:**

- Use one project-owned GF.UI form for every ColorTiming scene transition after framework readiness.
- Keep the current black fade, progress bar, input blocking, monotonic progress, and unscaled-time behavior.
- Remove StartMenu scene ownership of loading UI and BGM playback.
- Mark all runtime-created and pooled ColorTiming UI/entity instances with a stable `(Clone)` suffix.
- Keep Launch loading progress hidden during the present startup flow while preserving framework error dialogs.

**Non-Goals:**

- Do not remove or redesign the framework `BuiltinViewComponent` or its dialog API.
- Do not add startup animation art, a downloading UI, or a new external UI dependency.
- Do not make level actors dynamic in this change.
- Do not rename source assets or scene-authored objects with `(Clone)`.

## Decisions

### 1. Separate framework boot presentation from product scene presentation

ColorTiming will stop calling the Launch BuiltinView loading-progress API during product scene transitions. A `ColorTimingLoadingForm` opened through `GF.UI` will subscribe to the product flow before unload, report monotonic progress, block input, and close only after an unscaled fade completes. Launch BuiltinView will remain hidden for the current boot sequence and retain only the generic error-dialog role.

Alternatives considered:

- Extend BuiltinView with product fade: rejected because ColorTiming visual behavior would enter framework core.
- Keep `LoadScenes` as a persistent prefab: rejected because it retains scene scanning, static singleton ownership, and `DontDestroyOnLoad`.

### 2. Own StartMenu BGM in its UI form

The StartMenu form will explicitly request its BGM through `IColorTimingSoundService` on open and release it on close. The serialized StartMenu AudioSource will be removed after the code path is working. This avoids the current migration adapter that stops a scene AudioSource and restarts it through GF.Sound.

### 3. Normalize names at reusable framework lifecycle boundaries

`UIFormBase` will normalize its GameObject name on every UI form open, and the product transient entity base will do so on every Entity show. These run on both first instantiation and pool reuse. Direct Unity `Instantiate` already appends `(Clone)` by default; a small helper will preserve that suffix if a caller or pooled framework instance has lost it. Scene-authored names are untouched.

### 4. Migrate serialized assets through Unity Editor APIs

The user has delegated the Prefab and scene work. A project Editor migration rebuilds the
Loading Prefab as the approved GF.UI hierarchy, assigns serialized form fields and the
existing StartMenu BGM clip, removes the legacy StartMenu objects, refreshes the resource
collection, and regenerates framework tables. It does not hand-edit Unity YAML.

## Risks / Trade-offs

- [Loading form cannot open before a scene unload] → open it from `TransitionStarted`, before `BeginSceneTransition` unloads the outgoing scene, and verify StartMenu→Boss1/Boss1→Boss2/return paths.
- [GF.UI form close occurs before fade completes] → retain the form serial until its unscaled fade task completes, then close once.
- [BGM duplicates during transition] → explicitly stop the StartMenu BGM serial on form close/transition and remove the serialized source after manual verification.
- [A custom runtime name is overwritten on reuse] → normalize in every `OnOpen`/`OnShow`, not merely construction.
- [No startup progress feedback] → retain diagnostics and error dialogs; re-enable a framework presentation only when a startup UX requirement exists.

## Migration Plan

1. Register Loading as a GF.UI view and implement the product Loading Form plus tests.
2. Route ColorTiming scene flow progress to the form; remove product use of BuiltinView loading progress and suppress its automatic boot display.
3. Add BGM lifecycle and `(Clone)` normalization code.
4. Run the Editor migration after the Unity project is not open in another instance.
5. Run EditMode/PlayMode tests and perform manual scene-transition/BGM/no-duplicate-UI validation.

## Open Questions

无；当前视觉沿用用户创建的 Loading Prefab，不新增启动美术。
