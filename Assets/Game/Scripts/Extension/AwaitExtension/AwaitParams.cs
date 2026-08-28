// 文件职责：承载 Await 创建或调用所需参数。
// 所属模块：Extension / AwaitExtension。

using Cysharp.Threading.Tasks;
using GameFramework;

public class AwaitParams<T> : IReference
{
    public object UserData { get; private set; }

    public UniTaskCompletionSource<T> Source { get; private set; }

    // 创建并初始化新的实例。
    public static AwaitParams<T> Create(object userData, UniTaskCompletionSource<T> source)
    {
        AwaitParams<T> awaitDataWrap = ReferencePool.Acquire<AwaitParams<T>>();
        awaitDataWrap.UserData = userData;
        awaitDataWrap.Source = source;
        return awaitDataWrap;
    }

    // 清空当前保存的运行时状态，使对象可安全复用。
    public void Clear()
    {
        UserData = null;
        Source = null;
    }
}