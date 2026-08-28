using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Boss1ProximitySensor : MonoBehaviour
{
    //在意的区域
    public int csIndx;

    public Boss1ActorView controller;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        controller?.EnterCase(csIndx,true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        controller?.EnterCase(csIndx, false);
    }

}
