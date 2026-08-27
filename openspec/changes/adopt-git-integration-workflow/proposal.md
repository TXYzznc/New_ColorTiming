## Why

ColorTiming 将在后续重构中由客户端、技术美术、测试等多个长期窗口共同修改同一仓库。当前协作规范允许专业窗口直接争用 Git 索引，无法提供唯一的常规提交执行者和可审计的路径／验证门禁。

## What Changes

- 新增受控的 `git-integration` 角色源定义及生成的 Codex 配置，作为常规暂存与提交的唯一执行窗口。
- 智能同步框架仓库的 Git 集成工作流、派发模板、窗口自动化和高自治协作规则。
- 更新框架纯度审计与测试：GitIntegration 文档为协作必需制品，文本读取兼容 UTF-8 BOM。
- 初始化既有 FSR 子模块；不改变当前已一致的 FSR UPM 配置、版本或开发规则。

## Capabilities

### New Capabilities

- `git-integration-workflow`: 专职 Git 集成角色、请求合同、提交门禁、退回与升级规则。

### Modified Capabilities

- `ai-team-collaboration`: 将专业窗口的职责内自治和 Git 请求路由纳入协作合同。
- `framework-collaboration-purity`: 将 Git 集成文档和 BOM 安全读取纳入框架纯度审计合同。

## Impact

- 影响 `.claude` agent 源、生成的 `.codex/agents`、`Docs/Development/Dispatch`、项目入口指令、纯度审计及测试。
- 不修改 `Assets/`、`GameData/`、产品 Docs、已有业务 OpenSpec，亦不引入外部仓库历史。
- 后续常规 Git 提交必须由实际注册的 `git-integration` 长期窗口执行；当前未提交的工作树不被暂存、提交或清理。
