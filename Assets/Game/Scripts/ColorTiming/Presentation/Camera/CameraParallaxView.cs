using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ColorTiming.Input;

public sealed class CameraParallaxView : MonoBehaviour, IGameplayCameraConsumer
{
    public float caseLevel = 0.5f;

    Transform gameplayCamera;

    public void BindGameplayCamera(Camera camera)
    {
        gameplayCamera = camera != null ? camera.transform : null;
    }

    Vector3 startPos;
    private void Start()
    {
        startPos = transform.position;
    }

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
