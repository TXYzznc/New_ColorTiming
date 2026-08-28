using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using ColorTiming.Presentation.Audio;

public class Skill_Zhadan : Skill_base, IColorTimingSoundConsumer
{
    IColorTimingSoundService soundService;

    public void BindSoundService(IColorTimingSoundService service)
    {
        soundService = service ?? throw new ArgumentNullException(nameof(service));
    }
    //控制飞向目标点，到达目标后播放爆炸，并开启伤害

    public float speed = 10;
    //高度系数
    public bool bUseCurve;
    float dis = 0;

    public AudioClip baozhaAudio;

    Vector3  targetPos = Vector3.zero;
    Animator animator;

    //发起攻击
    bool ok;
    protected override void ChildStart()
    {
        ok = false;
        //print("kankan    :" + parm);
        string type = parm.Split('=')[0];
        //print("leixing" + type);
        string p = parm.Split("=")[1];
        p = p.Replace("(","");
        p = p.Replace(")","");
        //print("wezhi" + p);
        string[] v3s = p.Split(",");
        float x = float.Parse(v3s[0]);
        float y = float.Parse(v3s[1]);
        float z = float.Parse(v3s[2]);

        targetPos = new Vector3(x, y, 0);

        animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetInteger("Type",int.Parse(type));
            //设置一个朝向  旋转Z轴   -90 --90
            //animator.transform.position
            //Quaternion rotation = Quaternion.LookRotation(animator.transform.position, targetPos);

            var dis = targetPos - transform.position;
            var angle = Math.Atan2(dis.x, -dis.y) * (180 / Math.PI);

            //float f = angle > 0 ? 1 : -1;
            //animator.transform.localScale = new Vector3(f,1,1);

            //angle = Mathf.Abs((float)angle) - 90;

            angle = angle - 90;

            animator.transform.Rotate(new Vector3(0,0,(float)angle));
            print("看看角度:::::" + angle);
        }

        var collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        dis = Vector3.Distance(transform.position,targetPos);
    }
    //增加Y轴计算抛物线  类似跳跃的往返运动
    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.fixedDeltaTime);

        float _dis = Vector3.Distance(transform.position, targetPos);


        if ( _dis < 0.01f && !ok)
        {
            //print("已到达目标点");
            if (animator != null) {
                animator.SetTrigger("End");
                animator.transform.localRotation = Quaternion.identity;
                CircleCollider2D circleCollider2D = GetComponent<CircleCollider2D>();
                circleCollider2D.enabled = true;
                ok = true;

                soundService?.Play(baozhaAudio, ColorTimingSoundChannel.Boss, transform.position);
            }

            CinemachineImpulseSource impulseSource = GetComponent<CinemachineImpulseSource>();
            impulseSource?.GenerateImpulse();

            //Destroy(gameObject);
        }
        //不是曲线
        else if(!ok && bUseCurve)
        {
            //print("++++++" + dis);
            if (animator)
            {
                float _d = dis * 0.5f;
                Vector3 aix = _dis > _d ? Vector3.up : Vector3.down;
                float _f = Mathf.Abs(_dis - _d) / _d;


                float fg = Mathf.Lerp(0,1,_f);
                //print(_f +"输出fg"+ fg);
                //*重力  -- 假的重力模拟，越接近高点数值越大，反之越小
                animator.gameObject.transform.localPosition += aix * speed * fg * Time.fixedDeltaTime;
            }
        }


    }

}
