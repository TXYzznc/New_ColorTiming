# ColorTiming 美术资源命名规范

## 适用范围

本规范适用于 `Assets/Game/Sprites/ColorTiming` 下的项目业务美术资源。框架通用资源继续遵守
领域无关的 ASCII 资源规范。

## 命名分层

### 目录

- 使用稳定英文 ASCII 与 PascalCase。
- 按业务对象和技术用途分层，不使用拼音、中文目录、错别字或无意义缩写。
- 关卡使用 `Boss1`、`Boss2`；序列帧使用 `Sequences`；测试资源使用 `Tests`。

### 普通业务美术文件

- 使用明确中文语义与 ASCII 下划线。
- 序列帧末尾使用四位编号：`_0000`、`_0001`。
- 颜色统一为 `红色`、`绿色`、`紫色`、`橙色`。
- 状态统一使用 `待机`、`移动`、`攻击`、`蓄力`、`完成`、`受击`、`死亡` 等完整语义。
- 禁止空格、全角括号、破折号、拼音和 `a1`、`g1`、`t` 等无上下文名称。

示例：

```text
Hero/Hammer/Idle/Red/英雄_锤子_待机_红色_0000.png
UI/MainMenu/按钮_开始游戏_正常.png
Scenes/Boss1/Grass01/首领1场景_草丛01_待机_0000.png
```

### 技术资源

- Spine 配套文件、SpriteAtlas、GF 资源组、配置键和代码资源 ID 使用英文 ASCII。
- 同一 Spine 套件必须共享稳定基名，并保留工具生成后缀。

```text
Boss1/Core/Boss1Core.json
Boss1/Core/Boss1Core.atlas.txt
Boss1/Core/Boss1Core_Atlas.asset
Boss1/Core/Boss1Core_Material.mat
Boss1/Core/Boss1Core_SkeletonData.asset
```

## 特殊规则

- `Hero/白色.png` 是既有半透明遮罩类通用图案，作为用户确认的兼容例外保持原名。
- 制作参考图不得混在运行资源目录；确认无运行引用且原项目有备份时可以删除。
- 测试 AnimationClip 与 AnimatorController 放入对象目录下的 `Tests`，名称必须说明测试用途。
- 资源路径不作为业务 ID；代码和配置通过稳定英文资源 ID 或 Unity GUID 定位。

## 迁移与验收

- 资源移动和重命名必须通过 Unity AssetDatabase 执行并保持 GUID。
- Spine 重命名必须同步 `.atlas.txt` 内部页名称并重新导入。
- 每批检查目标路径冲突、重复 GUID、Missing Reference、动画绑定、场景加载和 Console。
- 迁移不得修改 PNG/JPG 像素、AnimationClip 曲线、材质参数或美术表现。
