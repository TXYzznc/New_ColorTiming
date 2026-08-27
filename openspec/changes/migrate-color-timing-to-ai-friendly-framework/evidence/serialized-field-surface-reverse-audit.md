# 源组件序列化字段面反向审计

日期：2026-08-24

## 目的

Unity 重构即使编译成功，也可能因 public/`[SerializeField]` 字段被改名、删除或移到错误继承层而静默丢失场景与 Prefab 数据。本审计从源组件字段反向验证目标序列化合同。

## 方法

- 使用 Unity 2022.3 自带 Roslyn 解析源、目标 C#，不以正则推断语法；
- 只纳入 `MonoBehaviour`、`StateMachineBehaviour`、`ScriptableObject` 及项目内派生类；
- Unity 序列化候选为非 static/const/readonly 的 public 字段或 `[SerializeField]` 字段，并排除 `[NonSerialized]`；
- 比较同名组件的完整继承链，因此移入 `WeaponSpawnerView`、`Skill_base` 等正确基类的字段仍算保留；
- 对字段名和声明类型同时比较。

执行：

```powershell
powershell -File tools/audit_color_timing_serialized_surface.ps1
```

机器可读结果：`serialized-field-surface-audit.json`。

## 结果

| 检查项 | 结果 |
|---|---:|
| 源 Unity 组件类 | 54 |
| 含继承展开的源序列化字段合同 | 241 |
| 目标组件类缺失 | 0 |
| 目标字段名缺失 | 0 |
| 同名字段类型变化 | 0 |
| 最终状态 | PASS |

`241` 是按每个具体组件展开继承链后的合同数，基类字段会在各派生组件中重复计入；这是有意的，因为 Unity 会为每个派生组件序列化继承字段。

## 与资源证据的组合结论

本审计证明脚本侧仍能接收源场景/Prefab 的字段名和类型；它与以下证据组合使用：

- 3575/3575 源资产唯一处置、目标 GUID 冲突 0；
- StartMenu、Boss1、Boss2 missing scripts/prefabs/references 均为 0；
- StartMenu GF.UI 转换后的引用、视频 RenderTexture 以及 Boss 场景运行时绑定另有专项验证；
- 方法面、UnityEvent、Animation Event、Spine listener 均有独立反向/合同审计。

因此，没有发现因脚本序列化字段改名、删除、类型变化或错误基类迁移导致的静默数据遗漏。实际画面、动画、声音和手感仍由最终人工回归证明。
