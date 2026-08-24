# 废弃候选与移除证据

本表只列出“候选”。只有在目标迁移完成后的全量引用复核仍为零，且功能追踪矩阵没有依赖时，才可真正不迁移或删除。

证据来源：`Baseline/source-scripts.csv`、场景/Prefab/Asset GUID 全文扫描、64 个产品脚本符号扫描。

| 候选 | GUID | 源代码事实 | 代码符号引用 | 序列化引用 | 初步处置 |
|---|---|---|---:|---:|---|
| `PlayerInput.cs` | `6f3bd58963b700548a6bfc9ecea94002` | 仅空 `Start`/`Update` | 0 | 0 | 不迁移；由 `IGameInput` 正式实现替代 |
| `Weapon_Hero.cs` | `54d1bc41ac524bb4b9cc08ddb6cd211a` | 仅空 `Start`/`Update` | 0 | 0 | 不迁移 |
| `Skill/Skill_Jiandao.cs` | `217873a8fbd96194ea721418e802587e` | 仅空 `Start`/`Update` | 0 | 0 | 不迁移；剪刀实际行为按引用中的其他 Skill/攻击逻辑迁移 |
| `Anim/PlayAnimation.cs` | `c298c5f60eeea844cb47bebbbe1acfe8` | 私有 `Animation` 从未赋值，`animation?.Play()` 永远无效果 | 0 | 0 | 不迁移 |
| `Anim/AnimStateMachine_DMD.cs` | `450f828c072c4c045a93df23b198fe33` | 未接入的自定义 Animation 原型；条件检查存在固定 `false` 分支 | 0 | 0 | 不迁移；正式玩家/Boss 代码状态机替代 |
| `ZZZZZZZZZZ.cs` | `749b72cac92083d409e0acf370df5a45` | 每帧读取 Spine `Zhen3` 骨骼的命名测试脚本 | 0 | 0 | 不迁移；若后续事件/骨骼审计发现等价正式功能则重新评估 |
| `GameManager.cs` | `a09252d9f50b87c4dbf26119d710562c` | 仅保存进程内 `OffKeyTip` 布尔值，未持久化 | 0（替换后） | 0 | 已由 `IColorTimingSettings` + GF.Setting 持久服务替代并移除 |
| `CTG` | 随 `UI_Game.cs` | 两个从未读写的静态布尔字段 | 0 | 不适用 | 已移除，不产生运行行为 |

## HeroController 调试热键

`HeroController.cs` 是 Boss1/Boss2 场景中的正式组件，不能删除；但其以下分支是无 UI、无配置、直接注入武器/Animator 测试状态的开发热键：

- `i`：强制切换紫色剪刀。
- `o`：强制切换紫色飞机。
- `p`：强制切换紫色炸弹。
- `t`：触发未列入正式参数合同的 `test` Trigger。

处置：正式迁移不暴露这些按键。七种武器和动画必须通过正常生成、拾取、攻击及测试适配器覆盖，不能把调试热键当作功能验证入口。

## 最终移除门槛

1. 目标导入后再次扫描场景、Prefab、Animator、Animation Event、Spine adapter 和代码引用。
2. 对应功能追踪行已有正式目标实现或明确“不产生行为”的证据。
3. Unity missing-script 与序列化引用审计为零。
4. 将最终结论及验证报告链接补入本文件。

## 最终复核结论

- `tools/audit_color_timing_asset_reconciliation.py` 已证明 3575/3575 个源资产均有唯一处置：3568 个迁移、7 个明确移除；移除 GUID 在目标中为零，目标 GUID 冲突为零。
- `tools/audit_color_timing_runtime_risks.py` 对正式运行时代码扫描 `KeyCode.I/O/P/T`，结果为零；正式输入只经 `IGameInput`/场景输入绑定器进入。
- Animation Event、Spine 资源、场景/Prefab missing reference 和功能追踪矩阵均另有审计证据；上述候选没有重新出现在序列化或运行路径中。
- 结论：表中 7 个脚本及 `CTG` 字段的移除均为有证据的废弃清理，不是功能缺失。
