// 文件职责：定义 Particle实体，承担 实体 模块中的对应职责。
// 所属模块：Entity。

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityGameFramework.Runtime;

public class ParticleEntity : EntityBase
{
    public const string LIFE_TIME = "LifeTime";
    public const string SORT_LAYER = "SortLayer";
    bool autoHide;
    public float LifeTime { get; private set; }
    // 实体显示时读取参数并建立本次生命周期状态。
    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        autoHide = true;

        LifeTime = Params.Get<VarFloat>(LIFE_TIME, 2f);

        autoHide = LifeTime > 0;

        if (Params.TryGet<VarInt32>(SORT_LAYER, out var pSortLayer))
        {
            SetParticlesSortLayer(pSortLayer);
        }

        if (autoHide)
        {
            UniTask.Delay(TimeSpan.FromSeconds(LifeTime)).ContinueWith(() =>
            {
                GF.Entity.HideEntitySafe(this);
            }).Forget();
        }
    }
    // 设置ParticlesSortLayer，并使后续流程使用最新状态。
    private void SetParticlesSortLayer(int layer)
    {
        var particles = GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem item in particles)
        {
            var render = item.GetComponent<Renderer>();
            render.sortingOrder = layer;
        }
    }
}
