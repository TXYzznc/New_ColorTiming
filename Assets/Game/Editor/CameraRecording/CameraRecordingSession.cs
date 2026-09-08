using System;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("CameraRecording.Editor.Tests")]

namespace UnityCameraRecording
{
    // One owner per editor: switching toolbox tabs must not interrupt the recording.
    [InitializeOnLoad]
    internal static class CameraRecordingSession
    {
        private static RecorderController _controller;
        private static RecorderControllerSettings _controllerSettings;
        private static MovieRecorderSettings _movieSettings;
        private static Camera _source;
        private static Camera _captureCamera;
        private static RenderTexture _texture;
        private static bool _previousRunInBackground;
        private static float _previousCaptureDeltaTime;
        private static int _previousTargetFrameRate;
        private static double _startTime;
        private static bool _started;
        private static bool _stopping;
        private static readonly System.Collections.Generic.List<(Canvas canvas, RenderMode mode, Camera camera, float planeDistance)> _canvasStates = new();

        internal static bool IsRecording => _started;
        internal static string LastFile { get; private set; }
        internal static string Status { get; private set; } = "等待开始录制";
        internal static RenderTexture Preview => _texture;
        internal static double Duration => _started ? Math.Max(0, Time.unscaledTimeAsDouble - _startTime) : 0;

        static CameraRecordingSession()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
            EditorApplication.quitting += Stop;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        internal static string Validate(Camera camera, int width, int height, int fps, string folder)
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling)
                return "请先进入 Play Mode，并等待编译完成。";
            if (EditorApplication.isPaused)
                return "请先恢复 Play Mode，再开始录制。";
            if (camera == null || !camera.gameObject.scene.IsValid() || !camera.isActiveAndEnabled)
                return "请选择当前场景中启用的 Camera（不是 Cinemachine 虚拟摄像机）。";
            if (GraphicsSettings.currentRenderPipeline != null && !(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
                return "当前工具支持 Built-in 和 URP 渲染管线。";
            if (camera.TryGetComponent<UniversalAdditionalCameraData>(out var data) && data.renderType != CameraRenderType.Base)
                return "请选择 URP Base 摄像机；Overlay 摄像机不能单独录制。";
            int max = Math.Min(4096, SystemInfo.maxTextureSize);
            if (width < 16 || height < 16 || width > max || height > max || (width & 1) != 0 || (height & 1) != 0)
                return $"MP4 宽高必须为 16～{max} 之间的偶数。";
            if (fps != 30 && fps != 60)
                return "帧率必须为 30 或 60 FPS。";
            if (AudioSettings.speakerMode != AudioSpeakerMode.Mono && AudioSettings.speakerMode != AudioSpeakerMode.Stereo)
                return "MP4 声音录制需要在 Audio 设置中使用 Mono 或 Stereo。";
            if (string.IsNullOrWhiteSpace(folder))
                return "请选择保存目录。";
            try { Path.GetFullPath(folder); }
            catch (Exception) { return "保存目录无效。"; }
            return null;
        }

        internal static void Start(Camera source, int width, int height, int fps, string folder, bool flip, bool includeUI)
        {
            if (_controller != null)
                throw new InvalidOperationException("已有录制正在进行。");
            string reason = Validate(source, width, height, fps, folder);
            if (reason != null)
                throw new InvalidOperationException(reason);
            if (Time.captureDeltaTime != 0)
                throw new InvalidOperationException("其他工具正在控制录制帧率，请先停止其他 Recorder。");

            _previousCaptureDeltaTime = Time.captureDeltaTime;
            _previousRunInBackground = Application.runInBackground;
            _previousTargetFrameRate = Application.targetFrameRate;
            LastFile = null;
            try
            {
                Directory.CreateDirectory(Path.GetFullPath(folder));
                string output = Path.Combine(Path.GetFullPath(folder),
                    $"Camera_{DateTime.Now:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid().ToString("N").Substring(0, 6)}_{width}x{height}");
                _source = source;
                if (includeUI && Camera.main != source)
                    throw new InvalidOperationException("包含 UI 时需要选择当前 Game View 使用的 Main Camera，这样视频才能与玩家看到的画面一致。");
                _texture = includeUI ? null : new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
                {
                    name = "Camera Recording Output",
                    hideFlags = HideFlags.HideAndDontSave,
                    antiAliasing = 1,
                    useMipMap = false,
                    autoGenerateMips = false
                };
                if (_texture != null && !_texture.Create())
                    throw new InvalidOperationException("无法分配目标分辨率的 RenderTexture。");

                var captureObject = includeUI ? null : new GameObject("Camera Recording Camera") { hideFlags = HideFlags.HideAndDontSave };
                if (includeUI)
                {
                    _captureCamera = null;
                }
                else
                {
                _captureCamera = captureObject.AddComponent<Camera>();
                // Copy only camera data. Never clone gameplay scripts or AudioListeners.
                if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset)
                {
                    var captureData = captureObject.AddComponent<UniversalAdditionalCameraData>();
                    if (source.TryGetComponent<UniversalAdditionalCameraData>(out var sourceData))
                        EditorUtility.CopySerialized(sourceData, captureData);
                    captureData.renderType = CameraRenderType.Base;
                    captureData.cameraStack?.Clear();
                    captureData.volumeTrigger = sourceData != null && sourceData.volumeTrigger != null
                        ? sourceData.volumeTrigger : source.transform;
                }
                SyncCamera();
                }
                RenderPipelineManager.beginFrameRendering += OnBeginFrameRendering;
                Camera.onPreCull += OnPreCull;

                _controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
                _controllerSettings.hideFlags = HideFlags.HideAndDontSave;
                _controllerSettings.SetRecordModeToManual();
                _controllerSettings.FrameRate = fps;
                _controllerSettings.FrameRatePlayback = FrameRatePlayback.Constant;
                _controllerSettings.CapFrameRate = true;
                _controllerSettings.ExitPlayMode = false;
                _movieSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
                _movieSettings.hideFlags = HideFlags.HideAndDontSave;
                _movieSettings.name = "Toolbox Camera Recording";
                _movieSettings.Enabled = true;
                _movieSettings.EncoderSettings = new CoreEncoderSettings
                {
                    Codec = CoreEncoderSettings.OutputCodec.MP4,
                    EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High
                };
                _movieSettings.ImageInputSettings = includeUI
                    ? new GameViewInputSettings { OutputWidth = width, OutputHeight = height }
                    : new RenderTextureInputSettings { RenderTexture = _texture, FlipFinalOutput = flip };
                _movieSettings.CaptureAlpha = false;
                _movieSettings.CaptureAudio = true;
                _movieSettings.OutputFile = output.Replace('\\', '/');
                _controllerSettings.AddRecorderSettings(_movieSettings);
                _controller = new RecorderController(_controllerSettings);
                _controller.PrepareRecording();
                if (!_controller.StartRecording())
                    throw new InvalidOperationException("Recorder 启动失败，请查看 Console 的具体错误。");
                LastFile = output + ".mp4";
                _started = true;
                _startTime = Time.unscaledTimeAsDouble;
                Status = "录制中；切换工具箱标签不会中断录制。";
                EditorApplication.update += Monitor;
                Debug.Log("[CameraRecording] 开始录制：" + LastFile);
            }
            catch (Exception ex)
            {
                Stop();
                Status = "录制失败：" + ex.Message;
                throw;
            }
        }

        private static void SyncCamera()
        {
            if (_source == null || _captureCamera == null)
                return;
            _captureCamera.CopyFrom(_source);
            // Screen Space - Overlay canvases do not use the player's camera mask.
            // Once temporarily converted to Screen Space - Camera, their layers must be visible.
            int canvasLayers = 0;
            foreach (var state in _canvasStates)
            {
                if (state.canvas != null && state.canvas.isRootCanvas && state.canvas.gameObject.activeInHierarchy)
                    canvasLayers |= 1 << state.canvas.gameObject.layer;
            }
            _captureCamera.cullingMask |= canvasLayers;
            _captureCamera.transform.SetPositionAndRotation(_source.transform.position, _source.transform.rotation);
            _captureCamera.targetTexture = _texture;
            _captureCamera.rect = new Rect(0, 0, 1, 1);
            _captureCamera.aspect = (float)_texture.width / _texture.height;
            _captureCamera.ResetProjectionMatrix();
            _captureCamera.allowDynamicResolution = false;
            _captureCamera.stereoTargetEye = StereoTargetEyeMask.None;
            _captureCamera.enabled = true;
        }

        private static void OnBeginFrameRendering(ScriptableRenderContext context, Camera[] cameras) => SyncCamera();
        private static void OnPreCull(Camera camera)
        {
            if (camera == _captureCamera && GraphicsSettings.currentRenderPipeline == null)
                SyncCamera();
        }

        private static void Monitor()
        {
            if (_stopping || !_started) return;
            if (_source == null || !_source.isActiveAndEnabled || (!_movieSettings.ImageInputSettings.GetType().Name.Contains("GameView") && _captureCamera == null))
            {
                Stop();
                Status = "摄像机已失效，录制已停止。";
            }
            else if (!_controller.IsRecording())
            {
                Stop();
                Status = "Recorder 已结束录制，请检查输出文件和 Console。";
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
                Stop();
        }

        internal static void Stop()
        {
            if (_stopping || (_controller == null && _texture == null && _captureCamera == null)) return;
            _stopping = true;
            _started = false;
            EditorApplication.update -= Monitor;
            RenderPipelineManager.beginFrameRendering -= OnBeginFrameRendering;
            Camera.onPreCull -= OnPreCull;
            try
            {
                _controller?.StopRecording();
                Status = LastFile == null ? "录制已取消。" : "录制完成：" + LastFile;
            }
            catch (Exception ex)
            {
                Status = "视频收尾失败，请检查文件：" + ex.Message;
                Debug.LogException(ex);
            }
            finally
            {
                _controller = null;
                if (_captureCamera != null) Object.DestroyImmediate(_captureCamera.gameObject);
                _captureCamera = null;
                _source = null;
                _canvasStates.Clear();
                if (_texture != null)
                {
                    _texture.Release();
                    Object.DestroyImmediate(_texture);
                }
                _texture = null;
                if (_controllerSettings != null) Object.DestroyImmediate(_controllerSettings);
                if (_movieSettings != null) Object.DestroyImmediate(_movieSettings);
                _controllerSettings = null;
                _movieSettings = null;
                Time.captureDeltaTime = _previousCaptureDeltaTime;
                Application.runInBackground = _previousRunInBackground;
                Application.targetFrameRate = _previousTargetFrameRate;
                _stopping = false;
            }
        }
    }
}
