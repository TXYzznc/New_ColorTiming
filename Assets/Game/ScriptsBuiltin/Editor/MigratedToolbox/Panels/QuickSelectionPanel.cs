using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UGF.EditorTools
{
    /// <summary>
    /// 在 Game 视图中通过鼠标中键选中射线命中的对象。
    /// </summary>
    [InitializeOnLoad]
    [ToolHubItem("常用工具/快速选中", "在 Game 视图中通过鼠标中键快速定位并选中 UI、3D 或 2D 对象", 10)]
    public class QuickSelectionPanel : IToolHubPanel
    {
        private const float MaxRayDistance = 10000f;
        private static readonly Dictionary<int, VisualElement> AttachedGameViewRoots = new Dictionary<int, VisualElement>();
        private static readonly List<RaycastResult> UIRaycastResults = new List<RaycastResult>();

        static QuickSelectionPanel()
        {
            EditorApplication.update += AttachGameViewCallbacks;
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        public void OnDestroy()
        {
        }

        public string GetHelpText()
        {
            return "开启后，在 Game 视图中按鼠标中键，可优先选中 UI，再选中 3D 或 2D Collider 对象。";
        }

        public void OnGUI()
        {
            var settings = EditorToolSettings.Instance;
            EditorGUI.BeginChangeCheck();
            bool enabled = EditorGUILayout.ToggleLeft("开启快速选中", settings.QuickSelectionToolEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(settings, "切换快速选中工具");
                settings.QuickSelectionToolEnabled = enabled;
                EditorUtility.SetDirty(settings);
                EditorToolSettings.Save();
            }

            EditorGUILayout.HelpBox("开启后，在 Game 视图中单击鼠标中键，即可优先选中 UI 射线命中的对象；未命中 UI 时，选中射线最先命中的 3D 或 2D Collider 所在对象。", MessageType.Info);
        }

        private static void AttachGameViewCallbacks()
        {
            if (!EditorToolSettings.Instance.QuickSelectionToolEnabled)
            {
                return;
            }

            var gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            if (gameViewType == null)
            {
                return;
            }

            var gameViews = Resources.FindObjectsOfTypeAll(gameViewType);
            for (int i = 0; i < gameViews.Length; i++)
            {
                var gameView = gameViews[i] as EditorWindow;
                if (gameView == null)
                {
                    continue;
                }

                int instanceId = gameView.GetInstanceID();
                var root = gameView.rootVisualElement;
                if (AttachedGameViewRoots.TryGetValue(instanceId, out var attachedRoot) && attachedRoot == root)
                {
                    continue;
                }

                root.RegisterCallback<PointerDownEvent>(pointerEvent => OnGameViewPointerDown(gameView, pointerEvent));
                AttachedGameViewRoots[instanceId] = root;
            }
        }

        private static void OnGameViewPointerDown(EditorWindow gameView, PointerDownEvent pointerEvent)
        {
            if (!EditorToolSettings.Instance.QuickSelectionToolEnabled || pointerEvent.button != 2)
            {
                return;
            }

            var rootBounds = gameView.rootVisualElement.worldBound;
            if (rootBounds.width <= 0f || rootBounds.height <= 0f)
            {
                return;
            }

            var viewportPosition = new Vector2(
                (pointerEvent.position.x - rootBounds.xMin) / rootBounds.width,
                (pointerEvent.position.y - rootBounds.yMin) / rootBounds.height);
            if (viewportPosition.x < 0f || viewportPosition.x > 1f || viewportPosition.y < 0f || viewportPosition.y > 1f)
            {
                return;
            }

            var camera = GetSelectionCamera();
            if (camera == null)
            {
                return;
            }

            var screenPoint = new Vector3(
                viewportPosition.x * camera.pixelWidth,
                (1f - viewportPosition.y) * camera.pixelHeight,
                0f);
            var selectedObject = GetUIRaycastObject(screenPoint);
            if (selectedObject == null)
            {
                var ray = camera.ScreenPointToRay(screenPoint);
                selectedObject = GetPhysicsRaycastObject(ray);
            }
            if (selectedObject == null)
            {
                return;
            }

            Selection.activeGameObject = selectedObject;
            EditorGUIUtility.PingObject(selectedObject);
            pointerEvent.StopImmediatePropagation();
        }

        private static Camera GetSelectionCamera()
        {
            if (Camera.main != null)
            {
                return Camera.main;
            }

            var cameras = Camera.allCameras;
            Camera selectedCamera = null;
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i].enabled && (selectedCamera == null || cameras[i].depth > selectedCamera.depth))
                {
                    selectedCamera = cameras[i];
                }
            }

            return selectedCamera;
        }

        private static GameObject GetUIRaycastObject(Vector2 screenPoint)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return null;
            }

            UIRaycastResults.Clear();
            var pointerData = new PointerEventData(eventSystem) { position = screenPoint };
            eventSystem.RaycastAll(pointerData, UIRaycastResults);
            for (int i = 0; i < UIRaycastResults.Count; i++)
            {
                if (UIRaycastResults[i].module is GraphicRaycaster)
                {
                    return UIRaycastResults[i].gameObject;
                }
            }

            return null;
        }

        private static GameObject GetPhysicsRaycastObject(Ray ray)
        {
            var hit3D = Physics.Raycast(ray, out var raycastHit, MaxRayDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
            var hit2D = Physics2D.GetRayIntersection(ray, MaxRayDistance, Physics2D.DefaultRaycastLayers);
            if (hit3D && (!hit2D || raycastHit.distance <= hit2D.distance))
            {
                return raycastHit.collider.gameObject;
            }

            return hit2D ? hit2D.collider.gameObject : null;
        }
    }
}
