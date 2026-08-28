using System;
using System.Collections;
using System.Collections.Generic;
using ColorTiming.Presentation.Entities;
using UnityEngine;


public class Skill_Bo1_Atk5_Item : MonoBehaviour, ITransientEntityConsumer, IFrameworkEntityParticipant
{
    public GameObject item;

    Vector2 dir = Vector2.zero;
    int index = 0;
    Skill_Bo1_Atk5_b con;
    ITransientEntityService transientEntities;
    Action frameworkRelease;
    bool releasing;
    //设置角度
    public void SetAtk5(Vector2 _dir,int _index,Skill_Bo1_Atk5_b _con)
    {
        dir = _dir;
        index = _index + 1;
        con = _con;
        con?.ZZZ(index);
    }


    public void Cerate()
    {
        if (index > 6)
        {
            //此时父项不再接受伤害，并准备删除
            con?.WaitEnd();
            return;
        }

        Vector3 pos = transform.position + new Vector3(dir.x, dir.y, 0);
        if (transientEntities == null)
        {
            throw new InvalidOperationException("Attack5 item entities were not bound before spawning the next ring.");
        }
        transientEntities.Spawn(
            item.name,
            pos,
            Quaternion.identity,
            transform.parent,
            instance => instance.GetComponent<Skill_Bo1_Atk5_Item>()?.SetAtk5(dir, index, con));
        //为啥只创建了3波？
    }

    public void End()
    {
        if (releasing)
        {
            return;
        }
        releasing = true;
        if (frameworkRelease != null)
        {
            frameworkRelease.Invoke();
        }
        else
        {
            Destroy(gameObject);
        }
        //print("weisha shanchu le ?");
    }

    public void BindTransientEntities(ITransientEntityService entities)
    {
        transientEntities = entities ?? throw new ArgumentNullException(nameof(entities));
    }

    public void BindFrameworkRelease(Action release)
    {
        frameworkRelease = release;
    }

    public void OnFrameworkEntitySpawned()
    {
        releasing = false;
    }

    public void OnFrameworkEntityDespawned()
    {
        con = null;
        releasing = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        con?.ChildTrigger(collision);
    }



}
