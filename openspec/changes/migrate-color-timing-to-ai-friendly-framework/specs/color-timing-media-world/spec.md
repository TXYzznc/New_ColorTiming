## ADDED Requirements

### Requirement: Start menu video sequence is preserved
The StartMenu SHALL play `1开头.mp4` as the intro and transition to looping `2循环.mp4` with source-equivalent timing, visibility and audio behavior.

#### Scenario: Enter StartMenu for the first time
- **WHEN** StartMenu becomes ready and the intro has not been skipped by an existing source rule
- **THEN** `1开头.mp4` prepares and plays once before the loop video becomes active

#### Scenario: Intro video completes
- **WHEN** the intro reaches its completion event
- **THEN** `2循环.mp4` starts without a blank-frame leak or duplicate audio and continues looping while the relevant menu is active

#### Scenario: Leave StartMenu during preparation
- **WHEN** a scene transition begins while a video is preparing or playing
- **THEN** preparation/playback callbacks are cancelled and no video/audio persists into the gameplay scene

### Requirement: Sound categories and cues are complete
The framework Sound system SHALL preserve BGM, UI click/hover, player movement, dash, pickup, drop, player/boss hit, boss attacks, burrow and environment audio cues with their source-equivalent timing, grouping and overlap policy.

#### Scenario: Play UI cue
- **WHEN** a supported button receives hover or click interaction
- **THEN** the configured UI SFX plays once in the SFX group and respects the persisted SFX mute state

#### Scenario: Play boss event cue
- **WHEN** an authored Boss Spine event requests a sound
- **THEN** the mapped sound asset plays through the correct framework group at the expected event time

#### Scenario: Change movement surface cue
- **WHEN** the player enters or exits the grass/footstep override region
- **THEN** movement audio uses the corresponding source-equivalent clip/rule and restores the previous rule on exit

### Requirement: Grass interaction remains responsive
Grass trigger objects SHALL play the authored reaction animation when the player traverses them and SHALL not retain stale player references after exit or scene unload.

#### Scenario: Player enters grass
- **WHEN** the player collider enters a migrated grass trigger
- **THEN** the correct grass animation/event and footstep override are activated once for that interaction

#### Scenario: Grass entity is disabled
- **WHEN** grass or its scene is disabled while an interaction is active
- **THEN** footstep override and subscriptions are safely released

### Requirement: Camera behaviors are preserved
The migration SHALL preserve parallax `CameraShow`, Cinemachine framing/size based on boss distance, confiner bounds, impulse feedback and death-camera behavior.

#### Scenario: Boss distance changes
- **WHEN** player-to-boss distance crosses a source camera threshold
- **THEN** the active Cinemachine composition/orthographic size changes with the same direction, clamp and smoothing behavior

#### Scenario: Combat hit generates impulse
- **WHEN** an attack configured for camera feedback produces a valid hit
- **THEN** the expected Cinemachine impulse is generated once with the mapped profile

#### Scenario: Camera reaches scene boundary
- **WHEN** its target approaches or crosses a configured confiner edge
- **THEN** the rendered camera remains inside the source-equivalent playable bounds

#### Scenario: Player dies
- **WHEN** the death sequence begins
- **THEN** the death camera transition/zoom occurs and is cancelled or reset cleanly on restart/scene exit

### Requirement: URP migration preserves authored appearance
The target SHALL render all three scenes through URP 14.0.12, and all migrated Sprite, particle, UI and Spine materials SHALL resolve to supported shaders without pink/error output.

#### Scenario: Scan materials and renderers
- **WHEN** the target assets and loaded scenes are validated
- **THEN** no required material has a missing/unsupported shader and no renderer references a missing material

### Requirement: Spine 3.8 Boss materials pass a dedicated compatibility gate
The 8 authored `Spine/Skeleton` Boss materials and 3 authored `Spine/Skeleton Fill` Boss materials SHALL migrate to a fixed, licensed Spine 3.8-compatible URP shader implementation and preserve PMA blending, masks, fill, vertex color, render order and visible texture output.

#### Scenario: Verify each Boss material
- **WHEN** each of the 11 source/target material pairs is rendered at its defined checkpoint
- **THEN** the target shows the intended texture, transparency, color/fill, mask and ordering with no shader fallback

#### Scenario: Shader module provenance audit
- **WHEN** dependency documentation is inspected
- **THEN** the exact Spine URP module source, version/commit, license and integrity hash are recorded before the module is accepted

### Requirement: Presentation objects release framework resources
Video players, sounds, effects, pooled entities and camera subscriptions SHALL stop or recycle on form close, entity hide, battle result and scene unload.

#### Scenario: Re-enter gameplay repeatedly
- **WHEN** the player enters and exits Boss scenes multiple times
- **THEN** no duplicate BGM, event listener, video callback, impulse source or orphan effect remains from a previous scene instance

