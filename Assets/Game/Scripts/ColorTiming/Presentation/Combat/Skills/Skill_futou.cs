using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_futou : Skill_base
{


    protected override void ChildStart()
    {
        transform.localScale = new Vector3(Facing < 0 ? -1f : 1f, 1f, 1f);
    }

    public void ChildTrigger(Collider2D collisio)
    {
        if (cTag != "" && cTag != collisio.gameObject.tag) return;
        OnHit(collisio, gameObject);
    }



}
