using System;
using System.Collections;
using System.Collections.Generic;
using ColorTiming.Application.Battle;
using UnityEngine;

namespace ColorTiming.Presentation.UI.Components
{
public class PlayerHealthPipsView : MonoBehaviour
{
    const float ItemSpacing = 35f;
    const float AlternateRowOffset = -33f;

    BattleSession session;
    public BattleSession Session => session;

    public GameObject hpItem;

    private void Awake()
    {
    }

    public void Bind(BattleSession battleSession)
    {
        if (session == battleSession && items.Count > 0) return;
        if (session != null) session.SnapshotChanged -= OnSnapshotChanged;
        session = battleSession;
        if (session != null) session.SnapshotChanged += OnSnapshotChanged;
        EnsureItems(session != null ? session.Snapshot.PlayerMaximumHealth : 5);
        SetHP();
    }

    private void Start()
    {
        SetHP();
    }

    private void OnSnapshotChanged(BattleSnapshot snapshot)
    {
        SetHP();
    }

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
            items[i].SetHeroHPItem(i, ItemSpacing, AlternateRowOffset);
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

    private void OnDestroy()
    {
        if (session != null) session.SnapshotChanged -= OnSnapshotChanged;
    }

}
}
