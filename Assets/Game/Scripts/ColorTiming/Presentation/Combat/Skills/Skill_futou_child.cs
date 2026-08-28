// 文件职责：实现战斗技能 Skill_futou_child 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_futou_child : MonoBehaviour
{
    Skill_futou skill_futou;
    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
        skill_futou = GetComponentInParent<Skill_futou>();
    }

    // 响应TriggerEnter2D回调，并更新本对象状态。
    private void OnTriggerEnter2D(Collider2D collision)
    {
        skill_futou?.ChildTrigger(collision);
    }
}
