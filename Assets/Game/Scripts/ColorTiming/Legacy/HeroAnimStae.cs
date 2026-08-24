using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HeroAnimStae : MonoBehaviour
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
    float _it;

    float _showColor;
    

    SpriteRenderer spriteRenderer;
    bool flip;

    private void Start()
    {    
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
   
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

    public void ShowHitColor()
    {
        _showTime = damageShowTime;
    }

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
    ColorType colorType;
    public void ShowWeaponColor(Weapon _weapon)
    {
        if (_weapon.weaponType == WeaponType.nor) return;

        colorType = _weapon.colorType;
        _showColor = switchTime;
        _it = 0;
    }

    void ShowColor()
    {
        Color _c = Color.white;
        _it += (Time.deltaTime * (1 / switchTime));
        if (_it < 0.9f) 
        {
            switch (colorType)
            {
                case ColorType.hong:
                    _c = new Color(1, _it, _it);
                    break;
                case ColorType.lv:
                    _c = new Color(0.6f, _it, 0);
                    break;
                case ColorType.zi:
                    _c = new Color(_it, 0, _it);
                    break;
                default:
                    break;
            }
        }
        //print(_c);
        spriteRenderer.color = _c;
    }

    public void DashWD(int enter)
    {
        bool e = enter > 0;
        OnDashWD?.Invoke(e);
    }

    public void DashEnd()
    {

        OnDashEnd?.Invoke();

    }

    public void Attack(string parm)
    {
        OnAttack?.Invoke(parm);
    }

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

    public void SkillMove(int _start) 
    {

        OnSkillMove?.Invoke(_start > 0);
    }

    public void Wudi(int _enter)
    {
        OnWudi?.Invoke(_enter > 0);
    }

}




