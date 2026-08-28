using ColorTiming.Combat;

namespace ColorTiming.Presentation.Combat
{
    /// <summary>Unity collision boundary that forwards typed damage into the active session.</summary>
    public interface IBattleDamageReceiver
    {
        ActorId DamageActorId { get; }
        void ReceiveDamage(BattleDamage damage);
    }
}
