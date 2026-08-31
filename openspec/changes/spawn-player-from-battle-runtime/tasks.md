## 1. 基线与运行时实现

- [x] 1.1 记录 Boss1/Boss2 Hero 本地结构、外部引用、武器能力、音效和镜头差异
- [x] 1.2 实现 PlayerSceneProfileAsset 与 Player View 配置接口
- [x] 1.3 实现 BattlePlayerManager 创建、bindings 与释放合同
- [x] 1.4 更新 BattleSceneAnchors 和 BattleRuntimeContext 初始化顺序

## 2. Unity 资产迁移

- [x] 2.1 创建完整 Player Prefab，并清除 Prefab 内场景对象引用
- [x] 2.2 创建 Boss1/Boss2 Player Scene Profile，保留全部 AudioClip 和镜头值
- [x] 2.3 更新两个 Scene Anchors、Cinemachine 与 DeathSequence 引用
- [x] 2.4 删除两个 Scene 的静态 Hero，并确认旧 Scene fileID 引用清零

## 3. 验证

- [x] 3.1 更新 EditMode 场景合同与 Manager 生命周期测试
- [x] 3.2 Unity 编译和 Missing Script/Reference 校验通过
- [x] 3.3 ColorTiming EditMode 与 PlayMode 全量通过
- [x] 3.4 记录资产引用证据并完成 OpenSpec strict validation
