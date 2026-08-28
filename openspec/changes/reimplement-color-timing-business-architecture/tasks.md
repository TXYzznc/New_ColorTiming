## 1. Baseline and safety evidence

- [x] 1.1 Create the pre-refactor protected asset manifest for GUIDs, hashes, importer settings, dependencies and serialized event contracts
- [x] 1.2 Record the source feature-ID to current implementation map and the approved FIX-001 through FIX-004 exceptions
- [x] 1.3 Add repeatable validators for raw-art immutability, prefab/scene presentation structure, missing scripts and event target signatures
- [x] 1.4 Obtain a Git integration checkpoint without staging unrelated Editor window-state changes

## 2. Physical module boundaries

- [x] 2.1 Create `ColorTiming.Domain` and `ColorTiming.Application` ordinary runtime asmdefs and wire their one-way dependency into the existing Unity/GF adapter assembly
- [x] 2.2 Move or recreate pure combat, player, weapon and boss rules under Domain without Unity/GF references
- [x] 2.3 Add architecture tests that reject Unity/GF/Presentation references from Domain and Application
- [x] 2.4 Update EditMode and PlayMode test assembly references for the new physical boundaries

## 3. Battle application core

- [x] 3.1 Implement typed actor IDs, commands, domain events, immutable snapshots and terminal battle result semantics
- [x] 3.2 Implement the single authoritative `BattleSession` with deterministic command ordering, pause and disposal
- [x] 3.3 Implement player health, invulnerability, action gating, movement intent, dash, attack and single-slot weapon inventory use cases
- [x] 3.4 Implement weakness, damage and phase rules for Boss1 and Boss2 while preserving source behavior
- [x] 3.5 Add pure EditMode tests for every domain rule, invalid transition, terminal-state guard and seeded random branch

## 4. Explicit runtime composition

- [x] 4.1 Implement `BattleSceneAnchors` and the runtime-created `BattleRuntimeContext (Clone)` with explicit bind/dispose order
- [x] 4.2 Replace whole-scene consumer scanning with explicit player, boss, camera, audio, UI and transient-entity bindings
- [x] 4.3 Implement application ports and GF adapters for input, time, scene flow, UI, sound, settings and transient entities
- [x] 4.4 Replace name-based audio classification with configured semantic sound cues and preserve loop/spatial behavior
- [x] 4.5 Guarantee deterministic `(Clone)` naming, pooling reset, cancellation and exactly-once release for runtime objects

## 5. Unity presentation migration

- [x] 5.1 Rewrite player controller, animation-event, firing, camera and sound scripts as thin session-bound views while preserving script GUIDs and public event signatures
- [x] 5.2 Rewrite shared skill, hitbox, pickup and weapon-spawner scripts to use typed payloads and damage/pickup commands
- [x] 5.3 Rewrite Boss1 actor, Spine event, sound, collision and skill presentation against the session
- [x] 5.4 Rewrite Boss2 body/tail actor, burrow, Spine event, sound, collision and skill presentation against the session
- [x] 5.5 Rewrite BattleHud, BattleTutorial, BattleResult and pause views to consume snapshots/intents and reset all pooled state
- [x] 5.6 Preserve MainMenu and Loading GF form behavior while replacing remaining legacy controller dependencies

## 6. Scene, prefab and legacy cutover

- [x] 6.1 Bind Boss1 and Boss2 scene anchors and required prefab references through Unity while preserving authored visual data
- [x] 6.2 Verify Launch Screen UI roots and WorldUIRoot contracts without introducing static UI roots into battle scenes
- [x] 6.3 Rebind any migrated serialized script fields and confirm all AnimationEvent, UnityEvent, Animator and Spine targets
- [x] 6.4 Delete replaced `Legacy` business controllers, compatibility `Weapon`, `I_Damage`, scan binders and obsolete adapters after all call sites are migrated
- [x] 6.5 Confirm the final product has no duplicate authoritative battle state and no missing script/reference

## 7. Verification and handoff

- [x] 7.1 Compile with zero errors and run targeted then full EditMode suites
- [x] 7.2 Run targeted then full PlayMode suites including GF UI pooling and StartMenu/Boss1/Boss2 load-unload-reload lifecycle
- [x] 7.3 Compare protected asset/event manifests and review every allowed prefab/scene serialized difference
- [x] 7.4 Run framework purity and repository integrity audits without modifying `ScriptsBuiltin`
- [x] 7.5 Update the feature audit, migration evidence and manual visual/audio/feel acceptance checklist
- [x] 7.6 Hand final scoped changes to the Git integration task for Chinese commits without pushing
