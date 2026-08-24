using ColorTiming.Player;
using NUnit.Framework;

namespace ColorTiming.Tests.EditMode
{
    public sealed class PlayerActionStateMachineTests
    {
        [Test]
        public void Dash_CapturesFacingAndVerticalInput_AndBlocksMovement()
        {
            var state = new PlayerActionStateMachine();
            state.SetMove(-0.75f, 0.4f);

            Assert.That(state.BeginDash(), Is.True);
            Assert.That(state.FacingX, Is.EqualTo(-1));
            Assert.That(state.DashY, Is.EqualTo(0.4f));
            Assert.That(state.CanMove, Is.False);
            Assert.That(state.BeginAttack(), Is.False);

            state.SetDashInvulnerable(true);
            Assert.That(state.CanEvadeDamage, Is.True);

            state.EndDash();
            Assert.That(state.CanMove, Is.True);
            Assert.That(state.CanEvadeDamage, Is.False);
        }

        [Test]
        public void HitStun_HasOneSecondDamageRejection_IndependentOfAnimationExit()
        {
            var state = new PlayerActionStateMachine();
            state.BeginHit();
            state.EndHit();

            Assert.That(state.RejectsDamage, Is.True);
            state.Tick(0.99f);
            Assert.That(state.RejectsDamage, Is.True);
            state.Tick(0.02f);
            Assert.That(state.RejectsDamage, Is.False);
        }

        [Test]
        public void Death_ClearsTransientFlags_AndPermanentlyBlocksActions()
        {
            var state = new PlayerActionStateMachine();
            state.SetAnimationInvulnerable(true);
            state.SetSkillMoving(true);

            state.Kill();

            Assert.That(state.State, Is.EqualTo(PlayerActionState.Dead));
            Assert.That(state.HasAnimationInvulnerability, Is.False);
            Assert.That(state.IsSkillMoving, Is.False);
            Assert.That(state.BeginDash(), Is.False);
            Assert.That(state.BeginAttack(), Is.False);
        }

        [Test]
        public void AttackGate_PreservesResumeGuardAndHeldAnimatorContract()
        {
            var gate = new PlayerAttackInputGate();
            gate.Tick(0.2f);
            Assert.That(gate.ShouldTrigger(true, false), Is.False);
            Assert.That(gate.HeldAnimatorValue(true), Is.EqualTo(0f));

            gate.Tick(0.001f);
            Assert.That(gate.ShouldTrigger(true, false), Is.True);
            Assert.That(gate.ShouldTrigger(true, true), Is.False);
            Assert.That(gate.HeldAnimatorValue(true), Is.EqualTo(1f));
            Assert.That(PlayerAttackInputGate.HeldAnimatorThreshold, Is.EqualTo(0.1f));

            gate.Reset();
            Assert.That(gate.IsReady, Is.False);
        }
    }
}
