using System.Collections.Generic;
using ColorTiming.Application.Battle;
using ColorTiming.Combat;
using ColorTiming.Player;
using ColorTiming.Presentation.Entities;
using UnityEngine;

/// <summary>
/// Shared Unity-facing lifecycle for the two stage-specific weapon generators.
/// Existing concrete MonoBehaviour types retain their scene GUIDs and boss event wiring.
/// </summary>
public abstract class WeaponSpawnerView : MonoBehaviour, ITransientEntityConsumer, IBattleSessionConsumer
{
    public int limitCount = 5;
    public float wTime = 5f;
    public GameObject weaponItem;
    public Transform weaponT;

    private readonly List<WeaponColor> activeColors = new List<WeaponColor>(10);
    private readonly List<Vector3> availablePositions = new List<Vector3>(16);
    private WeaponSpawnerRuntime runtime;
    private ITransientEntityService transientEntities;
    private int damageCount;
    private int tipCount;
    private BattleSession session;
    private int lastWeaknessCount = -1;

    protected abstract WeaponSpawnPolicy CreatePolicy(int activeLimit);
    protected abstract int TutorialTipId { get; }

    public void BindBattleSession(BattleSession battleSession)
    {
        if (session != null) session.SnapshotChanged -= OnSnapshotChanged;
        session = battleSession ?? throw new System.ArgumentNullException(nameof(battleSession));
        lastWeaknessCount = session.Snapshot.Weaknesses.Count;
        session.SnapshotChanged += OnSnapshotChanged;
    }

    public void BindTransientEntities(ITransientEntityService entities)
    {
        transientEntities = entities ?? throw new System.ArgumentNullException(nameof(entities));
    }

    protected virtual void Start()
    {
        if (wTime <= 0f)
        {
            Debug.LogError($"{name}: weapon spawn interval must be positive.", this);
            enabled = false;
            return;
        }

        runtime = new WeaponSpawnerRuntime(
            wTime,
            CreatePolicy(limitCount),
            new UnityWeaponRandomSource());
    }

    protected virtual void OnDestroy()
    {
        if (session != null) session.SnapshotChanged -= OnSnapshotChanged;
    }

    protected virtual void Update()
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

    void OnSnapshotChanged(BattleSnapshot snapshot)
    {
        if (lastWeaknessCount >= 0 && snapshot.Weaknesses.Count < lastWeaknessCount) OnBossDamaged();
        lastWeaknessCount = snapshot.Weaknesses.Count;
    }

    protected void OnBossDamaged()
    {
        if (weaponT == null)
        {
            return;
        }

        foreach (Transform child in weaponT)
        {
            child.GetComponent<WeaponPickupView>()?.HideTip();
        }

        damageCount++;
        CheckWeaponTip();
    }

    public void CreateWeapon_dis(Vector3 position, WeaponIdentity weapon)
    {
        CreateWeapon(weapon, position);
    }

    private void CollectActiveColors()
    {
        activeColors.Clear();
        foreach (Transform child in weaponT)
        {
            var pickup = child.GetComponent<WeaponPickupView>();
            if (pickup != null && pickup.HasWeapon)
            {
                activeColors.Add(pickup.Weapon.Color);
            }
        }
    }

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

                pickup.InitPickWeapon(identity);
            });
    }

    private Vector3 GetRandomPosition()
    {
        availablePositions.Clear();
        foreach (Transform anchor in transform)
        {
            var occupied = false;
            foreach (Transform weapon in weaponT)
            {
                if (Vector2.Distance(anchor.position, weapon.position) < 1f)
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
        if (damageCount > 2 || tipCount > damageCount || weaponT == null)
        {
            return;
        }
        if (!TryGetCurrentWeakness(out var weakness))
        {
            return;
        }

        foreach (Transform child in weaponT)
        {
            var pickup = child.GetComponent<WeaponPickupView>();
            if (pickup != null && pickup.HasWeapon && pickup.Weapon.Color == weakness)
            {
                pickup.ShowTip(TutorialTipId);
                tipCount++;
                return;
            }
        }
    }

    private sealed class UnityWeaponRandomSource : IRandomSource
    {
        public int Range(int minimumInclusive, int maximumExclusive)
        {
            return Random.Range(minimumInclusive, maximumExclusive);
        }
    }
}
