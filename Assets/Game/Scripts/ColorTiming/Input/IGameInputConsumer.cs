namespace ColorTiming.Input
{
    public interface IGameInputConsumer
    {
        void BindGameInput(IGameInput input);
    }

    public interface IGameplayPointerConsumer
    {
        void BindGameplayPointer(Adapters.IGameplayPointerWorld pointerWorld);
    }

    public interface IGameplayCameraConsumer
    {
        void BindGameplayCamera(UnityEngine.Camera camera);
    }
}
