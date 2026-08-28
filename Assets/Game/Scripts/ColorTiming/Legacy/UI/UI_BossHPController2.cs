using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_BossHPController2 : MonoBehaviour
{
    //获取Boss 血量进行初始化
    public GameObject HPItem;
    public Boss2_Controller boss1_Controller;

    int cCount;
    private void Awake()
    {
    }

    public void Bind(Boss2_Controller bossController)
    {
        if (boss1_Controller == bossController && items.Count > 0) return;
        boss1_Controller?.OnDamage_Event.RemoveListener(SetHPBox);
        boss1_Controller = bossController;
        cCount = 0;
        boss1_Controller?.OnDamage_Event.AddListener(SetHPBox);
        if (boss1_Controller == null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].HideTip();
                items[i].gameObject.SetActive(false);
            }
            return;
        }
        EnsureItems(7);
        SetHPBox();
    }

    void SetHPBox()
    {
        if (boss1_Controller == null || boss1_Controller.Boss1HP == null) return;
        //transform.childs
        //清理所有子项

        int hc = boss1_Controller.Boss1HP.Count;
        hc = hc > 7 ? 7: hc;

        for (int i = 0; i < items.Count; i++)
        {
            var uiItem = items[i];
            uiItem.gameObject.SetActive(i < hc);
            uiItem.HideTip();
            if (i >= hc) continue;
            uiItem.SetHpItem(boss1_Controller.Boss1HP[i], i);

            if (cCount < 3 && !(i > 0))
            {
                uiItem?.ShowTip(2);
            }
        }
        cCount++;
    }

    readonly List<UI_BossHP_Item> items = new List<UI_BossHP_Item>(7);

    void EnsureItems(int count)
    {
        if (HPItem == null) throw new MissingReferenceException("Boss HP item prefab is required.");
        while (items.Count < count)
        {
            var instance = Instantiate(HPItem, transform);
            var item = instance.GetComponent<UI_BossHP_Item>();
            if (item == null) throw new MissingComponentException("Boss HP item requires UI_BossHP_Item.");
            items.Add(item);
            item.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        boss1_Controller?.OnDamage_Event.RemoveListener(SetHPBox);
    }
}
