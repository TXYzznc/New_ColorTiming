// 文件职责：定义 玩家动画事件Relay，承担 玩家 模块中的对应职责。
// 所属模块：ColorTiming / Presentation / Actors / Player。

using Cinemachine;
using ColorTiming.Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Configuration;
using UnityEngine.Events;

public class PlayerAnimationEventRelay : MonoBehaviour, IColorTimingConfigurationConsumer
{
    public UnityEvent OnDashEnd;

    public UnityEvent<bool> OnWudi;
    public UnityEvent<bool> OnDashWD;
    public UnityEvent<string> OnAttack;
    public UnityEvent<int> OnHit;
    public UnityEvent<bool> OnSkillMove;

    public float damageShowTime = 1f;
    float _showTime;
    public float lerpSpeed = 10;

    public void BindConfiguration(IColorTimingConfiguration configuration, ColorTimingSceneId sceneId)
    {
        var battle = configuration.GetBattle(sceneId);
        var player = configuration.GetPlayer(battle.PlayerId);
        damageShowTime = player.HitInvulnerability;
        lerpSpeed = player.HitAnimatorSpeed;
    }
    float _it;

    float _showColor;


    SpriteRenderer spriteRenderer;
    bool flip;

    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 逐帧推进需要实时刷新的业务或表现状态。
    private void Update()
    {

        if(_showTime > 0)
        {
            _showTime -= Time.deltaTime;

            ShowHit();

        }
        else
        {
            spriteRenderer.color = Color.white;
        }

        if(_showColor > 0)
        {
            _showColor -= Time.deltaTime;
            ShowColor();
        }
    }

    // 显示Hit颜色并同步当前数据。
    public void ShowHitColor()
    {
        _showTime = damageShowTime;
    }

    // 显示Hit并同步当前数据。
    void ShowHit()
    {
        float _sp = flip ? -1 : 1;
        _it += (Time.deltaTime * lerpSpeed * _sp);

        spriteRenderer.color = new Color(_it, _it, _it);

        ///print(_it);
        if (flip)
        {
            //检查
            if(_it < 0) flip = false;
        }
        else
        {
            if (_it > 1) flip = true;
        }

    }

    float switchTime = 0.5f;
    WeaponColor colorType;
    // 显示武器颜色并同步当前数据。
    public void ShowWeaponColor(WeaponIdentity weapon)
    {
        if (weapon.IsNormal) return;

        colorType = weapon.Color;
        _showColor = switchTime;
        _it = 0;
    }

    // 显示颜色并同步当前数据。
    void ShowColor()
    {
        Color _c = Color.white;
        _it += (Time.deltaTime * (1 / switchTime));
        if (_it < 0.9f)
        {
            switch (colorType)
            {
                case WeaponColor.Red:
                    _c = new Color(1, _it, _it);
                    break;
                case WeaponColor.Green:
                    _c = new Color(0.6f, _it, 0);
                    break;
                case WeaponColor.Purple:
                    _c = new Color(_it, 0, _it);
                    break;
                default:
                    break;
            }
        }
        //print(_c);
        spriteRenderer.color = _c;
    }

    // 执行冲刺WD对应的主要流程。
    public void DashWD(int enter)
    {
        bool e = enter > 0;
        OnDashWD?.Invoke(e);
    }

    // 执行冲刺结束对应的主要流程。
    public void DashEnd()
    {

        OnDashEnd?.Invoke();

    }

    // 执行攻击对应的主要流程。
    public void Attack(string parm)
    {
        OnAttack?.Invoke(parm);
    }

    // 执行Hit对应的主要流程。
    public void Hit(int _e)
    {
        OnHit?.Invoke(_e);

        if (_e == 0)
        {
            CinemachineImpulseSource impulseSource = GetComponent<CinemachineImpulseSource>();
            impulseSource?.GenerateImpulse();
        }


        if (_e == 1)
        {
            ShowHitColor();

            //print("kankanyou meiyouyou "   + impulseSource);
        }
    }

    // 执行技能移动输入对应的主要流程。
    public void SkillMove(int _start)
    {

        OnSkillMove?.Invoke(_start > 0);
    }

    // 执行Wudi对应的主要流程。
    public void Wudi(int _enter)
    {
        OnWudi?.Invoke(_enter > 0);
    }

}
