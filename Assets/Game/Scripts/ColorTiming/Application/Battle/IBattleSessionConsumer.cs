// 文件职责：定义 战斗会话消费者 的依赖契约，供模块间解耦使用。
// 所属模块：ColorTiming / Application / Battle。

namespace ColorTiming.Application.Battle
{
    /// <summary>Implemented by presentation adapters that require the active scene session.</summary>
    public interface IBattleSessionConsumer
    {
        // 绑定战斗会话依赖或事件监听。
        void BindBattleSession(BattleSession session);
    }
}
