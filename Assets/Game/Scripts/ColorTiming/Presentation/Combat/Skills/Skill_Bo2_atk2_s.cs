// 文件职责：实现战斗技能 Skill_Bo2_atk2_s 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ColorTiming.Configuration;

public class Skill_Bo2_atk2_s : Skill_base
{
    public GameObject b;
    float spacing = 3.5f;
    float jitter = 2f;
    float startOffsetX = 7f;
    float startOffsetY = 5f;
    int countX = 5;
    int countY = 4;

    protected override void OnSkillConfigurationApplied(ColorTimingSkillTable configuration)
    {
        spacing = configuration.PatternA;
        jitter = configuration.PatternB;
        startOffsetX = configuration.PatternC;
        startOffsetY = configuration.EndDelay;
        countX = configuration.CountA;
        countY = configuration.CountB;
    }

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
        Vector3 stPos = targetPos - new Vector3(startOffsetX, startOffsetY, 0);

        for (int _x = 0; _x < countX; _x++)
        {
            for (int _y = 0; _y < countY; _y++)
            {
                Vector3 _p = new Vector3(Random.Range(-jitter, jitter) + _x * spacing,
                    Random.Range(-jitter, jitter) + _y * spacing, 0);
                _p += stPos;

                SpawnTransient(
                    b,
                    transform.position,
                    Quaternion.identity,
                    null,
                    instance => ConfigureNestedSkill<Skill_Bo2_Atk2>(instance).Set(_p));
            }
        }

        SpawnTransient(
            b,
            transform.position,
            Quaternion.identity,
            null,
            instance => ConfigureNestedSkill<Skill_Bo2_Atk2>(instance).Set(targetPos));

        ReleaseSelf();
    }
}
