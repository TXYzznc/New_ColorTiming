using System;
using System.Collections;
using System.Collections.Generic;
using ColorTiming.Input;
using ColorTiming.Combat;
using ColorTiming.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_HeroInfo : MonoBehaviour, IGameInputConsumer, IColorTimingUiConsumer
{

    public HeroController controller;

    public Image heroWeapon;

    public Sprite[] weapons;

    public Image weaponTip;
    public Image weaponTipx;

    public Texture2D[] cursors;

    public GameObject ESC;

    Weapon nowWeapon;
    IGameInput gameInput;
    IColorTimingUiService uiService;

    public void BindGameInput(IGameInput input)
    {
        gameInput = input ?? throw new ArgumentNullException(nameof(input));
    }

    public void BindUiService(IColorTimingUiService service)
    {
        uiService = service ?? throw new ArgumentNullException(nameof(service));
    }

    private void Start()
    {
        BindHero(controller);
        SetCursor(WeaponPresentationState.NormalCursorIndex);
    }

    public void BindHero(HeroController heroController)
    {
        if (controller == heroController)
        {
            if (controller != null && controller.nowweapon != null)
            {
                SwitchWapon(controller.nowweapon);
            }
            return;
        }

        controller?.OnSwitchWeapon.RemoveListener(SwitchWapon);
        controller = heroController;
        controller?.OnSwitchWeapon.AddListener(SwitchWapon);
        if (controller != null && controller.nowweapon != null)
        {
            SwitchWapon(controller.nowweapon);
        }
    }

    private void Update()
    {
        if (gameInput == null)
        {
            return;
        }

        if (nowWeapon != null && (uiService == null || !uiService.IsPauseOpen))
        {
            if (nowWeapon.weaponType == WeaponType.nor)
            {
                if (gameInput.AttackHeld)
                {
                    SetCursor(WeaponPresentationState.HeldNormalCursorIndex);
                }
                else
                {
                    SetCursor(WeaponPresentationState.NormalCursorIndex);
                }
            }
        }

        if (gameInput.PausePressed)
            TogglePause();
        
    }

    private void SwitchWapon(Weapon arg0)
    {
        if (arg0 == null)
        {
            return;
        }

        nowWeapon = arg0;
        var presentation = WeaponPresentationState.From(arg0.Identity);
        if (heroWeapon != null && TryGet(weapons, presentation.IconIndex, out var weaponIcon))
        {
            heroWeapon.sprite = weaponIcon;
        }

        if (weaponTip != null)
        {
            weaponTip.gameObject.SetActive(!presentation.UsesChargeHint);
        }
        if (weaponTipx != null)
        {
            weaponTipx.gameObject.SetActive(presentation.UsesChargeHint);
        }

        SetCursor(presentation.CursorIndex);
    }

    // Public for serialized UI compatibility and deterministic PlayMode validation.
    public void TogglePause()
    {
        var opened = uiService != null && uiService.TogglePause();
        if (opened)
        {
            SetCursor(WeaponPresentationState.PauseCursorIndex);
            return;
        }

        if (controller != null && controller.nowweapon != null)
        {
            SwitchWapon(controller.nowweapon);
        }
    }

    private void OnDestroy()
    {
        BindHero(null);
        SetCursor(WeaponPresentationState.NormalCursorIndex);
    }

    private void SetCursor(int index)
    {
        if (TryGet(cursors, index, out var cursor))
        {
            Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
        }
    }

    private static bool TryGet<T>(T[] items, int index, out T item)
    {
        if (items != null && index >= 0 && index < items.Length)
        {
            item = items[index];
            return true;
        }

        item = default;
        return false;
    }
}
