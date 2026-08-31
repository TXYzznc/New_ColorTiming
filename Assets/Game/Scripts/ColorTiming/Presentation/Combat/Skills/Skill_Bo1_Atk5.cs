// 文件职责：实现战斗技能 Skill_Bo1_Atk5 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Bo1_Atk5 : Skill_base
{
    // 执行ChildTrigger对应的主要流程。
    public void ChildTrigger(Collider2D collisio)
    {
        if (!MatchesTargetTag(collisio)) return;
        OnHit(collisio, gameObject);
    }
}
