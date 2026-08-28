using ColorTiming.Combat;
using ColorTiming.Player;

public sealed class Boss2WeaponSpawnerView : WeaponSpawnerView
{
    protected override int TutorialTipId => 2;

    protected override WeaponSpawnPolicy CreatePolicy(int activeLimit)
    {
        return WeaponSpawnPolicy.Boss2(activeLimit);
    }

}
