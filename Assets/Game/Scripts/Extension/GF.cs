// 文件职责：定义 GF，承担 Extension 模块中的对应职责。
// 所属模块：Extension。

using GameFramework;
using System;
using UnityEngine;
using UnityGameFramework.Runtime;

public class GF : GFBuiltin
{
    public static DataModelComponent DataModel { get; private set; }
    public static VariablePoolComponent VariablePool { get; private set; }
    public static StaticUIComponent StaticUI { get; private set; }

    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// Initializes framework extension components that have no project-specific
    /// configuration. Safe to call repeatedly from a startup procedure.
    /// </summary>
    // 执行Initialize对应的主要流程。
    public static void Initialize()
    {
        var baseComponent = GFBuiltin.Base ?? GameEntry.GetComponent<BaseComponent>();
        if (baseComponent == null)
        {
            GFTrace.Failure("GF", "Initialize.MissingBaseComponent");
            return;
        }

        DataModel = GameEntry.GetComponent<DataModelComponent>() ?? baseComponent.gameObject.AddComponent<DataModelComponent>();
        VariablePool = GameEntry.GetComponent<VariablePoolComponent>() ?? baseComponent.gameObject.AddComponent<VariablePoolComponent>();
        StaticUI = GameEntry.GetComponent<StaticUIComponent>();
        GFTrace.Success("GF", "Initialize", null, GFTrace.Data("hasDataModel", (DataModel != null).ToString(), "hasStaticUI", (StaticUI != null).ToString(), "hasVariablePool", (VariablePool != null).ToString()));
    }

    // 响应ApplicationQuit回调，并更新本对象状态。
    private void OnApplicationQuit()
    {
        OnExitGame();
    }

    // 响应Application暂停回调，并更新本对象状态。
    private void OnApplicationPause(bool pause)
    {
        if (Application.isMobilePlatform && pause)
        {
            OnExitGame();
        }
    }

    // 获取CanvasSize。
    public Vector2 GetCanvasSize()
    {
        var rect = RootCanvas.GetComponent<RectTransform>();
        return rect.sizeDelta;
    }

    // 执行世界坐标2ScreenPoint对应的主要流程。
    public Vector2 World2ScreenPoint(Camera cam, Vector3 worldPoint)
    {
        var rect = RootCanvas.GetComponent<RectTransform>();
        Vector2 sPoint = cam.WorldToViewportPoint(worldPoint) * rect.sizeDelta;
        return sPoint - rect.sizeDelta * 0.5f;
    }

    // 响应ExitGame回调，并更新本对象状态。
    private void OnExitGame()
    {
        GFTrace.Info("GF", "Application.Exit");
        GF.Event.FireNow(this, GFEventArgs.Create(GFEventType.ApplicationQuit));
        var exitTime = DateTime.UtcNow.ToString();
        GF.Setting.SetString(ConstBuiltin.Setting.QuitAppTime, exitTime);
        GF.Setting.Save();
        UnityGameFramework.Runtime.Log.Info("Application Quit:{0}", exitTime);
    }
}
