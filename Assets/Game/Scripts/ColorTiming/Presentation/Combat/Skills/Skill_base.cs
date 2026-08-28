
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

    private void Awake()
    {
        configuredLife = life;
    }

    public void SetSkillData(ActorId sourceActor, WeaponIdentity weapon, int facing, string parameter)
    {
        attackerId = sourceActor;
        filp = facing;
        parm = parameter;
        atkWeapon = weapon;
        hasDamagePayload = true;
    }
    void Start()
    {
        if (!initializedForSpawn)
        {
            InitializeForSpawn();
        }
    }
    protected virtual void ChildStart()
    {

    }

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

    protected virtual void ChildUpdate()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(cTag != "" && cTag != collision.gameObject.tag) return;

        //print(gameObject.name + "检测到进入:" + collision.gameObject.name);

        OnHit(collision,gameObject);
    }

    public void EventEnd_Destroy()
    {
        ReleaseSelf();
    }

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
        var receiver = collision.GetComponent<IBattleDamageReceiver>();
        //print("xxxxx" + i_);
        //collision.contacts[0].point;

        Vector2 dir = (gameObject.transform.position - collision.transform.position).normalized  ;
        var hitinfo = Physics2D.Raycast(gameObject.transform.position, dir);
        //hitinfo.
        Vector2 v2 = Vector2.zero;
        if (hitinfo) v2 = hitinfo.point;
        //用skill位置发射射线？
        if (receiver != null && hasDamagePayload)
        {
            receiver.ReceiveDamage(new BattleDamage(
                attackerId,
                receiver.DamageActorId,
                atkWeapon,
                new CombatPoint(v2.x, v2.y),
                damageParm));
        }

        //collision.
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
        InitializeForSpawn();
    }

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

    private void InitializeForSpawn()
    {
        life = configuredLife;
        cTime = life > 0f;
        initializedForSpawn = true;
        releasing = false;
        ChildStart();
    }

}
