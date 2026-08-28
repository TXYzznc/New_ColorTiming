# 美术资源命名迁移最终验证

## 结果

- 原始资源：3138 项。
- 通过 Unity `AssetDatabase` 移动并重命名：3129 项。
- 用户确认保持原名：`Hero/白色.png` 1 项。
- 用户确认删除的未使用制作参考/重复资源：8 项。
- 最终资源：3130 项。
- 最终根目录：`Boss1`、`Boss2`、`Cursors`、`Hero`、`Scenes`、`Shadows`、`UI`、`Weapons`。

## 完整性

- 迁移清单逐项复核：0 个缺失目标，0 个 GUID 不一致，8 个删除项均不存在。
- 每个批次在移动前后校验 PNG/JPG SHA-256：全部一致，未修改图片字节。
- 目录规范检查：0 个非 ASCII/PascalCase 目录。
- 文件规范检查：0 个空格、全角括号或连字符遗留。
- 普通序列帧均使用四位编号；`Shadows/投影_草丛02` 等数字是对象编号而非序列帧。
- 已删除资源 GUID 在 `Assets` 序列化文件中引用数均为 0。

## Unity 验证

- Unity 编译：完成，Console 0 Error。
- 全项目 Prefab/已加载场景 Missing Script：0。
- Boss1：Missing Reference 0，场景验证问题 0。
- Boss2：Missing Reference 0，场景验证问题 0。
- 动画参数、AnimationEvent 接收器与 StateMachineBehaviour 契约：PASS。
- Spine 监听器审计：PASS。

## Spine 特殊处理

Boss1 主体与第五招是多贴图页 Spine 套件。Atlas 页名改为英文后，Spine 导入器按
`Atlas基名_贴图页名` 规则生成材质。本次采用该工具规范名，并恢复五个原材质 GUID，随后同步
`Boss1Core_Atlas.asset` 与 `Boss1Attack5_Atlas.asset` 引用。重新导入后清单 GUID 检查为 0 问题。

## 已知既有问题

- `Hero.controller` 含两个指向不存在本地 AnimatorState fileID 的跳转；原项目同一控制器已存在相同
  悬空引用，因此不是本次迁移引入。运行时参数、事件及当前场景验证均通过，后续可作为独立动画控制器
  清理任务处理。
- `Refresh ColorTiming Resource Collection` 会因项目按既定决定不包含 `Assets/Game/HotfixDlls` 而在
  既有框架工具中抛出 `DirectoryNotFoundException`。美术导出组使用稳定根路径
  `Assets/Game/Sprites`，不受本次子目录改名影响。

## 跨设备与导出

美术目录受项目资源导出工作流管理。其他设备接收本次代码、场景和 OpenSpec 变更后，需要重新导入
最新“美术资源” UnityPackage；资源组入口仍是 `Assets/Game/Sprites`，无需改配置。导出前应关闭
Play Mode，等待 Unity 完成导入，并确认 Console 无 Error。

## 回退

- 完整旧路径、GUID、新路径见 `asset-migration-preview.csv`，可按清单反向通过 AssetDatabase 移动。
- 删除的 8 个制作参考/重复资源由原项目保留备份。
- `asset-migration-batches.md` 与 `approved-reference-cleanup.md` 记录了执行批次和清理边界。
