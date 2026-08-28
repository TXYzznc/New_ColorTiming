// 文件职责：实现战斗技能 Skill_Bo1_Atk5_I 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Bo1_Atk5_I : MonoBehaviour
{
    Skill_Bo1_Atk5 Skill_Bo1_Atk5;
    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
        Skill_Bo1_Atk5 = GetComponentInParent<Skill_Bo1_Atk5>();
    }

    // 响应TriggerEnter2D回调，并更新本对象状态。
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Skill_Bo1_Atk5?.ChildTrigger(collision);
    }
}
