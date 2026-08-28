// 文件职责：实现战斗技能 skill_base_randomHit 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

//随机特效
public class skill_base_randomHit : Skill_base
{
    public List<GameObject> hitFXs = new List<GameObject>();


    // 执行ChildStart对应的主要流程。
    protected override void ChildStart()
    {
        if (hitFXs.Count > 0)
        {
            int _r = Random.Range(0, hitFXs.Count);
            HitFX = hitFXs[_r];
            //hitFXs = hitFXs
        }
    }
}
