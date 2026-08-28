// 文件职责：定义 Boss2动画事件Relay，承担 Boss2 模块中的对应职责。
// 所属模块：ColorTiming / Presentation / Actors / Boss2。

using Spine;
using System;
using ColorTiming.Combat;
using ColorTiming.Presentation.Actors;
using ColorTiming.Presentation.Entities;
using UnityEngine;


public class Boss2AnimationEventRelay : MonoBehaviour, ITransientEntityConsumer, IPlayerTargetConsumer
{
    public GameObject sk0;
    public GameObject sk1;
    public GameObject sk2;

    public Transform mao0;
    public Transform mao1;
    public Transform mao2;

    public MeshRenderer meshRenderer1;


    Boss2SoundView soundManager1;
    Transform playerTarget;
    ITransientEntityService transientEntities;

    // 绑定TransientEntities依赖或事件监听。
    public void BindTransientEntities(ITransientEntityService entities)
    {
        transientEntities = entities ?? throw new ArgumentNullException(nameof(entities));
    }

    // 绑定玩家目标依赖或事件监听。
    public void BindPlayerTarget(Transform target)
    {
        playerTarget = target != null ? target : throw new ArgumentNullException(nameof(target));
    }

    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
        soundManager1 = GetComponentInParent<Boss2SoundView>();
    }
    bool flip;
    float _it;
    float lerpSpeed = 10;
    float _showTime = -1;
    // 逐帧推进需要实时刷新的业务或表现状态。
    private void Update()
    {

        if (_showTime > 0)
        {
            _showTime -= Time.deltaTime;
            //print(_showTime);

            ShowHit();

        }
        else
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            //mpb.SetColor("_Black", Color.black);
            mpb.SetFloat("_FillPhase", 0);

            meshRenderer1.SetPropertyBlock(mpb);
            //skeletonAnimation1?.skeleton?.SetColor(Color.white);
            //skeletonAnimation2?.skeleton?.SetColor(Color.white);
            //print("sssssssssssss");
        }


    }
    // 执行GoAtk对应的主要流程。
    public void GoAtk(TrackEntry trackEntry, Spine.Event e)
    {
        GameObject wsk = null;
        Transform wmao = null;
        string parm = "";
        if (e.ToString() == "attack")
        {
            switch (e.String)
            {
                case "atk1":
                    //soundManager1?.PlayBoss2Sound("atk1_t");
                    wsk = sk1;
                    wmao = mao1;
                    break;

                case "atk2":
                    //soundManager1?.PlayBoss2Sound("atk2_t");
                    wsk = sk2;
                    wmao = mao2;
                    parm = playerTarget != null
                        ? playerTarget.position.ToString()
                        : string.Empty;
                    break;
                case "atk0":
                    soundManager1?.Play(Boss2SoundCue.HeadExitBurrow);
                    wsk = sk0;
                    wmao = mao0;
                    break;

                default:
                    break;
            }
            //print(e.String + "看看");
        }


        //print("看看：" + e.String);

        if (wsk && wmao)
        {
            if (transientEntities == null)
            {
                throw new InvalidOperationException("Boss2 attack entities were not bound by the composition root.");
            }
            int _flip = transform.localScale.x > 0 ? 1 : -1;
            transientEntities.Spawn(
                wsk.name,
                wmao.position,
                wmao.rotation,
                wmao,
                instance =>
                {
                    instance.transform.localPosition = wsk.transform.localPosition;
                    instance.transform.localRotation = wsk.transform.localRotation;
                    instance.transform.localScale = wsk.transform.localScale;
                    instance.GetComponent<Skill_base>()?.SetSkillData(
                        ActorId.BossHead,
                        new WeaponIdentity(WeaponColor.Red, ColorTiming.Combat.WeaponType.Normal),
                        _flip,
                        parm);
                });

        }
        else
        {
            print("未正确创建技能:" + e.String);
        }

    }

    // 执行Rutu对应的主要流程。
    public void Rutu()
    {
        soundManager1?.Play(Boss2SoundCue.HeadEnterBurrow);
    }

    // 响应Hit回调，并更新本对象状态。
    public void OnHit()
    {
        soundManager1?.Play(Boss2SoundCue.Hit);
        _showTime = 0.2f;
    }


    // 显示Hit并同步当前数据。
    void ShowHit()
    {
        float _sp = flip ? -1 : 1;
        _it += (Time.deltaTime * lerpSpeed * _sp);

        Color _c = new Color(_it, _it, _it, _it);

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        //mpb.SetColor("_Black", _c);
        mpb.SetFloat("_FillPhase", _it);
        meshRenderer1.SetPropertyBlock(mpb);
        // meshRenderer1.material.SetColor("Dark Color", _c);


        //print(_c);

        //print(_it);
        //skeletonAnimation1?.skeleton?.SetColor(_c);
        //skeletonAnimation2?.skeleton?.SetColor(_c);


        if (flip)
        {
            //检查
            if (_it < 0) flip = false;
        }
        else
        {
            if (_it > 1) flip = true;
        }
    }


    // 播放音效对应的动画、音频或表现。
    public void PlaySound(string atk)
    {
        switch (atk)
        {
            case "atk1":
                soundManager1?.Play(Boss2SoundCue.HeadAttack1);
                break;
            case "atk2":
                soundManager1?.Play(Boss2SoundCue.HeadAttack2);
                break;
            //case "atk0":
            //    soundManager1?.PlayBoss2Sound("ct_t");
            //    break;

            default:
                break;
        }
    }

}
