// 文件职责：定义 VarVector3Array，承担 Variable 模块中的对应职责。
// 所属模块：Extension / Variable。

using GameFramework;
using UnityEngine;
/// <summary>
/// UnityEngine.Vector3 数组变量类。
/// </summary>
public sealed class VarVector3Array : Variable<Vector3[]>
{
    public VarVector3Array()
    {
    }


    // 执行operatorVarVector3Array对应的主要流程。
    public static implicit operator VarVector3Array(Vector3[] value)
    {
        VarVector3Array varValue = ReferencePool.Acquire<VarVector3Array>();
        varValue.Value = value;
        return varValue;
    }

    // 执行operatorVector3对应的主要流程。
    public static implicit operator Vector3[](VarVector3Array value)
    {
        return value.Value;
    }
}