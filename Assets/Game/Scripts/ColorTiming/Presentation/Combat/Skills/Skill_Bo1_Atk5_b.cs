// 文件职责：实现战斗技能 Skill_Bo1_Atk5_b 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ColorTiming.Configuration;

public class Skill_Bo1_Atk5_b : Skill_base
{
    public GameObject atk5_item;
    int burstCount = 16;
    float burstRadius = 1.8f;
    float burstStartAngle = -180f;

    protected override void OnSkillConfigurationApplied(ColorTimingSkillTable configuration)
    {
        burstCount = configuration.CountA;
        burstRadius = configuration.PatternA;
        burstStartAngle = configuration.PatternB;
    }

    // 执行ChildStart对应的主要流程。
    protected override void ChildStart()
    {
        //在周围一圈创建16个 技能，并设置其方向
        tr = false;
        index = -1;
        CreateItem();

        //Destroy(gameObject);
    }

    // 创建项目并完成必要的初始配置。
    void CreateItem()
    {
        List<Vector2> dirs = new List<Vector2>();
        //传入角度和半径
        //FunctionLibrary.GetPositionOnCircle();
        int _c = burstCount;
        for (int i = 0;i<_c;i++)
        {
            float ang = burstStartAngle + (i * (180 / _c));
            dirs.Add(FunctionLibrary.GetPositionOnCircle(ang, burstRadius));
        }
        foreach (Vector2 dir in dirs)
        {
            Vector3 pos = transform.position + new Vector3(dir.x,dir.y,0);
            SpawnTransient(
                atk5_item,
                pos,
                Quaternion.identity,
                transform.parent,
                instance => instance.GetComponent<Skill_Bo1_Atk5_Item>()?.SetAtk5(dir, 0, this));

        }
    }

    bool tr = false;
    // 执行ChildTrigger对应的主要流程。
    public void ChildTrigger(Collider2D collision)
    {
        if(tr || !gameObject) return;
        if (!MatchesTargetTag(collision)) return;
        OnHit(collision, gameObject);

        tr = true;
        //Destroy(gameObject);
    }

    int index = -1;
    // 执行ZZZ对应的主要流程。
    public void ZZZ(int _int)
    {
        if(_int > index)
        {
            //3-25 需求取消震动效果
            //CinemachineImpulseSource impulseSource = GetComponent<CinemachineImpulseSource>();
            //impulseSource?.GenerateImpulse();
            index = _int;
        }
    }

    // 执行Wait结束对应的主要流程。
    public void WaitEnd()
    {
        ReleaseSelf();
    }
}
