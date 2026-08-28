using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ColorTiming.Presentation.Audio;

public class UiButtonSoundView : MonoBehaviour, IPointerEnterHandler, IUiSoundConsumer
{
    IUiSoundSink sound;

    public void BindUiSound(IUiSoundSink uiSound)
    {
        sound = uiSound ?? throw new ArgumentNullException(nameof(uiSound));
    }
    PointerEventData BtnEnter ;

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


    public void OnPointerEnter(PointerEventData eventData)
    {
        sound?.PlayHover();
    }

    private void OnDestroy()
    {
        var button = GetComponent<Button>();
        button?.onClick.RemoveListener(BtnClick);
    }
}
