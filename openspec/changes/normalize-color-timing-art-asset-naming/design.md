# 设计

## 已确认命名分层

- 目录使用英文 ASCII 与 PascalCase，降低脚本、GF 工具、命令行和跨设备路径风险。
- 普通业务美术文件使用中文语义、ASCII 下划线和固定四位序列号。
- Spine 配套文件、SpriteAtlas、GF 资源组、配置资源 ID 等技术标识使用英文 ASCII。
- 禁止拼音、错别字、无意义占位名、空格、全角括号、破折号和不受控缩写。

示例：

```text
Hero/Hammer/Idle/Red/英雄_锤子_待机_红色_0000.png
Boss1/Body/Boss1Body_SkeletonData.asset
```

## 安全迁移原则

- 资源和文件夹只通过 Unity AssetDatabase 移动或重命名，不直接搬运资产与 `.meta`。
- 迁移前生成旧路径、GUID、新路径清单，并执行重名、大小写和目标路径冲突检查。
- 普通资源按独立业务批次迁移；Spine 文件集必须作为原子批次处理。
- Spine `.atlas.txt` 中的贴图页名称必须与重命名后的 PNG 同步，并完成重新导入验证。
- 每批完成后检查 Missing Reference、Missing Script、动画绑定、场景加载和 Console。
- 任一批次失败时按迁移清单反向移动，不以重新生成或覆盖源美术作为修复手段。

## 待确认

- 无；后续若迁移清单发现无法从路径、引用和内容共同确认的新增项，继续跳过并询问用户。

## 已确认特殊项

- `Hero/白色.png` 是半透明圆形遮罩类通用图案，本轮保持原文件名不变。
- `HeroTest.controller`、`t.anim`、`tttttt.controller` 保留并迁移到 `Hero/Tests`，使用明确测试名称。
- 六张未被运行时、场景、预制体或配置引用的效果参考图可以删除；原项目保留备份。
- Boss1 `Spine` 是承载待机、受击和主要攻击的主 Spine 套件，目标目录为 `Boss1/Core`。
- Boss1 `Spine2` 不是废弃资源；它仅承载第五招专用动画 `attack_5_test1_60fps2` 及正式事件，
  运行时第五招临时切换到该对象，目标目录为 `Boss1/Attack5`。
- Boss1 `tip` 与主动画同步提供攻击范围预警，目标目录为 `Boss1/Telegraph`。
- `Boss/第二关BOSS拆分3.png` 与 `Scene/B2/第二关BOSS拆分3.png` 内容哈希完全相同；前者仅由
  Boss2 场景中默认关闭、无脚本引用的 `Square` 调试对象使用，后者没有引用。两份均属于不参与运行
  的重复拆分合成参考图，可以与对应调试对象一并清理。
