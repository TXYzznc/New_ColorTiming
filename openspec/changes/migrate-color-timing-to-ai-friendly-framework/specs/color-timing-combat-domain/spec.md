## ADDED Requirements

### Requirement: Weapon and color vocabulary is complete
The combat domain SHALL represent the four colors red, green, purple and orange and the seven weapon types normal, scissors, hammer, bomb, knife, axe and airplane without relying on sprite or Animator indices as the domain identity.

#### Scenario: Convert presentation identifiers
- **WHEN** a scene, DataTable row or animation adapter supplies a valid legacy color or weapon identifier
- **THEN** it maps to exactly one typed domain value and can round-trip to the required presentation index

### Requirement: Boss damage requires matching weakness
Boss damage SHALL be accepted only when the attacking weapon color matches the current boss weakness and the target is damageable; rejected hits SHALL not remove a health segment.

#### Scenario: Matching-color hit
- **WHEN** a damageable boss receives a valid attack whose weapon color equals its current weakness
- **THEN** exactly one applicable health segment is removed and hit presentation events are emitted once

#### Scenario: Mismatched-color hit
- **WHEN** a boss receives an attack whose weapon color differs from its current weakness
- **THEN** boss health and weakness order remain unchanged

#### Scenario: Hit during invulnerability
- **WHEN** a matching-color attack reaches a boss during an explicit invulnerability state
- **THEN** no health segment is removed and no false victory is emitted

### Requirement: Weakness queues preserve level distributions
Boss1 SHALL start each battle with 11 shuffled weakness segments containing red 4, green 3 and purple 4; Boss2 SHALL start with 15 shuffled segments containing red 4, green 4, purple 4 and orange 3.

#### Scenario: Initialize Boss1 weaknesses
- **WHEN** a new Boss1 battle context is created
- **THEN** its queue contains exactly 11 entries with the required color counts regardless of shuffle order

#### Scenario: Initialize Boss2 weaknesses
- **WHEN** a new Boss2 battle context is created
- **THEN** its queue contains exactly 15 entries with the required color counts regardless of shuffle order

#### Scenario: Advance weakness
- **WHEN** a valid matching hit removes the current segment and segments remain
- **THEN** the next queued color becomes current and the upcoming-seven projection updates without mutating later order

### Requirement: Player health and damage rules remain equivalent
The player SHALL start a battle with a maximum of 5 health, SHALL lose health only on valid non-invulnerable hits, and SHALL never exceed the maximum when healed.

#### Scenario: Player takes a valid hit
- **WHEN** the player is not invulnerable and receives enemy damage
- **THEN** health decreases by the configured amount, hit/knockback events fire once and the held weapon is dropped according to source behavior

#### Scenario: Player hit during invulnerability
- **WHEN** player damage arrives during dash or hit invulnerability
- **THEN** health does not change and duplicate hit state is not entered

#### Scenario: Successful dash effect
- **WHEN** the source-equivalent successful-dash condition is satisfied
- **THEN** game speed transitions to `0.45`, player health increases by 1 up to 5 and the effect restores safely after its configured duration

### Requirement: Combat results are deterministic and single-shot
Defeat SHALL occur when player health reaches zero; boss victory SHALL occur when the final weakness segment is validly removed; each result SHALL be emitted at most once per battle.

#### Scenario: Player reaches zero health
- **WHEN** a valid hit reduces player health to zero
- **THEN** the battle enters defeat, further combat mutations stop and the death presentation sequence is requested once

#### Scenario: Boss loses final segment
- **WHEN** the final boss segment is removed by a valid matching attack
- **THEN** the battle enters victory and emits one level-complete result after the required presentation sequence

### Requirement: Damage requests retain collision context
The combat boundary SHALL preserve attacker, weapon, collision point and optional parameter data equivalent to the source `I_Damage.OnDamage(attacker, weapon, collisionPoint, parm)` contract without coupling domain rules to a Collider callback.

#### Scenario: Resolve projectile contact
- **WHEN** a projectile adapter reports a collision
- **THEN** the domain receives an immutable damage request containing the originating attacker, weapon identity, contact point and configured parameter

