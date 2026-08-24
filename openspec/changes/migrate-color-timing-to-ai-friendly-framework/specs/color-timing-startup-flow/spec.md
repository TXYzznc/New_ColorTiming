## ADDED Requirements

### Requirement: Framework launch is the only product entry
The target project SHALL enter ColorTiming through the AI-Friendly-Project launch and procedure chain, and product startup logic SHALL be implemented outside `ScriptsBuiltin` through `IFrameworkStartupProcedure`.

#### Scenario: Cold start reaches menu
- **WHEN** the player starts a development build or enters Play Mode from the configured launch scene
- **THEN** framework initialization, preload and ready procedures complete before the ColorTiming startup procedure opens `StartMenu`

#### Scenario: Product logic remains outside framework core
- **WHEN** the framework purity audit scans `Assets/Game/ScriptsBuiltin/`
- **THEN** no ColorTiming-specific class, asset path, scene name or gameplay rule is present in that directory

### Requirement: Scene flow preserves all original routes
The system SHALL preserve the playable route `StartMenu → Boss1 → Boss2 → StartMenu/result` and SHALL support restart, previous level, next level and return-to-menu actions at the same availability points as the source project.

#### Scenario: Start normal game
- **WHEN** the player presses the main Start button
- **THEN** the system transitions from `StartMenu` to `Boss1` through the framework scene-change procedure

#### Scenario: Select a test level
- **WHEN** the player selects the existing Boss1 or Boss2 test-level button
- **THEN** the selected gameplay scene loads with the same initial gameplay state as direct selection in the source project

#### Scenario: Complete both bosses
- **WHEN** Boss1 is defeated and then Boss2 is defeated
- **THEN** the system first advances to `Boss2` and ultimately shows the result flow that returns to `StartMenu`

#### Scenario: Use pause navigation
- **WHEN** the player chooses restart, previous, next or return-to-menu from a scene where that action is available
- **THEN** exactly the requested destination is loaded once and no stale battle state survives

### Requirement: Loading presentation reports real progress
Every product scene transition SHALL use the framework loading lifecycle while preserving a visible loading/progress presentation and fade behavior equivalent to the source `LoadScenes` flow.

#### Scenario: Load a product scene
- **WHEN** a transition to `StartMenu`, `Boss1` or `Boss2` begins
- **THEN** the loading presentation opens, displays nondecreasing progress, fades at the expected boundaries and closes only after the destination is ready

#### Scenario: Transition is requested twice
- **WHEN** two navigation actions occur before the first scene transition completes
- **THEN** the system accepts at most one effective transition and does not load duplicate scenes or leave duplicate loading UI

### Requirement: Pause and time effects compose safely
The system SHALL pause and resume gameplay through an explicit time-state service, and pause, death and dash slow motion SHALL not overwrite each other's restoration state.

#### Scenario: Toggle pause
- **WHEN** the player presses Pause during active combat
- **THEN** gameplay simulation stops, pause UI remains interactive and a second valid resume action restores the prior gameplay time state

#### Scenario: Pause during dash slow motion
- **WHEN** pause is entered while the successful-dash slow-motion effect is active
- **THEN** resuming returns to the still-valid effect state or normal speed according to elapsed real-time policy, never to an arbitrary time scale

### Requirement: Persistent player settings survive scene changes
The key-tip, BGM and SFX settings SHALL remain effective across `StartMenu`, `Boss1` and `Boss2` and SHALL be applied when a new UI or sound group is created.

#### Scenario: Disable key tips and start a level
- **WHEN** the player disables key tips in settings and enters a boss scene
- **THEN** key-tip UI remains disabled in that scene and on subsequent supported transitions

#### Scenario: Toggle audio groups
- **WHEN** the player disables BGM or SFX
- **THEN** the corresponding framework sound group is muted without muting the other group, and its state persists across scene changes

