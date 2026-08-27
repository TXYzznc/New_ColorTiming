using ColorTiming.Bootstrap.Flow;
using ColorTiming.Settings;

namespace ColorTiming.Presentation.UI
{
    public interface IColorTimingUiService
    {
        bool IsPauseOpen { get; }
        bool TogglePause();
        void PresentScene(ColorTimingSceneId scene);
        bool ShowBattleHud(BattleHudPresentation presentation);
        bool ShowBattleTutorial(HeroController hero);
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
        void BindRuntime(
            IColorTimingSceneFlow sceneFlow,
            IColorTimingSettings settings,
            ColorTiming.Presentation.Audio.IColorTimingSoundService soundService);
    }

    public interface IColorTimingLoadingForm
    {
        void SetProgress(float progress);
        void CompleteAndClose();
    }

    public interface IColorTimingBattleResultForm
    {
        void BindRuntime(
            IColorTimingSceneFlow sceneFlow,
            ColorTiming.Input.IGameInput gameInput,
            BattlePresentationResult result);
    }

    public interface IColorTimingBattleHudForm
    {
        void BindRuntime(
            ColorTiming.Input.IGameInput gameInput,
            IColorTimingUiService uiService,
            BattleHudPresentation presentation);
    }

    public interface IColorTimingBattleTutorialForm
    {
        void BindRuntime(
            HeroController hero,
            ColorTiming.Input.IGameInput gameInput,
            ColorTiming.Combat.IGameTime gameTime,
            IColorTimingSettings settings);
    }
}
