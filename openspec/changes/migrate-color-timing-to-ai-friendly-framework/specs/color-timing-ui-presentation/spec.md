## ADDED Requirements

### Requirement: Start menu interactions are complete
The migrated StartMenu SHALL preserve intro/start selection, level selection, settings, back navigation and exit actions corresponding to `StartGameBtnDown`, `BackStartBtnDown`, `SettingBtnDwon`, `BackSettingBtnDwon`, `GoTest1`, `GoTest2` and `ExitGameBtn`.

#### Scenario: Navigate main menu panels
- **WHEN** the player opens level selection or settings and then uses the corresponding Back action
- **THEN** the same panels, focus/interactivity and visual states as the source flow are shown without duplicate UI forms

#### Scenario: Exit application
- **WHEN** the player confirms the existing exit action in a build
- **THEN** the application requests a clean quit; in Editor/test mode the request is observable without terminating the test runner

### Requirement: Settings UI controls audio and key tips
The settings UI SHALL preserve `SetBGM`, `SetSFX`, `OffKeyTip` and `OpenKeyTip` behavior and SHALL display the current persisted state whenever opened.

#### Scenario: Toggle BGM
- **WHEN** the player changes the BGM control
- **THEN** only the framework BGM sound group changes mute state and the UI reflects the new value

#### Scenario: Toggle SFX
- **WHEN** the player changes the SFX control
- **THEN** only the framework SFX-related sound group changes mute state and the UI reflects the new value

#### Scenario: Toggle key tips
- **WHEN** the player disables or enables key tips
- **THEN** all applicable tip UI follows the setting immediately and on later scene/UI creation

### Requirement: Player and boss HUD remain complete
Gameplay HUD SHALL show five player HP items, the boss current weakness, up to the next seven weakness items, and the source-equivalent animated first/current tip.

#### Scenario: Player health changes
- **WHEN** player health decreases or heals
- **THEN** exactly the corresponding HP items update and the display remains within zero to five

#### Scenario: Boss weakness advances
- **WHEN** a valid boss hit removes the current segment
- **THEN** current and next-seven items shift to the remaining queue and the current-tip animation targets the new weakness

### Requirement: Weapon cursor and charge UI reflect held weapon
The HUD SHALL show the correct icon/cursor for every held color/type combination and SHALL show charge tips only for charge-capable states.

#### Scenario: Change held weapon
- **WHEN** the player picks up, switches or drops a weapon
- **THEN** icon, cursor color/type and any charge affordance update atomically with the gameplay state

#### Scenario: Charge begins and ends
- **WHEN** a charge-capable attack enters and exits charge state
- **THEN** the charge tip/progress appears, advances with the correct time source and clears on release, cancellation, hit, death or scene exit

### Requirement: Weapon tutorials preserve blocking and dismissal behavior
The first-three weak-point/weapon tutorial sequence SHALL display the same instructional content, use real-time waiting when gameplay is paused and dismiss on accepted any-key input without immediately triggering combat.

#### Scenario: First eligible tutorial appears
- **WHEN** the source-equivalent first encounter with a tutorial weapon/weakness occurs and key tips are enabled
- **THEN** the tutorial UI opens, gameplay is blocked as authored and the correct weapon/color content is displayed

#### Scenario: Dismiss tutorial
- **WHEN** accepted any-key input occurs after the tutorial's minimum display guard
- **THEN** the tutorial closes once, gameplay resumes correctly and the dismissing edge is consumed

### Requirement: Pause, victory, defeat and result forms are complete
The UI SHALL preserve pause controls (`OpenKeyTip`, `OffKeyTip`, restart, next level and back menu), defeat, victory and final result interactions.

#### Scenario: Open pause form
- **WHEN** Pause is pressed during active gameplay
- **THEN** one pause form opens with actions valid for the current scene and remains operable while game time is zero

#### Scenario: Show defeat
- **WHEN** the player death presentation completes
- **THEN** defeat UI exposes the source-equivalent restart/menu actions and prevents underlying gameplay input

#### Scenario: Show victory or final result
- **WHEN** a boss victory sequence completes
- **THEN** the correct next-level or final-result UI appears and confirmation performs exactly one scene-flow action

### Requirement: UI subscriptions and instances are lifecycle-safe
Every migrated UI Form SHALL subscribe and unsubscribe symmetrically, cancel owned async work on close and prevent multiple instances unless the design explicitly allows stacking.

#### Scenario: Reopen a form repeatedly
- **WHEN** a menu, settings or pause form is opened and closed multiple times
- **THEN** each user action invokes one handler, no stale callback targets a closed form and memory does not accumulate owned tasks

