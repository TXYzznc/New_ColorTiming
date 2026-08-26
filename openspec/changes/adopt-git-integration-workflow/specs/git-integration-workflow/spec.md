## ADDED Requirements

### Requirement: 专职 Git 集成角色与请求合同
项目 MUST 定义 `git-integration` 角色，作为共享工作树中常规暂存和提交的唯一执行者。该角色 MUST 仅处理包含派发/OpenSpec ID、精确路径、验证证据、中文提交说明、永久排除项、资源状态和授权依据的请求。

#### Scenario: 完整的提交请求
- **WHEN** 专业窗口提交包含全部必填信息且路径授权一致的请求
- **THEN** Git 集成窗口 MUST 复核请求并继续提交流程，而无需制作人逐项审批。

### Requirement: 明确暂存和提交门禁
Git 集成窗口 MUST 读取工作区和暂存区状态、仅使用精确路径暂存、复核暂存文件清单并执行 `git diff --cached --check`。提交标题、概要和正文 MUST 使用中文；术语、文件名、类型名、派发 ID 与 OpenSpec ID 可保留原文。

#### Scenario: 请求范围外的工作树改动存在
- **WHEN** 工作树包含其他窗口或用户的未提交改动
- **THEN** Git 集成窗口 MUST 只暂存获批精确路径，且 MUST NOT 使用全量、通配或目录根级暂存。

### Requirement: 禁止的 Git 操作与异常升级
Git 集成窗口 MUST NOT 修改专业实现、补写测试、批准范围、推送、合并、变基、切换分支、改写历史或清理归属不明的现场。它遇到路径授权或归属不明、只读内容混入且不能安全分离、仓库异常或新用户授权需求时 MUST 停止并升级用户或制作人。

#### Scenario: 暂存区归属不明
- **WHEN** Git 集成窗口无法安全识别暂存内容的归属
- **THEN** 它 MUST 不提交或清理该内容，并 MUST 请求用户或制作人处置。
