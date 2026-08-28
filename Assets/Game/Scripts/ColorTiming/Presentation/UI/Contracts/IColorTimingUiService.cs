// 文件职责：定义 ColorTimingUIService 的依赖契约，供模块间解耦使用。
// 所属模块：ColorTiming / Presentation / UI / Contracts。

using ColorTiming.Bootstrap.Flow;
using ColorTiming.Settings;
using ColorTiming.Presentation.Audio;
using ColorTiming.Presentation.UI.Models;
using ColorTiming.Presentation.UI.Presenters;

namespace ColorTiming.Presentation.UI.Contracts
{
    public interface IColorTimingUiService
    {
        bool IsPauseOpen { get; }
        // 执行Toggle暂停对应的主要流程。
        bool TogglePause();
        // 执行Present场景对应的主要流程。
        void PresentScene(ColorTimingSceneId scene);
        // 显示战斗Hud并同步当前数据。
        bool ShowBattleHud(BattleHudPresentation presentation);
        // 显示战斗Tutorial并同步当前数据。
        bool ShowBattleTutorial(ColorTiming.Application.Battle.BattleSession session);
        // 显示战斗结果并同步当前数据。
        bool ShowBattleResult(BattlePresentationResult result);
        // 恢复组件的默认配置或初始运行状态。
        void Reset();
    }

    public interface IColorTimingUiConsumer
    {
        // 绑定UIService依赖或事件监听。
        void BindUiService(IColorTimingUiService uiService);
    }

    public interface IColorTimingPauseForm
    {
        // 绑定运行时依赖或事件监听。
        void BindRuntime(IColorTimingSceneFlow sceneFlow, IColorTimingSettings settings, IUiSoundSink uiSound);
    }

    public interface IColorTimingStartMenuForm
    {
        // 绑定运行时依赖或事件监听。
        void BindRuntime(
            IColorTimingSceneFlow sceneFlow,
            IColorTimingSettings settings,
            ColorTiming.Presentation.Audio.IColorTimingSoundService soundService);
        IUiSoundSink UiSound { get; }
    }

    public interface IColorTimingLoadingForm
    {
        // 设置进度，并使后续流程使用最新状态。
        void SetProgress(float progress);
        // 执行完成AndClose对应的主要流程。
        void CompleteAndClose();
    }

    public interface IColorTimingBattleResultForm
    {
        // 绑定运行时依赖或事件监听。
        void BindRuntime(
            IColorTimingSceneFlow sceneFlow,
            ColorTiming.Input.IGameInput gameInput,
            BattlePresentationResult result);
    }

    public interface IColorTimingBattleHudForm
    {
        // 绑定运行时依赖或事件监听。
        void BindRuntime(
            ColorTiming.Input.IGameInput gameInput,
            IColorTimingUiService uiService,
            BattleHudPresentation presentation);
    }

    public interface IColorTimingBattleTutorialForm
    {
        // 绑定运行时依赖或事件监听。
        void BindRuntime(
            ColorTiming.Application.Battle.BattleSession session,
            ColorTiming.Input.IGameInput gameInput,
            ColorTiming.Combat.IGameTime gameTime,
            IColorTimingSettings settings);
    }
}
