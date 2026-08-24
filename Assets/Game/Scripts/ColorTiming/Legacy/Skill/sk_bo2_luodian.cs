using System;
using System.Collections;
using System.Collections.Generic;
using ColorTiming.Presentation.Entities;
using UnityEngine;

public class sk_bo2_luodian : MonoBehaviour, IFrameworkEntityParticipant
{
    Transform cseF;
    SpriteRenderer sp;
    // Update is called once per frame

    float fTime;
    float _f = 1;
    float count = 0;

    float dis;
    float releaseDelay = -1f;
    Action frameworkRelease;
    bool releasing;
    public void SetCaseF(Transform _c)
    {
        cseF = _c;
        dis = Vector2.Distance(cseF.position, transform.position);
    }

    void Update()
    {
        if (releaseDelay > 0f)
        {
            releaseDelay -= Time.deltaTime;
            if (releaseDelay <= 0f)
            {
                ReleaseEntity();
                return;
            }
        }

        if (sp == null)
        {
            sp = GetComponent<SpriteRenderer>();
        }
        else
        {
            float tf = 0.3f;
            tf -= count * 0.05f;
            tf = tf > 0.05f ? tf : 0.05f;

            if (fTime > tf)
            {
                _f = -1;
                
            }
            else if (fTime < 0)
            {
                _f = 1;
                count++;

            }

            //print(tf);
            fTime += Time.deltaTime * _f * tf * 5;

            float fg = Mathf.Lerp(0, 1, fTime / tf);

            sp.color = new Color(1, 1, 1, fg);

        }



        //if (sp == null)
        //{
        //    sp = GetComponent<SpriteRenderer>();
        //}
        //else
        //{
        //    float tf = 0.1f;
        //    //tf -= count * 0.05f;
        //    //tf = tf > 0.05f ? tf : 0.05f;

        //    if (cseF != null)
        //    {
        //        float _dis = Vector2.Distance(cseF.position, transform.position);


        //        tf = Mathf.Lerp(0.05f, 0.3f, _dis / dis);
        //        //print(tf);
        //    }

        //    if (tf > 0.05f)
        //    {
        //        if (fTime > tf)
        //        {
        //            _f = -1;
        //        }
        //        else if (fTime < 0)
        //        {
        //            _f = 1;
        //            count++;
        //        }

        //        fTime += Time.deltaTime * _f * (1 - tf) * 3;

        //        //print(fTime);

        //        float fg = Mathf.Lerp(0, 1, fTime / tf);

        //        sp.color = new Color(1, 1, 1, fg);
        //    }
        //    else
        //    {
        //        sp.color = new Color(1, 1, 1, 1);
        //    }
        //}
    }

    
    public void SetWaitEnd()
    {
        releaseDelay = 0.5f;
    }

    public void ReleaseEntity()
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

    public void BindFrameworkRelease(Action release)
    {
        frameworkRelease = release;
    }

    public void OnFrameworkEntitySpawned()
    {
        fTime = 0f;
        _f = 1f;
        count = 0f;
        releaseDelay = -1f;
        releasing = false;
        sp = sp != null ? sp : GetComponent<SpriteRenderer>();
    }

    public void OnFrameworkEntityDespawned()
    {
        cseF = null;
        releaseDelay = -1f;
        releasing = false;
    }

}
