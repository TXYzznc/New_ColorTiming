using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Bo1_Atk5 : Skill_base
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChildTrigger(Collider2D collisio)
    {
        if (cTag != "" && cTag != collisio.gameObject.tag) return;
        OnHit(collisio, gameObject);
    }
}
