// 文件职责：实现战斗技能 Skill_base 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using System;
using ColorTiming.Combat;
using ColorTiming.Presentation.Combat;
using ColorTiming.Presentation.Entities;
using UnityEngine;

public class Skill_base : MonoBehaviour, ITransientEntityConsumer, IFrameworkEntityParticipant
{
    public float life = 1;
    public GameObject HitFX;

    public string cTag;
    public string damageParm = "";
    ActorId attackerId;
    int filp = 1;
    protected int Facing => filp;
    protected string parm;
    WeaponIdentity atkWeapon;
    bool hasDamagePayload;
    bool cTime;
    bool initializedForSpawn;
    bool releasing;
    float configuredLife;
    Action frameworkRelease;
    ITransientEntityService transientEntities;

    // 缓存本组件依赖，并完成不依赖外部服务的本地初始化。
    private void Awake()
    {
        configuredLife = life;
    }

    // 设置技能数据，并使后续流程使用最新状态。
    public void SetSkillData(ActorId sourceActor, WeaponIdentity weapon, int facing, string parameter)
    {
        attackerId = sourceActor;
        filp = facing;
        parm = parameter;
        atkWeapon = weapon;
        hasDamagePayload = true;
    }
    // 在首帧启动依赖就绪后的业务或表现流程。
    void Start()
    {
        if (!initializedForSpawn)
        {
            InitializeForSpawn();
        }
    }
    // 执行ChildStart对应的主要流程。
    protected virtual void ChildStart()
    {

    }

    // 逐帧推进需要实时刷新的业务或表现状态。
    private void Update()
    {
        if (cTime)
        {
            if (life > 0)
            {
                //transform.SetPositionAndRotation(transform.position + new Vector3(0.0001f, 0, 0), transform.rotation);
                life -= Time.deltaTime;
            }
            else
            {
                ReleaseSelf();
            }
        }
        ChildUpdate();
    }

    // 执行ChildUpdate对应的主要流程。
    protected virtual void ChildUpdate()
    {

    }

    // 响应TriggerEnter2D回调，并更新本对象状态。
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!MatchesTargetTag(collision)) return;

        //print(gameObject.name + "检测到进入:" + collision.gameObject.name);

        OnHit(collision,gameObject);
    }

    // 执行事件结束Destroy对应的主要流程。
    public void EventEnd_Destroy()
    {
        ReleaseSelf();
    }

    // 响应Hit回调，并更新本对象状态。
    protected virtual void OnHit(Collider2D collision,GameObject ht)
    {
        //一个技能只传递一次伤害


        if (HitFX)
        {
            filp = filp > 0 ? 1 : -1;
            if (transientEntities != null)
            {
                transientEntities.Spawn(
                    HitFX.name,
                    transform.position,
                    Quaternion.identity,
                    null,
                    fx => fx.transform.localScale = new Vector3(filp, 1, 1));
            }
            else
            {
                GameObject fx = Instantiate(HitFX, transform.position, Quaternion.identity);
                fx.transform.localScale = new Vector3(filp, 1, 1);
            }
        }
        var receiver = collision.GetComponent<IBattleDamageReceiver>()
                       ?? collision.GetComponentInParent<IBattleDamageReceiver>();
        //print("xxxxx" + i_);
        //collision.contacts[0].point;

        Vector2 dir = (gameObject.transform.position - collision.transform.position).normalized  ;
        var hitinfo = Physics2D.Raycast(gameObject.transform.position, dir);
        //hitinfo.
        Vector2 v2 = Vector2.zero;
        if (hitinfo) v2 = hitinfo.point;
        //用skill位置发射射线？
        if (receiver == null)
        {
            Debug.LogWarning(
                $"[ColorTiming.Combat][SkillHit] action=ResolveReceiver result=missing-receiver skill={name} target={collision.name}",
                collision);
            return;
        }

        if (!hasDamagePayload)
        {
            Debug.LogError(
                $"[ColorTiming.Combat][SkillHit] action=Deliver result=missing-payload skill={name} target={collision.name} receiver={receiver.DamageActorId}",
                this);
            return;
        }

        var damage = new BattleDamage(
            attackerId,
            receiver.DamageActorId,
            atkWeapon,
            new CombatPoint(v2.x, v2.y),
            damageParm);
        receiver.ReceiveDamage(damage);
        Debug.Log(
            $"[ColorTiming.Combat][SkillHit] action=Deliver result=sent skill={name} attacker={damage.Attacker} target={damage.Target} weapon={damage.Weapon.Color}",
            this);

        //collision.
    }

    private bool MatchesTargetTag(Collider2D collision)
    {
        if (string.IsNullOrEmpty(cTag)) return true;

        for (var target = collision.transform; target != null; target = target.parent)
        {
            if (target.CompareTag(cTag)) return true;
        }

        return false;
    }


    // 绑定TransientEntities依赖或事件监听。
    public void BindTransientEntities(ITransientEntityService entities)
    {
        transientEntities = entities ?? throw new ArgumentNullException(nameof(entities));
    }

    // 绑定FrameworkRelease依赖或事件监听。
    public void BindFrameworkRelease(Action release)
    {
        frameworkRelease = release;
    }

    // 响应Framework实体Spawned回调，并更新本对象状态。
    public void OnFrameworkEntitySpawned()
    {
        InitializeForSpawn();
    }

    // 响应Framework实体Despawned回调，并更新本对象状态。
    public void OnFrameworkEntityDespawned()
    {
        cTime = false;
        initializedForSpawn = false;
        releasing = false;
        attackerId = default;
        atkWeapon = default;
        hasDamagePayload = false;
        parm = null;
    }

    // 释放Self及其临时资源。
    protected void ReleaseSelf()
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

    // 生成Transient并交给对应生命周期系统管理。
    protected int SpawnTransient(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent,
        Action<GameObject> configure)
    {
        if (prefab == null)
        {
            throw new ArgumentNullException(nameof(prefab));
        }
        if (transientEntities == null)
        {
            throw new InvalidOperationException("Transient entities were not bound before a nested spawn.");
        }

        return transientEntities.Spawn(prefab.name, position, rotation, parent, configure);
    }

    // 初始化For生成及其依赖关系。
    private void InitializeForSpawn()
    {
        life = configuredLife;
        cTime = life > 0f;
        initializedForSpawn = true;
        releasing = false;
        ChildStart();
    }

}
