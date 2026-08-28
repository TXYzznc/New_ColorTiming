using ColorTiming.Combat;
using ColorTiming.Player;

public sealed class Boss1WeaponSpawnerView : WeaponSpawnerView
{
    protected override int TutorialTipId => 1;

    protected override WeaponSpawnPolicy CreatePolicy(int activeLimit)
    {
        return WeaponSpawnPolicy.Boss1(activeLimit);
    }

}
