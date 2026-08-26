using System;
using UnityEngine;

/// <summary>Normalizes names of runtime-created or pooled Unity objects.</summary>
public static class RuntimeObjectNaming
{
    private const string CloneSuffix = "(Clone)";

    public static void EnsureCloneSuffix(GameObject gameObject)
    {
        if (gameObject == null || gameObject.name.EndsWith(CloneSuffix, StringComparison.Ordinal))
        {
            return;
        }

        gameObject.name += CloneSuffix;
    }
}
