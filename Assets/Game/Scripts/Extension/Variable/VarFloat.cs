// 文件职责：定义 VarFloat，承担 Variable 模块中的对应职责。
// 所属模块：Extension / Variable。

using GameFramework;

public sealed class VarFloat : Variable<float>
{
    /// <summary>
    /// 初始化 System.Float 变量类的新实例。
    /// </summary>
    public VarFloat()
    {
    }

    /// <summary>
    /// 从 System.Float 到 System.Float 变量类的隐式转换。
    /// </summary>
    /// <param name="value">值。</param>
    public static implicit operator VarFloat(float value)
    {
        VarFloat varValue = ReferencePool.Acquire<VarFloat>();
        varValue.Value = value;
        return varValue;
    }

    /// <summary>
    /// 从 System.Float 变量类到 System.Float 的隐式转换。
    /// </summary>
    /// <param name="value">值。</param>
    public static implicit operator float(VarFloat value)
    {
        return value.Value;
    }
}
