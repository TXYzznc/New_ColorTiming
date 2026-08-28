// 文件职责：负责 相机Parallax 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / Camera。

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ColorTiming.Input;

public sealed class CameraParallaxView : MonoBehaviour, IGameplayCameraConsumer
{
    public float caseLevel = 0.5f;

    Transform gameplayCamera;

    // 绑定Gameplay相机依赖或事件监听。
    public void BindGameplayCamera(Camera camera)
    {
        gameplayCamera = camera != null ? camera.transform : null;
    }

    Vector3 startPos;
    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
        startPos = transform.position;
    }

    // 逐帧推进需要实时刷新的业务或表现状态。
    private void Update()
    {
        //获取差值
        if (gameplayCamera == null) return;
        Vector2 cP = gameplayCamera.position - startPos;
        //设置反向
        Vector2 cp = cP * caseLevel;
        transform.position = new Vector3(cp.x,cp.y,0) + startPos;


    }
}
