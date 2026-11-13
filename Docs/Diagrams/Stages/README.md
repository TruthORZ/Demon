# Demon 三阶段架构展示材料

Updated: 2026-11-12 Asia/Shanghai

本材料依据 GitHub 已发布历史，整理为“基础九天规则 → 工具追逐与探索整合 → 每日任务与重试背包”三个展示阶段。版本使用完整提交 SHA 固定。老师允许 Unity 3D Route A 的确认来自用户。

## 三个阶段

| 阶段 | 固定提交与日期 | 功能范围 | 游戏入口 |
| --- | --- | --- | --- |
| Stage 1 基础九天规则版 | [a37036c](https://github.com/TruthORZ/Demon/commit/a37036cd166d246270040bf7f843c9a6c221daa0) · 2026-10-28 | 统一时钟、六块记录发现、T07 拼板与固化、箱 B / 湖封印交换、D8 时间保护与 D9 午夜结局。T05/T06 为只读内容；尚无独立监听、密码、光束、追逐或每日任务系统。 | [DemonNineDay.unity](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scenes/DemonNineDay.unity) |
| Stage 2 工具追逐与探索整合版 | [b31abc5](https://github.com/TruthORZ/Demon/commit/b31abc5c65524662a4448b4e0117d486da1d1fa7) · 2026-11-07 | 新增 T05 风铃、密码箱 A / T06 光束、导航追逐、按住取回 T07、D1–D8 搜寻/准备、SOUND/LIGHT 后果、接口/引导重构、职责命名、Snow 场景与 F8 重放。探索仍可选，允许跳过日期；尚无每日强制门槛和玩家当天恢复。 | [DemonNineDay_Art.unity](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scenes/DemonNineDay_Art.unity) |
| Stage 3 每日任务与重试背包版 | [b94f864](https://github.com/TruthORZ/Demon/commit/b94f864388d0dd524bdc07cfe165b7b70746711f) · 2026-11-12 | 新增每日事项与巡查、睡眠门槛/超时失败、实际当日起点恢复、可收录格子背包、普通手电/左键开关、逐日追逐与木屋安全区、D8 主动等待、固定太阳、两针挂钟，整合美术分支追踪脚印并清理旧占位。 | [DemonNineDay_Art.unity](https://github.com/TruthORZ/Demon/blob/b94f864388d0dd524bdc07cfe165b7b70746711f/Assets/Demon/Scenes/DemonNineDay_Art.unity) |

Stage 1 选择首个已提交、具有完整九天与终局闭环的版本。Stage 2 选择完整流程新需求之前已确认发布的 `b31abc5`，同一源码功能提交是 `487c894`，发布说明提交为其补充证据。Stage 3 选择本次查询到的远程 main `b94f864`，保留真实 art/main 合并历史。

这些 Stage 编号属于本次按真实提交整理的展示分组。仓库目前没有 `stage-1/2/3` 标签。Git 能核实提交和代码顺序，无法单凭提交推断老师何时发布需求或证明当时已经按课程正式冻结。报告引用具体 SHA，课堂阶段认定需同时保留老师确认及真实需求记录。本次没有创建、移动标签或发布提交。

本机当前尚未提交的湖中黑影和道具接入位于 `b94f864` 之后，本报告固定的 Stage 3 使用提交内内容。最终交付版本变化后，需要同步阶段终点、类图与清单。

## 三张核心类图

| 图 | 演示和排版用 PNG | 可放大 SVG | 可编辑源 |
| --- | --- | --- | --- |
| Stage 1 | [PNG](stage-1-core.png) | [SVG](stage-1-core.svg) | [Mermaid](stage-1-core.mmd) / [Graphviz](stage-1-core.dot) |
| Stage 2 | [PNG](stage-2-core.png) | [SVG](stage-2-core.svg) | [Mermaid](stage-2-core.mmd) / [Graphviz](stage-2-core.dot) |
| Stage 3 | [PNG](stage-3-core.png) | [SVG](stage-3-core.svg) | [Mermaid](stage-3-core.mmd) / [Graphviz](stage-3-core.dot) |

图保留各提交实际名称，Stage 1 的 Slice 前缀通过对照映射到后续名称。绿色为纯 C# 规则，蓝色为接口，灰色为 Unity 组件，浅黄为配置。连线沿用实线/虚线和空心三角/普通箭头，分别表示继承、实现、关联和依赖。

新增 / 修改 / 延续相对于前一个选定阶段。图中“延续”采用规范化类型声明对照，文件是否字节相同请查变化清单。图仅挑选核心关系，组件没有出现在某张图中不代表其已被删除。

## 两次阶段转换

- [Stage 1 到 Stage 2 变化清单](stage-1-to-2-changes.md)包含功能主题、代码位置、全部运行时文件与类型状态。重点讲新增工具如何复用时钟/规则，以及接口化改善近景协作。
- [Stage 2 到 Stage 3 变化清单](stage-2-to-3-changes.md)包含每日门槛、快照、背包和表现变化。重点讲哪些规则继续复用，哪些为恢复状态而增加局部接口。

| 指标 | Stage 1 → 2 | Stage 2 → 3 |
| --- | ---: | ---: |
| 新增运行时 C# 文件 | 32 | 9 |
| 同路径修改文件 | 4 | 28 |
| 改名且内容修改文件 | 14 | 0 |
| 删除运行时文件（沿 GUID 匹配） | 0 | 0 |
| 字节完全相同的运行时文件 | 1 | 23 |
| 新增类型（含接口、struct、嵌套类型） | 41 | 20 |
| 修改类型 | 13 | 30 |
| 规范化声明一致的类型 | 11 | 35 |

类名变化与结构变化分开解释。例如 Stage 1 的 SliceSession 到 Stage 2 的 GameSession，规范化声明一致。Stage 3 的 GameSession 则增加 Capture / FromSnapshot，属于修改并复用。类图保留部分具体控制器依赖与 CompletePlaythrough 的协调集中度，讲解时可以作为取舍讨论。

## 关键类与方法

| 类型 | 关键方法 | 展示重点 |
| --- | --- | --- |
| GameClock | AdvanceTo、Stop | 统一游戏时间和跨日处理 |
| RuneBoardModel | ClickSlot、TryFreeze | 符文盘操作与固化，Stage 3 另加 Capture / Restore |
| SliceSession / GameSession | TryCompile、SleepToNextDay | 封印交换与九天规则，Stage 3 另加 Capture / FromSnapshot |
| ICloseViewInteraction | Confirm、Click | 不同近景交互遵守相同输入契约 |
| ITabletProgram | BeginInteraction、TryCompile | T05 与 T07 的程序提供方连接同一石板组件 |
| DailyProgress | CanSleep、Complete、Restore | 每日条件与状态恢复的纯规则 |
| CompletePlaythrough | RequestSleep、RetryDay | 主线协调及玩家当日起点恢复 |
| JournalInventory | Show、Select | 格子背包展示与现场谜题操作分工 |

## 演示与历史验证

Stage 1 的提交中 TASK-005 保存了九天正常/跳过/晚到/虚拟输入和 Windows 构建记录。Stage 2 的 TASK-022 保存了原地重建、81 项规则、原九天正常/跳过与调试跳天记录。Stage 3 的 TASK-024 和美术合并报告保存了 89 项规则、每日失败/重试、背包/左键输入和追踪贴花验证。以上均属于当时的历史记录。

本次对三份提交提取 19 / 51 / 60 份运行时 C#，得到 24 / 65 / 85 个类型。使用本项目 Unity 引用集的 Roslyn 类型解析诊断错误均为 0，核对核心类图的每条关系及源码位置。没有在本轮启动旧 Unity 项目或构建/人工重玩三版，单靠源码诊断不能替代运行验证。

重现旧版本时应使用独立目录，从对应提交及锁定的 Unity 版本打开。Stage 2/3 含 Git LFS 资源，需要获取相应提交的完整 LFS 内容。当前工作区包含其他任务的未提交变动，使用独立副本可保护正在编辑的项目。

## 证据文件

- [版本清单与完整仓库差异](version-manifest.json)保存完整 SHA、日期、按 GUID 的文件配对、类型变化和 Git 原始路径状态。
- [类图与源码依据](class-map-evidence.json)保存每阶段完整类型索引、所有 partial 声明位置及本图每条边的源码命中位置。嵌套快照使用带外层名的完整短名，避免将多个 Snapshot / State 混成一个类。
- 本次范围和核查记录见 [TASK-028](../../Tasks/TASK-028-stage-architecture-evidence.md)。原有全量运行时类图仍保留其 2026-11-05 快照，本目录为独立的三阶段展示材料。
