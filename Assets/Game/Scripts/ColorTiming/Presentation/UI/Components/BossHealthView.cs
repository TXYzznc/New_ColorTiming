// 文件职责：使用单一视图显示当前战斗 Boss 的弱点血量。
// 所属模块：ColorTiming / Presentation / UI / Components。

using System.Collections.Generic;
using ColorTiming.Application.Battle;
using ColorTiming.Combat;
using UnityEngine;

namespace ColorTiming.Presentation.UI.Components
{
    /// <summary>所有 Boss 共用的弱点血条表现与订阅生命周期。</summary>
    public sealed class BossHealthView : MonoBehaviour
    {
        [SerializeField] private GameObject hpItem;

        private readonly List<BossWeaknessPipView> _items = new List<BossWeaknessPipView>(7);
        private BattleSession _session;
        private int _tipRefreshCount;
        private int _lastWeaknessCount = -1;

        public BattleSession Session => _session;
        private float _pipFloatSpeed;
        private float _pipMinY;
        private float _pipMaxY;
        private int _maximumVisiblePips = 1;

        public void Configure(float floatSpeed, float minimumY, float maximumY, int maximumVisiblePips)
        {
            if (maximumVisiblePips <= 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(maximumVisiblePips));
            }
            _pipFloatSpeed = floatSpeed;
            _pipMinY = minimumY;
            _pipMaxY = maximumY;
            _maximumVisiblePips = maximumVisiblePips;
            for (var i = 0; i < _items.Count; i++) _items[i].Configure(_pipFloatSpeed, _pipMinY, _pipMaxY);
        }

        /// <summary>绑定当前战斗；传入 null 时对称退订并隐藏全部运行时项。</summary>
        public void Bind(BattleSession battleSession)
        {
            if (_session == battleSession && _items.Count > 0)
            {
                return;
            }

            if (_session != null)
            {
                _session.SnapshotChanged -= OnSnapshotChanged;
            }

            _session = battleSession;
            _tipRefreshCount = 0;
            _lastWeaknessCount = -1;

            if (_session == null)
            {
                HideItems();
                return;
            }

            _session.SnapshotChanged += OnSnapshotChanged;
            EnsureItems(Mathf.Min(_session.Snapshot.Weaknesses.Count, _maximumVisiblePips));
            Refresh(_session.Snapshot);
        }

        private void OnSnapshotChanged(BattleSnapshot snapshot)
        {
            if (snapshot.Weaknesses.Count != _lastWeaknessCount)
            {
                Refresh(snapshot);
            }
        }

        private void Refresh(BattleSnapshot snapshot)
        {
            if (_session == null)
            {
                return;
            }

            var visibleCount = Mathf.Min(snapshot.Weaknesses.Count, Mathf.Min(_items.Count, _maximumVisiblePips));
            var tipTheme = _session.Kind == BattleKind.Boss1 ? 1 : 2;
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                item.gameObject.SetActive(i < visibleCount);
                item.HideTip();
                if (i >= visibleCount)
                {
                    continue;
                }

                item.SetHpItem(snapshot.Weaknesses[i], i);
                if (_tipRefreshCount < 3 && i == 0)
                {
                    item.ShowTip(tipTheme);
                }
            }

            _tipRefreshCount++;
            _lastWeaknessCount = snapshot.Weaknesses.Count;
        }

        private void EnsureItems(int count)
        {
            if (hpItem == null)
            {
                throw new MissingReferenceException("Boss HP item prefab is required.");
            }

            while (_items.Count < count)
            {
                var instance = Instantiate(hpItem, transform);
                var item = instance.GetComponent<BossWeaknessPipView>();
                if (item == null)
                {
                    throw new MissingComponentException("Boss HP item requires BossWeaknessPipView.");
                }

                _items.Add(item);
                item.Configure(_pipFloatSpeed, _pipMinY, _pipMaxY);
                item.gameObject.SetActive(false);
            }
        }

        private void HideItems()
        {
            for (var i = 0; i < _items.Count; i++)
            {
                _items[i].HideTip();
                _items[i].gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_session != null)
            {
                _session.SnapshotChanged -= OnSnapshotChanged;
            }
        }
    }
}
