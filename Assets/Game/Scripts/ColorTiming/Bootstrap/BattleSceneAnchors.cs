// 文件职责：定义 战斗场景锚点，承担 Bootstrap 模块中的对应职责。
// 所属模块：ColorTiming / Bootstrap。

using System;
using System.Collections.Generic;
using ColorTiming.Application.Battle;
using ColorTiming.Combat;
using ColorTiming.Presentation.Audio;
using Cinemachine;
using UnityEngine;

namespace ColorTiming.Bootstrap
{
    /// <summary>
    /// Authoring-only references for a battle scene. This component owns no runtime service or
    /// battle state; the dynamic composition root consumes it once when the scene is loaded.
    /// </summary>
    public sealed class BattleSceneAnchors : MonoBehaviour
    {
        [Serializable]
        public sealed class PlayerSetup
        {
            [SerializeField] private PlayerActorView _prefab;
            [SerializeField] private Vector3 _spawnPosition;
            [SerializeField] private WeaponSpawnerView _weaponSpawner;
            [SerializeField] private CinemachineVirtualCamera _virtualCamera;
            [SerializeField] private Transform _cameraTarget;
            [SerializeField] private PlayerDeathSequenceView _deathSequence;

            public PlayerActorView Prefab => _prefab;
            public Vector3 SpawnPosition => _spawnPosition;
            public WeaponSpawnerView WeaponSpawner => _weaponSpawner;
            public CinemachineVirtualCamera VirtualCamera => _virtualCamera;
            public Transform CameraTarget => _cameraTarget;
            public PlayerDeathSequenceView DeathSequence => _deathSequence;

            public void Validate()
            {
                if (_prefab == null) throw new InvalidOperationException("Player setup requires a Player Prefab.");
                if (_weaponSpawner == null) throw new InvalidOperationException("Player setup requires a WeaponSpawnerView.");
                if (_virtualCamera == null) throw new InvalidOperationException("Player setup requires a CinemachineVirtualCamera.");
                if (_cameraTarget == null) throw new InvalidOperationException("Player setup requires a Boss camera target.");
                if (_deathSequence == null) throw new InvalidOperationException("Player setup requires a PlayerDeathSequenceView.");
            }
        }

        [SerializeField] private PlayerSetup player = new PlayerSetup();
        [SerializeField] Camera gameplayCamera;
        [SerializeField] MonoBehaviour[] explicitBindings = Array.Empty<MonoBehaviour>();

        public PlayerSetup Player => player;
        public Camera GameplayCamera => gameplayCamera;
        public MonoBehaviour[] ExplicitBindings => explicitBindings;

        /// <summary>Collects the configured weapon sets without coupling the bootstrap to a boss type.</summary>
        public IReadOnlyList<WeaponIdentity> GetSupportedWeapons()
        {
            var result = new List<WeaponIdentity>();
            var spawners = GetComponentsInChildren<WeaponSpawnerView>(true);
            for (var i = 0; i < spawners.Length; i++)
            {
                var weapons = spawners[i].GetSupportedWeapons();
                for (var j = 0; j < weapons.Count; j++)
                {
                    if (!result.Contains(weapons[j])) result.Add(weapons[j]);
                }
            }
            return result;
        }

        /// <summary>
        /// Creates a reusable, configuration-derived resource request. Level and wave code
        /// can use the same API without requiring a Unity scene reload.
        /// </summary>
        public BattleLoadContext CreateLoadContext(string contextId)
        {
            return new BattleLoadContext(contextId, GetSupportedWeapons());
        }

        // 执行Validate对应的主要流程。
        public void Validate(BattleKind expectedBattle)
        {
            player.Validate();
            if (gameplayCamera == null) throw new InvalidOperationException("BattleSceneAnchors requires a gameplay camera.");
            var bossCount = 0;
            IBossBattleSessionConsumer boss = null;
            for (var i = 0; i < explicitBindings.Length; i++)
            {
                var binding = explicitBindings[i];
                if (binding == null) throw new InvalidOperationException($"BattleSceneAnchors binding {i} is missing.");
                if (binding is IBossBattleSessionConsumer candidate)
                {
                    boss = candidate;
                    bossCount++;
                }
            }
            if (bossCount != 1)
                throw new InvalidOperationException($"BattleSceneAnchors requires exactly one boss session consumer; found {bossCount}.");
            if (boss.BattleKind != expectedBattle)
                throw new InvalidOperationException(
                    $"BattleSceneAnchors boss kind {boss.BattleKind} does not match loaded battle {expectedBattle}.");
        }
    }
}
