# Boss 运行态进度与结果验证

日期：2026-08-24  
入口：正式 `Launch` → `StartMenu` → `Boss1` → `Boss2` → `StartMenu`  
证据：本目录的 `playmode-color-timing-latest.xml`；可再生成的完整 Console 流水已按文档保留策略清理。

## 自动化结果

`BossRuntimeProgressionPlayModeTests.FormalFlow_ConsumesEveryBossColor_ActivatesTailAndShowsFinalResult`：PASS。

- Boss1 初始 11 段；错误颜色不扣血。
- 按实时弱点顺序消费全部 11 段，运行中实际覆盖红、绿、紫三色。
- Boss1 胜利经 `IBattleResultSink` 自动进入 Boss2。
- Boss2 初始 15 段；错误颜色不扣血。
- 按实时弱点顺序消费全部 15 段，运行中实际覆盖红、绿、紫、橙四色。
- 尾部在剩余段数 12→11 时由 inactive 单次变为 active。
- 最终一击同帧设置头部死亡、同步停止尾部并关闭尾部 Collider。
- 最终结果通过 GF.UI 打开，获得暂停租约；返回 StartMenu 后 `timeScale` 恢复为 1。

完整 ColorTiming PlayMode：7/7 passed；完整 EditMode：201/201 passed；测试后 Unity Console：0 error / 0 warning。

## 尚不替代的人工范围

该自动化证明生命、颜色、阶段和结果控制流，但不替代六种 Boss1 攻击、Boss2 潜地/近战/远程各模式的视觉、声音、命中框和手感观察。OpenSpec 6.7、7.10 与 12.x 在这些人工证据完成前继续保持未勾选。
