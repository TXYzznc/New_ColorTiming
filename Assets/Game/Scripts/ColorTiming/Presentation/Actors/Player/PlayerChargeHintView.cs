using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using ColorTiming.Combat;
using UnityEngine;
using CombatWeaponType = ColorTiming.Combat.WeaponType;

public class PlayerChargeHintView : MonoBehaviour
{
    public GameObject tip;

    bool isTip;
    PlayerActorView controller;

    private void Start()
    {
        controller = GetComponent<PlayerActorView>();
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

    private void SwitchWeapon(WeaponIdentity weapon)
    {
        if (!isTip )
        {
            if (weapon.Type == CombatWeaponType.Hammer || weapon.Type == CombatWeaponType.Axe)
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
