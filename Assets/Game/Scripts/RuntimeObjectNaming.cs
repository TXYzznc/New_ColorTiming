// 文件职责：统一运行时创建或复用对象的 Clone 后缀命名规则。
// 所属模块：Scripts 根模块。

using System;
using UnityEngine;

/// <summary>Normalizes names of runtime-created or pooled Unity objects.</summary>
public static class RuntimeObjectNaming
{
    private const string CloneSuffix = "(Clone)";

    // 确保运行时对象名称带有 Clone 后缀。
    public static void EnsureCloneSuffix(GameObject gameObject)
    {
        if (gameObject == null || gameObject.name.EndsWith(CloneSuffix, StringComparison.Ordinal))
        {
            return;
        }

        gameObject.name += CloneSuffix;
    }
}
