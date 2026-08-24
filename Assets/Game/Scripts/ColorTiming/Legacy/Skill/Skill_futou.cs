using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_futou : Skill_base
{


    protected override void ChildStart()
    {
        HeroController _c = attacker.GetComponent<HeroController>();
        if (_c)
        {
            //characterSprite.transform.localScale = new Vector3(look, 1, 1);
            transform.localScale = _c.characterSprite.transform.localScale;
        }
    }

    public void ChildTrigger(Collider2D collisio)
    {
        if (cTag != "" && cTag != collisio.gameObject.tag) return;
        OnHit(collisio, gameObject);
    }



}
