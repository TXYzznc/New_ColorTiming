// 文件职责：定义 实体基类，承担 Core 模块中的对应职责。
// 所属模块：Entity / Core。

using GameFramework;
using UnityGameFramework.Runtime;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(EntityBase), true)]
public class EntityBaseInspector : Editor
{
    // 响应检视面板GUI回调，并更新本对象状态。
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (!EditorApplication.isPlaying) return;

        EditorGUILayout.SelectableLabel($"EntityId: {(target as EntityBase).Id}");
    }
}
#endif
public class EntityBase : EntityLogic
{
    public int Id { get; private set; }
    public EntityParams Params { get; private set; }
    // 在 GF 对象首次初始化时建立持久引用。
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        if (userData == null)
        {
            Log.Error("创建Entity失败! 你必须为Entity传入EntityParams数据");
        }
        Params = userData as EntityParams;
        Id = this.Entity.Id;
    }

    // 实体显示时读取参数并建立本次生命周期状态。
    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        RuntimeObjectNaming.EnsureCloneSuffix(gameObject);
        Id = this.Entity.Id;
        if (userData == null)
        {
            Log.Error("创建Entity失败! 你必须为Entity传入EntityParams数据");
            return;
        }
        Params = userData as EntityParams;
        if (GF.Entity.IsValidEntity(Params.AttchToEntity))
        {
            GF.Entity.AttachEntity(this.Entity, Params.AttchToEntity, Params.ParentTransform);
        }
        if (Params.position != null)
        {
            this.CachedTransform.position = Params.position.Value;
        }
        if (Params.eulerAngles != null)
        {
            this.CachedTransform.eulerAngles = Params.eulerAngles.Value;
        }
        if (Params.localScale != null)
        {
            this.CachedTransform.localScale = Params.localScale.Value;
        }
        if (Params.gameObjectLayer >= 0)
        {
            gameObject.layer = Params.gameObjectLayer;
            //gameObject.SetLayerRecursively(Params.gameObjectLayer);
        }

        Params.OnShowCallback?.Invoke(this);
    }
    // 实体隐藏时清理本次显示产生的运行时状态。
    protected override void OnHide(bool isShutdown, object userData)
    {
        Params.OnHideCallback?.Invoke(this);
        base.OnHide(isShutdown, userData);
        if (!isShutdown && Params != null)
        {
            ReferencePool.Release(Params);
        }
    }
}
