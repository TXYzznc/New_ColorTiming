using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraLifecycleView : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public Transform bossT;

    public float maxSize = 12;
    public float minSize = 8;

    public float disRi = 5;

    private void Update()
    {
        float dis = Vector2.Distance(transform.position,bossT.position);

        float l = 0;
        if(dis > 5)
        {
            l = (dis - 5) / disRi;
        }

        float _s = Mathf.Lerp(minSize,maxSize,l);

        virtualCamera.m_Lens.OrthographicSize = _s;
    }
}
