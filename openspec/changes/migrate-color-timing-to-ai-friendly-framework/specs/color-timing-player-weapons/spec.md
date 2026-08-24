## ADDED Requirements

### Requirement: Player locomotion and facing remain equivalent
The player SHALL preserve two-dimensional movement, facing, dash timing, dash invulnerability, hit knockback and movement-audio behavior from the source project.

#### Scenario: Move and face
- **WHEN** nonzero Move input is supplied during a locomotion-capable state
- **THEN** the player moves at the source-equivalent speed, faces the movement/aim direction according to current rules and updates `moveSpeed` and `moveV` presentation parameters

#### Scenario: Dash while available
- **WHEN** Dash is pressed in a state that permits dashing
- **THEN** the player enters dash once, moves with the configured dash profile and is invulnerable only for the animation-event-defined window

#### Scenario: Movement is blocked by state
- **WHEN** the player is dead or in another state that forbids locomotion
- **THEN** Move and Dash input cannot alter the player transform or re-enter locomotion

### Requirement: Weapon pickup, switch and drop are complete
The player SHALL pick up an eligible nearby weapon, replace or switch the held weapon using the existing animation sequence, and drop the current weapon on explicit drop or qualifying damage.

#### Scenario: Pick up a weapon
- **WHEN** the player interacts with an eligible world weapon while pickup is allowed
- **THEN** the weapon becomes held, its world representation is hidden/recycled as designed, cursor/icon state updates and `weaponType`/`switchWeapon` presentation is synchronized

#### Scenario: Drop held weapon
- **WHEN** Drop is pressed or a qualifying hit forces a drop
- **THEN** exactly one world weapon instance appears with the correct color/type, fade/outline behavior and pickup cooldown, and the player returns to the normal weapon state

#### Scenario: Attempt pickup without eligibility
- **WHEN** no eligible weapon is in range or the current state forbids pickup
- **THEN** held-weapon state and world entities remain unchanged

### Requirement: All seven weapon families retain their behavior
Boss1 gameplay SHALL support scissors, hammer and bomb across red/green/purple; Boss2 gameplay SHALL support knife, axe and airplane across red/green/purple/orange; the normal type SHALL remain the unarmed/default state.

#### Scenario: Use Boss1 weapon
- **WHEN** scissors, hammer or bomb is held and Attack is performed
- **THEN** the corresponding source-equivalent melee/skill behavior, color, damage request, animation and audio are produced

#### Scenario: Use Boss2 weapon
- **WHEN** knife, axe or airplane is held and Attack is performed
- **THEN** the corresponding source-equivalent melee, charge or projectile behavior, color, damage request, animation and audio are produced

#### Scenario: Attack unarmed
- **WHEN** the player attacks in the normal weapon state
- **THEN** only the source-supported normal attack behavior occurs and no stale previous-weapon skill is spawned

### Requirement: Attack and charge semantics follow animation timing
Attack execution SHALL preserve pressed/held behavior, charge indicators and animation-event-controlled spawn/damage timing rather than applying damage immediately on input.

#### Scenario: Standard attack
- **WHEN** AttackPressed is accepted in an attack-capable state
- **THEN** the correct `Atk`/`Atk_x` presentation trigger is set and damage or skill creation occurs only at the mapped animation event

#### Scenario: Charge-capable weapon
- **WHEN** AttackHeld remains active for a charge-capable weapon
- **THEN** charge state and UI progress follow the source thresholds, and release or completion produces the correct charged behavior once

### Requirement: Legacy animation events remain wired
The migration SHALL preserve functional equivalents for all authored player/skill Animation Events: `Attack`, `PlayAuido`, `PlayAuido_Random`, `DashWD`, `DashEnd`, `SkillMove`, `Wudi`, `Hit`, `DeathOver`, `EventEnd_Destroy`, `OnFXEnd`, `Cerate` and `End`.

#### Scenario: Audit animation event receivers
- **WHEN** all migrated Animation Clips are scanned for event method names
- **THEN** every event resolves to a compatible adapter method with the expected parameter shape and no event is silently removed

#### Scenario: Recycle event-driven skill
- **WHEN** a skill's end event fires
- **THEN** the skill is returned through the configured Entity/object-pool lifecycle and all subscriptions and async work are cleared

### Requirement: Weapon spawning preserves limits and weakness guarantees
Each boss scene SHALL preserve its timed spawn cadence, maximum active-weapon limit and the source rule that ensures obtainable weapons can satisfy upcoming weaknesses.

#### Scenario: Spawn under the active limit
- **WHEN** the spawn timer elapses and the scene is below its active-weapon limit
- **THEN** one allowed weapon entity is spawned at a valid point with a source-equivalent color/type selection

#### Scenario: Spawn at the active limit
- **WHEN** the spawn timer elapses while the active limit is reached
- **THEN** no additional weapon is spawned and accounting remains accurate

#### Scenario: Required color is unavailable
- **WHEN** the current/near weakness color has no obtainable matching weapon according to the source guarantee condition
- **THEN** the next eligible spawn includes a matching color without introducing a weapon family unavailable in that boss scene

### Requirement: Player death sequence is preserved
Player death SHALL stop combat input, play the authored death animation, run the source-equivalent camera zoom and expose restart/menu flow only after the death sequence reaches its completion event.

#### Scenario: Complete death presentation
- **WHEN** the combat domain emits player defeat
- **THEN** `Death` presentation is entered once, camera behavior runs, `DeathOver` completes the sequence and no weapon or attack can be activated during death

