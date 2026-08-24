using ColorTiming.Bootstrap.Flow;
using ColorTiming.Settings;

namespace ColorTiming.Presentation.UI
{
    public interface IColorTimingUiService
    {
        bool IsPauseOpen { get; }
        bool TogglePause();
        void PresentScene(ColorTimingSceneId scene);
        bool ShowBattleResult(BattlePresentationResult result);
        void Reset();
    }

    public interface IColorTimingUiConsumer
    {
        void BindUiService(IColorTimingUiService uiService);
    }

    public interface IColorTimingPauseForm
    {
        void BindRuntime(IColorTimingSceneFlow sceneFlow, IColorTimingSettings settings);
    }

    public interface IColorTimingStartMenuForm
    {
        void BindRuntime(IColorTimingSceneFlow sceneFlow, IColorTimingSettings settings);
    }

    public interface IColorTimingBattleResultForm
    {
        void BindRuntime(
            IColorTimingSceneFlow sceneFlow,
            ColorTiming.Input.IGameInput gameInput,
            BattlePresentationResult result);
    }
}
