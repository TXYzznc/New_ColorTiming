using System.Collections.Generic;
using ColorTiming.Bootstrap.Flow;
using NUnit.Framework;

namespace ColorTiming.Tests.EditMode
{
    public sealed class ColorTimingSceneFlowTests
    {
        [Test]
        public void TransitionProgress_IsMonotonicAndCompletesAtOne()
        {
            var requested = new List<ColorTimingSceneId>();
            var progress = new List<float>();
            using (var flow = new ColorTimingSceneFlow(requested.Add))
            {
                flow.TransitionProgress += progress.Add;

                Assert.That(flow.TryLoad(ColorTimingSceneId.Boss1), Is.True);
                flow.ReportTransitionProgress(0.5f);
                flow.ReportTransitionProgress(0.25f);
                flow.ReportTransitionProgress(0.8f);
                flow.CompleteTransition(ColorTimingSceneId.Boss1);

                CollectionAssert.AreEqual(new[] { 0.5f, 0.8f, 1f }, progress);
                CollectionAssert.AreEqual(new[] { ColorTimingSceneId.Boss1 }, requested);
            }
        }

        [Test]
        public void Transition_RejectsConcurrentAndSameSceneRequests()
        {
            using (var flow = new ColorTimingSceneFlow(_ => { }))
            {
                Assert.That(flow.TryLoad(ColorTimingSceneId.StartMenu), Is.True);
                Assert.That(flow.TryLoad(ColorTimingSceneId.Boss1), Is.False);
                flow.CompleteTransition(ColorTimingSceneId.StartMenu);
                Assert.That(flow.TryLoad(ColorTimingSceneId.StartMenu), Is.False);
                Assert.That(flow.TryLoad(ColorTimingSceneId.StartMenu, true), Is.True);
            }
        }
    }
}
