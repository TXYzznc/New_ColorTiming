# `<dispatch-id>`：`<summary>`

> 本文件用于 OpenSpec 建立前的方案探索派发；它不代表实现方案已确认，也不改变项目权威计划。

## 1. 身份信息

| 字段 | 内容 |
|---|---|
| 派发 ID | `<planned-change-id>` |
| 管理批次 | `b<two-digit-sequence>` |
| 覆盖工作 | `<two-to-five-related-work-identifiers>` |
| 专业窗口 | `<role-window>` |
| 协作状态 | `<待领取/方案讨论/等待复核/可实施/实施中/待验收/暂停>` |
| 制作人入口 | `<producer-window-or-contact>` |

## 2. 目标、范围与权威输入

### 目标与验收

`<verifiable-result-and-acceptance>`

### 纳入与排除

- 纳入：`<in-scope>`
- 排除：`<out-of-scope>`

### 权威计划和输入边界

- 人类/外部权威计划：`<read-only-source-and-maintainer>`
- AI 可编辑输入与交付物：`<paths-or-artifact-types>`
- 不重新开放的已确认结论：`<confirmed-decisions>`

## 3. 启动与讨论检查

- [ ] 阅读项目入口、角色路由、对应 agent 配置、SKILL 与 OpenSpec 工作流。
- [ ] 检查目标项目、依赖、编译/诊断和外部工具状态。
- [ ] 核对目录/文件、工具和 Git 索引占用，不接管未交接工作。
- [ ] 与用户完成至少三轮关键方案讨论；每累计两轮增量归档检查点。
- [ ] 在用户确认实现细节和架构后，创建或填写 OpenSpec artifacts。

## 4. 修改权限与占用

### 可写与只读范围

- 可写：`<directory-locks>`
- 高冲突单文件：`<file-locks-or-none>`
- 只读：`<authority-sources-and-inputs>`
- 禁止修改：`<framework-core-or-other-boundaries>`

### 工具和 Git 索引

| 资源 | 实例/文件 | 占用者 | 释放条件 |
|---|---|---|---|
| `<Unity/DCC/external-tool>` | `<configured-instance>` | `<role-window>` | `<condition>` |

- 仅在中央占用表取得 Git 索引短锁后，显式暂存本派发范围并提交。
- 提交标题、概要和正文使用中文；术语、文件名、类型名、派发 ID 与 OpenSpec change ID 可保留原文。
- 禁止 `git add -A`、`git add .`、夹带其他窗口或用户改动、推送和改写历史。

## 5. 回传、暂停与恢复

回传须包含：决策/确认状态、OpenSpec 阶段、修改范围、验证与人工验收入口、提交号、限制、未决项、权威计划手动更新建议和已释放/仍持有的锁。

发生越界需求、公共契约改变、占用冲突、工具故障或扩大 OpenSpec 的验收失败时，停止相关工作、保留现场并按 [PauseAndRecovery.md](./PauseAndRecovery.md) 向制作人请求动作。
