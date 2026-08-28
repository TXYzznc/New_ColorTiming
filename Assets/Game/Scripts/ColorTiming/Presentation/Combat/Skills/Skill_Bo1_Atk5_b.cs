using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Bo1_Atk5_b : Skill_base
{
    public GameObject atk5_item;

    protected override void ChildStart()
    {
        //在周围一圈创建16个 技能，并设置其方向
        tr = false;
        index = -1;
        CreateItem();

        //Destroy(gameObject);
    }

    void CreateItem()
    {
        List<Vector2> dirs = new List<Vector2>();
        //传入角度和半径
        //FunctionLibrary.GetPositionOnCircle();
        int _c = 16;
        for (int i = 0;i<_c;i++)
        {
            float ang = -180 + (i * (180 / _c));
            dirs.Add(FunctionLibrary.GetPositionOnCircle(ang,1.8f));
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
    public void ChildTrigger(Collider2D collision)
    {
        if(tr || !gameObject) return;
        if (cTag != "" && cTag != collision.gameObject.tag) return;
        OnHit(collision, gameObject);

        tr = true;
        //Destroy(gameObject);
    }

    int index = -1;
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

    public void WaitEnd()
    {
        ReleaseSelf();
    }
}
