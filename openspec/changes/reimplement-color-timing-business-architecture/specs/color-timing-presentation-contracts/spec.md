## ADDED Requirements

### Requirement: Unity adapters are thin and state-safe
MonoBehaviour adapters SHALL translate Unity callbacks, physics contacts and serialized events into typed commands, then render application snapshots/events. They MUST NOT duplicate health, inventory, phase, result or pause authority.

#### Scenario: Adapter is disabled and re-enabled
- **WHEN** a pooled or scene adapter passes through disable, reuse and rebind
- **THEN** it removes old subscriptions, clears transient presentation state and binds only the current runtime context

### Requirement: Authored event entry points remain valid
All existing Animation Event, UnityEvent, Animator StateMachineBehaviour and Spine Event entry points SHALL resolve to compatible public adapter methods with preserved parameter signatures and timing unless an audited asset migration explicitly rebinds them.

#### Scenario: Serialized event contract is audited
- **WHEN** the event manifest scans all protected clips, controllers, prefabs and scenes
- **THEN** each event target method exists on the referenced component with a compatible signature

#### Scenario: Spine handler lifecycle completes
- **WHEN** a Spine presentation adapter is disabled, reused or destroyed
- **THEN** every registered track/event handler is unsubscribed exactly once and cannot mutate a disposed session

### Requirement: Physics is a presentation input boundary
Colliders and Rigidbody2D SHALL remain Unity-facing, while collision adapters SHALL construct typed contact/damage commands that contain stable actor identity and value data rather than passing `GameObject` references into domain rules.

#### Scenario: Skill collider contacts a target
- **WHEN** a configured hitbox receives a valid 2D contact
- **THEN** the adapter submits one typed damage command respecting hit policy and does not call a target MonoBehaviour business interface directly

### Requirement: GF UI forms render snapshots and issue intents
BattleHud, BattleTutorial, BattleResult, PauseMenu, MainMenu and Loading SHALL remain GF UI forms. Their scripts SHALL receive view models/snapshots and issue application intents without owning battle lifecycle.

#### Scenario: Pooled battle HUD is reopened
- **WHEN** GF reuses a BattleHud form for a new session
- **THEN** all health items, weapon icon, tutorial hints, cursor state and counters reflect only the new session
