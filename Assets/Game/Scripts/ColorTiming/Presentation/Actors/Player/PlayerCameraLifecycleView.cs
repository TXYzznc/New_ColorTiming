// 文件职责：负责 玩家相机生命周期 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / Actors / Player。

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

    // 逐帧推进需要实时刷新的业务或表现状态。
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
