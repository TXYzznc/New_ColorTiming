// 文件职责：定义 Transient实体Service 的依赖契约，供模块间解耦使用。
// 所属模块：ColorTiming / Presentation / Entities。

using System;
using UnityEngine;

namespace ColorTiming.Presentation.Entities
{
    public interface ITransientEntityService
    {
        // 执行生成对应的主要流程。
        int Spawn(
            string prefabName,
            Vector3 position,
            Quaternion rotation,
            Transform parent,
            Action<GameObject> configure);

        // 释放All及其临时资源。
        void ReleaseAll();
    }

    public interface ITransientEntityConsumer
    {
        // 绑定TransientEntities依赖或事件监听。
        void BindTransientEntities(ITransientEntityService entities);
    }

    public interface IFrameworkEntityParticipant
    {
        // 绑定FrameworkRelease依赖或事件监听。
        void BindFrameworkRelease(Action release);
        // 响应Framework实体Spawned回调，并更新本对象状态。
        void OnFrameworkEntitySpawned();
        // 响应Framework实体Despawned回调，并更新本对象状态。
        void OnFrameworkEntityDespawned();
    }
}
