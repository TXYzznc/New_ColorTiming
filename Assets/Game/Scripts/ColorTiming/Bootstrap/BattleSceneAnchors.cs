using System;
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
            public AudioSource source;
            public ColorTimingSoundChannel channel;
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
        }
    }
}
