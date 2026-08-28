// 文件职责：提供 UnityUI 相关的通用扩展方法。
// 所属模块：Extension。

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides small value-type helpers used by reusable UI animation tooling.
/// </summary>
public static class UnityUIExtension
{
    // 设置Anchored位置X，并使后续流程使用最新状态。
    public static void SetAnchoredPositionX(this RectTransform rectTransform, float value)
    {
        Vector2 position = rectTransform.anchoredPosition;
        position.x = value;
        rectTransform.anchoredPosition = position;
    }

    // 设置Anchored位置Y，并使后续流程使用最新状态。
    public static void SetAnchoredPositionY(this RectTransform rectTransform, float value)
    {
        Vector2 position = rectTransform.anchoredPosition;
        position.y = value;
        rectTransform.anchoredPosition = position;
    }

    // 设置Anchored位置3DZ，并使后续流程使用最新状态。
    public static void SetAnchoredPosition3DZ(this RectTransform rectTransform, float value)
    {
        Vector3 position = rectTransform.anchoredPosition3D;
        position.z = value;
        rectTransform.anchoredPosition3D = position;
    }

    // 设置颜色Alpha，并使后续流程使用最新状态。
    public static void SetColorAlpha(this Graphic graphic, float value)
    {
        Color color = graphic.color;
        color.a = value;
        graphic.color = color;
    }

    // 设置FlexibleSize，并使后续流程使用最新状态。
    public static void SetFlexibleSize(this LayoutElement layoutElement, Vector2 value)
    {
        layoutElement.flexibleWidth = value.x;
        layoutElement.flexibleHeight = value.y;
    }

    // 获取FlexibleSize。
    public static Vector2 GetFlexibleSize(this LayoutElement layoutElement)
    {
        return new Vector2(layoutElement.flexibleWidth, layoutElement.flexibleHeight);
    }

    // 设置MinSize，并使后续流程使用最新状态。
    public static void SetMinSize(this LayoutElement layoutElement, Vector2 value)
    {
        layoutElement.minWidth = value.x;
        layoutElement.minHeight = value.y;
    }

    // 获取MinSize。
    public static Vector2 GetMinSize(this LayoutElement layoutElement)
    {
        return new Vector2(layoutElement.minWidth, layoutElement.minHeight);
    }

    // 设置PreferredSize，并使后续流程使用最新状态。
    public static void SetPreferredSize(this LayoutElement layoutElement, Vector2 value)
    {
        layoutElement.preferredWidth = value.x;
        layoutElement.preferredHeight = value.y;
    }

    // 获取PreferredSize。
    public static Vector2 GetPreferredSize(this LayoutElement layoutElement)
    {
        return new Vector2(layoutElement.preferredWidth, layoutElement.preferredHeight);
    }
}
