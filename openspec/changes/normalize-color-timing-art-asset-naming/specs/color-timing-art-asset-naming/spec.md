# ColorTiming 美术资源命名规范增量

## ADDED Requirements

### Requirement: 业务美术资源采用分层命名

系统 SHALL 使用英文 ASCII 目录、中文普通美术文件名和英文技术标识，并保持名称语义明确、格式一致。

#### Scenario: 普通序列帧

- **WHEN** 一组业务序列帧被整理
- **THEN** 目录使用英文 PascalCase
- **AND** 文件使用中文语义、ASCII 下划线与四位序列号

#### Scenario: 技术资源

- **WHEN** 资源参与 Spine、SpriteAtlas、GF 资源组或代码配置
- **THEN** 技术文件基名与资源 ID 使用稳定英文 ASCII 标识

### Requirement: 重命名不得破坏资源内容和引用

系统 SHALL 保留源美术内容、Unity GUID 和既有运行效果。

#### Scenario: Unity 资产迁移

- **WHEN** 资源或目录被重命名
- **THEN** 操作通过 Unity AssetDatabase 执行
- **AND** `.meta` 与 GUID 保持一致
- **AND** 场景、预制体和动画引用仍然有效

#### Scenario: 不明确语义

- **WHEN** 无法从可靠证据判断资源用途
- **THEN** 该资源不得被推测重命名
- **AND** 必须等待用户确认正式语义
