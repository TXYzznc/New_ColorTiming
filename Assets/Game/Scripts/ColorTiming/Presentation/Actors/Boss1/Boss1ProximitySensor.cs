// 文件职责：定义 Boss1ProximitySensor，承担 Boss1 模块中的对应职责。
// 所属模块：ColorTiming / Presentation / Actors / Boss1。

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Boss1ProximitySensor : MonoBehaviour
{
    //在意的区域
    public int csIndx;

    public Boss1ActorView controller;

    // 响应TriggerEnter2D回调，并更新本对象状态。
    private void OnTriggerEnter2D(Collider2D collision)
    {
        controller?.EnterCase(csIndx,true);
    }

    // 响应TriggerExit2D回调，并更新本对象状态。
    private void OnTriggerExit2D(Collider2D collision)
    {
        controller?.EnterCase(csIndx, false);
    }

}
