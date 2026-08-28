// 文件职责：负责 UIButton音效 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / UI / Components。

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ColorTiming.Presentation.Audio;

namespace ColorTiming.Presentation.UI.Components
{
public class UiButtonSoundView : MonoBehaviour, IPointerEnterHandler, IUiSoundConsumer
{
    IUiSoundSink sound;

    // 绑定UI音效依赖或事件监听。
    public void BindUiSound(IUiSoundSink uiSound)
    {
        sound = uiSound ?? throw new ArgumentNullException(nameof(uiSound));
    }
    PointerEventData BtnEnter ;

    // 在首帧启动依赖就绪后的业务或表现流程。
    void Start()
    {
        Button button = GetComponent<Button>();
        if (button)
        {

            button.onClick.AddListener(BtnClick);
        }

     }

    private void BtnClick()
    {
        sound?.PlayClick();
    }


    // 响应指针Enter回调，并更新本对象状态。
    public void OnPointerEnter(PointerEventData eventData)
    {
        sound?.PlayHover();
    }

    // 组件销毁时释放订阅、句柄和运行时资源。
    private void OnDestroy()
    {
        var button = GetComponent<Button>();
        button?.onClick.RemoveListener(BtnClick);
    }
}
}
