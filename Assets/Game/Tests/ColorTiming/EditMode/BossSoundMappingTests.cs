using System;
using System.Collections.Generic;
using ColorTiming.Presentation.Actors;
using ColorTiming.Presentation.Audio;
using NUnit.Framework;
using UnityEngine;

namespace ColorTiming.Tests.EditMode
{
    public sealed class BossSoundMappingTests
    {
        GameObject host;
        RecordingSoundService sound;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("BossSoundMappingTests");
            sound = new RecordingSoundService();
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(host);

        [Test]
        public void SemanticAndAnimationCuesUseTheSameConfiguredCueId()
        {
            var view = host.AddComponent<BossSoundView>();
            view.SetMappingsForTests(
                (Boss1SoundCues.Hit.Value, "hit"),
                (Boss1SoundCues.Attack1.Value, "atk1"));
            view.BindSoundService(sound);

            Assert.That(view.TryPlay(Boss1SoundCues.Hit), Is.True);
            Assert.That(sound.Cues, Is.EqualTo(new[] { Boss1SoundCues.Hit.Value }));
            sound.Cues.Clear();
            Assert.That(view.TryPlayAnimationCue("atk1"), Is.True);
            Assert.That(sound.Cues, Is.EqualTo(new[] { Boss1SoundCues.Attack1.Value }));
            Assert.That(view.TryPlayAnimationCue("unknown"), Is.False);
        }

        [Test]
        public void DuplicateSemanticCueIsRejected()
        {
            var view = host.AddComponent<BossSoundView>();
            Assert.Throws<InvalidOperationException>(() => view.SetMappingsForTests(
                (Boss2SoundCues.Hit.Value, "hit"),
                (Boss2SoundCues.Hit.Value, "hit-again")));
        }

        [Test]
        public void HitFlash_ResetsShaderPropertyAndDisablesIdleUpdate()
        {
            var renderer = host.AddComponent<MeshRenderer>();
            var flash = host.AddComponent<BossHitFlashView>();
            flash.ConfigureForTests(new Renderer[] { renderer }, 0.2f, 10f);
            flash.Play();
            flash.Advance(0.05f);
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            Assert.That(block.GetFloat(Shader.PropertyToID("_FillPhase")), Is.GreaterThan(0f));
            flash.Advance(0.2f);
            renderer.GetPropertyBlock(block);
            Assert.That(block.GetFloat(Shader.PropertyToID("_FillPhase")), Is.Zero);
            Assert.That(flash.enabled, Is.False);
        }

        sealed class RecordingSoundService : IColorTimingSoundService
        {
            public readonly List<string> Cues = new List<string>();
            public int PlayCue(string cueId, Vector3 position) { Cues.Add(cueId); return Cues.Count; }
            public int Play(AudioClip clip, ColorTimingSoundChannel channel, Vector3 position, bool loop = false) => 0;
            public void ResetTrackedSounds() => Cues.Clear();
            public void Stop(int serialId) { }
        }
    }
}
