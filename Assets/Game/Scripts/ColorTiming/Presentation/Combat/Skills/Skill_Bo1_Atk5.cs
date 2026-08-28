// 文件职责：实现战斗技能 Skill_Bo1_Atk5 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Bo1_Atk5 : Skill_base
{
    // Start is called before the first frame update
    // 在首帧启动依赖就绪后的业务或表现流程。
    void Start()
    {

    }

    // Update is called once per frame
    // 逐帧推进需要实时刷新的业务或表现状态。
    void Update()
    {

    }

    // 执行ChildTrigger对应的主要流程。
    public void ChildTrigger(Collider2D collisio)
    {
        if (cTag != "" && cTag != collisio.gameObject.tag) return;
        OnHit(collisio, gameObject);
    }
}
