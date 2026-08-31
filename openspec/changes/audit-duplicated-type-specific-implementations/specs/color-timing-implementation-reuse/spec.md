## ADDED Requirements

### Requirement: Type differences do not duplicate identical implementation
The project SHALL represent behavior that shares the same structure, lifecycle and core algorithm with one reusable implementation, while expressing Boss, level, weapon or UI differences as validated data or explicit strategy inputs.

#### Scenario: Two boss health bars share one presentation contract
- **WHEN** Boss1 and Boss2 expose the same weakness sequence and use the same health item visuals and layout
- **THEN** BattleHud uses one Boss health slot and one presentation component, with Boss-specific tip content supplied as data

#### Scenario: Similar implementations have real behavioral differences
- **WHEN** two types use different state machines, timing, resource ownership or engine event contracts
- **THEN** the audit records them as legitimate independent implementations instead of forcing a shared abstraction

### Requirement: Ambiguous reuse candidates require an explicit decision
The audit SHALL preserve uncertain candidates until their behavioral differences and migration risks are reviewed by the producer.

#### Scenario: A candidate is mostly identical but lacks sufficient evidence
- **WHEN** static analysis finds duplicated structure but runtime or product evidence does not prove equivalence
- **THEN** the report classifies it as a decision candidate and records the unresolved difference without modifying implementation
