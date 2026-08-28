// 文件职责：定义 ColorTimingUrp相机Stack，承担 相机 模块中的对应职责。
// 所属模块：ColorTiming / Presentation / Camera。

using System.Linq;
using ColorTiming.Bootstrap.Flow;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ColorTiming.Presentation.Camera
{
    /// <summary>
    /// Composes the framework UI camera over the active ColorTiming gameplay camera.
    /// URP does not composite two independent Base cameras like the Built-in pipeline.
    /// </summary>
    internal static class ColorTimingUrpCameraStack
    {
        // 执行Configure对应的主要流程。
        public static void Configure(Scene scene, ColorTimingSceneId sceneId)
        {
            var uiCamera = GFBuiltin.UICamera;
            if (uiCamera == null || !scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            var gameplayCamera = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<UnityEngine.Camera>(true))
                .FirstOrDefault(camera => camera != null && camera.enabled && camera.CompareTag("MainCamera"));
            if (gameplayCamera == null)
            {
                return;
            }

            var gameplayData = gameplayCamera.GetUniversalAdditionalCameraData();
            var uiData = uiCamera.GetUniversalAdditionalCameraData();
            if (sceneId == ColorTimingSceneId.StartMenu)
            {
                gameplayCamera.backgroundColor = Color.black;
            }
            gameplayData.renderType = CameraRenderType.Base;
            uiData.renderType = CameraRenderType.Overlay;

            gameplayData.cameraStack.RemoveAll(camera => camera == null || camera == uiCamera);
            gameplayData.cameraStack.Add(uiCamera);
        }

        // 恢复组件的默认配置或初始运行状态。
        public static void Reset()
        {
            var uiCamera = GFBuiltin.UICamera;
            if (uiCamera == null)
            {
                return;
            }

            uiCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Base;
        }
    }
}
