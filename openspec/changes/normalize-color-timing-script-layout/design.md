## Context

ColorTiming 当前同时保留两代目录设计。2026-08-24 的迁移按 `Combat/Player/Bosses/UI/Presentation` 功能目录建立骨架；2026-08-28 的深层重构改用 `Domain/Application/Presentation/Infrastructure/Bootstrap` 单向依赖，但只完整落实了 Domain、Application、Presentation 与 Bootstrap。结果是四个空目录仍被 Git 跟踪，GF/Unity adapter 未进入设计声明的 Infrastructure，且 UI 中新 Form、旧 Form、叶子组件、Presenter、模型与相机辅助代码混放。

约束如下：

- 不修改 `ScriptsBuiltin`、Prefab/Scene 层级、UI RectTransform、美术资产内容和资源 ID。
- 所有脚本移动必须保留 `.meta` 和 GUID。
- `ColorTiming.Domain` 与 `ColorTiming.Application` 的 asmdef 及单向依赖保持不变。
- 当前不增加热更程序集，也不新增第三方依赖。
- MonoBehaviour 命名空间调整必须通过编译、Missing Script 和 Prefab/Scene 回归证明序列化安全。

## Goals / Non-Goals

**Goals:**

- 让物理目录与已确认的分层架构一致。
- 让 UI Form、组件、Presenter、模型和 GF 端口实现可以按职责直接定位。
- 落实 `Infrastructure/GF` 与 `Infrastructure/Unity`，消除 adapter 散落。
- 清理所有旧迁移空目录和笼统 `Views` 目录。
- 统一本次迁移涉及脚本的命名空间，同时保持运行时行为与资源引用不变。

**Non-Goals:**

- 不重新设计战斗、UI 交互、场景流或资源加载行为。
- 不修改 GF UI Core、`UIViews` 生成方式或 `Hotfix.asmdef` 历史名称。
- 不为 Presentation/Infrastructure 新建额外 asmdef。
- 不在本次处理全部遗留技能脚本的类型命名风格；只处理目录和命名空间边界。

## Decisions

### 1. 延续 layer-first，而不是退回 feature-first

采用 `Domain/Application/Presentation/Infrastructure/Bootstrap`。旧的 `ColorTiming/UI`、`Player`、`Bosses` 顶层目录被删除。相比退回功能目录，这能保持纯 C# 程序集边界和依赖测试；相比建立 `Features/<feature>` 垂直切片，当前项目规模不需要复制跨场景基础设施。

### 2. UI 在 Presentation 内按角色分组

目标结构：

```text
Presentation/UI/
├── Contracts/
├── Forms/
├── Components/
├── Presenters/
└── Models/
```

- `Forms`：GF `UIFormBase` 派生类。
- `Components`：挂在 UI Prefab 子节点上的 MonoBehaviour 与局部序列组件。
- `Presenters`：把 Application snapshot 转换为 UI 显示状态的普通 C# 对象。
- `Models`：仅用于表现层的枚举和值状态。
- `Contracts`：UI 服务和消费方的窄接口。

`Assets/Game/Scripts/UI/Core` 继续承载 GF UI 通用基类与生成注册表，不接收 ColorTiming 业务 Form。

### 3. GF 与 Unity adapter 进入 Infrastructure

目标结构：

```text
Infrastructure/
├── GF/
│   ├── UI/
│   ├── Audio/
│   ├── Entity/
│   └── Settings/
└── Unity/
    ├── Input/
    └── Time/
```

GF UI/Sound/Entity/Setting 服务实现与 Unity Input/Time adapter 迁入对应目录。接口、输入帧和业务可读设置契约仍留在上层现有边界；本次不额外抽象无实际替换需求的端口。

### 4. AssetDatabase 移动优先，命名空间随后调整

先记录受影响 `.cs.meta` GUID，再由 Unity AssetDatabase 创建目录和批量移动脚本，使 `.meta` 与脚本一起迁移。移动完成后修改命名空间和引用，最后刷新并编译。禁止通过复制新脚本、删除旧脚本的方式迁移，因为这会产生新 GUID 并破坏 Prefab/Scene 引用。

### 5. 不把所有路径都机械映射为命名空间

本次新目录采用明确命名空间：

- `ColorTiming.Presentation.UI.Forms`
- `ColorTiming.Presentation.UI.Components`
- `ColorTiming.Presentation.UI.Presenters`
- `ColorTiming.Presentation.UI.Models`
- `ColorTiming.Presentation.UI.Contracts`
- `ColorTiming.Infrastructure.GF.*`
- `ColorTiming.Infrastructure.Unity.*`

Domain 现有 `ColorTiming.Combat`、`ColorTiming.Player`、`ColorTiming.Bosses.*` 命名空间由程序集边界表达层级，本次不改，避免无收益的公共纯 C# API 扰动。未被本次移动的遗留全局表现脚本也不顺带改名。

### 6. 删除只限已确认迁移残留

只删除 `ColorTiming/UI`、`ColorTiming/Player`、`ColorTiming/Bosses/Boss1`、`ColorTiming/Bosses/Boss2` 及移动后为空的 `Presentation/UI/Views`、`Input/Adapters`、`Combat`。删除前必须确认目录中无非 `.meta` 文件；不运行面向整个 Assets 的空目录清理。

## Risks / Trade-offs

- [MonoBehaviour 命名空间变化导致 Unity 无法解析脚本] → 保留 GUID，执行全项目 Missing Script、Prefab/Scene 验证和 PlayMode 回归；失败时按移动映射回退命名空间，不恢复旧目录混放。
- [批量移动后代码在中间态暂时无法编译] → 单批完成目录创建和资产移动，随后一次性修改命名空间与引用，避免在中间态进入 Play Mode。
- [GF 生成器继续输出 `UIViews.cs` 到 `UI/Core`] → 保留现状并记录该文件属于生成注册表；本次不修改生成器以控制范围。
- [Infrastructure 与 Presentation 同属 Hotfix，编译器不能物理阻止逆向依赖] → 依靠命名空间、目录审计和现有架构测试；当前不增加程序集数量。
- [遗留全局命名空间仍存在] → 本次只统一被移动的 UI 与 adapter；战斗技能/Actor 类型另行重构，避免扩大序列化风险。

## Migration Plan

1. 记录所有待移动脚本 GUID、引用数量、Git 状态和受保护资产基线。
2. 通过 Unity AssetDatabase 创建目标目录并批量移动脚本。
3. 修改命名空间和编译引用，等待 Domain Reload 完成并修复全部编译错误。
4. 删除限定范围内的空目录及 `.meta`。
5. 运行目录审计、GUID 对照、Missing Script、EditMode、PlayMode 和受保护资产审计。
6. 将验证证据写入本 change；若任一资源合同失败，通过 AssetDatabase 按映射反向移动并恢复命名空间补丁。

## Open Questions

无。目录方向已经由现有深层重构设计和本次用户确认共同确定。
