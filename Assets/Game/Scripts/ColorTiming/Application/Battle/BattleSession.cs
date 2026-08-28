using System;
using ColorTiming.Bosses.Boss2;
using ColorTiming.Combat;
using ColorTiming.Player;

namespace ColorTiming.Application.Battle
{
    /// <summary>
    /// Single authoritative battle state. Unity views submit intent here and render snapshots;
    /// no method may be called after <see cref="Dispose"/> except <see cref="Snapshot"/>.
    /// </summary>
    public sealed class BattleSession : IDisposable
    {
        private readonly PlayerVitality playerVitality;
        private readonly BossBattleHealth bossHealth;
        private readonly Boss2PhaseCoordinator boss2Phase;
        private long version;
        private bool disposed;
        private bool tailActive;

        public BattleSession(BattleKind kind, IRandomSource random, int playerMaximumHealth = 5)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            Kind = kind;
            PlayerActions = new PlayerActionStateMachine();
            Inventory = new PlayerWeaponInventory();
            playerVitality = new PlayerVitality(playerMaximumHealth);
            var weaknesses = kind == BattleKind.Boss1
                ? WeaknessQueue.CreateBoss1(random)
                : WeaknessQueue.CreateBoss2(random);
            bossHealth = new BossBattleHealth(weaknesses);
            boss2Phase = kind == BattleKind.Boss2 ? new Boss2PhaseCoordinator(weaknesses.Count) : null;
            Lifecycle = BattleLifecycle.Running;
            Inventory.Changed += OnWeaponChanged;
            Snapshot = BuildSnapshot();
        }

        public BattleKind Kind { get; }
        public BattleLifecycle Lifecycle { get; private set; }
        public PlayerActionStateMachine PlayerActions { get; }
        public PlayerWeaponInventory Inventory { get; }
        public BattleSnapshot Snapshot { get; private set; }
        public event Action<BattleSnapshot> SnapshotChanged;
        public event Action<BattlePresentationEvent> PresentationRequested;

        public void Tick(float deltaTime)
        {
            EnsureRunningOrPaused();
            if (Lifecycle == BattleLifecycle.Running) PlayerActions.Tick(deltaTime);
        }

        public void SetPaused(bool paused)
        {
            EnsureMutable();
            if (IsTerminal) return;
            var next = paused ? BattleLifecycle.Paused : BattleLifecycle.Running;
            if (Lifecycle == next) return;
            Lifecycle = next;
            PublishSnapshot();
        }

        public void SetMove(float x, float y)
        {
            EnsureMutable();
            if (Lifecycle == BattleLifecycle.Running) PlayerActions.SetMove(x, y);
        }

        public void SetSkillMoving(bool active)
        {
            EnsureMutable();
            if (!IsTerminal) PlayerActions.SetSkillMoving(active);
        }

        public void SetHitAnimationActive(bool active)
        {
            EnsureMutable();
            if (active) PlayerActions.BeginHit();
            else PlayerActions.EndHit();
        }

        public bool TryBeginDash()
        {
            EnsureMutable();
            if (Lifecycle != BattleLifecycle.Running || !PlayerActions.BeginDash()) return false;
            PublishSnapshot();
            return true;
        }

        public void EndDash()
        {
            EnsureMutable();
            var before = PlayerActions.State;
            PlayerActions.EndDash();
            if (before != PlayerActions.State) PublishSnapshot();
        }

        public void SetDashInvulnerable(bool active)
        {
            EnsureMutable();
            PlayerActions.SetDashInvulnerable(active);
        }

        public void SetAnimationInvulnerable(bool active)
        {
            EnsureMutable();
            PlayerActions.SetAnimationInvulnerable(active);
        }

        public bool TryBeginAttack()
        {
            EnsureMutable();
            if (Lifecycle != BattleLifecycle.Running || !PlayerActions.BeginAttack()) return false;
            PublishSnapshot();
            return true;
        }

        public void EndAttack()
        {
            EnsureMutable();
            var before = PlayerActions.State;
            PlayerActions.EndAttack();
            if (before != PlayerActions.State) PublishSnapshot();
        }

        public bool TryPickup(WeaponIdentity weapon)
        {
            EnsureMutable();
            return Lifecycle == BattleLifecycle.Running && Inventory.TryPickup(weapon);
        }

        public bool TryDrop(out WeaponIdentity weapon)
        {
            EnsureMutable();
            return Inventory.TryDrop(out weapon);
        }

        public bool ConsumeAttackWeapon(out WeaponIdentity weapon)
        {
            EnsureMutable();
            return Inventory.ConsumeAttackWeapon(out weapon);
        }

        public PlayerDamageResolution ApplyPlayerDamage(BattleDamage damage)
        {
            EnsureMutable();
            if (damage.Target != ActorId.Player) throw new ArgumentException("Damage target is not the player.", nameof(damage));
            var resolution = playerVitality.TakeDamage(1, PlayerActions.RejectsDamage, damage.IsInstantKill);
            if (resolution != PlayerDamageResolution.Damaged && resolution != PlayerDamageResolution.Defeated) return resolution;

            Inventory.TryDrop(out _);
            PlayerActions.BeginHit();
            PresentationRequested?.Invoke(new BattlePresentationEvent(BattlePresentationEventKind.PlayerDamaged));
            if (resolution == PlayerDamageResolution.Defeated)
            {
                PlayerActions.Kill();
                Lifecycle = BattleLifecycle.Defeat;
                PresentationRequested?.Invoke(new BattlePresentationEvent(BattlePresentationEventKind.BattleLost));
            }
            PublishSnapshot();
            return resolution;
        }

        public int ResolveSuccessfulDash()
        {
            EnsureMutable();
            if (!PlayerActions.CanEvadeDamage) return 0;
            var healed = playerVitality.ResolveSuccessfulDash();
            if (healed > 0)
            {
                PresentationRequested?.Invoke(new BattlePresentationEvent(BattlePresentationEventKind.PlayerHealed));
                PublishSnapshot();
            }
            return healed;
        }

        public BossDamageResolution ApplyBossDamage(BattleDamage damage)
        {
            EnsureMutable();
            if (damage.Target != ActorId.BossHead && damage.Target != ActorId.BossTail)
                throw new ArgumentException("Damage target is not a boss actor.", nameof(damage));
            var resolution = bossHealth.Apply(damage);
            if (resolution != BossDamageResolution.Damaged && resolution != BossDamageResolution.Victory) return resolution;

            PresentationRequested?.Invoke(new BattlePresentationEvent(BattlePresentationEventKind.BossDamaged, damage.Weapon.Color));
            if (boss2Phase != null && boss2Phase.ObserveRemaining(bossHealth.Weaknesses.Count) && !tailActive)
            {
                tailActive = true;
                PresentationRequested?.Invoke(new BattlePresentationEvent(BattlePresentationEventKind.BossTailActivated));
            }
            if (resolution == BossDamageResolution.Victory)
            {
                Lifecycle = BattleLifecycle.Victory;
                PresentationRequested?.Invoke(new BattlePresentationEvent(BattlePresentationEventKind.BattleWon));
            }
            PublishSnapshot();
            return resolution;
        }

        public void SetBossDamageable(bool damageable)
        {
            EnsureMutable();
            if (bossHealth.IsDamageable == damageable) return;
            bossHealth.IsDamageable = damageable;
            PublishSnapshot();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            Inventory.Changed -= OnWeaponChanged;
            Lifecycle = BattleLifecycle.Disposed;
            PublishSnapshot(false);
            SnapshotChanged = null;
            PresentationRequested = null;
        }

        private bool IsTerminal => Lifecycle == BattleLifecycle.Victory || Lifecycle == BattleLifecycle.Defeat;

        private void OnWeaponChanged(WeaponIdentity weapon)
        {
            PresentationRequested?.Invoke(new BattlePresentationEvent(BattlePresentationEventKind.PlayerWeaponChanged, weapon.Color));
            PublishSnapshot();
        }

        private void PublishSnapshot(bool notify = true)
        {
            Snapshot = BuildSnapshot();
            if (notify) SnapshotChanged?.Invoke(Snapshot);
        }

        private BattleSnapshot BuildSnapshot()
        {
            return new BattleSnapshot(
                ++version,
                Kind,
                Lifecycle,
                playerVitality.Health.Current,
                playerVitality.Health.Maximum,
                PlayerActions.State,
                Inventory.Current,
                bossHealth.Weaknesses.Upcoming(bossHealth.Weaknesses.Count),
                bossHealth.IsDamageable,
                tailActive);
        }

        private void EnsureRunningOrPaused()
        {
            EnsureMutable();
            if (IsTerminal) return;
        }

        private void EnsureMutable()
        {
            if (disposed) throw new ObjectDisposedException(nameof(BattleSession));
        }
    }
}
