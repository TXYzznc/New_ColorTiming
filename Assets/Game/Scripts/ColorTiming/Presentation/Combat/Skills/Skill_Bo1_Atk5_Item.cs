// 文件职责：实现战斗技能 Skill_Bo1_Atk5_Item 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using System;
using ColorTiming.Presentation.Entities;
using UnityEngine;


public class Skill_Bo1_Atk5_Item : MonoBehaviour, IFrameworkEntityParticipant
{
    public GameObject item;

    Vector2 dir = Vector2.zero;
    int index = 0;
    Skill_Bo1_Atk5_b con;
    Action frameworkRelease;
    bool releasing;
    Animator effectAnimator;

    private void Awake()
    {
        effectAnimator = GetComponent<Animator>();
    }
    //设置角度
    public void SetAtk5(Vector2 _dir,int _index,Skill_Bo1_Atk5_b _con)
    {
        dir = _dir;
        index = _index + 1;
        con = _con;
        con?.ZZZ(index);
    }


    // 执行Cerate对应的主要流程。
    public void Cerate()
    {
        if (index > 6)
        {
            //此时父项不再接受伤害，并准备删除
            con?.WaitEnd();
            return;
        }

        // This authored chain depends on the next wave appearing synchronously on the
        // Cerate animation event. GF.Entity.ShowEntity is asynchronous and can miss that
        // visual window, so only the root/first wave uses GF and subsequent waves retain
        // the source project's synchronous Instantiate/Destroy contract.
        Vector2 nextDirection = dir;
        int nextIndex = index;
        Skill_Bo1_Atk5_b nextController = con;
        Transform nextParent = transform.parent;
        Vector3 nextPosition = transform.position + new Vector3(nextDirection.x, nextDirection.y, 0);
        GameObject nextItem = Instantiate(item, nextPosition, Quaternion.identity, nextParent);
        nextItem.GetComponent<Skill_Bo1_Atk5_Item>()
            ?.SetAtk5(nextDirection, nextIndex, nextController);
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
    }

    // 绑定FrameworkRelease依赖或事件监听。
    public void BindFrameworkRelease(Action release)
    {
        frameworkRelease = release;
    }

    // 响应Framework实体Spawned回调，并更新本对象状态。
    public void OnFrameworkEntitySpawned()
    {
        releasing = false;

        // The source project instantiated a fresh Animator for every spike. GF.Entity reuses
        // the same object, so explicitly restore the default state's first frame on every show.
        if (effectAnimator != null && effectAnimator.runtimeAnimatorController != null)
        {
            effectAnimator.Play(0, 0, 0f);
            effectAnimator.Update(0f);
        }
    }

    // 响应Framework实体Despawned回调，并更新本对象状态。
    public void OnFrameworkEntityDespawned()
    {
        dir = Vector2.zero;
        index = 0;
        con = null;
        releasing = false;
    }

    // 响应TriggerEnter2D回调，并更新本对象状态。
    private void OnTriggerEnter2D(Collider2D collision)
    {
        con?.ChildTrigger(collision);
    }



}
