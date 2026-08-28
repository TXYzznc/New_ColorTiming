
using System;
using UnityEngine;

public class Skill_Bo2_Atk2 : Skill_base
{

    public Transform pao;
    public Transform zhituan;
    public Transform tuowei;
    public Transform yin;
    public GameObject luodianINS;

    public float flySpeed = 15;

    sk_bo2_luodian luodian;
    Vector3 targetPos;

    Vector3 lastPos;

    float dis = 0;
    //默认不要启动，等待设置
    bool ok = true;
    float sFH = 0;

    float maxH;
    float endDelay = -1f;
    bool initialPoseCaptured;
    Vector3 initialPaoLocalPosition;
    Quaternion initialZhiTuanRotation;
    Quaternion initialTrailRotation;

    public void Set(Vector3 target)
    {
        if (!initialPoseCaptured)
        {
            initialPoseCaptured = true;
            initialPaoLocalPosition = pao.localPosition;
            initialZhiTuanRotation = zhituan.localRotation;
            initialTrailRotation = tuowei.localRotation;
        }

        //string p = parm;
        //p = p.Replace("(", "");
        //p = p.Replace(")", "");
        ////print("wezhi" + p);
        //string[] v3s = p.Split(",");
        //float x = float.Parse(v3s[0]);
        //float y = float.Parse(v3s[1]);
        //float z = float.Parse(v3s[2]);

        //targetPos = new Vector3(x, y, 0);

        targetPos = target;
        lastPos = transform.position;
        ok = false;
        endDelay = -1f;
        pao.gameObject.SetActive(true);
        yin.gameObject.SetActive(true);
        pao.localPosition = initialPaoLocalPosition;
        zhituan.localRotation = initialZhiTuanRotation;
        tuowei.localRotation = initialTrailRotation;
        GetComponent<PolygonCollider2D>().enabled = false;

        sFH = zhituan.localPosition.y;

        SpawnTransient(
            luodianINS,
            targetPos,
            Quaternion.identity,
            null,
            instance =>
            {
                luodian = instance.GetComponent<sk_bo2_luodian>();
                luodian?.SetCaseF(transform);
            });
        dis = Vector2.Distance(transform.position, targetPos);
        sFH = dis * (zhituan.localPosition.y / (dis * 0.5f));
        ok = false;
    }

    //现在的初始高度为8   ，最大高度，应大约为  距离的一半。  也就是发射的越远 ，需求的发射角度越大， 飞行曲线高度越高
    //也就是要求，  Boss与玩家的距离大于8 时，才会向上飞行
    private void FixedUpdate()
    {
        if (endDelay > 0f)
        {
            endDelay -= Time.fixedDeltaTime;
            if (endDelay <= 0f)
            {
                End();
            }
            return;
        }

        //当前距离
        float _dis = Vector3.Distance(transform.position, targetPos);

        if (!ok)
        {

            float speed = 50;
            if (_dis < 0.01f)
            {
                //到达
                ok = true;

                luodian?.ReleaseEntity();
                luodian = null;
                //luodian.SetWaitEnd();
                pao.gameObject.SetActive(false);
                yin.gameObject.SetActive(false);
                GetComponent<PolygonCollider2D>().enabled = true;

                endDelay = 0.5f;

            }
            else
            {
                float _d = dis * 0.75f ;
                Vector3 aix = _dis  < _d ? Vector3.up:Vector3.down;
                //把前1/4的飞行作为向上飞行。


                if (_dis > _d)
                {
                    aix = Vector3.up;
                    float _dd = dis - _d;

                    float _f = Mathf.Abs(_dis - _dd) / _dd;
                    float fg = Mathf.Lerp(0, 1, _f);

                    pao.transform.localPosition += aix * Time.fixedDeltaTime * fg * speed * 0.25f;

                }
                else
                {
                    aix = Vector3.down;

                    float _f = Mathf.Abs(_dis - _d) / _d;

                    float fg = Mathf.Lerp(0,1,_f);

                    //zhituan.transform.localPosition = new Vector3(0, fg, 0);
                    pao.transform.localPosition += aix * Time.fixedDeltaTime  * fg * (speed * 0.5f);
                }
            }

            //移动部分
            transform.position = Vector3.MoveTowards(transform.position, targetPos, flySpeed * Time.fixedDeltaTime);

            //旋转部分
            zhituan.transform.Rotate(Vector3.forward * 500 * Time.fixedDeltaTime);


            var di = transform.position - lastPos ;
            var angle = Math.Atan2(di.x, -di.y) * (180 / Math.PI);

            tuowei.localRotation = Quaternion.Euler(0,0, (float)angle );




            lastPos = transform.position;
        }
        //
    }

    void End()
    {



        ReleaseSelf();
    }
}
