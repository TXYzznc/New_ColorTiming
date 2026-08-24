using Spine;
using System;
using ColorTiming.Presentation.Entities;
using UnityEngine;


public class Boss2Anim_s : MonoBehaviour, ITransientEntityConsumer
{
    public GameObject sk0;
    public GameObject sk1;
    public GameObject sk2;

    public Transform mao0;
    public Transform mao1;
    public Transform mao2;

    public MeshRenderer meshRenderer1;


    Boss2SoundManager soundManager1;
    Boss2_Controller controller;
    ITransientEntityService transientEntities;

    public void BindTransientEntities(ITransientEntityService entities)
    {
        transientEntities = entities ?? throw new ArgumentNullException(nameof(entities));
    }

    private void Start()
    {
        soundManager1 = GetComponentInParent<Boss2SoundManager>();
        controller = GetComponentInParent<Boss2_Controller>();
    }
    bool flip;
    float _it;
    float lerpSpeed = 10;
    float _showTime = -1;
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
                    parm = controller != null && controller.hero != null
                        ? controller.hero.transform.position.ToString()
                        : string.Empty;
                    break;
                case "atk0":
                    soundManager1?.PlayBoss2Sound("ct_t");
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
                        gameObject,
                        new Weapon(ColorType.hong, WeaponType.nor),
                        _flip,
                        parm);
                });

        }
        else
        {
            print("未正确创建技能:" + e.String);
        }

    }

    public void Rutu()
    {
        soundManager1?.PlayBoss2Sound("rt_t");
    }

    public void OnHit()
    {
        soundManager1?.PlayBoss2Sound("hit");
        _showTime = 0.2f;
    }


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


    public void PlaySound(string atk)
    {
        switch (atk)
        {
            case "atk1":
                soundManager1?.PlayBoss2Sound("atk1_t");
                break;
            case "atk2":
                soundManager1?.PlayBoss2Sound("atk2_t");
                break;
            //case "atk0":
            //    soundManager1?.PlayBoss2Sound("ct_t");
            //    break;

            default:
                break;
        }
    }

}
