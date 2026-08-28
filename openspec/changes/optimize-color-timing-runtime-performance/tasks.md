# 任务

- [x] 1. 完成第三轮决策并锁定 MainMenu 视频重入行为和单项验收门槛
- [x] 2. 修正 MainMenu RenderTexture 关闭释放、重新打开重建的生命周期
- [ ] 3. 验证 StartMenu 首次进入、离开、返回和重复进入的视频行为
- [x] 4. 调整长 BGM/环境音的加载配置并记录听感验收项
- [x] 5. 将 Boss1/Boss2 静态 AudioSource 迁入 GF 音频生命周期
- [ ] 5.1 用户完成听感验收，并重新导出被 Git 忽略的“美术资源”资源组
- [x] 6. 逐组调整 Boss2 高内存纹理的 Windows Importer 参数
- [x] 6.1 将 Boss1 的 POT Spine 图集改为 Windows BC7，并完成画面验收
- [x] 7. 对每组纹理执行 1080p 画面验收和内存复测
- [x] 8. 优化 Cursor 重复设置和 WeaponSpawner 每帧组件查询
- [x] 8.1 恢复 Hero Animator 状态行为兼容脚本，并验证 Boss1 运行时 Missing Script 为 0
- [ ] 8.2 重新设计 Hero 逐帧动画资源的按武器加载与释放方案；禁止继续使用当前 Unity 2022 SpriteAtlas 原生打包路径
- [ ] 9. 完成 Console、EditMode、PlayMode、场景功能和性能回归
- [ ] 10. 汇总逐项变更、收益、风险及回退说明
