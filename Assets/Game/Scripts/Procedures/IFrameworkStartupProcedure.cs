// 文件职责：定义 FrameworkStartupProcedure 的依赖契约，供模块间解耦使用。
// 所属模块：Procedures。

/// <summary>
/// Marks the procedure that should begin after the generic framework preload has completed.
/// A project or an optional package may register at most one such procedure in AppConfigs.
/// </summary>
public interface IFrameworkStartupProcedure
{
}
