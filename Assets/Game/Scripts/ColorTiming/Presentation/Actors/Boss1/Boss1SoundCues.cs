// 文件职责：保留 Boss1 动画与行为代码使用的语义化音效入口。
// 所属模块：ColorTiming / Presentation / Actors / Boss1。

using ColorTiming.Presentation.Audio;

public static class Boss1SoundCues
{
    public static readonly BossSoundCueId Hit = new BossSoundCueId("boss1.hit");
    public static readonly BossSoundCueId AttackReady = new BossSoundCueId("boss1.attack-ready");
    public static readonly BossSoundCueId AttackEnd = new BossSoundCueId("boss1.attack-end");
    public static readonly BossSoundCueId Attack1 = new BossSoundCueId("boss1.attack-1");
    public static readonly BossSoundCueId Attack2 = new BossSoundCueId("boss1.attack-2");
    public static readonly BossSoundCueId Attack3 = new BossSoundCueId("boss1.attack-3");
    public static readonly BossSoundCueId Attack4 = new BossSoundCueId("boss1.attack-4");
    public static readonly BossSoundCueId Attack5 = new BossSoundCueId("boss1.attack-5");
    public static readonly BossSoundCueId Attack6 = new BossSoundCueId("boss1.attack-6");
}
