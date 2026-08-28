// 文件职责：定义 UI音效Sink 的依赖契约，供模块间解耦使用。
// 所属模块：ColorTiming / Presentation / Audio。

namespace ColorTiming.Presentation.Audio
{
    public interface IUiSoundSink
    {
        // 播放Click对应的动画、音频或表现。
        void PlayClick();
        // 播放Hover对应的动画、音频或表现。
        void PlayHover();
    }

    public interface IUiSoundConsumer
    {
        // 绑定UI音效依赖或事件监听。
        void BindUiSound(IUiSoundSink sound);
    }
}
