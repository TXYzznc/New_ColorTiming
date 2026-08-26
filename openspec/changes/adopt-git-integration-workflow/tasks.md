## 1. 决策与变更规格

- [x] 1.1 完成 Git 集成角色、智能增量同步和立即启用范围的三轮决策收敛。
- [x] 1.2 归档两轮和最终决策检查点，并建立协作同步 OpenSpec。
- [x] 1.3 完成 Git 集成、协作自治和纯度审计规格。

## 2. Git 集成能力

- [x] 2.1 新增 `.claude/agents/git-integration.md`，定义请求合同、提交门禁和异常升级边界。
- [x] 2.2 更新 SKILL_MATRIX、项目入口和 producer 职责；生成 `.codex/agents/git-integration.toml`。
- [x] 2.3 新增 GitIntegration 工作流，并将 Dispatch 入口、派发、角色路由、暂停恢复和注册表同步为单一规则。

## 3. 审计与 FSR 就绪

- [x] 3.1 将 GitIntegration 设为协作必需制品，增加 UTF-8 BOM 安全读取和回归测试。
- [x] 3.2 收窄 ColorTiming 中将正式 `MainMenu` 误判为示例内容的审计规则。
- [x] 3.3 初始化并验证已锁定的 FSR 子模块，不修改 UPM 配置或子模块指针。

## 4. 验证

- [x] 4.1 运行 agent 同步检查、纯度审计 Python 测试与框架纯度审计。
- [x] 4.2 严格验证 OpenSpec、检查本变更范围的 Git diff，并确认现有工作树未被暂存或提交。
