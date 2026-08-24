using ColorTiming.Combat;
using ColorTiming.Player;

public sealed class WeaponControSystem : WeaponSpawnerView
{
    public Boss1_Controller boss;

    protected override bool HasActiveBoss => boss != null;
    protected override int TutorialTipId => 1;

    protected override bool TryGetCurrentWeakness(out WeaponColor weakness)
    {
        if (boss != null && boss.Boss1HP.Count > 0)
        {
            weakness = (WeaponColor)boss.Boss1HP[0];
            return true;
        }

        weakness = default;
        return false;
    }

    protected override WeaponSpawnPolicy CreatePolicy(int activeLimit)
    {
        return WeaponSpawnPolicy.Boss1(activeLimit);
    }

    protected override void SubscribeBossDamage()
    {
        boss?.OnDamage_Event.AddListener(OnBossDamaged);
    }

    protected override void UnsubscribeBossDamage()
    {
        boss?.OnDamage_Event.RemoveListener(OnBossDamaged);
    }
}
