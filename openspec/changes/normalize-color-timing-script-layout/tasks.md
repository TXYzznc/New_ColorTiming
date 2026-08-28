## 1. Baseline and mapping

- [x] 1.1 Record the exact script move map, pre-move GUIDs, reference baseline and unrelated dirty files
- [x] 1.2 Add a repeatable layout validator for empty legacy directories, UI roles, Infrastructure ownership and namespace placement

## 2. Physical layout migration

- [x] 2.1 Create Presentation UI role directories and move Forms, Components, Presenters, Models and Contracts through Unity AssetDatabase
- [x] 2.2 Create Infrastructure/GF and Infrastructure/Unity directories and move existing adapters through Unity AssetDatabase
- [x] 2.3 Move UI camera helpers to Presentation/Camera and remove only confirmed migration-residue directories

## 3. Namespace and reference normalization

- [x] 3.1 Apply role-specific namespaces to moved UI scripts and update all compile-time references
- [x] 3.2 Apply Infrastructure namespaces to moved GF/Unity adapters and update composition/bootstrap references
- [x] 3.3 Preserve Domain/Application assembly boundaries and leave unrelated legacy serialized presentation types unchanged

## 4. Documentation and verification

- [x] 4.1 Update project architecture documentation with the final script tree and ownership rules
- [x] 4.2 Verify every moved script GUID, protected asset contract, compile state, Missing Script scan and layout validator
- [x] 4.3 Run the ColorTiming EditMode and PlayMode suites and archive evidence in this change
