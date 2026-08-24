## ADDED Requirements

### Requirement: Gameplay consumes semantic input only
All ColorTiming gameplay and UI application code SHALL consume `IGameInput` or narrower semantic interfaces, and direct `UnityEngine.Input` calls SHALL exist only inside the selected Unity input adapter.

#### Scenario: Static input boundary audit
- **WHEN** project code outside `Assets/Game/Scripts/ColorTiming/Input/Adapters/` is scanned
- **THEN** it contains no direct calls to `Input.GetAxis`, `Input.GetAxisRaw`, `Input.GetButton`, `Input.GetButtonDown`, `Input.GetKey`, `Input.GetKeyDown`, `Input.mousePosition` or `Input.anyKeyDown`

### Requirement: Existing control semantics are preserved
The input abstraction SHALL preserve Move, Dash/Jump, Attack pressed and held, Drop/Fire2, Pause/Escape, pointer position, any-key tutorial dismissal and result confirmation semantics from the source project.

#### Scenario: Move with keyboard axes
- **WHEN** the player holds WASD or arrow keys
- **THEN** Move reports the same normalized direction and dead-zone behavior as the source Horizontal and Vertical axes

#### Scenario: Dash action edge
- **WHEN** the configured Jump/Dash control changes from released to pressed
- **THEN** Dash reports true for exactly the valid press frame and does not retrigger while continuously held

#### Scenario: Attack press and hold
- **WHEN** the configured Fire1 control is pressed and then held
- **THEN** AttackPressed reports the initial edge while AttackHeld remains true for charge-capable behavior until release

#### Scenario: Drop action
- **WHEN** the configured Fire2 control is pressed while a weapon is held
- **THEN** exactly one Drop request is produced for that press

#### Scenario: Pause while game time is zero
- **WHEN** the game is paused and the player presses the configured pause/resume control
- **THEN** the UI receives the action using unscaled frame processing and can resume the game

#### Scenario: Dismiss tutorial with any valid key
- **WHEN** the weapon tutorial is visible and the player produces any accepted keyboard, mouse or controller input
- **THEN** the tutorial dismisses once without leaking the same input into an unintended combat action

### Requirement: Pointer world position uses the active gameplay camera
The input layer SHALL expose pointer screen position separately from world conversion, and the world pointer used by aiming SHALL be calculated with the explicitly active gameplay camera.

#### Scenario: Aim after camera changes
- **WHEN** Cinemachine changes the active virtual-camera composition while the cursor remains at a fixed screen position
- **THEN** the resulting world aim point is recomputed from the current output camera and remains spatially correct

### Requirement: Input adapter can be replaced without gameplay changes
Input API selection SHALL be isolated behind the semantic contract; changing from the initial Legacy Input Manager adapter to a future adapter SHALL require no changes in player, boss, UI or scene-flow code.

#### Scenario: Test adapter drives player logic
- **WHEN** an EditMode or PlayMode test supplies a deterministic fake `IGameInput`
- **THEN** the same player and flow logic responds without accessing hardware input

