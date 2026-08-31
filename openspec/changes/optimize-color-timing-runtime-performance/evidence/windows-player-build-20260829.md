# Windows Development Player 构建与冷启动验收（2026-08-29）

## 构建

- 平台：`StandaloneWindows64`
- 模式：Development Build（仅本次构建期间临时启用，`Tools/Jenkins/BuildAppConfig.json` 已恢复）
- 结果：Unity 报告 `Succeeded`，Player 生成于 `BuildApp/StandaloneWindows64/GameDesinger.exe`。

## 资源发布链路

首次构建发现 `ResourceMode.Package` 的 Player 缺少
`StreamingAssets/GameFrameworkVersion.dat`，导致资源初始化 404。项目层
`ColorTimingPackageStreamingAssetsBuildProcessor` 在 Player 构建前将最新 GF
Package 输出同步到 `StreamingAssets`，不修改框架核心。

修复后 Player 内含 10 个资源包，其中包括：

- `GameFrameworkVersion.dat`：177,646 bytes
- `Animation.dat`：16,818,514 bytes
- `ScriptableAssets.dat`：2,131 bytes
- `Scene.dat`：43,123,590 bytes

`Animation.dat` 对应 ResourceCollection 中的基础 Controller 与 21 个武器专用
Controller；`ScriptableAssets.dat` 包含 Hero 动画映射配置。

## 冷启动运行

修复后的 Windows Player 运行 25 秒后：

- 进程保持运行；
- 私有内存约 983.2 MB，工作集约 597.9 MB；
- 日志显示 `Framework preload completed. Entering the generic ready state.`；
- 未发现 `Exception`、`Error`、`404`、资源版本清单无效或资源加载失败。

该测量只证明 Package 发布和冷启动链路，不能替代 Boss1/Boss2 战斗、拾取、武器
切换与场景往返的峰值内存/帧时间验收。
