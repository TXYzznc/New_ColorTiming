using System.Collections;
using System.Collections.Generic;

using UnityEngine;

//随机特效
public class skill_base_randomHit : Skill_base
{
    public List<GameObject> hitFXs = new List<GameObject>();


    protected override void ChildStart()
    {
        if (hitFXs.Count > 0)
        {
            int _r = Random.Range(0, hitFXs.Count);
            HitFX = hitFXs[_r];
            //hitFXs = hitFXs
        }
    }
}
