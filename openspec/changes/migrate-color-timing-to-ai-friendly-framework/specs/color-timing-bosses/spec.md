## ADDED Requirements

### Requirement: Boss1 attack state machine is complete
Boss1 SHALL preserve three player-proximity zones, source attack-selection rules, all six authored attacks, Spine animation/event timing and per-attack sound behavior.

#### Scenario: Select attack by proximity
- **WHEN** Boss1 is ready to attack and the player occupies one of the three defined proximity zones
- **THEN** the code state machine selects only an attack valid for that zone and requests the matching Spine animation

#### Scenario: Complete each Boss1 attack
- **WHEN** any of the six Boss1 attacks runs from entry through its final Spine event
- **THEN** all authored movement, hit windows, spawned effects/skills, sounds and return-to-decision transitions occur once

### Requirement: Boss1 attack-five invulnerability is exact
Boss1 attack 5 SHALL temporarily make the boss immune to weakness damage and SHALL dim/disable weak-point presentation only for the authored interval.

#### Scenario: Hit during attack-five immunity
- **WHEN** a matching-color attack contacts Boss1 inside attack 5's invulnerable interval
- **THEN** no health segment is removed and weak-point presentation reflects immunity

#### Scenario: Attack-five immunity ends
- **WHEN** the authored end event for the invulnerable interval fires
- **THEN** damageability and normal weak-point presentation are restored exactly once

### Requirement: Boss1 hit and victory flow are preserved
Boss1 SHALL update hit visuals and HP UI for every accepted weakness hit and SHALL advance to Boss2 only after the final-hit victory presentation completes.

#### Scenario: Boss1 loses a nonfinal segment
- **WHEN** a valid hit removes a Boss1 segment while segments remain
- **THEN** hit visuals, HP UI and current/upcoming weakness display update without triggering scene completion

#### Scenario: Boss1 is defeated
- **WHEN** the final Boss1 segment is removed
- **THEN** Boss1 victory/death presentation completes and the framework scene flow transitions once to `Boss2`

### Requirement: Boss2 head and tail state machines coordinate
Boss2 SHALL preserve the head/controller behavior, tail/secondary controller, burrow/relocation flow, trail effects and attack decisions based on distance and facing.

#### Scenario: Burrow and relocate
- **WHEN** Boss2 enters the source-equivalent burrow condition
- **THEN** the boss becomes unavailable/hidden as authored, trail and relocation effects run, a valid destination is selected and the boss resurfaces into the correct next state

#### Scenario: Choose melee or projectile behavior
- **WHEN** Boss2 evaluates an attack with a known player distance and facing relationship
- **THEN** only the source-valid melee/projectile branch is selected and its Spine/presentation sequence is requested

### Requirement: Boss2 tail phase activates below twelve segments
The Boss2 tail controller SHALL activate when remaining boss health becomes less than 12 and SHALL not activate early or activate more than once.

#### Scenario: Cross tail threshold
- **WHEN** a valid hit changes Boss2 remaining segments from 12 to 11
- **THEN** the tail phase activates once with the authored presentation and behavior

#### Scenario: Remain above threshold
- **WHEN** Boss2 has 12 or more remaining segments
- **THEN** tail-phase-only attacks and presentation remain inactive

### Requirement: Boss2 skills preserve projectile patterns and markers
Boss2 SHALL spawn every authored projectile pattern, movement behavior and landing marker at the same Spine/animation event timing as the source.

#### Scenario: Spawn patterned projectile attack
- **WHEN** the corresponding Boss2 skill event fires
- **THEN** the configured number, direction, speed and color/visual pattern of projectile entities is created once

#### Scenario: Show landing marker
- **WHEN** a Boss2 attack has a telegraphed landing location
- **THEN** the marker appears before impact at the resolved position and is recycled after the authored attack window

### Requirement: Boss2 weak points include orange correctly
Boss2 SHALL display and remove red, green, purple and orange segments from the same logical slot, and orange handling SHALL not read a different color array index.

#### Scenario: Remove orange segment
- **WHEN** an accepted orange hit removes the current Boss2 orange segment
- **THEN** the exact current segment's visual is removed, the orange count decrements once and the next queued weakness becomes current

### Requirement: Boss Spine events and sounds remain complete
Every Spine event used by Boss1 and Boss2 SHALL resolve to an adapter that produces the expected state transition, skill/effect, hit window or sound without embedding domain decisions in the Spine callback.

#### Scenario: Audit Spine event listeners
- **WHEN** migrated SkeletonData and boss adapters are checked against the source event inventory
- **THEN** every authored event has one documented receiver and no missing-listener exception or duplicate subscription occurs

### Requirement: Boss2 victory returns through result flow
Boss2 defeat SHALL stop all head, tail, projectile and relocation behavior, complete the authored victory/result presentation and return to `StartMenu` through the defined flow.

#### Scenario: Defeat Boss2
- **WHEN** the final Boss2 weakness segment is removed by a valid hit
- **THEN** all boss-owned entities stop or recycle, result UI is shown once and confirmation returns to `StartMenu`

