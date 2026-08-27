## ADDED Requirements

### Requirement: Product scene loading presentation
The system SHALL show exactly one ColorTiming-owned GF.UI loading form for every product scene transition after framework readiness. The form SHALL block underlying interaction, report non-decreasing loading progress, and remain visible until its unscaled fade completes.

#### Scenario: StartMenu transitions to Boss1
- **WHEN** the product scene flow begins a StartMenu-to-Boss1 transition
- **THEN** the Loading form opens before StartMenu unloads, shows non-decreasing progress, and closes after Boss1 is bound and the fade completes

#### Scenario: No duplicate framework progress view
- **WHEN** a ColorTiming product scene transition is active
- **THEN** Launch BuiltinView loading progress is not visible

### Requirement: Loading visual hierarchy fidelity
The ColorTiming Loading UIForm SHALL preserve the source `LoadScene_s` visual hierarchy, migrated Sprite references, active states, sibling order and RectTransform values. The implementation MAY normalize node names and replace the legacy loader controller with `ColorTimingLoadingForm`, but MUST NOT replace the source artwork/layout with a generic loading overlay.

#### Scenario: Rebuild the Loading Prefab
- **WHEN** the project Editor migration rebuilds `Loading.prefab`
- **THEN** the resulting form binds the source-equivalent Canvas, progress root, Slider and fade image, including the source Slider's inactive Fill/Background nodes and active Handle subtree

### Requirement: Startup loading presentation suppression
The system SHALL keep Launch BuiltinView loading progress hidden during the current framework startup path while retaining its error dialog behavior.

#### Scenario: Framework enters product startup
- **WHEN** framework initialization and preloading complete without an error
- **THEN** no Launch loading progress UI is presented before the ColorTiming StartMenu is shown

### Requirement: StartMenu BGM runtime ownership
The system SHALL start StartMenu BGM through GF.Sound when the StartMenu form opens and SHALL stop it when the form closes or a scene transition begins. The behavior SHALL not depend on a scene AudioSource.

#### Scenario: Leave StartMenu
- **WHEN** the user begins a scene transition away from StartMenu
- **THEN** the StartMenu BGM serial is stopped and does not play in the destination scene

### Requirement: Runtime instance clone naming
The system SHALL suffix every runtime-created or pooled ColorTiming UI form and transient entity root name with `(Clone)` on each open or show. Scene-authored object names SHALL remain unchanged.

#### Scenario: Reused entity is shown
- **WHEN** GF.Entity reuses a transient entity instance from its pool
- **THEN** its root GameObject name ends with `(Clone)` before participant callbacks run

#### Scenario: UI form opens
- **WHEN** GF.UI opens or reuses a ColorTiming UI form
- **THEN** its root GameObject name ends with `(Clone)`
