using System.Collections;
using ColorTiming.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ColorTiming.Tests.PlayMode
{
    public sealed class PlayerEntityLifecyclePlayModeTests
    {
        [UnityTest]
        public IEnumerator SkillAnimationEnd_ReleasesFrameworkEntityExactlyOnce()
        {
            var host = new GameObject("Skill lifecycle test");
            var skill = host.AddComponent<Skill_base>();
            var releases = 0;
            skill.BindFrameworkRelease(() => releases++);
            skill.OnFrameworkEntitySpawned();

            skill.EventEnd_Destroy();
            skill.EventEnd_Destroy();

            Assert.That(releases, Is.EqualTo(1));
            skill.OnFrameworkEntityDespawned();
            Object.Destroy(host);
            yield return null;
        }

        [UnityTest]
        public IEnumerator UnityGameTimeAdapter_ComposesAndReleasesRequests()
        {
            var host = new GameObject("Game time test");
            var gameTime = host.AddComponent<UnityGameTimeAdapter>();
            var slow = gameTime.Acquire(0.45f);
            var pause = gameTime.Acquire(0f);

            Assert.That(Time.timeScale, Is.EqualTo(0f));
            pause.Dispose();
            Assert.That(Time.timeScale, Is.EqualTo(0.45f));
            slow.Dispose();
            Assert.That(Time.timeScale, Is.EqualTo(1f));

            gameTime.Pulse(0.2f, 0.02f);
            Assert.That(Time.timeScale, Is.EqualTo(0.2f));
            yield return new WaitForSecondsRealtime(0.04f);
            Assert.That(Time.timeScale, Is.EqualTo(1f));

            Object.Destroy(host);
            yield return null;
        }
    }
}
