// 文件职责：负责 玩家生命值Pips 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / UI / Components。

using System;
using System.Collections;
using System.Collections.Generic;
using ColorTiming.Application.Battle;
using UnityEngine;

namespace ColorTiming.Presentation.UI.Components
{
public class PlayerHealthPipsView : MonoBehaviour
{
    float itemSpacing;
    float alternateRowOffset;

    public void Configure(float spacing, float rowOffset)
    {
        itemSpacing = spacing;
        alternateRowOffset = rowOffset;
    }

    BattleSession session;
    public BattleSession Session => session;

    public GameObject hpItem;

    // 缓存本组件依赖，并完成不依赖外部服务的本地初始化。
    private void Awake()
    {
    }

    // 执行Bind对应的主要流程。
    public void Bind(BattleSession battleSession)
    {
        if (session == battleSession && items.Count > 0) return;
        if (session != null) session.SnapshotChanged -= OnSnapshotChanged;
        session = battleSession;
        if (session != null) session.SnapshotChanged += OnSnapshotChanged;
        EnsureItems(session != null ? session.Snapshot.PlayerMaximumHealth : 5);
        SetHP();
    }

    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
        SetHP();
    }

    // 响应快照变化回调，并更新本对象状态。
    private void OnSnapshotChanged(BattleSnapshot snapshot)
    {
        SetHP();
    }

    // 设置HP，并使后续流程使用最新状态。
    void SetHP()
    {
        if (session == null)
        {
            for (var i = 0; i < items.Count; i++) items[i].gameObject.SetActive(false);
            return;
        }
        for (int i = 0; i < items.Count; i++)
        {
            var active = i < session.Snapshot.PlayerHealth;
            items[i].gameObject.SetActive(active);
            items[i].SetHeroHPItem(i, itemSpacing, alternateRowOffset);
        }
    }

    readonly List<PlayerHealthPipView> items = new List<PlayerHealthPipView>(5);

    void EnsureItems(int count)
    {
        if (hpItem == null) throw new MissingReferenceException("Hero HP item prefab is required.");
        while (items.Count < count)
        {
            var instance = Instantiate(hpItem, transform);
            var item = instance.GetComponent<PlayerHealthPipView>();
            if (item == null) throw new MissingComponentException("Hero HP item requires PlayerHealthPipView.");
            items.Add(item);
            item.gameObject.SetActive(false);
        }
    }

    // 组件销毁时释放订阅、句柄和运行时资源。
    private void OnDestroy()
    {
        if (session != null) session.SnapshotChanged -= OnSnapshotChanged;
    }

}
}
