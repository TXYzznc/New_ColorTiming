using ColorTiming.Input;
using NUnit.Framework;
using UnityEngine;

namespace ColorTiming.Tests.EditMode
{
    public sealed class GameInputStateTests
    {
        [Test]
        public void FramePreservesPressAndHeldSemantics()
        {
            var input = new FakeGameInput();
            input.SetFrame(new GameInputFrame(
                new Vector2(1f, -1f), true, true, true, true, false,
                new Vector2(100f, 200f), true, true));

            Assert.That(input.Move, Is.EqualTo(new Vector2(1f, -1f)));
            Assert.That(input.DashPressed, Is.True);
            Assert.That(input.AttackPressed, Is.True);
            Assert.That(input.AttackHeld, Is.True);
            Assert.That(input.DropPressed, Is.True);

            input.SetFrame(new GameInputFrame(
                Vector2.zero, false, false, true, false, false,
                Vector2.zero, false, false));

            Assert.That(input.DashPressed, Is.False);
            Assert.That(input.AttackPressed, Is.False);
            Assert.That(input.AttackHeld, Is.True);

            input.SetFrame(GameInputFrame.Empty);
            Assert.That(input.AttackHeld, Is.False);
        }

        [Test]
        public void TutorialConsumptionSuppressesSameFrameGameplayOnlyOnce()
        {
            var input = new FakeGameInput();
            input.SetFrame(new GameInputFrame(
                Vector2.one, true, true, true, true, false,
                Vector2.zero, true, true));

            Assert.That(input.ConsumeAnyPressForOverlay(), Is.True);
            Assert.That(input.ConsumeAnyPressForOverlay(), Is.False);
            Assert.That(input.AnyPressed, Is.False);
            Assert.That(input.Move, Is.EqualTo(Vector2.zero));
            Assert.That(input.DashPressed, Is.False);
            Assert.That(input.AttackPressed, Is.False);
            Assert.That(input.AttackHeld, Is.False);
            Assert.That(input.DropPressed, Is.False);
            Assert.That(input.ConfirmPressed, Is.False);
        }

        [Test]
        public void PauseRemainsAvailableWhileGameplayIsSuppressed()
        {
            var input = new FakeGameInput();
            input.SetFrame(new GameInputFrame(
                Vector2.zero, false, false, false, false, true,
                Vector2.zero, true, false));

            input.ConsumeAnyPressForOverlay();

            Assert.That(input.PausePressed, Is.True);
        }
    }
}
