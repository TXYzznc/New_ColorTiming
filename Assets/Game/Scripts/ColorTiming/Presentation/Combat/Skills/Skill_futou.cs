// 文件职责：实现战斗技能 Skill_futou 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_futou : Skill_base
{


    // 执行ChildStart对应的主要流程。
    protected override void ChildStart()
    {
        transform.localScale = new Vector3(Facing < 0 ? -1f : 1f, 1f, 1f);
    }

    // 执行ChildTrigger对应的主要流程。
    public void ChildTrigger(Collider2D collisio)
    {
        if (cTag != "" && cTag != collisio.gameObject.tag) return;
        OnHit(collisio, gameObject);
    }



}
