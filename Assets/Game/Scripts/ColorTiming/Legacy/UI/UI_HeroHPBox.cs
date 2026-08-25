using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_HeroHPBox : MonoBehaviour
{
    const float ItemSpacing = 35f;
    const float AlternateRowOffset = -33f;

    public HeroController controller;

    public GameObject hpItem;

    private void Awake()
    {
    }

    public void Bind(HeroController heroController)
    {
        if (controller == heroController && items.Count > 0) return;
        controller?.OnSetHP_Event.RemoveListener(OnDamage);
        controller = heroController;
        controller?.OnSetHP_Event.AddListener(OnDamage);
        EnsureItems(controller != null ? controller.heroMaxHP : 5);
        SetHP();
    }

    private void Start()
    {
        SetHP();
    }

    private void OnDamage()
    {
        SetHP();
    }

    void SetHP()
    {
        if (controller == null) return;
        for (int i = 0; i < items.Count; i++)
        {
            var active = i < controller.heroHP;
            items[i].gameObject.SetActive(active);
            items[i].SetHeroHPItem(i, ItemSpacing, AlternateRowOffset);
        }
    }

    readonly List<UI_HeroHPItem> items = new List<UI_HeroHPItem>(5);

    void EnsureItems(int count)
    {
        if (hpItem == null) throw new MissingReferenceException("Hero HP item prefab is required.");
        while (items.Count < count)
        {
            var instance = Instantiate(hpItem, transform);
            var item = instance.GetComponent<UI_HeroHPItem>();
            if (item == null) throw new MissingComponentException("Hero HP item requires UI_HeroHPItem.");
            items.Add(item);
            item.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        controller?.OnSetHP_Event.RemoveListener(OnDamage);
    }

}
