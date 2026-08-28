// 文件职责：负责 Boss2生命值 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / UI / Components。

using System.Collections;
using System.Collections.Generic;
using ColorTiming.Application.Battle;
using ColorTiming.Combat;
using UnityEngine;

namespace ColorTiming.Presentation.UI.Components
{
public class Boss2HealthView : MonoBehaviour
{
    //获取Boss 血量进行初始化
    public GameObject HPItem;
    BattleSession session;
    public BattleSession Session => session;

    int cCount;
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
        cCount = 0;
        lastWeaknessCount = -1;
        if (session != null && session.Kind != BattleKind.Boss2)
            throw new System.ArgumentException("Boss2 HP view requires a Boss2 session.", nameof(battleSession));
        if (session != null) session.SnapshotChanged += OnSnapshotChanged;
        if (session == null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].HideTip();
                items[i].gameObject.SetActive(false);
            }
            return;
        }
        EnsureItems(7);
        SetHPBox(session.Snapshot);
    }

    int lastWeaknessCount = -1;
    // 响应快照变化回调，并更新本对象状态。
    void OnSnapshotChanged(BattleSnapshot snapshot)
    {
        if (snapshot.Weaknesses.Count == lastWeaknessCount) return;
        SetHPBox(snapshot);
    }

    // 设置HPBox，并使后续流程使用最新状态。
    void SetHPBox(BattleSnapshot snapshot)
    {
        if (session == null) return;
        //transform.childs
        //清理所有子项

        int hc = snapshot.Weaknesses.Count;
        hc = hc > 7 ? 7: hc;

        for (int i = 0; i < items.Count; i++)
        {
            var uiItem = items[i];
            uiItem.gameObject.SetActive(i < hc);
            uiItem.HideTip();
            if (i >= hc) continue;
            uiItem.SetHpItem(snapshot.Weaknesses[i], i);

            if (cCount < 3 && !(i > 0))
            {
                uiItem?.ShowTip(2);
            }
        }
        cCount++;
        lastWeaknessCount = snapshot.Weaknesses.Count;
    }

    readonly List<BossWeaknessPipView> items = new List<BossWeaknessPipView>(7);

    void EnsureItems(int count)
    {
        if (HPItem == null) throw new MissingReferenceException("Boss HP item prefab is required.");
        while (items.Count < count)
        {
            var instance = Instantiate(HPItem, transform);
            var item = instance.GetComponent<BossWeaknessPipView>();
            if (item == null) throw new MissingComponentException("Boss HP item requires BossWeaknessPipView.");
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
