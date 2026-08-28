// 文件职责：负责 玩家生命值Pip 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / UI / Components。

using UnityEngine;

namespace ColorTiming.Presentation.UI.Components
{
public class PlayerHealthPipView : MonoBehaviour
{

    // 设置HeroHP项目，并使后续流程使用最新状态。
    public void SetHeroHPItem(int idx, float spacing, float alternateRowOffset)
    {
        var rectTransform = transform as RectTransform;
        if (rectTransform == null) return;
        rectTransform.anchoredPosition = new Vector2(
            idx * spacing,
            idx % 2 == 0 ? 0f : alternateRowOffset);
    }
}
}
