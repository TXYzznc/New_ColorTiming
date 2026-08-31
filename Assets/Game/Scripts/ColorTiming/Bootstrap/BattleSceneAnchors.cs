// 文件职责：定义 战斗场景锚点，承担 Bootstrap 模块中的对应职责。
// 所属模块：ColorTiming / Bootstrap。

using System;
using System.Collections.Generic;
using ColorTiming.Combat;
using ColorTiming.Presentation.Audio;
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
        public struct SoundCue
        {
            [SerializeField] AudioClip clip;
            [SerializeField] ColorTimingSoundChannel channel;
            [SerializeField] bool loop;
            [SerializeField] Vector3 position;

            public AudioClip Clip => clip;
            public ColorTimingSoundChannel Channel => channel;
            public bool Loop => loop;
            public Vector3 Position => position;
        }

        [SerializeField] PlayerActorView hero;
        [SerializeField] Boss1ActorView boss1;
        [SerializeField] Boss2ActorView boss2;
        [SerializeField] Camera gameplayCamera;
        [SerializeField] MonoBehaviour[] explicitBindings = Array.Empty<MonoBehaviour>();
        [SerializeField] SoundCue[] soundCues = Array.Empty<SoundCue>();

        public PlayerActorView Hero => hero;
        public Boss1ActorView Boss1 => boss1;
        public Boss2ActorView Boss2 => boss2;
        public Camera GameplayCamera => gameplayCamera;
        public MonoBehaviour[] ExplicitBindings => explicitBindings;
        public SoundCue[] SoundCues => soundCues;

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
        public void Validate(bool expectBoss1)
        {
            if (hero == null) throw new InvalidOperationException("BattleSceneAnchors requires one PlayerActorView.");
            if ((boss1 != null) == (boss2 != null))
                throw new InvalidOperationException("BattleSceneAnchors requires exactly one supported boss.");
            if (expectBoss1 != (boss1 != null))
                throw new InvalidOperationException("BattleSceneAnchors boss does not match the loaded scene.");
            if (gameplayCamera == null) throw new InvalidOperationException("BattleSceneAnchors requires a gameplay camera.");
            for (var i = 0; i < explicitBindings.Length; i++)
                if (explicitBindings[i] == null) throw new InvalidOperationException($"BattleSceneAnchors binding {i} is missing.");
            for (var i = 0; i < soundCues.Length; i++)
                if (soundCues[i].Clip == null) throw new InvalidOperationException($"BattleSceneAnchors sound cue {i} is missing its clip.");
        }
    }
}
