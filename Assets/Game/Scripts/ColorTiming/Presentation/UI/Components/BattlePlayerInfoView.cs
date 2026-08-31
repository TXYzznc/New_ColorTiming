// 文件职责：负责 战斗玩家Info 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / UI / Components。

using System;
using System.Collections;
using System.Collections.Generic;
using ColorTiming.Application.Battle;
using ColorTiming.Input;
using ColorTiming.Combat;
using ColorTiming.Configuration;
using ColorTiming.Presentation.UI.Contracts;
using ColorTiming.Presentation.UI.Models;
using UnityEngine;
using UnityEngine.UI;

namespace ColorTiming.Presentation.UI.Components
{
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
    int appliedCursorIndex = -1;
    Texture2D appliedCursor;
    IGameInput gameInput;
    IColorTimingUiService uiService;
    IColorTimingConfiguration configuration;
    ColorTimingPresentationTable presentationConfiguration;
    ColorTimingWeaponTable normalWeaponConfiguration;

    public void BindConfiguration(IColorTimingConfiguration source)
    {
        configuration = source ?? throw new ArgumentNullException(nameof(source));
        presentationConfiguration = configuration.Presentation;
        normalWeaponConfiguration = configuration.GetWeapon(new WeaponIdentity(WeaponColor.Red, WeaponType.Normal));
    }

    // 绑定Game输入依赖或事件监听。
    public void BindGameInput(IGameInput input)
    {
        gameInput = input ?? throw new ArgumentNullException(nameof(input));
    }

    // 绑定UIService依赖或事件监听。
    public void BindUiService(IColorTimingUiService service)
    {
        uiService = service ?? throw new ArgumentNullException(nameof(service));
    }

    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
        BindSession(session);
        EnsureConfiguration();
        SetCursor(normalWeaponConfiguration.CursorIndex);
    }

    // 绑定会话依赖或事件监听。
    public void BindSession(BattleSession battleSession)
    {
        if (session == battleSession)
        {
            if (session != null) SwitchWeapon(session.Snapshot.Weapon);
            // 执行Reset展示对应的主要流程。
            else ResetPresentation();
            return;
        }

        if (session != null) session.SnapshotChanged -= OnSnapshotChanged;
        session = battleSession;
        if (session != null) session.SnapshotChanged += OnSnapshotChanged;
        if (session != null) SwitchWeapon(session.Snapshot.Weapon);
        // 执行Reset展示对应的主要流程。
        else ResetPresentation();
    }

    // 逐帧推进需要实时刷新的业务或表现状态。
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
                    SetCursor(presentationConfiguration.HeldNormalCursorIndex);
                }
                else
                {
                    SetCursor(normalWeaponConfiguration.CursorIndex);
                }
            }
        }

        if (gameInput.PausePressed)
            TogglePause();

    }

    // 响应快照变化回调，并更新本对象状态。
    private void OnSnapshotChanged(BattleSnapshot snapshot)
    {
        if (!hasWeaponState || !snapshot.Weapon.Equals(nowWeapon)) SwitchWeapon(snapshot.Weapon);
    }

    private void SwitchWeapon(WeaponIdentity weapon)
    {
        nowWeapon = weapon;
        hasWeaponState = true;
        EnsureConfiguration();
        var presentation = WeaponPresentationState.From(configuration.GetWeapon(weapon));
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
    // 执行Toggle暂停对应的主要流程。
    public void TogglePause()
    {
        var opened = uiService != null && uiService.TogglePause();
        if (opened)
        {
            SetCursor(presentationConfiguration.PauseCursorIndex);
            return;
        }

        if (session != null) SwitchWeapon(session.Snapshot.Weapon);
    }

    // 组件销毁时释放订阅、句柄和运行时资源。
    private void OnDestroy()
    {
        BindSession(null);
    }

    private void ResetPresentation()
    {
        nowWeapon = default;
        hasWeaponState = false;
        if (heroWeapon != null
            && TryGet(weapons, normalWeaponConfiguration.IconIndex, out var normalWeaponIcon))
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

        SetCursor(normalWeaponConfiguration.CursorIndex);
    }

    void EnsureConfiguration()
    {
        if (configuration == null) throw new InvalidOperationException("BattlePlayerInfoView requires runtime configuration.");
    }

    // 设置Cursor，并使后续流程使用最新状态。
    private void SetCursor(int index)
    {
        if (TryGet(cursors, index, out var cursor))
        {
            // 攻击按住状态会逐帧求值；只有光标资源真正变化时才调用 Unity 原生接口。
            if (appliedCursorIndex == index && appliedCursor == cursor)
            {
                return;
            }

            Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
            appliedCursorIndex = index;
            appliedCursor = cursor;
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
}
