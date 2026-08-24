using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface I_Damage 
{
    public void OnDamage(GameObject _attacker,Weapon _weapon,Vector2 cPoint,string parm);
}
