## ADDED Requirements

### Requirement: Single authoritative battle session
The system SHALL maintain player, active boss, weapon inventory, weakness sequence, pause state and battle result in exactly one `BattleSession` per loaded battle scene. Presentation components MUST NOT own duplicate authoritative battle state.

#### Scenario: Battle scene starts
- **WHEN** Boss1 or Boss2 scene composition completes
- **THEN** exactly one session is created with the configured player and boss state
- **AND** all presentation adapters observe or command that session

#### Scenario: Battle scene is disposed
- **WHEN** the battle scene begins unloading or its composition root is destroyed
- **THEN** the session stops accepting commands and releases every event subscription deterministically

### Requirement: Pure deterministic combat rules
The system SHALL implement health, damage immunity, color weakness, weapon inventory, attack gating, boss phase selection and battle result rules as pure C# types that do not depend on `GameObject`, `Transform`, `MonoBehaviour`, GF_X or static globals.

#### Scenario: Rule is tested outside PlayMode
- **WHEN** an EditMode test supplies commands, configuration and a seeded random source
- **THEN** the rule returns deterministic state transitions and domain events without loading a Unity scene

### Requirement: Commands and events separate intent from presentation
Unity-facing code SHALL send typed commands into the application layer and SHALL react to immutable domain events or snapshots. Domain code MUST NOT directly manipulate animation, physics, audio, UI, cameras, scenes or prefabs.

#### Scenario: Player attack is requested
- **WHEN** the input adapter submits an attack command
- **THEN** the session validates the command against action, pause and weapon state
- **AND** emits a presentation instruction only when the command is accepted

#### Scenario: Damage resolves battle result
- **WHEN** accepted damage reduces player or boss health to zero
- **THEN** the session publishes exactly one terminal result and rejects later combat mutations

### Requirement: Existing gameplay behavior remains the product baseline
The reimplementation SHALL cover every feature ID in the source-function audit and MUST NOT intentionally change externally observable gameplay except for behavior changes already approved and recorded as FIX-001 through FIX-004.

#### Scenario: Feature traceability is reviewed
- **WHEN** the implementation is prepared for acceptance
- **THEN** every audited feature ID maps to a new implementation location and at least one verification method or explicit preservation note
