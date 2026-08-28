// 文件职责：定义 UI项目Object，承担 Core 模块中的对应职责。
// 所属模块：UI / Core。

using GameFramework;
using GameFramework.ObjectPool;
using UnityEngine;

public class UIItemObject : ObjectBase
{
#pragma warning disable IDE1006 // 命名样式
    public GameObject gameObject { get; private set; }
    public UIItemBase itemLogic { get; private set; }
#pragma warning restore IDE1006 // 命名样式
    public static T Create<T>(GameObject itemInstance) where T : UIItemObject, new()
    {
        var instance = ReferencePool.Acquire<T>();
        instance.Initialize(itemInstance);
        instance.gameObject = itemInstance;
        instance.itemLogic = itemInstance.GetComponent<UIItemBase>();
        instance.OnInit();
        return instance;
    }
    // 释放当前对象及其持有的临时资源。
    protected override void Release(bool isShutdown)
    {
        if (gameObject == null)
        {
            return;
        }
        Object.Destroy(gameObject);
    }

    // 在 GF 对象首次初始化时建立持久引用。
    protected virtual void OnInit() { }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        var transform = gameObject.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        gameObject.SetActive(true);
    }
    // 响应Unspawn回调，并更新本对象状态。
    protected override void OnUnspawn()
    {
        base.OnUnspawn();
        gameObject.SetActive(false);
    }
}
