using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FunctionLibrary
{
    public static List<T> RandomSort<T>(List<T> list)
    {
        var random = new System.Random();
        var newList = new List<T>();
        foreach (var item in list)
        {
            newList.Insert(random.Next(newList.Count), item);
        }
        return newList;
    }

    public static Vector2 GetPositionOnCircle(float angleInDegrees, float radius)
    {
        // 将角度转换为弧度
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        // 计算x和y坐标
        float x = Mathf.Cos(angleInRadians) * radius;
        float y = Mathf.Sin(angleInRadians) * radius;
        return new Vector2(x, y);
    }
}
