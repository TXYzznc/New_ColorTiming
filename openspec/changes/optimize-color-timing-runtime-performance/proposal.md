# 变更：逐项优化 ColorTiming 运行时性能

## 背景

运行时采样显示，Boss2 场景的纹理内存明显偏高；StartMenu 视频输出纹理在 GF.UI 表单关闭后仍随池化表单保留；长音频仍采用整段解压方式；部分界面和玩法脚本存在可避免的逐帧工作。

本变更按风险和收益逐项处理，不批量改写资源，也不改变既有玩法、美术内容和界面表现。

## 已确认边界

- 目标平台为 Windows，目标显示基准为 1920×1080，运行目标为稳定 60 FPS。
- 优先平衡画质与性能；Boss 场景纹理内存阶段目标约为 1 GB 以内，最终以实测和视觉验收为准。
- AI 可直接修改代码、场景引用以及可逆的 Unity Importer 设置。
- 每一项均记录资源或文件路径、修改前后参数、原因、预期收益、风险、验收和回退方式。
- 用户负责画面与声音的最终主观验收。
- 不修改源图片像素，不覆盖或重新编码原始美术与音频文件，不进行破坏式资源处理。
- 各优化项依次实施并分别验收，前一项失败不得通过降低既有视觉或功能要求掩盖问题。

## 初步顺序

1. 修正 MainMenu 视频 RenderTexture 生命周期。
2. 调整长音频加载方式，并将 Boss 场景静态 AudioSource 纳入 GF 音频生命周期。
3. 逐组优化 Boss 高内存纹理的 Windows Importer 覆盖参数。
4. 清理已确认的 UI 与玩法逐帧冗余工作。

## 影响范围

- ColorTiming MainMenu 视频表现层。
- Boss1、Boss2 音频资源与场景音频生命周期。
- Boss1、Boss2 纹理的 Windows 平台导入设置。
- 少量已确认的业务 Update/FixedUpdate 热路径。
- 性能采样、视觉验收、听感验收及回退记录。

## Hero 动画常驻内存治理（A+B）

Hero 当前单一 AnimatorController 直接引用所有武器逐帧动画，是 Boss 场景在未使用多数武器时仍保留大量纹理的结构性风险。本变更采用两阶段改造：先新增基础动作与武器专用的运行时 Controller／Prefab 组合，并在武器即将生成时后台预热；验证行为与资源事件等价后，再直接精简原 Hero AnimatorController 的依赖。原始贴图、Animation Clip、Spine 数据及 Animation／Spine／UnityEvent 的名称和时机不改变。

不引入 Addressables。项目继续使用既有 GF Resource／AssetBundle 体系，资源加载、取消和释放必须由明确拥有者管理。最终验收以 Windows Development Player、1920×1080 为准：Boss 战稳定 60 FPS，武器生成、拾取和首次攻击不发生可感知同步加载，Boss2 峰值纹理内存目标不高于 1 GB。
