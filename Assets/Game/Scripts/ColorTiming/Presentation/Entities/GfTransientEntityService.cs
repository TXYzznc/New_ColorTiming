using System;
using System.Collections.Generic;
using System.Linq;
using ColorTiming.Bootstrap;
using ColorTiming.Presentation.Audio;
using UnityEngine;

namespace ColorTiming.Presentation.Entities
{
    /// <summary>
    /// The only ColorTiming boundary that talks directly to GF.Entity.
    /// Existing skill prefabs are loaded by canonical resource id and recycled by the Effect group.
    /// </summary>
    public sealed class GfTransientEntityService : ITransientEntityService
    {
        private readonly HashSet<int> activeEntityIds = new HashSet<int>();
        private readonly IColorTimingSoundService soundService;

        public GfTransientEntityService(IColorTimingSoundService soundService)
        {
            this.soundService = soundService ?? throw new ArgumentNullException(nameof(soundService));
        }

        public int Spawn(
            string prefabName,
            Vector3 position,
            Quaternion rotation,
            Transform parent,
            Action<GameObject> configure)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                throw new ArgumentException("A prefab resource name is required.", nameof(prefabName));
            }
            if (GFBuiltin.Entity == null)
            {
                throw new InvalidOperationException("GF.Entity is unavailable; ColorTiming must start through Launch.");
            }

            var parameters = EntityParams.Create(position, rotation.eulerAngles);
            var entityId = parameters.Id;
            activeEntityIds.Add(entityId);
            parameters.OnShowCallback = logic =>
            {
                if (logic is ColorTimingTransientEntity transientEntity)
                {
                    transientEntity.BindTracking(entityId, id => activeEntityIds.Remove(id));
                }
                if (parent != null)
                {
                    logic.CachedTransform.SetParent(parent, true);
                }

                var behaviours = logic.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var consumer in behaviours.OfType<ITransientEntityConsumer>())
                {
                    consumer.BindTransientEntities(this);
                }
                foreach (var consumer in behaviours.OfType<IColorTimingSoundConsumer>())
                {
                    consumer.BindSoundService(soundService);
                }
                configure?.Invoke(logic.gameObject);
            };

            GFBuiltin.Entity.ShowEntity<ColorTimingTransientEntity>(
                ColorTimingResourceIds.Entity(prefabName),
                Const.EntityGroup.Effect.ToString(),
                entityId,
                parameters);
            return entityId;
        }

        public void ReleaseAll()
        {
            if (GFBuiltin.Entity == null)
            {
                activeEntityIds.Clear();
                return;
            }

            var entityIds = activeEntityIds.ToArray();
            activeEntityIds.Clear();
            foreach (var entityId in entityIds)
            {
                var logic = GFBuiltin.Entity.GetEntity<ColorTimingTransientEntity>(entityId);
                if (logic != null && logic.CachedTransform != null)
                {
                    // EntityManager unspawns on its next update. Move scene-parented effects
                    // back under the persistent framework root before the old scene unloads.
                    logic.CachedTransform.SetParent(GFBuiltin.Entity.transform, true);
                }
                GFBuiltin.Entity.HideEntitySafe(entityId);
            }
        }
    }

    public sealed class ColorTimingTransientEntity : EntityBase
    {
        private IFrameworkEntityParticipant[] participants = Array.Empty<IFrameworkEntityParticipant>();
        private Action<int> released;
        private int trackedEntityId;

        internal void BindTracking(int entityId, Action<int> onReleased)
        {
            trackedEntityId = entityId;
            released = onReleased;
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            participants = GetComponentsInChildren<MonoBehaviour>(true)
                .OfType<IFrameworkEntityParticipant>()
                .ToArray();
            foreach (var participant in participants)
            {
                participant.BindFrameworkRelease(Release);
                participant.OnFrameworkEntitySpawned();
            }
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            foreach (var participant in participants)
            {
                participant.OnFrameworkEntityDespawned();
                participant.BindFrameworkRelease(null);
            }
            participants = Array.Empty<IFrameworkEntityParticipant>();
            released?.Invoke(trackedEntityId);
            released = null;
            trackedEntityId = 0;
            base.OnHide(isShutdown, userData);
        }

        private void Release()
        {
            GFBuiltin.Entity.HideEntitySafe(this);
        }
    }
}
