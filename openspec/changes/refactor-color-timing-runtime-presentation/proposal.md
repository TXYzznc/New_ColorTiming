## Why

ColorTiming currently has two concurrent loading presentations during product scene transitions: the persistent legacy `LoadScenes` object in StartMenu and the framework `BuiltinView` progress view. StartMenu BGM is likewise serialized as a scene `AudioSource` and only redirected to GF.Sound at runtime. These transitional scene-owned objects obscure lifecycle ownership and can render duplicate loading UI.

The project needs one GF.UI-owned loading presentation for all product scene transitions, explicit GF.Sound ownership of StartMenu BGM, and a consistent `(Clone)` marker for every runtime-created or pooled UI/entity instance.

## What Changes

- Add a ColorTiming GF.UI Loading Form which owns product loading progress, input blocking, and unscaled fade-out.
- Remove the legacy persistent `LoadScenes` runtime path and its StartMenu scene instance after the new prefab has been registered and validated.
- Move StartMenu BGM playback from its serialized scene `AudioSource` to the StartMenu UI form through GF.Sound.
- Keep Launch `BuiltinView` available for errors and future startup presentation, but prevent its loading progress view from being displayed during current startup and ColorTiming scene transitions.
- Apply an explicit `(Clone)` suffix whenever runtime UI forms and framework entities are opened or shown, including pool reuse.

## Capabilities

### New Capabilities

- `color-timing-runtime-presentation`: Project-owned loading UI, scene-transition presentation ownership, StartMenu BGM lifetime, and runtime instance naming.

### Modified Capabilities

无。上述 ColorTiming 能力仍属于尚未归档的迁移变更，本次以独立补充能力记录其运行时表现边界。

## Impact

- Product runtime code: Bootstrap, UI presentation, StartMenu form, transient entity lifecycle, and UI form base lifecycle.
- Product assets: the user-created `Assets/Game/Prefabs/UI/ColorTiming/Game/Loading.prefab`, the UI table/enum entry, and removal of the two legacy StartMenu scene objects after the code path is ready.
- Framework startup UI: no core structural change; its default loading-progress display is suppressed while its error dialog remains available.
