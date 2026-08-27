## ADDED Requirements

### Requirement: Battle scenes have no authored UI root
Boss1 and Boss2 SHALL not contain a Canvas, UI form, UI bridge, or EventSystem whose
responsibility is product battle presentation. The scenes SHALL retain only gameplay and
level content required by their gameplay contracts.

#### Scenario: Inspect a loaded battle scene
- **WHEN** Boss1 or Boss2 has completed scene binding
- **THEN** no `UI_BasePanel`, `BattleHudContext`, or scene-authored product Canvas exists

### Requirement: Dynamic battle presentation composition
The system SHALL create one `BattlePresentationInstaller (Clone)` for each loaded battle
scene. It SHALL wait until scene actor initialization is complete, validate one Hero and
exactly one supported Boss, and dynamically open the battle HUD and tutorial forms.

#### Scenario: Boss1 is loaded
- **WHEN** ColorTiming completes the Boss1 scene transition
- **THEN** one installer resolves the Boss1 Hero and Boss1 controller, then opens one HUD
  and one tutorial form with those runtime dependencies

#### Scenario: Leave a battle scene
- **WHEN** a transition starts away from Boss1 or Boss2
- **THEN** tracked battle forms close, their event subscriptions and pause leases release,
  and the scene-parented installer is destroyed with the unloaded scene

### Requirement: Independent battle tutorial form
The system SHALL provide a GF.UI `BattleTutorial` form that preserves the legacy weapon-tip
behavior for the active battle. It SHALL subscribe only to its bound Hero and SHALL show a
tip once for each non-normal weapon type during that form lifetime.

#### Scenario: First use of a weapon type
- **WHEN** the bound Hero switches to a non-normal weapon type not previously shown in the
  active BattleTutorial form
- **THEN** the corresponding tip becomes visible, game time pauses, and an input press can
  dismiss it only after two seconds of unscaled time

### Requirement: Persistent world UI attachment root
Launch SHALL contain exactly one `WorldUIRoot` reserved for dynamically attached World Space
UI. It SHALL remain empty for the current product and SHALL not host current Screen Space
GF.UI forms.

#### Scenario: Launch initializes the product
- **WHEN** the framework has initialized Launch
- **THEN** `WorldUIRoot` is available as a sibling of the framework UI canvas root and has
  no scene-specific child UI

### Requirement: Battle-result routing without scene UI bridges
Battle result consumers in a battle scene SHALL report their results to the dynamic battle
presentation installer rather than a scene-authored `UI_Game` object. Boss1 progression to
Boss2 SHALL retain its existing real-time delay and scene-flow transition behavior.

#### Scenario: Boss1 is defeated
- **WHEN** the Boss1 result consumer reports Boss1 defeated
- **THEN** the dynamic installer schedules the existing delayed transition to Boss2 without
  a scene-authored UI result bridge
