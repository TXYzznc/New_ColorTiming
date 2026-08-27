# ColorTiming 重构决策检查点 01

- 日期：2026-08-24
- 源项目：`D:\unity\UnityProject\ColorTimeing\ColorTimeing`
- 框架基线：`D:\unity\UnityProject\AIFriendly_Frame\AI-Friendly-Project`
- 目标项目：`D:\unity\UnityProject\ColorTimeing\New\_ColorTiming`

## 已确认决策

1. 采用旁路迁移：在目标目录建立新项目，源项目保持不动并作为功能与资源对照基线。
2. 兼容策略采用“行为等价并修复明确缺陷”：
   - 完整保留玩家可见玩法、流程、数值、资源和动画事件。
   - 移除证据明确的空脚本、未引用原型和测试热键。
   - 只修复能够由现有代码或资源证明的缺陷。
   - 每一项行为修复都必须进入 OpenSpec，并具备迁移前后验证证据。

## 尚待确认

- 渲染管线：迁移到框架 URP、暂留 Built-in，或维护双管线。
- 验收门槛与人工回归范围。

## 不变量

- 不直接修改源项目。
- 不以“能够编译”替代功能完整性验证。
- 未被功能清单、资源引用检查和逐场景回归证明的功能，不视为已迁移完成。
