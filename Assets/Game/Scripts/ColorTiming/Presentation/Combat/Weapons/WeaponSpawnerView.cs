// 文件职责：负责 武器Spawner 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / Combat / Weapons。

using Array = System.Array;
using System.Collections.Generic;
using ColorTiming.Application.Battle;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Combat;
using ColorTiming.Configuration;
using ColorTiming.Player;
using ColorTiming.Presentation.Entities;
using UnityEngine;

/// <summary>
/// 所有 Boss 战共用的武器生成表现；关卡差异由 GF DataTable 注入。
/// </summary>
public sealed class WeaponSpawnerView : MonoBehaviour, ITransientEntityConsumer, IBattleSessionConsumer,
    IColorTimingConfigurationConsumer
{
    public GameObject weaponItem;
    public Transform weaponT;

    private readonly List<WeaponColor> activeColors = new List<WeaponColor>(10);
    private readonly List<Vector3> availablePositions = new List<Vector3>(16);
    private readonly List<WeaponPickupView> trackedPickups = new List<WeaponPickupView>(10);
    private WeaponSpawnerRuntime runtime;
    private ITransientEntityService transientEntities;
    private int damageCount;
    private int tipCount;
    private BattleSession session;
    private int lastWeaknessCount = -1;
    private int trackedWeaponChildCount = -1;
    private WeaponSpawnConfiguration spawnConfiguration;
    private int tutorialTipId;
    private bool started;

    public void BindConfiguration(IColorTimingConfiguration configuration, ColorTimingSceneId sceneId)
    {
        if (configuration == null) throw new System.ArgumentNullException(nameof(configuration));
        var battle = configuration.GetBattle(sceneId);
        spawnConfiguration = configuration.GetWeaponSpawnRule(battle.WeaponSpawnRuleId);
        tutorialTipId = battle.TutorialId;
        TryInitializeRuntime();
    }

    /// <summary>Exposes the authored weapon set for scene-level presentation preloading.</summary>
    public IReadOnlyList<WeaponIdentity> GetSupportedWeapons()
    {
        return spawnConfiguration != null ? spawnConfiguration.AllowedWeapons : Array.Empty<WeaponIdentity>();
    }

    // 绑定战斗会话依赖或事件监听。
    public void BindBattleSession(BattleSession battleSession)
    {
        if (session != null) session.SnapshotChanged -= OnSnapshotChanged;
        session = battleSession ?? throw new System.ArgumentNullException(nameof(battleSession));
        lastWeaknessCount = session.Snapshot.Weaknesses.Count;
        session.SnapshotChanged += OnSnapshotChanged;
    }

    // 绑定TransientEntities依赖或事件监听。
    public void BindTransientEntities(ITransientEntityService entities)
    {
        transientEntities = entities ?? throw new System.ArgumentNullException(nameof(entities));
    }

    // 在首帧启动依赖就绪后的业务或表现流程。
    void Start()
    {
        started = true;
        TryInitializeRuntime();
    }

    private void TryInitializeRuntime()
    {
        if (!started || runtime != null || spawnConfiguration == null)
        {
            return;
        }
        if (spawnConfiguration.SpawnInterval <= 0f)
        {
            throw new System.InvalidOperationException(
                $"{name}: weapon spawn table interval must be greater than zero.");
        }
        runtime = new WeaponSpawnerRuntime(
            spawnConfiguration.SpawnInterval,
            spawnConfiguration.CreatePolicy(),
            new UnityWeaponRandomSource());
        RefreshPickupCache();
    }

    // 组件销毁时释放订阅、句柄和运行时资源。
    void OnDestroy()
    {
        if (session != null) session.SnapshotChanged -= OnSnapshotChanged;
    }

    // 逐帧推进需要实时刷新的业务或表现状态。
    void Update()
    {
        if (runtime == null || session == null
            || session.Snapshot.Lifecycle != BattleLifecycle.Running
            || weaponT == null || weaponItem == null)
        {
            return;
        }
        if (!TryGetCurrentWeakness(out var weakness))
        {
            return;
        }

        CollectActiveColors();
        var decision = runtime.Tick(Time.deltaTime, activeColors, weakness);
        if (decision.ShouldSpawn)
        {
            CreateWeapon(decision.Weapon, GetRandomPosition());
            CheckWeaponTip();
        }
    }

    // 尝试Get当前项弱点，并通过返回值报告是否成功。
    bool TryGetCurrentWeakness(out WeaponColor weakness)
    {
        if (session != null && session.Snapshot.Weaknesses.Count > 0)
        {
            weakness = session.Snapshot.Weaknesses[0];
            return true;
        }
        weakness = default;
        return false;
    }

    // 响应快照变化回调，并更新本对象状态。
    void OnSnapshotChanged(BattleSnapshot snapshot)
    {
        if (lastWeaknessCount >= 0 && snapshot.Weaknesses.Count < lastWeaknessCount) OnBossDamaged();
        lastWeaknessCount = snapshot.Weaknesses.Count;
    }

    // 响应BossDamaged回调，并更新本对象状态。
    protected void OnBossDamaged()
    {
        if (weaponT == null)
        {
            return;
        }

        EnsurePickupCacheCurrent();
        foreach (var pickup in trackedPickups)
        {
            if (pickup != null && pickup.HasWeapon)
            {
                pickup.HideTip();
            }
        }

        damageCount++;
        CheckWeaponTip();
    }

    // 创建武器dis并完成必要的初始配置。
    public void CreateWeapon_dis(Vector3 position, WeaponIdentity weapon)
    {
        CreateWeapon(weapon, position);
    }

    private void CollectActiveColors()
    {
        activeColors.Clear();
        EnsurePickupCacheCurrent();
        foreach (var pickup in trackedPickups)
        {
            if (pickup != null && pickup.HasWeapon)
            {
                activeColors.Add(pickup.Weapon.Color);
            }
        }
    }

    // 创建武器并完成必要的初始配置。
    private void CreateWeapon(WeaponIdentity identity, Vector3 position)
    {
        if (transientEntities == null)
        {
            throw new System.InvalidOperationException(
                $"{name}: transient entity service was not bound before spawning a weapon.");
        }

        transientEntities.Spawn(
            weaponItem.name,
            position,
            Quaternion.identity,
            weaponT,
            instance =>
            {
                var pickup = instance.GetComponent<WeaponPickupView>();
                if (pickup == null)
                {
                    Debug.LogError($"{name}: weapon prefab is missing WeaponPickupView.", weaponItem);
                    return;
                }

                TrackPickup(pickup);
                pickup.InitPickWeapon(identity);
            });
    }

    private Vector3 GetRandomPosition()
    {
        availablePositions.Clear();
        EnsurePickupCacheCurrent();
        foreach (Transform anchor in transform)
        {
            var occupied = false;
            foreach (var pickup in trackedPickups)
            {
                if (pickup != null && pickup.HasWeapon
                    && Vector2.Distance(anchor.position, pickup.transform.position) < spawnConfiguration.MinimumAnchorDistance)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                availablePositions.Add(anchor.position);
            }
        }

        return availablePositions.Count == 0
            ? Vector3.zero
            : availablePositions[Random.Range(0, availablePositions.Count)];
    }

    private void CheckWeaponTip()
    {
        if (damageCount >= spawnConfiguration.TutorialDamageLimit || tipCount > damageCount || weaponT == null)
        {
            return;
        }
        if (!TryGetCurrentWeakness(out var weakness))
        {
            return;
        }

        EnsurePickupCacheCurrent();
        foreach (var pickup in trackedPickups)
        {
            if (pickup != null && pickup.HasWeapon && pickup.Weapon.Color == weakness)
            {
                pickup.ShowTip(tutorialTipId);
                tipCount++;
                return;
            }
        }
    }

    // 子对象结构没有变化时复用组件缓存，避免每帧对每个武器执行 GetComponent。
    private void EnsurePickupCacheCurrent()
    {
        if (weaponT == null)
        {
            trackedPickups.Clear();
            trackedWeaponChildCount = -1;
            return;
        }

        if (trackedWeaponChildCount != weaponT.childCount)
        {
            RefreshPickupCache();
        }
    }

    private void RefreshPickupCache()
    {
        trackedPickups.Clear();
        if (weaponT == null)
        {
            trackedWeaponChildCount = -1;
            return;
        }

        foreach (Transform child in weaponT)
        {
            if (child.TryGetComponent(out WeaponPickupView pickup))
            {
                trackedPickups.Add(pickup);
            }
        }

        trackedWeaponChildCount = weaponT.childCount;
    }

    private void TrackPickup(WeaponPickupView pickup)
    {
        if (pickup != null && !trackedPickups.Contains(pickup))
        {
            trackedPickups.Add(pickup);
        }
        trackedWeaponChildCount = weaponT != null ? weaponT.childCount : -1;
    }

    private sealed class UnityWeaponRandomSource : IRandomSource
    {
        // 执行Range对应的主要流程。
        public int Range(int minimumInclusive, int maximumExclusive)
        {
            return Random.Range(minimumInclusive, maximumExclusive);
        }
    }
}
