using UnityEngine;

namespace ColorTiming.Presentation.UI.Components
{
public class PlayerHealthPipView : MonoBehaviour
{

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
