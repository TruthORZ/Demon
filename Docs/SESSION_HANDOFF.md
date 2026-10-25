# Session Handoff

Updated: 2026-10-25 Asia/Shanghai
已完成：[TASK-001](Tasks/TASK-001-project-foundation.md) 文档初始化；当前发布：[TASK-003](Tasks/TASK-003-github-initial-push.md)；后续制作：[TASK-002](Tasks/TASK-002-playable-slice.md)，状态planned。

## 工程与工作状态

- 根目录与Unity项目：`D:\Unity\Projects\Demon`。
- Git：用户已授权初始化并push；分支main，origin为`https://github.com/TruthORZ/Demon.git`。HEAD需用`git rev-parse HEAD`现场读取，提交不嵌入其自身SHA。
- 现有模板资源保留。初次目录检查原70个文件SHA256一致，详见TASK-001；当前发布新增忽略/属性文件、TASK-003并更新相关文档，不改Unity资源。
- 当前请求为push并说明下一步，不是游戏实施授权。TASK-002保持planned。

## 当前状态

设计唯一来源为 [GAME_SPEC.md](GAME_SPEC.md)。已观察工程配置和实际资产在 [ARCHITECTURE.md](ARCHITECTURE.md)。不再把外部交付的技术Markdown整份复制进项目作为第二个权威。

当前仍为模板工程，无游戏玩家、时钟、石板、追逐、结局或测试。新增目录为空，不是已经实现的模块。Unity导入meta、编译、Play Mode和构建未验证。

## 重要约束

- 采用当前实际6000.6.0f1与已装包，不切换管线或安装新依赖。
- 第七块就是第五封印；D8当天完成立即执行，D9循环继续一次；不可双重调度。
- 固化程序不依赖可拆表面符文；投影可影响现实且无伤害；允许跳过探索直接D9降临。
- 先做一个D8到D9样例，场景/Prefab/配置必须可手工编辑，不在运行时隐藏生成整套关卡。
- 计划和待定项不冒充已实现架构或用户已确认内容。

## 验证与阻碍

初次文档静态检查通过，详细数量为当时快照见TASK-001；当前Git发布检查见TASK-003。没有Unity MCP连接，未确认活动Build Profile或Editor Console。`Temp/UnityLockfile`存在，不足以认定Editor当前运行，不启动竞争实例。Unity导入、编译、Play Mode与构建均未运行。

游戏实施前需要确认展示平台与样例相关的少量交互细节；其他设计问题见GAME_SPEC，不在交接重复维护。Git方案已由用户明确，网络操作可能需要沙箱外联网权限；不以此更改remote或强制push。

## 从这里继续

1. 取得实施指令后，先读AGENTS和本交接，再读TASK-002；复查工程实际状态和任何人新增内容。
2. 检查Unity导入、Console、输入资源和活动Build Profile；复查现有Git状态，保留未提交人工作业，不重复初始化或擅自提交。
3. 以 `Assets/Demon/Scenes/DemonSlice.unity` 为计划入口完成最小样例，并依TASK-002验证。该场景目前尚不存在。
