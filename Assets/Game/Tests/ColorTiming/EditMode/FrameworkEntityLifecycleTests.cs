using NUnit.Framework;
using UnityEngine;

namespace ColorTiming.Tests.EditMode
{
    public sealed class FrameworkEntityLifecycleTests
    {
        [Test]
        public void SkillAnimationEnd_ReleasesFrameworkEntityExactlyOnce()
        {
            var host = new GameObject("Skill lifecycle test");
            try
            {
                var skill = host.AddComponent<Skill_base>();
                var releases = 0;
                skill.BindFrameworkRelease(() => releases++);
                skill.OnFrameworkEntitySpawned();

                skill.EventEnd_Destroy();
                skill.EventEnd_Destroy();

                Assert.That(releases, Is.EqualTo(1));
                skill.OnFrameworkEntityDespawned();
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }
    }
}
