# Session Handoff
Updated: 2026-11-06 Asia/Shanghai
Active task: [TASK-020 美术雪地接入九天流程](Tasks/TASK-020-snow-gameplay-integration.md)
Repository/Unity project: D:/Unity/Projects/Demon
Branch: `main`，上游为 `origin/main`。

## 当前状态

- 整合试玩入口是 `Assets/Demon/Scenes/DemonNineDay_Art.unity`。玩家在木屋床边开始，箱子、轮盘、拼板台在屋内，T07 暂放在门廊下方的围栏内。摆放对象在 `Gameplay_Editable` 下。正式构建入口仍是 `DemonNineDay.unity`。
- 交付包含整合场景依赖的 [D7–D8 玩法](Tasks/TASK-015-final-days-preparation.md)、[架构重构](Tasks/TASK-016-solid-refactor.md)、[类名迁移](Tasks/TASK-019-remove-slice-class-prefix.md)，以及 [Snow 地形二进制修复与原运行管线设置](Tasks/TASK-017-art-scene-pull.md)。[类图](Diagrams/runtime-classes.md)对应 TASK-019 的运行时快照。
- 本机保留 `ProjectSettings/ProjectSettings.asset` 和 `ProjectSettings/UnityConnectSettings.asset` 的既有本地差异。未跟踪的原始 `Snow-main/` 属于本地参考目录。

## 验证与下一步

1. 整合场景保存重开、T07 移动随动、取板区往返碰撞检查，以及 D1–D2、D8 的 Play Mode 组件检查通过。81 项 EditMode 规则通过，原九天场景引用验证通过。证据和测试边界见 TASK-020。
2. 在整合场景用键鼠连续实走 D1–D2，并从 D8 22:00 验证追逐中取板、返程和安放。之后验收剩余日期和独立 Windows 构建。
3. 地形与追逐导航为 Unity 原生二进制资产，沿用 `.gitattributes` 的精确路径规则。改地面、障碍物或木屋通道后，重烘焙整合场景的导航。
