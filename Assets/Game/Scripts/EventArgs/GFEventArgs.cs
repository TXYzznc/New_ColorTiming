// 文件职责：定义 GF事件Args，承担 事件Args 模块中的对应职责。
// 所属模块：EventArgs。

using GameFramework;
using GameFramework.Event;

public enum GFEventType
{
    ApplicationQuit //游戏退出
}
public class GFEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(GFEventArgs).GetHashCode();
    public override int Id => EventId;
    public GFEventType EventType { get; private set; }
    public object UserData { get; private set; }
    // 清空当前保存的运行时状态，使对象可安全复用。
    public override void Clear()
    {
        UserData = null;
    }
    // 创建并初始化新的实例。
    public static GFEventArgs Create(GFEventType eventType, object userDt = null)
    {
        var instance = ReferencePool.Acquire<GFEventArgs>();
        instance.EventType = eventType;
        instance.UserData = userDt;
        return instance;
    }
}
