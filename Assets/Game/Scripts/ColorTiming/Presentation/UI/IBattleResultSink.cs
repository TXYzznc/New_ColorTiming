namespace ColorTiming.Presentation.UI
{
    public enum BattlePresentationResult
    {
        Boss1Defeated = 0,
        FinalVictory = 1,
        PlayerDefeated = 2,
    }

    public interface IBattleResultSink
    {
        void Show(BattlePresentationResult result);
    }

    public interface IBattleResultConsumer
    {
        void BindBattleResultSink(IBattleResultSink sink);
    }
}
