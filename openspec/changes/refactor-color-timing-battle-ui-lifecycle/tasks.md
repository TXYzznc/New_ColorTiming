## 1. Presentation contracts

- [x] 1.1 Add BattleTutorial form and UI-service contracts with explicit runtime dependency injection.
- [x] 1.2 Implement dynamic battle presentation installation and scene-contract validation.
- [x] 1.3 Route battle-result consumers to the dynamic installer without `UI_Game`.

## 2. Serialized UI and scene migration

- [x] 2.1 Create and register the GF.UI BattleTutorial prefab using the legacy WeaponTip visual subtree.
- [x] 2.2 Add the empty Launch `WorldUIRoot` and validate its reserved attachment contract.
- [x] 2.3 Remove the Boss1/Boss2 `UI_BasePanel` roots and all legacy scene UI bridge content through Unity Editor APIs.

## 3. Verification and documentation

- [ ] 3.1 Add focused tests for installer lifecycle, tutorial first-use behavior, result routing, and scene UI absence.
- [x] 3.2 Run scoped Unity validation and PlayMode tests; record manual Boss1/Boss2 acceptance steps.
- [x] 3.3 Update project scene-flow and UI lifecycle documentation with the dynamic battle presentation contract.
