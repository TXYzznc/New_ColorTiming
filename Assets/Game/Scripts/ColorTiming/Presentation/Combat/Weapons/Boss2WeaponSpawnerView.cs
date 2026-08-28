// 文件职责：负责 Boss2武器Spawner 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / Combat / Weapons。

using ColorTiming.Combat;
using ColorTiming.Player;

public sealed class Boss2WeaponSpawnerView : WeaponSpawnerView
{
    protected override int TutorialTipId => 2;

    // 创建Policy并完成必要的初始配置。
    protected override WeaponSpawnPolicy CreatePolicy(int activeLimit)
    {
        return WeaponSpawnPolicy.Boss2(activeLimit);
    }

}
