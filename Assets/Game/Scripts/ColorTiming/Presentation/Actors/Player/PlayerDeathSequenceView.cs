using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System;
using ColorTiming.Bootstrap.Flow;
using UnityEngine;

public class PlayerDeathSequenceView : MonoBehaviour, IColorTimingSceneFlowConsumer
{
    IColorTimingSceneFlow sceneFlow;

    public void BindSceneFlow(IColorTimingSceneFlow flow)
    {
        sceneFlow = flow ?? throw new ArgumentNullException(nameof(flow));
    }
    public Transform hero;

    public string reStartScene = "Boss1";
    CinemachineVirtualCamera virtualCamera;

    float showtime = 0.3f;
    float _st;

    float startSize = 8f;
    bool restartRequested;
    private void Start()
    {
        // 控制镜头缩进到5.2   先获取镜头大小
        virtualCamera = GetComponentInParent<CinemachineVirtualCamera>();
        GetComponentInParent<CinemachineConfiner2D>().enabled = false;
        GetComponentInParent<CinemachineImpulseListener>().enabled = false;
        startSize = virtualCamera.m_Lens.OrthographicSize;


        _st = showtime;
        //transform.localScale = new Vector3(_l, _l, 1);
    }

    private void Update()
    {
        if(_st > 0)
        {
            _st -= Time.deltaTime;
            //缩进镜头

            float v = _st / showtime;

            float _s = Mathf.Lerp(5.2f, startSize, v);

            float _l = Mathf.Lerp(1, 2.5f, v);

            virtualCamera.m_Lens.OrthographicSize = _s;

            transform.localScale = new Vector3(_l, _l,1);
            transform.position = hero.position;
        }

    }


    public void DeathOver(int _e)
    {
        if (_e > 0 && !restartRequested)
        {
            restartRequested = true;
            if (Enum.TryParse(reStartScene, true, out ColorTimingSceneId scene))
            {
                sceneFlow?.TryLoad(scene, true);
            }
            else
            {
                restartRequested = false;
                Debug.LogError($"Unknown ColorTiming restart scene '{reStartScene}'.", this);
            }
        }
    }
}
