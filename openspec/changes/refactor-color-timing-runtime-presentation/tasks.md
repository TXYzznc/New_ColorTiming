## 1. GF.UI loading presentation

- [x] 1.1 Add a Loading UI view identifier and UI table integration contract for the user-created Loading prefab.
- [x] 1.2 Implement `ColorTimingLoadingForm` with input blocking, monotonic progress, and unscaled fade completion.
- [x] 1.3 Extend the project UI service and scene-flow lifecycle to open, update, and close exactly one Loading form per product transition.

## 2. Runtime ownership cleanup

- [x] 2.1 Route StartMenu BGM through the StartMenu form and GF.Sound, with deterministic close/transition cleanup.
- [x] 2.2 Stop using BuiltinView loading-progress APIs in ColorTiming transitions and suppress its automatic startup display while retaining dialogs.
- [x] 2.3 Normalize runtime UI form and transient entity names with a `(Clone)` suffix on open/show and pool reuse.

## 3. Verification and serialized migration

- [ ] 3.1 Add focused tests for loading-form lifecycle, BGM lifecycle, and clone naming.
- [ ] 3.2 Run relevant EditMode and PlayMode test suites and record results. (EditMode: 203/203 passed; PlayMode full suite needs separate follow-up because the existing duplicate-EventSystem warning loop prevented completion.)
- [x] 3.3 Run the Unity Editor migration and validate the generated Prefab and StartMenu scene.

## 4. Restore source Loading visual hierarchy

- [x] 4.1 Confirm that the source `LoadScene_s` visual hierarchy, image resources, active states and RectTransform values are the migration baseline.
- [x] 4.2 Define the GF.UI boundary: replace only the lifecycle controller; retain the original visual tree, including inactive Slider child nodes.
- [x] 4.3 Rebuild `Loading.prefab` through the Editor migration using the source hierarchy and migrated loading sprites.
- [x] 4.4 Validate serialized references, structural equivalence and StartMenu-to-Boss transition behavior in Unity. (Scoped hierarchy batch validator passed; PlayMode `PauseForm_ReopensAndSceneExitReleasesPauseLease` passed 1/1.)
- [ ] 4.5 Perform manual visual acceptance against the source prefab in the Unity Game view, including the exact intended visibility of the source-authored zero-scale Canvas.
