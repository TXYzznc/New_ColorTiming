## ADDED Requirements

### Requirement: Battle resource plans support scene and in-scene contexts
The game SHALL preload required battle resources through a configured load context before gameplay begins.

#### Scenario: Enter a boss scene
- **WHEN** a transition targets a boss scene
- **THEN** the scene and its configured required resources load under one aggregated Loading progress, and battle input remains disabled until both succeed

#### Scenario: Change a level inside a loaded scene
- **WHEN** gameplay requests a new configured battle load context without unloading the Unity scene
- **THEN** the new context preloads its required resources, reports progress through the same Loading contract, and releases the replaced context only after the replacement succeeds

### Requirement: Hero controller installation is state-safe
The player SHALL never replace its RuntimeAnimatorController during an unsafe animation state.

#### Scenario: Pick up while moving
- **WHEN** a player picks up a weapon while moving
- **THEN** the weapon is authoritative for gameplay immediately, but its controller installs only after the stable idle boundary

#### Scenario: Controller becomes ready during an attack
- **WHEN** a requested weapon controller becomes ready while the player is attacking, dashing, hit-stunned or dying
- **THEN** no Animator Rebind occurs until the player reaches the configured safe installation boundary

#### Scenario: Context is released before an asynchronous completion
- **WHEN** a battle load context is replaced or released before a resource callback completes
- **THEN** the stale callback cannot install a controller or retain a resource lease
