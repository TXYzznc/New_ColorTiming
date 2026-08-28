// 文件职责：提供 实体 相关的通用扩展方法。
// 所属模块：Extension。

using GameFramework;
using UnityGameFramework.Runtime;

public static class EntityExtension
{
    public static int ShowEntity(
        this EntityComponent entityComponent,
        string prefabName,
        string logicTypeName,
        string groupName,
        int priority,
        int entityId,
        object userData = null)
    {
        string assetName = UtilityBuiltin.AssetsPath.GetEntityPath(prefabName);
        entityComponent.ShowEntity(
            entityId,
            Utility.Assembly.GetType(logicTypeName),
            assetName,
            groupName,
            priority,
            userData);
        return entityId;
    }

    // 显示实体并同步当前数据。
    public static int ShowEntity(
        this EntityComponent entityComponent,
        string prefabName,
        string logicTypeName,
        string groupName,
        int entityId,
        object userData = null)
    {
        return entityComponent.ShowEntity(prefabName, logicTypeName, groupName, 0, entityId, userData);
    }

    public static int ShowEntity<T>(
        this EntityComponent entityComponent,
        string prefabName,
        string groupName,
        int priority,
        int entityId,
        object userData = null)
        where T : EntityLogic
    {
        string assetName = UtilityBuiltin.AssetsPath.GetEntityPath(prefabName);
        entityComponent.ShowEntity<T>(entityId, assetName, groupName, priority, userData);
        return entityId;
    }

    public static int ShowEntity<T>(
        this EntityComponent entityComponent,
        string prefabName,
        string groupName,
        int entityId,
        object userData = null)
        where T : EntityLogic
    {
        return entityComponent.ShowEntity<T>(prefabName, groupName, 0, entityId, userData);
    }

    // 隐藏分组并停止相关交互。
    public static void HideGroup(this EntityComponent entityComponent, string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            Log.Warning("Entity group name is empty.");
            return;
        }

        var group = entityComponent.GetEntityGroup(groupName);
        if (group == null)
        {
            return;
        }

        foreach (Entity entity in group.GetAllEntities())
        {
            entityComponent.HideEntity(entity);
        }
    }

    // 隐藏实体Safe并停止相关交互。
    public static void HideEntitySafe(this EntityComponent entityComponent, int entityId)
    {
        if (entityComponent.IsLoadingEntity(entityId))
        {
            GF.VariablePool.ClearVariables(entityId);
            entityComponent.HideEntity(entityId);
            return;
        }

        if (entityComponent.HasEntity(entityId))
        {
            entityComponent.HideEntity(entityId);
        }
    }

    // 隐藏实体Safe并停止相关交互。
    public static void HideEntitySafe(this EntityComponent entityComponent, EntityLogic logic)
    {
        if (logic != null && logic.Available)
        {
            entityComponent.HideEntity(logic.Entity);
        }
    }

    public static T GetEntity<T>(this EntityComponent entityComponent, int entityId)
        where T : EntityLogic
    {
        return entityComponent.HasEntity(entityId)
            ? entityComponent.GetEntity(entityId).Logic as T
            : null;
    }
}
