using UnityEngine;

using System.Collections;

public sealed class ColorTimingBattleHudBootstrap : MonoBehaviour
{
    public GameObject hudPrefab;
    GameObject instance;

    IEnumerator Start()
    {
        var parentCanvas = GetComponentInParent<Canvas>();
        Debug.Log(
            $"[ColorTiming HUD] bootstrap-start scene={UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} " +
            $"active={gameObject.activeInHierarchy} parent={transform.parent?.name ?? "<root>"} " +
            $"parentScale={transform.parent?.lossyScale.ToString() ?? "<none>"} " +
            $"canvas={parentCanvas?.name ?? "<none>"}",
            this);

        if (hudPrefab == null)
        {
            Debug.LogError("ColorTiming battle HUD prefab is not assigned.", this);
            yield break;
        }

        instance = Instantiate(hudPrefab, transform);
        instance.name = hudPrefab.name;
        Debug.Log(
            $"[ColorTiming HUD] instantiated hud={instance.name} active={instance.activeInHierarchy} " +
            $"parent={instance.transform.parent.name} scale={instance.transform.lossyScale}",
            this);

        // Scene actors initialize their runtime health in Start; bind after that setup completes.
        yield return null;

        var hero = FindInScene<HeroController>();
        var boss1 = FindInScene<Boss1_Controller>();
        var boss2 = FindInScene<Boss2_Controller>();
        var heroHp = instance.GetComponentInChildren<UI_HeroHPBox>(true);
        if (heroHp != null) heroHp.Bind(hero);
        var bossHp = instance.GetComponentInChildren<UI_BossHPController>(true);
        var bossHp2 = instance.GetComponentInChildren<UI_BossHPController2>(true);

        var hasBoss1 = boss1 != null;
        var hasBoss2 = boss2 != null;
        if (hasBoss1 == hasBoss2)
        {
            Debug.LogError(
                $"[ColorTiming HUD] bind-failed scene={UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} " +
                $"boss1={hasBoss1} boss2={hasBoss2}. A battle HUD requires exactly one supported boss.", this);
            yield break;
        }

        if (bossHp != null)
        {
            bossHp.enabled = hasBoss1;
            if (hasBoss1) bossHp.Bind(boss1);
        }
        if (bossHp2 != null)
        {
            bossHp2.enabled = hasBoss2;
            if (hasBoss2) bossHp2.Bind(boss2);
        }

        LogHudState(heroHp, bossHp, bossHp2);
    }

    static T FindInScene<T>() where T : Component
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            var result = root.GetComponentInChildren<T>(true);
            if (result != null) return result;
        }
        return null;
    }

    void LogHudState(UI_HeroHPBox heroHp, UI_BossHPController bossHp, UI_BossHPController2 bossHp2)
    {
        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        var heroBoxes = FindObjectsOfType<UI_HeroHPBox>(true);
        var bossControllers = FindObjectsOfType<UI_BossHPController>(true);
        var bossControllers2 = FindObjectsOfType<UI_BossHPController2>(true);
        var heroItems = heroHp == null ? 0 : heroHp.GetComponentsInChildren<UI_HeroHPItem>(true).Length;
        var bossItems = bossHp == null ? 0 : bossHp.GetComponentsInChildren<UI_BossHP_Item>(true).Length;
        var bossItems2 = bossHp2 == null ? 0 : bossHp2.GetComponentsInChildren<UI_BossHP_Item>(true).Length;
        var staticOutsideHud = 0;
        for (var i = 0; i < heroBoxes.Length; i++)
            if (!heroBoxes[i].transform.IsChildOf(instance.transform)) staticOutsideHud++;
        for (var i = 0; i < bossControllers.Length; i++)
            if (!bossControllers[i].transform.IsChildOf(instance.transform)) staticOutsideHud++;
        for (var i = 0; i < bossControllers2.Length; i++)
            if (!bossControllers2[i].transform.IsChildOf(instance.transform)) staticOutsideHud++;

        var heroBoxPath = heroHp == null ? "<none>" : GetPath(heroHp.transform);
        var bossPath = bossHp == null ? "<none>" : GetPath(bossHp.transform);
        var boss2Path = bossHp2 == null ? "<none>" : GetPath(bossHp2.transform);
        var rootRect = instance.GetComponent<RectTransform>();
        var pHpBox = heroHp == null ? null : heroHp.transform;
        var bossBox = bossHp == null ? null : bossHp.transform;
        var boss2Box = bossHp2 == null ? null : bossHp2.transform;

        Debug.Log(
            $"[ColorTiming HUD] state scene={sceneName} hud={instance.name} active={instance.activeInHierarchy} " +
            $"runtimeHud={instance.transform.IsChildOf(transform)} rootType={instance.transform.GetType().Name} " +
            $"rootRect={rootRect != null} rootPath={GetPath(instance.transform)} " +
            $"heroBoxes={heroBoxes.Length} " +
            $"boss1Controllers={bossControllers.Length} boss2Controllers={bossControllers2.Length} " +
            $"staticOutsideHud={staticOutsideHud} heroItems={heroItems} boss1Items={bossItems} boss2Items={bossItems2} " +
            $"heroPath={heroBoxPath} boss1Path={bossPath} boss2Path={boss2Path}",
            this);

        if (pHpBox != null)
        {
            var items = heroHp.GetComponentsInChildren<UI_HeroHPItem>(true);
            Debug.Log(
                $"[ColorTiming HUD] hero scene={sceneName} path={heroBoxPath} childCount={pHpBox.childCount} " +
                $"heroMaxHP={heroHp.controller?.heroMaxHP ?? -1} heroHP={heroHp.controller?.heroHP ?? -1} " +
                $"itemCount={items.Length}", heroHp);
            for (var i = 0; i < items.Length; i++)
            {
                var rect = items[i].transform as RectTransform;
                Debug.Log(
                    $"[ColorTiming HUD] hero-item scene={sceneName} index={i} path={GetPath(items[i].transform)} " +
                    $"active={items[i].gameObject.activeSelf} anchored={(rect == null ? "<null>" : rect.anchoredPosition.ToString())}",
                    items[i]);
            }
        }

        LogBossState(sceneName, "boss1", bossBox, bossHp == null ? null : bossHp.GetComponentsInChildren<UI_BossHP_Item>(true));
        LogBossState(sceneName, "boss2", boss2Box, bossHp2 == null ? null : bossHp2.GetComponentsInChildren<UI_BossHP_Item>(true));
    }

    static void LogBossState(string sceneName, string id, Transform box, UI_BossHP_Item[] items)
    {
        if (box == null) return;
        Debug.Log($"[ColorTiming HUD] {id} path={GetPath(box)} childCount={box.childCount} itemCount={items.Length}", box);
        for (var i = 0; i < items.Length; i++)
            Debug.Log($"[ColorTiming HUD] {id}-item scene={sceneName} index={i} path={GetPath(items[i].transform)} active={items[i].gameObject.activeSelf}", items[i]);
    }

    static string GetPath(Transform current)
    {
        var path = current.name;
        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }
        return path;
    }

    void OnDestroy()
    {
        if (instance != null) Destroy(instance);
    }
}
