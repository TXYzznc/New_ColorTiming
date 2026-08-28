## ADDED Requirements

### Requirement: Raw art assets are immutable inputs
Textures, sprites, fonts, audio, video, materials, shaders, particles, Spine data, Animator Controllers, AnimationClips and Timeline assets SHALL retain their content, import settings and GUIDs during the business reimplementation. A move or rename MUST preserve the corresponding `.meta` file.

#### Scenario: Protected manifest is compared
- **WHEN** pre- and post-refactor manifests are compared
- **THEN** every raw protected asset has the same GUID, content hash and relevant importer settings unless an individually approved exception is recorded

### Requirement: Prefab and scene presentation remains intact
Prefab and scene migrations MAY replace script components, serialized field bindings and purely functional nodes, but SHALL preserve authored visual hierarchy, RectTransform values, render order and referenced art assets unless a documented functional requirement requires a change.

#### Scenario: Serialized presentation diff is reviewed
- **WHEN** a modified prefab or scene is compared against its protected baseline
- **THEN** each non-script visual change is either absent or explicitly justified by the approved migration table

### Requirement: Event manifests are complete
The migration SHALL account for the audited 129 Animation Events, 19 UnityEvents and all Spine Event subscriptions, including target method, parameter signature, binding location and lifecycle cleanup.

#### Scenario: Event count changes
- **WHEN** the post-refactor event inventory differs from the protected baseline
- **THEN** validation fails unless every difference is mapped to an approved, behavior-equivalent rebinding

### Requirement: Visual evidence is non-blocking automation
Automated acceptance SHALL prioritize GUID, hash, dependency, import setting, hierarchy, RectTransform, render-order and event-contract evidence. Screenshots MAY supplement diagnostics but SHALL NOT be required to complete automated validation.

#### Scenario: Automated contracts pass without screenshots
- **WHEN** code, lifecycle, resource and serialized-event checks pass
- **THEN** the implementation may proceed to producer-run visual, audio and feel acceptance

### Requirement: Rollback remains available
The pre-refactor Git state and protected manifests SHALL remain available until manual acceptance finishes. The implementation MUST NOT rewrite Git history or destroy the source project.

#### Scenario: Manual acceptance finds a regression
- **WHEN** the producer reports a visual, audio or gameplay regression
- **THEN** the affected mapping can be traced and corrected against the pre-refactor state without recreating art assets
