## ADDED Requirements

### Requirement: Battle runtime owns one dynamically created Player

Each battle scene SHALL declare a Player Prefab and its scene dependencies without containing a static PlayerActorView instance. `BattleRuntimeContext` SHALL create exactly one Player through a dedicated runtime manager and SHALL release it with the battle context.

#### Scenario: Boss battle becomes ready

- **WHEN** Boss1 or Boss2 is loaded through the normal scene flow
- **THEN** exactly one `Player(Clone)` exists before battle readiness is published
- **AND** no static Hero root exists in the authored battle scene

### Requirement: Player scene dependencies are explicit

The runtime manager SHALL configure the player spawn position, WeaponSpawner, DeathSequence, Cinemachine camera, Boss target and per-scene presentation profile from `BattleSceneAnchors`. Runtime object discovery by name SHALL NOT be used.

#### Scenario: Player is created

- **WHEN** the manager instantiates the Player Prefab
- **THEN** all local player dependency consumers and all scene-side player target consumers are bound before gameplay readiness
- **AND** the Cinemachine Follow target references the runtime Player Transform

### Requirement: Existing authored presentation remains intact

Boss1 and Boss2 SHALL retain their existing Player audio references and camera tuning through separate scene profiles. The migration SHALL reuse existing art and audio assets without destructive modification.

#### Scenario: Each battle configures Player presentation

- **WHEN** the runtime Player is configured for Boss1 or Boss2
- **THEN** its sound cue collection and camera parameters equal the values authored in that scene before migration
