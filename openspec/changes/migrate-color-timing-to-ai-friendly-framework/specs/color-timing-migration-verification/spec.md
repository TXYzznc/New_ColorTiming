## ADDED Requirements

### Requirement: Source project remains an immutable comparison baseline
The migration process MUST NOT modify files under `D:\unity\UnityProject\ColorTimeing\ColorTimeing`; all product implementation and evidence SHALL be stored in the target repository.

#### Scenario: Compare source Git and file baseline
- **WHEN** a migration phase completes
- **THEN** no implementation change attributable to the migration appears in the source project and the target evidence references the source path read-only

### Requirement: Every inventoried feature has traceability
The project SHALL maintain a feature traceability matrix covering StartMenu, global flow/settings, player, input, combat, weapons, Boss1, Boss2, UI/HUD, audio/video/world/camera and every inventoried animation/Spine event.

#### Scenario: Audit a feature row
- **WHEN** any source feature row is selected
- **THEN** it identifies source evidence, target implementation, required assets, verification method and current result; no row can be marked complete with a missing field

#### Scenario: Audit source button binding
- **WHEN** the persistent button-binding inventory is compared
- **THEN** all actions including key-tip, BGM/SFX, settings, back, level selection, start, exit, restart, next and menu have a target binding and test/manual evidence

### Requirement: Asset and GUID migration is lossless
The migration SHALL preserve source `.meta` GUIDs for migrated product assets, SHALL avoid overwriting framework folder metadata and SHALL account for every required scene, Prefab, Animator, Animation Clip, Spine asset, image, audio and video.

#### Scenario: Compare asset manifests
- **WHEN** source and target product-asset manifests are generated
- **THEN** every required source asset has exactly one target disposition (migrated, deliberately replaced with mapping, or removed with approved evidence) and no duplicate GUID is introduced

#### Scenario: Validate serialized references
- **WHEN** all migrated scenes, Prefabs, ScriptableObjects, Animator Controllers, materials and clips are scanned/imported
- **THEN** there are no missing script, missing GUID, broken object reference or unresolved Animation Event receiver required by a feature

### Requirement: Code removal requires positive evidence
An empty, prototype or test-only source script MAY be omitted only when reference searches across scenes, Prefabs, animation assets and code prove it is not part of a required runtime path; the evidence SHALL be recorded.

#### Scenario: Omit a legacy script
- **WHEN** a candidate script such as an empty or prototype class is not migrated
- **THEN** the removal record contains its GUID/class, search scope, zero required references and replacement/not-needed rationale

### Requirement: Behavior fixes are isolated and testable
Every intentional behavior difference from the source SHALL be listed in `Documentation/Refactor/behavior-fixes.md` with source evidence, defect rationale, target behavior and a regression check; unlisted gameplay or balance changes MUST NOT be introduced.

#### Scenario: Fix Boss2 orange slot indexing
- **WHEN** the known orange-segment index mismatch is corrected
- **THEN** the record points to the source mismatch, specifies the same-slot invariant and links to the orange-segment regression test

#### Scenario: Detect an unrecorded difference
- **WHEN** manual or automated comparison finds target behavior that differs from source and lacks a fix record
- **THEN** the relevant migration task remains incomplete until the behavior is restored or the difference is documented and approved under the confirmed policy

### Requirement: Unity compilation has zero errors
The target project SHALL import and compile with Unity `2022.3.62f3c1` with zero compiler errors; warnings SHALL be triaged and no new warning may conceal a missing feature or broken serialization.

#### Scenario: Clean batch import and compile
- **WHEN** Unity runs a clean target-project import/compile in batch mode or an equivalently fresh Library
- **THEN** the process exits successfully with zero C# compile errors, zero package-resolution failures and zero missing required shader errors

### Requirement: Core rules have deterministic EditMode tests
EditMode tests SHALL cover color/weapon mapping, Boss1/Boss2 weakness distributions, matching damage, health/invulnerability, dash heal/time-effect arbitration, weapon guarantee logic, state transitions and single-shot battle results.

#### Scenario: Run EditMode suite
- **WHEN** the target EditMode test suite runs from a clean compiled state
- **THEN** every required domain case passes deterministically without loading a gameplay scene or reading hardware input

### Requirement: Framework integration has PlayMode smoke tests
PlayMode tests SHALL cover the Launch-to-StartMenu chain, each scene transition, semantic input injection, representative player attack/weapon lifecycle, pause/resume, UI form lifecycle and Entity/Sound cleanup.

#### Scenario: Run PlayMode smoke suite
- **WHEN** the target PlayMode suite runs
- **THEN** it completes all defined framework integration paths without unexpected log errors, duplicate persistent objects or leaked scene-owned entities/forms

### Requirement: Three-scene manual regression is mandatory
Completion SHALL require a recorded full manual regression of `StartMenu`, `Boss1` and `Boss2` from the framework Launch entry, including visuals, audio, video, input, animation/Spine events, menus, both victory routes and defeat/restart routes.

#### Scenario: Execute StartMenu checklist
- **WHEN** the StartMenu manual run is performed
- **THEN** intro/loop video, start/selection/settings/back/exit, key-tip and BGM/SFX controls, loading presentation and both test-level routes have pass/fail evidence

#### Scenario: Execute Boss1 checklist
- **WHEN** the Boss1 manual run is performed from Launch
- **THEN** player controls, all three Boss1 weapon families/colors, tutorials, six boss attacks, attack-five immunity, HUD, pause, damage/death/restart and victory-to-Boss2 have pass/fail evidence

#### Scenario: Execute Boss2 checklist
- **WHEN** the Boss2 manual run is performed from Launch
- **THEN** all four colors, knife/axe/airplane, head/tail threshold, burrow/relocation, projectile patterns/markers, HUD, pause, damage/death/restart and victory/result-to-menu have pass/fail evidence

### Requirement: Completion status is evidence-based
No migration task, capability or overall change SHALL be marked complete solely because files exist, code compiles or obvious errors are absent; all required direct evidence MUST be present and passing.

#### Scenario: Evaluate final completion
- **WHEN** the team evaluates whether the OpenSpec change can be archived
- **THEN** every requirement, task, traceability row and validation gate has passing direct evidence and no unresolved required item remains

