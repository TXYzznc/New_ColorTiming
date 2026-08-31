// 文件职责：保留 Boss2 头部与尾部代码使用的语义化音效入口。
// 所属模块：ColorTiming / Presentation / Actors / Boss2。

using ColorTiming.Presentation.Audio;

public static class Boss2SoundCues
{
    public static readonly BossSoundCueId Hit = new BossSoundCueId("boss2.hit");
    public static readonly BossSoundCueId HeadEnterBurrow = new BossSoundCueId("boss2.head.enter-burrow");
    public static readonly BossSoundCueId HeadExitBurrow = new BossSoundCueId("boss2.head.exit-burrow");
    public static readonly BossSoundCueId TailEnterBurrow = new BossSoundCueId("boss2.tail.enter-burrow");
    public static readonly BossSoundCueId TailExitBurrow = new BossSoundCueId("boss2.tail.exit-burrow");
    public static readonly BossSoundCueId HeadAttack1 = new BossSoundCueId("boss2.head.attack-1");
    public static readonly BossSoundCueId HeadAttack2 = new BossSoundCueId("boss2.head.attack-2");
    public static readonly BossSoundCueId TailAttack1 = new BossSoundCueId("boss2.tail.attack-1");
    public static readonly BossSoundCueId TailAttack2 = new BossSoundCueId("boss2.tail.attack-2");
}
