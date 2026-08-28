// 文件职责：定义 StaticUIComponent，承担 Extension 模块中的对应职责。
// 所属模块：Extension。

using UnityEngine;
using UnityGameFramework.Runtime;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class StaticUIComponent : GameFrameworkComponent
{
    [Header("Waiting View:")]
    [SerializeField] GameObject waitingView = null;
    [SerializeField] GameObject joystickView = null;
    public GameObject JoystickView => joystickView;

    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
        UpdateCanvasScaler();
        if (waitingView != null)
        {
            waitingView.SetActive(false);
        }

        if (joystickView == null)
        {
            var joystickTransform = transform.Find("Joystick Base");
            joystickView = joystickTransform != null ? joystickTransform.gameObject : null;
        }

        if (joystickView != null)
        {
            joystickView.SetActive(false);
        }
    }

    // 根据当前状态更新CanvasScaler。
    public void UpdateCanvasScaler()
    {
        var uiRootCanvas = GFBuiltin.RootCanvas;
        var canvasRoot = this.GetComponent<Canvas>();
        canvasRoot.worldCamera = uiRootCanvas.worldCamera;
        canvasRoot.planeDistance = uiRootCanvas.planeDistance;
        canvasRoot.sortingLayerID = uiRootCanvas.sortingLayerID;
        canvasRoot.sortingOrder = uiRootCanvas.sortingOrder;

        var canvasScaler = this.GetComponent<CanvasScaler>();
        var uiRootScaler = uiRootCanvas.GetComponent<CanvasScaler>();

        canvasScaler.uiScaleMode = uiRootScaler.uiScaleMode;
        canvasScaler.screenMatchMode = uiRootScaler.screenMatchMode;
        canvasScaler.matchWidthOrHeight = uiRootScaler.matchWidthOrHeight;
        canvasScaler.referencePixelsPerUnit = uiRootScaler.referencePixelsPerUnit;
        canvasScaler.referenceResolution = uiRootScaler.referenceResolution;
    }
}
