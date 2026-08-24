# 多窗口协作调度

本目录是框架默认提供的多窗口协作规范与模板。它规定派发、占用、交接和恢复的最小契约；项目必须先按[初始化指南](../AI-Team-Collaboration-Initialization.md)填入自身配置，框架基线不得提交活动派发、真实占用、窗口 ID 或项目状态。

## 制品职责

- `RoleRouting.md`：长期用户可见工作窗口的职能与优先能力方向。
- `DispatchTemplate.md`：OpenSpec 建立前的方案探索派发单模板。
- `ActiveAssignments.template.md`：由制作人复制到项目协作区后独占维护的中央占用表模板。
- `PauseAndRecovery.md`：暂停、抢占、交接与恢复协议。
- `WindowAutomation.md`：首次注册、自动派发、通信门槛和手动降级规则。
- `WindowRegistry.example.json`：本机窗口注册表结构示例；真实文件位于被忽略的 `.ai/dispatch/window-registry.local.json`。
- `DecisionArchiveTemplate.md`：方案讨论检查点的增量归档模板。
- `OpenSpecBatching.md`：change 范围、批次和并行命名规范。
- `Active/README.md`：OpenSpec 前派发单与 change 内派发单的迁移规则。

## 派发生命周期

1. 制作人核对权威计划、范围、依赖与并行条件，生成派发单并登记中央占用表。
2. 已注册窗口接收最小启动消息；注册失效或发送失败时由用户手动派发。
3. 专业窗口完成启动检查和至少三轮关键方案讨论；每累计两轮先增量归档已确认内容。
4. 用户确认实现细节和架构后，创建 OpenSpec artifacts；制作人授予写入、工具和 Git 索引短锁。
5. 实施窗口交付验证证据、提交号、限制与锁释放信息；测试窗口独立报告缺陷，修复交回原实现窗口。
6. 用户验收后，制作人归档 OpenSpec，并将任何权威计划更新作为建议交给其维护者。

## 占用原则

- 目录写锁是默认粒度；场景、Prefab、共享配置、DCC 源等高冲突资产另列单文件锁。
- Unity、DCC 与外部工具实例为独占资源；即使只读驱动工具也须先取得工具锁。
- 多窗口共享工作区时，Git 索引是短期独占资源。仅持锁窗口可显式暂存授权路径并提交；禁止 `git add -A`、`git add .`、推送或重写历史。
- `ActiveAssignments` 仅由制作人窗口修改，其他窗口通过回传请求更新，避免中央表成为并行写冲突点。
