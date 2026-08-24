# Player, weapons and Boss1 implementation coverage

This document closes OpenSpec implementation items 5.1–5.13 and 6.1–6.6. It does not claim the broad manual acceptance items 6.7 or section 12.

| OpenSpec | Primary implementation evidence | Automated/contract evidence |
|---|---|---|
| 5.1–5.4 | `HeroController`, `HeroAnimStae`, `HeroCamera_`, `Death_sc_Over`; framework input/time/scene-flow consumers | `PlayerActionStateMachineTests`: locomotion gates, Dash capture, hit rejection, death lock; combat health tests; animation contract |
| 5.5 | `Pickup_Weapon`, `WeaponSpawnerView`, `WeaponInventory`, GF transient entity participant/release callback | `PlayerWeaponRuntimeTests`; framework entity lifecycle; all-scene PlayMode cleanup |
| 5.6 | `WeaponVocabulary`, `WeaponPresentationState`, migrated sprite/UI arrays | all legacy indices round-trip; every color/type maps to authored presentation slots |
| 5.7 | `HeroController` attack gate plus `Atk`/`Atk_x` Animator contract | `AttackGate_PreservesResumeGuardAndHeldAnimatorContract`; Animator audit |
| 5.8–5.9 | `HeroFrireSystem`, `Skill_base` family, `GfTransientEntityService`, DamageRequest adapter, HitFX and Cinemachine impulse bridges | entity release exactly once; DamageRequest/color tests; Cinemachine serialization/runtime audit |
| 5.10–5.12 | hero/skill Animation Event receivers, EnterAnimStateEvent, RestXuli, Xuli and controllers | animation contract PASS: 13 receiver families, 9 parameters and all SMB references |
| 5.13 | Boss-specific spawner policies, active-limit scan, current-weakness guarantee and first-tip logic; world weapons now GF.Entity-owned | spawn policy/runtime tests and 5/5 PlayMode lifecycle across both Boss scenes |
| 6.1 | `Boss1DistanceZones`, `Boss1AttackSelector`, `Boss1AttackCycle` | distance precedence, source weight boundaries, anti-repeat and cooldown tests |
| 6.2–6.3 | `Boss1_Controller`, `Boss1Anim`, six authored attack animation/skill/sound branches and Spine events | Spine listener audit; source/target asset and Animation Event reconciliation |
| 6.4 | attack-5 animation selection, temporary invulnerability and weakness view restoration | wrong-color/invulnerability mutation test; attack-5 anti-repeat/selection test; FIX-002 |
| 6.5 | 11-segment weakness queue, damage projection and GF.UI HUD presenters | distribution, match/mismatch, upcoming projection and UI lifecycle tests |
| 6.6 | single-shot `BattleState` victory and framework scene flow to Boss2 | `BossVictoryIsSingleShot`; StartMenu→Boss1→Boss2 PlayMode route |

Latest regression after entity/listener fixes: full EditMode 201/201, ColorTiming PlayMode 5/5, compile and console errors 0.

Manual attack-by-attack visuals, player death camera feel, audio timing and final-hit presentation remain explicitly open in 6.7/12.2/12.4.
