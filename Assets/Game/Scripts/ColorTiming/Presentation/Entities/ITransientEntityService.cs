using System;
using UnityEngine;

namespace ColorTiming.Presentation.Entities
{
    public interface ITransientEntityService
    {
        int Spawn(
            string prefabName,
            Vector3 position,
            Quaternion rotation,
            Transform parent,
            Action<GameObject> configure);

        void ReleaseAll();
    }

    public interface ITransientEntityConsumer
    {
        void BindTransientEntities(ITransientEntityService entities);
    }

    public interface IFrameworkEntityParticipant
    {
        void BindFrameworkRelease(Action release);
        void OnFrameworkEntitySpawned();
        void OnFrameworkEntityDespawned();
    }
}
