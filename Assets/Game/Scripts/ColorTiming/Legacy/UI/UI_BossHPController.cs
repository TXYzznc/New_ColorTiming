using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_BossHPController : MonoBehaviour
{
    //获取Boss 血量进行初始化
    public GameObject HPItem;
    public Boss1_Controller boss1_Controller;

    private void Awake()
    {
        boss1_Controller?.OnDamage_Event.AddListener(SetHPBox);
        EnsureItems(7);
    }

    int cCount;
    void SetHPBox()
    {
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

            if(cCount < 3 && !(i > 0))
            {
                uiItem?.ShowTip(1);
            }
            //it?.GetComponent<UI_BossHP_Item>()?.SetHpItem(boss1_Controller.Boss1HP[i],i);
        }

        cCount++;
    }

    readonly List<UI_BossHP_Item> items = new List<UI_BossHP_Item>(7);

    void EnsureItems(int count)
    {
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
