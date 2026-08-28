# 深层业务重构实施与验证证据

> 日期：2026-08-28
> 变更：`reimplement-color-timing-business-architecture`

## 实施结论

- 已建立普通本地运行时程序集 `ColorTiming.Domain` 与 `ColorTiming.Application`；它们不加入热更新程序集清单。
- 框架既有 `Hotfix.asmdef` 仅继续编译 Unity/GF 表现、基础设施和启动适配；本变更不设计多热更程序集、跨热更 DTO、代理或加载边界。
- `BattleSession` 是玩家、Boss、武器、暂停与终局结果的唯一权威战斗状态。
- Boss1/Boss2 使用 `BattleSceneAnchors` 声明作者化场景引用；`BattleRuntimeContext (Clone)` 在运行时显式组合并确定性释放。
- 已删除全场景扫描绑定、重复结果消费者、旧 `Weapon`／`I_Damage` 兼容层和 `Legacy` 业务目录。
- 原脚本 `.meta` GUID 在语义重命名与目录迁移时保留，AnimationEvent/UnityEvent 公开资源入口保持可调用。

## 自动验证

| 项目 | 结果 |
|---|---|
| Unity 编译 | 0 error |
| 全量 EditMode | 212/212 通过 |
| 全量 PlayMode | 15/15 通过（84.810 秒） |
| Missing Script | Prefab 搜索 0 项 |
| GF.UI 结构验证 | BattleHud、Loading、Battle UI 生命周期验证通过 |
| OpenSpec | `openspec validate reimplement-color-timing-business-architecture --strict` 通过 |
| 框架纯度 | `python tools/audit_framework_purity.py` 通过 |
| 受保护资产 | 3355 资产、3298 原始资产、57 序列化资产、129 AnimationEvent、19 UnityEvent 与基线一致 |

原始结果：[EditMode XML](TestResults/editmode-color-timing-latest.xml)、[PlayMode XML](TestResults/playmode-color-timing-latest.xml)。

完整源功能与操作步骤继续以[源功能验收清单](../../migrate-color-timing-to-ai-friendly-framework/evidence/source-feature-acceptance-checklist.md)为唯一产品验收基线。本文件只记录架构实施和自动化证据，不缩减任何功能 ID。

## 允许的序列化差异

- `Assets/Game/Scene/Boss1.unity`：新增/更新 `BattleSceneAnchors` 显式组合引用。
- `Assets/Game/Scene/Boss2.unity`：新增/更新 `BattleSceneAnchors` 显式组合引用。

以上差异只建立生命周期锚点，不修改关卡视觉节点、纹理、Sprite、材质、Spine、动画、视频、音频或粒子内容。

## 制作人人工验收

自动化通过后仍需在真实运行中逐项确认：

1. Launch → StartMenu → Boss1 → Boss2 → 结果 → 返回菜单及重复进入流程。
2. 菜单视频、BGM、UI 音效、玩家/Boss/环境音的触发、循环、空间感与离场停止。
3. 玩家移动、朝向、普通攻击、Dash 时序/无敌/回血/慢动作、受击、击退、掉武器和死亡手感。
4. 七种武器的拾取、主动/受击丢弃、动画事件、技能生成、命中、FX 和回收。
5. Boss1 十一段弱点、六种攻击、攻击 5 特殊表现与 Boss2 头尾/潜地/十五段进度。
6. HUD、教程、暂停、Loading、结果页在反复打开、切场景和对象池复用后无残留。
7. URP、Spine、材质、字体、视频、粒子、Animator、相机与视差的最终画面无退化。

任何未列入 FIX-001～FIX-004 的可见或可听差异均按缺陷处理，不因架构重构自动接受。
