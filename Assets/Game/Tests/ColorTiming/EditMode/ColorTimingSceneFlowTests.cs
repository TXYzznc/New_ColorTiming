using System.Collections.Generic;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Infrastructure.GF.UI;
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
                Assert.That(requested, Is.Empty,
                    "Scene work must wait until the transition presentation signals that it is ready.");
                Assert.That(flow.BeginPendingTransition(), Is.True);
                flow.ReportTransitionProgress(0.5f);
                flow.ReportTransitionProgress(0.25f);
                flow.ReportTransitionProgress(0.8f);
                flow.CompleteTransition(ColorTimingSceneId.Boss1);

                CollectionAssert.AreEqual(new[] { 0.5f, 0.8f, 1f }, progress);
                CollectionAssert.AreEqual(new[] { ColorTimingSceneId.Boss1 }, requested);
            }
        }

        [Test]
        public void TransitionDispatch_IsSingleShotAfterPresentation()
        {
            int dispatchCount = 0;
            using (var flow = new ColorTimingSceneFlow(_ => dispatchCount++))
            {
                Assert.That(flow.TryLoad(ColorTimingSceneId.Boss1), Is.True);
                Assert.That(flow.BeginPendingTransition(), Is.True);
                Assert.That(flow.BeginPendingTransition(), Is.False);

                Assert.That(dispatchCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void Transition_RejectsConcurrentAndSameSceneRequests()
        {
            using (var flow = new ColorTimingSceneFlow(_ => { }))
            {
                Assert.That(flow.TryLoad(ColorTimingSceneId.StartMenu), Is.True);
                Assert.That(flow.BeginPendingTransition(), Is.True);
                Assert.That(flow.TryLoad(ColorTimingSceneId.Boss1), Is.False);
                flow.CompleteTransition(ColorTimingSceneId.StartMenu);
                Assert.That(flow.TryLoad(ColorTimingSceneId.StartMenu), Is.False);
                Assert.That(flow.TryLoad(ColorTimingSceneId.StartMenu, true), Is.True);
                Assert.That(flow.BeginPendingTransition(), Is.True);
            }
        }

        [Test]
        public void TransitionContext_DistinguishesInitialAndSubsequentTransitions()
        {
            var transitions = new List<SceneTransitionContext>();
            using (var flow = new ColorTimingSceneFlow(_ => { }))
            {
                flow.TransitionStarted += transitions.Add;

                Assert.That(flow.TryLoad(ColorTimingSceneId.StartMenu), Is.True);
                Assert.That(flow.BeginPendingTransition(), Is.True);
                flow.CompleteTransition(ColorTimingSceneId.StartMenu);
                Assert.That(flow.TryLoad(ColorTimingSceneId.Boss1), Is.True);
                Assert.That(flow.BeginPendingTransition(), Is.True);

                Assert.That(transitions, Has.Count.EqualTo(2));
                Assert.That(transitions[0].SourceScene, Is.Null);
                Assert.That(transitions[0].TargetScene, Is.EqualTo(ColorTimingSceneId.StartMenu));
                Assert.That(transitions[0].IsInitialTransition, Is.True);
                Assert.That(transitions[1].SourceScene, Is.EqualTo(ColorTimingSceneId.StartMenu));
                Assert.That(transitions[1].TargetScene, Is.EqualTo(ColorTimingSceneId.Boss1));
                Assert.That(transitions[1].IsInitialTransition, Is.False);
            }
        }

        [TestCase(null, ColorTimingSceneId.StartMenu, false)]
        [TestCase(null, ColorTimingSceneId.Boss1, true)]
        [TestCase(ColorTimingSceneId.Boss1, ColorTimingSceneId.StartMenu, true)]
        [TestCase(ColorTimingSceneId.StartMenu, ColorTimingSceneId.Boss2, true)]
        public void LoadingPolicy_OnlySkipsInitialStartMenu(
            ColorTimingSceneId? source,
            ColorTimingSceneId target,
            bool expected)
        {
            var context = new SceneTransitionContext(source, target);
            Assert.That(GfColorTimingUiService.ShouldPresentLoading(context), Is.EqualTo(expected));
        }
    }
}
