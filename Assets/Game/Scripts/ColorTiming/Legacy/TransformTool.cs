using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformTool
{

    public static void ClearChild(Transform obj)
    {
        for (int i = 0; i < obj.childCount; i++)
        {
            Object.Destroy(obj.GetChild(i).gameObject);
        }
    }

}
