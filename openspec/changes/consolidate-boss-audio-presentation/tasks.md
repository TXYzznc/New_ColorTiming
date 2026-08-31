## 1. 基线与通用实现

- [x] 1.1 记录两个旧 Sound View GUID、Scene AudioClip 字段和 Relay Renderer 引用
- [x] 1.2 实现 BossSoundCueId、BossSoundCueCatalogAsset 与通用 BossSoundView
- [x] 1.3 实现缓存 MaterialPropertyBlock 的 BossHitFlashView
- [x] 1.4 更新 Boss 专属 Cue 常量和 EditMode 合同测试

## 2. Unity 资产迁移

- [x] 2.1 实现幂等 Editor migration，创建并填充 Boss1/Boss2 Cue Catalog
- [x] 2.2 迁移 Boss1/Boss2 Scene 的 Sound View 和 Hit Flash 组件引用
- [x] 2.3 验证所有 AudioClip、Renderer 与 GF.Sound 绑定完整
- [x] 2.4 删除旧 Sound View/Cue enum 并确认旧 GUID 引用清零

## 3. 验证

- [x] 3.1 Unity 编译零错误并完成 EditMode Cue/Flash 测试
- [x] 3.2 完成 Boss1/Boss2 音效与攻击 PlayMode 回归
- [x] 3.3 校验 Scene 无 Missing Script/Reference 且未修改美术源资产
- [x] 3.4 更新审计结论、实施证据并执行 OpenSpec strict validation
