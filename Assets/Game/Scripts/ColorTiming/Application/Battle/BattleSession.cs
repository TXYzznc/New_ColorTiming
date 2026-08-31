// 文件职责：协调单场战斗的生命周期、领域状态和展示快照。
// 所属模块：ColorTiming / Application / Battle。

using System;
using ColorTiming.Bosses.Boss2;
using ColorTiming.Combat;
using ColorTiming.Player;
using ColorTiming.Configuration;

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
        private readonly int damagePerHit;

        // 初始化战斗会话实例及其核心依赖。
        public BattleSession(BattleRulesConfiguration configuration, IRandomSource random)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (random == null) throw new ArgumentNullException(nameof(random));
            Kind = configuration.Kind;
            PlayerActions = new PlayerActionStateMachine(configuration.Player.HitInvulnerabilitySeconds);
            Inventory = new PlayerWeaponInventory();
            playerVitality = new PlayerVitality(configuration.Player.MaximumHealth, configuration.Player.DashHeal);
            damagePerHit = configuration.Player.DamagePerHit;
            var weaknesses = WeaknessQueue.Create(random, configuration.Weaknesses);
            bossHealth = new BossBattleHealth(weaknesses);
            boss2Phase = Kind == BattleKind.Boss2
                ? new Boss2PhaseCoordinator(weaknesses.Count, configuration.TailActivationRemaining)
                : null;
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

        // 按当前时间步推进核心状态，并发布必要的状态变化。
        public void Tick(float deltaTime)
        {
            EnsureRunningOrPaused();
            if (Lifecycle == BattleLifecycle.Running) PlayerActions.Tick(deltaTime);
        }

        // 设置暂停状态，并使后续流程使用最新状态。
        public void SetPaused(bool paused)
        {
            EnsureMutable();
            if (IsTerminal) return;
            var next = paused ? BattleLifecycle.Paused : BattleLifecycle.Running;
            if (Lifecycle == next) return;
            Lifecycle = next;
            PublishSnapshot();
        }

        // 设置移动输入，并使后续流程使用最新状态。
        public void SetMove(float x, float y)
        {
            EnsureMutable();
            if (Lifecycle == BattleLifecycle.Running) PlayerActions.SetMove(x, y);
        }

        // 设置技能移动状态，并使后续流程使用最新状态。
        public void SetSkillMoving(bool active)
        {
            EnsureMutable();
            if (!IsTerminal) PlayerActions.SetSkillMoving(active);
        }

        // 设置Hit动画激活状态，并使后续流程使用最新状态。
        public void SetHitAnimationActive(bool active)
        {
            EnsureMutable();
            if (active) PlayerActions.BeginHit();
            else PlayerActions.EndHit();
        }

        // 尝试开始冲刺，并通过返回值报告是否成功。
        public bool TryBeginDash()
        {
            EnsureMutable();
            if (Lifecycle != BattleLifecycle.Running || !PlayerActions.BeginDash()) return false;
            PublishSnapshot();
            return true;
        }

        // 执行结束冲刺对应的主要流程。
        public void EndDash()
        {
            EnsureMutable();
            var before = PlayerActions.State;
            PlayerActions.EndDash();
            if (before != PlayerActions.State) PublishSnapshot();
        }

        // 设置冲刺无敌状态，并使后续流程使用最新状态。
        public void SetDashInvulnerable(bool active)
        {
            EnsureMutable();
            PlayerActions.SetDashInvulnerable(active);
        }

        // 设置动画无敌状态，并使后续流程使用最新状态。
        public void SetAnimationInvulnerable(bool active)
        {
            EnsureMutable();
            PlayerActions.SetAnimationInvulnerable(active);
        }

        // 尝试开始攻击，并通过返回值报告是否成功。
        public bool TryBeginAttack()
        {
            EnsureMutable();
            if (Lifecycle != BattleLifecycle.Running || !PlayerActions.BeginAttack()) return false;
            PublishSnapshot();
            return true;
        }

        // 执行结束攻击对应的主要流程。
        public void EndAttack()
        {
            EnsureMutable();
            var before = PlayerActions.State;
            PlayerActions.EndAttack();
            if (before != PlayerActions.State) PublishSnapshot();
        }

        // 尝试拾取，并通过返回值报告是否成功。
        public bool TryPickup(WeaponIdentity weapon)
        {
            EnsureMutable();
            return Lifecycle == BattleLifecycle.Running
                   && PlayerActions.CanInteractWithWeapons
                   && Inventory.TryPickup(weapon);
        }

        // 尝试丢弃，并通过返回值报告是否成功。
        public bool TryDrop(out WeaponIdentity weapon)
        {
            EnsureMutable();
            weapon = Inventory.Current;
            return Lifecycle == BattleLifecycle.Running
                   && PlayerActions.CanInteractWithWeapons
                   && Inventory.TryDrop(out weapon);
        }

        // 执行Consume攻击武器对应的主要流程。
        public bool ConsumeAttackWeapon(out WeaponIdentity weapon)
        {
            EnsureMutable();
            return Inventory.ConsumeAttackWeapon(out weapon);
        }

        // 把当前规则或配置应用到玩家伤害。
        public PlayerDamageResolution ApplyPlayerDamage(BattleDamage damage)
        {
            EnsureMutable();
            if (damage.Target != ActorId.Player) throw new ArgumentException("Damage target is not the player.", nameof(damage));
            var resolution = playerVitality.TakeDamage(damagePerHit, PlayerActions.RejectsDamage, damage.IsInstantKill);
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

        // 解析成功冲刺并返回可供上层使用的结果。
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

        // 把当前规则或配置应用到Boss伤害。
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

        // 设置BossDamageable，并使后续流程使用最新状态。
        public void SetBossDamageable(bool damageable)
        {
            EnsureMutable();
            if (bossHealth.IsDamageable == damageable) return;
            bossHealth.IsDamageable = damageable;
            PublishSnapshot();
        }

        // 释放本对象持有的订阅、服务和临时资源。
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

        // 响应武器变化回调，并更新本对象状态。
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

        // 根据当前配置构建快照。
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
