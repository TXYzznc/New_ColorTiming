namespace ColorTiming.Presentation.Audio
{
    public interface IUiSoundSink
    {
        void PlayClick();
        void PlayHover();
    }

    public interface IUiSoundConsumer
    {
        void BindUiSound(IUiSoundSink sound);
    }
}
