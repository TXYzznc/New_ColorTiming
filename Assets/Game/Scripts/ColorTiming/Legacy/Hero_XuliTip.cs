using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class Hero_XuliTip : MonoBehaviour
{
    public GameObject tip;

    bool isTip;
    HeroController controller;

    private void Start()
    {
        controller = GetComponent<HeroController>();
        controller.OnSwitchWeapon.AddListener(SwitchWeapon);
        controller.OnAnimState.AddListener(OnAnimState);
        tip.SetActive(false);
    }

    private void OnDestroy()
    {
        if (controller == null)
        {
            return;
        }

        controller.OnSwitchWeapon.RemoveListener(SwitchWeapon);
        controller.OnAnimState.RemoveListener(OnAnimState);
        controller = null;
    }

    private void OnAnimState(bool enter, AnimatorStateInfo info)
    {
        if (enter)
        {
            if (info.IsName("xuliw"))
            {
                isTip = true;
                tip.SetActive(false);
            }
        }
    }

    private void SwitchWeapon(Weapon arg0)
    {
        if (!isTip )
        {
            if (arg0.weaponType == WeaponType.chuizhi || arg0.weaponType == WeaponType.futou)
            {
                
                tip.SetActive(true);
            }
            else 
            {
                //未完成蓄力
                tip.SetActive(false);
            }
        }
    }
}
