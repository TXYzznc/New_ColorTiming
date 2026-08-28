// 文件职责：定义 战斗伤害Receiver 的依赖契约，供模块间解耦使用。
// 所属模块：ColorTiming / Presentation / Combat。

using ColorTiming.Combat;

namespace ColorTiming.Presentation.Combat
{
    /// <summary>Unity collision boundary that forwards typed damage into the active session.</summary>
    public interface IBattleDamageReceiver
    {
        ActorId DamageActorId { get; }
        // 执行Receive伤害对应的主要流程。
        void ReceiveDamage(BattleDamage damage);
    }
}
