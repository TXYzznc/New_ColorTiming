// 文件职责：实现战斗技能 Skill_Bo2_atk2_s 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Bo2_atk2_s : Skill_base
{
    public GameObject b;

    // 执行ChildStart对应的主要流程。
    protected override void ChildStart()
    {
        string p = parm;
        p = p.Replace("(", "");
        p = p.Replace(")", "");
        //print("wezhi" + p);
        string[] v3s = p.Split(",");
        float x = float.Parse(v3s[0]);
        float y = float.Parse(v3s[1]);
        float z = float.Parse(v3s[2]);

        Vector3 targetPos = new Vector3(x, y, 0);

        //在目标位置 周围随机创建
        Vector3 stPos = targetPos - new Vector3(7, 5, 0);

        for (int _x = 0; _x < 5; _x++)
        {
            for (int _y = 0; _y < 4; _y++)
            {
                Vector3 _p = new Vector3(Random.Range(-2.0f, 2.0f) + _x * 3.5f, Random.Range(-2.0f, 2.0f) + _y * 3.5f, 0);
                _p += stPos;

                SpawnTransient(
                    b,
                    transform.position,
                    Quaternion.identity,
                    null,
                    instance => instance.GetComponent<Skill_Bo2_Atk2>()?.Set(_p));
            }
        }

        SpawnTransient(
            b,
            transform.position,
            Quaternion.identity,
            null,
            instance => instance.GetComponent<Skill_Bo2_Atk2>()?.Set(targetPos));

        ReleaseSelf();
    }
}
