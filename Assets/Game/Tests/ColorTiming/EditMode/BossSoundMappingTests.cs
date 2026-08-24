using System.Collections.Generic;
using ColorTiming.Presentation.Audio;
using NUnit.Framework;
using UnityEngine;

namespace ColorTiming.Tests.EditMode
{
    public sealed class BossSoundMappingTests
    {
        GameObject host;
        RecordingSoundService sound;
        readonly List<AudioClip> clips = new List<AudioClip>();

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("BossSoundMappingTests");
            sound = new RecordingSoundService();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host);
            foreach (var clip in clips)
            {
                Object.DestroyImmediate(clip);
            }
            clips.Clear();
        }

        [Test]
        public void Boss1_MapsEveryAuthoredCueToBossSoundChannel()
        {
            var manager = host.AddComponent<Boss1SoundManager>();
            var expected = new Dictionary<string, AudioClip>
            {
                ["hit"] = manager.hit = Clip("boss1-hit"),
                ["atkReady"] = manager.atkReady = Clip("boss1-ready"),
                ["atkEnd"] = manager.atkEnd = Clip("boss1-end"),
                ["atk1"] = manager.atk1 = Clip("boss1-atk1"),
                ["atk2"] = manager.atk2 = Clip("boss1-atk2"),
                ["atk3_1"] = manager.atk3 = Clip("boss1-atk3"),
                ["atk4"] = manager.atk4 = Clip("boss1-atk4"),
                ["atk5"] = manager.atk5 = Clip("boss1-atk5"),
                ["atk6"] = manager.atk6 = Clip("boss1-atk6"),
            };
            manager.BindSoundService(sound);

            foreach (var pair in expected)
            {
                sound.Reset();
                manager.PlayBoss1Sound(pair.Key);
                AssertSingleBossCue(pair.Value, pair.Key);
            }

            sound.Reset();
            manager.PlayBoss1Sound("unknown");
            Assert.That(sound.Calls, Is.Empty);
        }

        [Test]
        public void Boss2_MapsEveryAuthoredCueToBossSoundChannel()
        {
            var manager = host.AddComponent<Boss2SoundManager>();
            var expected = new Dictionary<string, AudioClip>
            {
                ["hit"] = manager.hit = Clip("boss2-hit"),
                ["rt_t"] = manager.rt_tou = Clip("boss2-head-enter"),
                ["ct_t"] = manager.ct_tou = Clip("boss2-head-exit"),
                ["rt_w"] = manager.rt_wei = Clip("boss2-tail-enter"),
                ["ct_w"] = manager.ct_wei = Clip("boss2-tail-exit"),
                ["atk1_t"] = manager.atk1_tou = Clip("boss2-head-atk1"),
                ["atk2_t"] = manager.atk2_tou = Clip("boss2-head-atk2"),
                ["atk1_w"] = manager.atk1_wei = Clip("boss2-tail-atk1"),
                ["atk2_w"] = manager.atk2_wei = Clip("boss2-tail-atk2"),
            };
            manager.BindSoundService(sound);

            foreach (var pair in expected)
            {
                sound.Reset();
                manager.PlayBoss2Sound(pair.Key);
                AssertSingleBossCue(pair.Value, pair.Key);
            }

            sound.Reset();
            manager.PlayBoss2Sound("unknown");
            Assert.That(sound.Calls, Is.Empty);
        }

        void AssertSingleBossCue(AudioClip expected, string key)
        {
            Assert.That(sound.Calls, Has.Count.EqualTo(1), $"Cue '{key}' must play exactly once.");
            Assert.That(sound.Calls[0].Clip, Is.SameAs(expected));
            Assert.That(sound.Calls[0].Channel, Is.EqualTo(ColorTimingSoundChannel.Boss));
            Assert.That(sound.Calls[0].Loop, Is.False);
        }

        AudioClip Clip(string name)
        {
            var clip = AudioClip.Create(name, 1, 1, 44100, false);
            clips.Add(clip);
            return clip;
        }

        sealed class RecordingSoundService : IColorTimingSoundService
        {
            public readonly List<Call> Calls = new List<Call>();

            public int Play(AudioClip clip, ColorTimingSoundChannel channel, Vector3 position, bool loop = false)
            {
                Calls.Add(new Call(clip, channel, loop));
                return Calls.Count;
            }

            public void ResetTrackedSounds()
            {
                Reset();
            }

            public void Reset()
            {
                Calls.Clear();
            }
        }

        readonly struct Call
        {
            public Call(AudioClip clip, ColorTimingSoundChannel channel, bool loop)
            {
                Clip = clip;
                Channel = channel;
                Loop = loop;
            }

            public AudioClip Clip { get; }
            public ColorTimingSoundChannel Channel { get; }
            public bool Loop { get; }
        }
    }
}
