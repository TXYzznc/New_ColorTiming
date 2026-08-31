// 文件职责：负责 玩家相机生命周期 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / Actors / Player。

using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraLifecycleView : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public Transform bossT;

    [NonSerialized] public float maxSize = 12;
    [NonSerialized] public float minSize = 8;

    [NonSerialized] public float disRi = 5;

    private bool _configured;

    /// <summary>由 BattlePlayerManager 注入当前场景的相机、Boss 与调节参数。</summary>
    public void Configure(
        CinemachineVirtualCamera camera,
        Transform bossTarget,
        float minimumSize,
        float maximumSize,
        float distanceRange,
        float startDistance)
    {
        virtualCamera = camera != null ? camera : throw new ArgumentNullException(nameof(camera));
        bossT = bossTarget != null ? bossTarget : throw new ArgumentNullException(nameof(bossTarget));
        if (minimumSize <= 0f || maximumSize < minimumSize || distanceRange <= 0f || startDistance < 0f)
            throw new ArgumentOutOfRangeException(nameof(minimumSize), "Camera configuration is invalid.");
        minSize = minimumSize;
        maxSize = maximumSize;
        disRi = distanceRange;
        _startDistance = startDistance;
        _configured = true;
    }

    private float _startDistance;

    // 逐帧推进需要实时刷新的业务或表现状态。
    private void Update()
    {
        if (!_configured) return;
        float dis = Vector2.Distance(transform.position,bossT.position);

        float l = 0;
        if(dis > _startDistance)
        {
            l = (dis - _startDistance) / disRi;
        }

        float _s = Mathf.Lerp(minSize,maxSize,l);

        virtualCamera.m_Lens.OrthographicSize = _s;
    }
}
