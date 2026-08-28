// 文件职责：承载 UI 创建或调用所需参数。
// 所属模块：UI / Core。

using GameFramework;
using UnityGameFramework.Runtime;

public class UIParams : RefParams
{
    public bool? AllowEscapeClose { get; set; } = null;
    public int? SortOrder { get; set; } = null;
    public bool IsSubUIForm { get; set; } = false;
    public GameFrameworkAction<UIFormLogic> OpenCallback { get; set; } = null;
    public GameFrameworkAction<UIFormLogic> CloseCallback { get; set; } = null;
    public GameFrameworkAction<object, string> ButtonClickCallback { get; set; } = null;
    // 创建并初始化新的实例。
    public static UIParams Create(bool? allowEscape = null, int? sortOrder = null)
    {
        var uiParms = ReferencePool.Acquire<UIParams>();
        uiParms.CreateRoot();
        uiParms.AllowEscapeClose = allowEscape;
        uiParms.SortOrder = sortOrder;
        uiParms.IsSubUIForm = false;
        return uiParms;
    }


    // 重置对象属性，使实例可以安全复用。
    protected override void ResetProperties()
    {
        base.ResetProperties();
        AllowEscapeClose = null;
        SortOrder = null;
        OpenCallback = null;
        CloseCallback = null;
        ButtonClickCallback = null;
        IsSubUIForm = false;
    }
}
