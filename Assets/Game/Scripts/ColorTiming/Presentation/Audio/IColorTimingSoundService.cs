// 文件职责：定义 ColorTiming音效Service 的依赖契约，供模块间解耦使用。
// 所属模块：ColorTiming / Presentation / Audio。

using UnityEngine;

namespace ColorTiming.Presentation.Audio
{
    public enum ColorTimingSoundChannel
    {
        BGM = 0,
        UI = 1,
        Player = 2,
        Boss = 3,
        Environment = 4,
    }

    public interface IColorTimingSoundService
    {
        // 启动当前配置的动画、音频或其他表现。
        int Play(AudioClip clip, ColorTimingSoundChannel channel, Vector3 position, bool loop = false);
        /// <summary>按 DataTable 中的语义 Cue 播放，不要求业务对象持有 AudioClip。</summary>
        int PlayCue(string cueId, Vector3 position);
        // 执行Stop对应的主要流程。
        void Stop(int serialId);
        // 执行ResetTrackedSounds对应的主要流程。
        void ResetTrackedSounds();
    }

    public interface IColorTimingSoundConsumer
    {
        // 绑定音效Service依赖或事件监听。
        void BindSoundService(IColorTimingSoundService soundService);
    }
}
