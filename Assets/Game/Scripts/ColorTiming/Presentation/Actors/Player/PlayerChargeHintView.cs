// 文件职责：负责 玩家ChargeHint 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / Actors / Player。

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

    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
        controller = GetComponent<PlayerActorView>();
        controller.OnSwitchWeapon.AddListener(SwitchWeapon);
        controller.OnAnimState.AddListener(OnAnimState);
        tip.SetActive(false);
    }

    // 组件销毁时释放订阅、句柄和运行时资源。
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

    // 响应Anim状态回调，并更新本对象状态。
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
