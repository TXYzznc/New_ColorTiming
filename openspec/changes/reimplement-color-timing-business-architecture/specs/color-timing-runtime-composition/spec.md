## ADDED Requirements

### Requirement: Explicit scene composition root
Each battle scene SHALL be composed by one runtime-created composition root that owns `BattleSession`, input routing, presentation adapters, GF services and disposal order. Business composition MUST NOT depend on scanning every scene `MonoBehaviour` by type.

#### Scenario: Supported battle scene is loaded
- **WHEN** GF scene loading reports Boss1 or Boss2 ready
- **THEN** Bootstrap creates one composition root and binds explicitly declared scene anchors
- **AND** duplicate composition attempts fail with a diagnostic without creating duplicate UI or subscriptions

### Requirement: Scene content exposes anchors rather than services
Static battle-scene objects SHALL contain only level content and Unity presentation anchors required by authored assets. Runtime services, contexts and UI forms SHALL be created and owned by Bootstrap/GF lifecycle rather than serialized into Boss scenes.

#### Scenario: Boss scene hierarchy is audited
- **WHEN** Boss1 or Boss2 is opened outside PlayMode
- **THEN** it contains no static GF UI root, battle service locator or authoritative battle context object

### Requirement: GF infrastructure is accessed through application ports
Scene flow, UI forms, sound playback, input, time and transient entities SHALL be implemented behind product-owned ports. Domain and application rules MUST NOT reference GF_X concrete components.

#### Scenario: Runtime adapter is replaced by a fake
- **WHEN** a test constructs the application with fake input, time, random and output ports
- **THEN** battle behavior executes without invoking GF_X or Unity scene APIs

### Requirement: Runtime objects have deterministic ownership and naming
All runtime-created or pooled GameObjects SHALL have one owner, SHALL be released through the matching GF/pool lifecycle and SHALL use the `(Clone)` suffix while active so they remain distinguishable from authored objects.

#### Scenario: Scene unloads with spawned content
- **WHEN** a battle scene unloads while skills, pickups or UI item instances are active
- **THEN** the composition root cancels pending work and releases all owned instances exactly once

### Requirement: Audio routing is semantic
Music, ambience, UI and combat effects SHALL be routed by explicit semantic IDs or channels. Runtime behavior MUST NOT infer sound role from a GameObject name or clip filename.

#### Scenario: Authored looping ambience starts
- **WHEN** a scene audio anchor requests its configured sound cue
- **THEN** the sound service uses the configured channel and loop policy without inspecting names
