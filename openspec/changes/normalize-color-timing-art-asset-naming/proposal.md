# 变更：统一 ColorTiming 美术资源命名

## 背景

`Assets/Game/Sprites/ColorTiming` 当前混用中文、英文、拼音、大小写、错别字、空格和含义不明缩写，
共包含 3138 个非 `.meta` 资源与 88 个子文件夹。混乱命名降低检索、资源分组、图集制作、跨岗位协作和
后续资源生命周期重构的可靠性。

## 目标

- 统一目录、普通美术文件、动画资源和技术资源命名。
- 通过 Unity AssetDatabase 移动资产，保留 `.meta` 与 GUID 引用。
- 对 Spine 等包含文件名级内部引用的资源成组迁移并验证。
- 不修改图片像素、动画内容、材质表现和既有美术效果。
- 对无法可靠判断含义的资源先取得用户确认，不推测命名。

## 影响范围

- `Assets/Game/Sprites/ColorTiming` 全部资源与目录。
- 引用这些资源的场景、预制体、AnimationClip、AnimatorController、材质、Spine 数据和配置。
- GF ResourceCollection、资源导出组和文档中的相关路径。
