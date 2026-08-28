using System;
using System.Collections;
using System.Collections.Generic;
using ColorTiming.Application.Battle;
using ColorTiming.Input;
using ColorTiming.Combat;
using ColorTiming.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

public class BattlePlayerInfoView : MonoBehaviour, IGameInputConsumer, IColorTimingUiConsumer
{

    BattleSession session;
    public BattleSession Session => session;

    public Image heroWeapon;

    public Sprite[] weapons;

    public Image weaponTip;
    public Image weaponTipx;

    public Texture2D[] cursors;

    public GameObject ESC;

    WeaponIdentity nowWeapon;
    bool hasWeaponState;
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
        BindSession(session);
        SetCursor(WeaponPresentationState.NormalCursorIndex);
    }

    public void BindSession(BattleSession battleSession)
    {
        if (session == battleSession)
        {
            if (session != null) SwitchWeapon(session.Snapshot.Weapon);
            else ResetPresentation();
            return;
        }

        if (session != null) session.SnapshotChanged -= OnSnapshotChanged;
        session = battleSession;
        if (session != null) session.SnapshotChanged += OnSnapshotChanged;
        if (session != null) SwitchWeapon(session.Snapshot.Weapon);
        else ResetPresentation();
    }

    private void Update()
    {
        if (gameInput == null)
        {
            return;
        }

        if (hasWeaponState && (uiService == null || !uiService.IsPauseOpen))
        {
            if (nowWeapon.Type == ColorTiming.Combat.WeaponType.Normal)
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

    private void OnSnapshotChanged(BattleSnapshot snapshot)
    {
        if (!hasWeaponState || !snapshot.Weapon.Equals(nowWeapon)) SwitchWeapon(snapshot.Weapon);
    }

    private void SwitchWeapon(WeaponIdentity weapon)
    {
        nowWeapon = weapon;
        hasWeaponState = true;
        var presentation = WeaponPresentationState.From(weapon);
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

        if (session != null) SwitchWeapon(session.Snapshot.Weapon);
    }

    private void OnDestroy()
    {
        BindSession(null);
    }

    private void ResetPresentation()
    {
        nowWeapon = default;
        hasWeaponState = false;
        if (heroWeapon != null
            && TryGet(weapons, WeaponPresentationState.NormalIconIndex, out var normalWeaponIcon))
        {
            heroWeapon.sprite = normalWeaponIcon;
        }

        if (weaponTip != null)
        {
            weaponTip.gameObject.SetActive(true);
        }
        if (weaponTipx != null)
        {
            weaponTipx.gameObject.SetActive(false);
        }

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
