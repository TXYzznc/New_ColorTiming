# 摄像机视频录制

入口：`Tools > Unity开发工具箱 > 资源工具 > 摄像机视频录制`。未出现在标签栏时，从工具箱首页或添加工具菜单打开。

1. 进入 Play Mode，等待场景和摄像机就绪。
2. 拖入实际渲染的 Camera，或点击“使用 Main Camera”/“使用当前选中”。Cinemachine 请选其驱动的 Camera。
3. 选择 1080p、2K、4K、竖屏或自定义偶数宽高，再选择 30/60 FPS 和保存目录。
4. 点击“开始录制”，正常操作游戏。面板内可预览采集画面。
5. 点击“停止录制并保存”，再点“定位最近的视频”。输出文件名自动带时间和随机后缀，避免覆盖。

默认输出到项目根目录 `Recordings/`（Git 忽略）。面板默认勾选“包含画面 UI”，会临时把 Screen Space - Overlay Canvas 绑定到录制摄像机近裁剪面前，并自动把这些 Canvas 所在 Layer 加入录制摄像机的剔除掩码；项目原有的 sorting layer / sorting order 保持不变，因此 UI 会像玩家看到的一样叠加在画面最上层。停止时恢复 Canvas 原设置。视频包含 Unity 游戏音频混音，不包含桌面声音或麦克风。

## 采集范围

使用独立摄像机和指定尺寸的 RenderTexture；原摄像机的输出目标、尺寸和场景组件不变。摄像机位置、旋转、镜头和剔除层随源摄像机更新。分辨率改变会改变宽高比和取景范围，并非放大已有屏幕图像。URP render scale 与资源自身质量仍会影响最终细节。

支持 Built-in 和 URP（包括 RenderTexture 路径下的 2D Renderer）。URP 选择 Base Camera，启动时复制其渲染器和后处理设置，Volume 位置跟随源摄像机；修改渲染器配置后请重新开始录制。仅采集这一个摄像机，不含摄像机栈中的其他 Overlay 摄像机。World Space UI 受摄像机剔除层控制。

不复制摄像机上的业务脚本、自定义 OnRenderImage 效果或自定义投影矩阵。以自定义脚本/Renderer Feature 严格绑定原 Camera 的效果，需要单独适配。

固定帧率录制可能使游戏运行变慢，尤其是 4K/60 FPS，但视频以所选帧率播放。音频由官方 Recorder 同步采集。切换工具箱标签继续录制；移除面板、关闭工具箱、退出 Play Mode、脚本重载或退出编辑器会停止并收尾。源摄像机禁用或销毁也会停止。

实现位于 `Assets/Game/Editor/CameraRecording/`，仅 Editor 程序集引用官方 `com.unity.recorder@4.0.3`。不修改工具箱核心，通过 `IToolHubPanel` 和 `ToolHubItem` 自动发现。配置使用 EditorPrefs；不持久修改场景对象，停止后释放临时摄像机、RenderTexture 并恢复录制相关全局设置。

## 验证

在 Test Runner 的 EditMode 中运行 `UnityCameraRecording.Tests.CameraRecordingTests`。集成测试会短暂进入 Play Mode，录制颜色变化和 440 Hz 音频，并检查手动停止、重复停止、退出 Play Mode 的文件与资源清理。输出在 Git 忽略的 `outputs/camera-recorder-tests/`，不作为项目内容资产提交。
