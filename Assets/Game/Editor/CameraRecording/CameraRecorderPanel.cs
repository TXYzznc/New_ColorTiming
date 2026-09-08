using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityCameraRecording
{
    [ToolHubItem("资源工具/摄像机视频录制", "指定摄像机高分辨率 MP4 录制，包含游戏声音", 11)]
    public sealed class CameraRecorderPanel : IToolHubPanel
    {
        private const string PrefKey = "Toolbox.CameraRecorder.";
        private static readonly string[] Presets = { "1080p（1920 × 1080）", "2K（2560 × 1440）", "4K（3840 × 2160）", "竖屏（1080 × 1920）", "自定义" };
        private static readonly Vector2Int[] Sizes = { new Vector2Int(1920, 1080), new Vector2Int(2560, 1440), new Vector2Int(3840, 2160), new Vector2Int(1080, 1920) };
        private static readonly string[] FrameRates = { "30 FPS", "60 FPS" };
        private Camera _camera;
        private int _preset;
        private int _width = 1920;
        private int _height = 1080;
        private int _fps = 30;
        private string _folder;
        private bool _flip;
        private bool _includeUI = true;
        private Vector2 _scroll;
        private double _nextRepaint;

        public void OnEnable()
        {
            _width = EditorPrefs.GetInt(PrefKey + "Width", 1920);
            _height = EditorPrefs.GetInt(PrefKey + "Height", 1080);
            _fps = EditorPrefs.GetInt(PrefKey + "FPS", 30);
            _folder = EditorPrefs.GetString(PrefKey + "Folder." + Application.dataPath,
                Path.GetFullPath(Path.Combine(Application.dataPath, "../Recordings")));
            _flip = EditorPrefs.GetBool(PrefKey + "Flip", false);
            _preset = Array.FindIndex(Sizes, s => s.x == _width && s.y == _height);
            if (_preset < 0) _preset = 4;
            EditorApplication.update -= Refresh;
            EditorApplication.update += Refresh;
        }

        public void OnDisable() { SavePreferences(); EditorApplication.update -= Refresh; }
        public void OnDestroy() { OnDisable(); CameraRecordingSession.Stop(); }
        public string GetHelpText() => "Play Mode 中选择 Camera，设置分辨率和帧率，开始录制。停止后生成带游戏声音的 MP4。";

        private void Refresh()
        {
            if (!CameraRecordingSession.IsRecording || EditorApplication.timeSinceStartup < _nextRepaint) return;
            _nextRepaint = EditorApplication.timeSinceStartup + 0.1;
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        private void SavePreferences()
        {
            EditorPrefs.SetInt(PrefKey + "Width", _width);
            EditorPrefs.SetInt(PrefKey + "Height", _height);
            EditorPrefs.SetInt(PrefKey + "FPS", _fps);
            EditorPrefs.SetBool(PrefKey + "Flip", _flip);
            if (!string.IsNullOrEmpty(_folder)) EditorPrefs.SetString(PrefKey + "Folder." + Application.dataPath, _folder);
        }

        public void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("摄像机视频录制", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("按目标分辨率独立渲染摄像机画面，输出高质量 MP4 + 游戏声音。仅 Play Mode 可用，不包含 Screen Space - Overlay UI 或其他叠加摄像机。", MessageType.Info);
            using (new EditorGUI.DisabledScope(CameraRecordingSession.IsRecording))
            {
                _camera = (Camera)EditorGUILayout.ObjectField("录制摄像机", _camera, typeof(Camera), true);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("使用 Main Camera")) _camera = Camera.main;
                    if (GUILayout.Button("使用当前选中")) _camera = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<Camera>() : null;
                }
                int preset = EditorGUILayout.Popup("分辨率", _preset, Presets);
                if (preset != _preset)
                {
                    _preset = preset;
                    if (preset < Sizes.Length) { _width = Sizes[preset].x; _height = Sizes[preset].y; }
                }
                using (new EditorGUI.DisabledScope(_preset != 4))
                {
                    _width = EditorGUILayout.IntField("宽度", _width);
                    _height = EditorGUILayout.IntField("高度", _height);
                }
                _fps = EditorGUILayout.Popup("帧率", _fps == 60 ? 1 : 0, FrameRates) == 0 ? 30 : 60;
                _flip = EditorGUILayout.Toggle("垂直翻转输出", _flip);
                _includeUI = EditorGUILayout.Toggle("包含画面 UI", _includeUI);
                using (new EditorGUILayout.HorizontalScope())
                {
                    _folder = EditorGUILayout.TextField("保存目录", _folder);
                    if (GUILayout.Button("选择…", GUILayout.Width(65)))
                    {
                        string path = EditorUtility.OpenFolderPanel("选择视频保存目录", _folder, "");
                        if (!string.IsNullOrEmpty(path)) _folder = path;
                    }
                }
            }
            EditorGUILayout.HelpBox("录制采用固定帧率。4K / 60 FPS 会增加渲染和编码开销，运行时可能变慢。声音来自 Unity 的游戏音频混音，不录麦克风。", MessageType.None);
            if (CameraRecordingSession.IsRecording)
            {
                EditorGUILayout.LabelField("视频时长", TimeSpan.FromSeconds(CameraRecordingSession.Duration).ToString(@"hh\:mm\:ss"));
                if (GUILayout.Button("停止录制并保存", GUILayout.Height(34))) CameraRecordingSession.Stop();
                var texture = CameraRecordingSession.Preview;
                if (texture != null)
                {
                    Rect rect = GUILayoutUtility.GetRect(160, 220, GUILayout.ExpandWidth(true));
                    EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
                }
            }
            else
            {
                string reason = CameraRecordingSession.Validate(_camera, _width, _height, _fps, _folder);
                if (reason != null) EditorGUILayout.HelpBox(reason, MessageType.Warning);
                using (new EditorGUI.DisabledScope(reason != null))
                {
                    if (GUILayout.Button("开始录制", GUILayout.Height(34)))
                    {
                        SavePreferences();
                        try { CameraRecordingSession.Start(_camera, _width, _height, _fps, _folder, _flip, _includeUI); }
                        catch (Exception ex) { Debug.LogException(ex); }
                    }
                }
            }
            EditorGUILayout.HelpBox(CameraRecordingSession.Status, MessageType.Info);
            using (new EditorGUI.DisabledScope(CameraRecordingSession.IsRecording || !File.Exists(CameraRecordingSession.LastFile)))
                if (GUILayout.Button("定位最近的视频")) EditorUtility.RevealInFinder(CameraRecordingSession.LastFile);
            EditorGUILayout.EndScrollView();
        }
    }
}
