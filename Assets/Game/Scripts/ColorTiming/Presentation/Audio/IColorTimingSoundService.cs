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
        int Play(AudioClip clip, ColorTimingSoundChannel channel, Vector3 position, bool loop = false);
        void Stop(int serialId);
        void ResetTrackedSounds();
    }

    public interface IColorTimingSoundConsumer
    {
        void BindSoundService(IColorTimingSoundService soundService);
    }
}
