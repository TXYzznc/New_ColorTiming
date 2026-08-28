namespace ColorTiming.Application.Battle
{
    /// <summary>Implemented by presentation adapters that require the active scene session.</summary>
    public interface IBattleSessionConsumer
    {
        void BindBattleSession(BattleSession session);
    }
}
