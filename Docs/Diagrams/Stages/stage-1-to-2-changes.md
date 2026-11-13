# Stage 1 到 Stage 2 变化清单

版本从 `a37036c`（基础九天规则版）到 `b31abc5`（工具追逐与探索整合版）。
[GitHub 版本比较](https://github.com/TruthORZ/Demon/compare/a37036cd166d246270040bf7f843c9a6c221daa0...b31abc5c65524662a4448b4e0117d486da1d1fa7) · [阶段总览](README.md)

## 本次扩展的功能与动机

| 变化主题 | 新需求或行为 | 主要承载类型或文件 |
| --- | --- | --- |
| 工具与密码 | 增加可操作的 T05、方位密码箱和 T06，配套区域风铃与稳定光束规则。 | ListeningTool / ListeningChime / RegionEntryLatch；CipherBox / CipherWheel / ThreeDigitLock；BeamTether / StableBeamMotion。 |
| 威胁与取回 | 从阅读石板扩展到实际追逐、按住拾取、打断重试和回棚放置。 | ChaseController / ClosePassGate；ThreatPickup / PickupAttempt；FinalTabletRetrieval / FinalTabletProgress / FinalTabletDock。 |
| 探索与日程 | 增加观察点、D1–D8 搜寻和世界声音/光后果，并将提示选择移到有序规则。 | ExplorationFlow / ExplorationGuidance / ExplorationDefinition；WorldAttributeConsequences / ScheduledWorldAttributes。 |
| 交互契约 | 玩家持有 IPlayerSession，石板和密码箱实现共同近景操作。T05 与 T07 使用同一石板展示组件连接各自程序。 | SceneContracts.cs 定义 5 个实际消费者接口；PlayerController、RuneTablet、GameSceneController 和 ListeningTool 完成连接。 |
| 运行与作者化 | 接入 Snow 环境、门口导航和 F8 调试菜单，使用原地会话重建补齐目标日期前的进度。 | CabinDoorInteraction、DayJumpMenu、CompletedDayReplay；雪地场景、导航与配置资产。 |
| 命名迁移 | 旧 Slice 类按职责改名。改名后的规则是否变化需看声明和代码，不能仅从路径判断。 | 14 份运行时脚本沿相同 meta GUID 找到改名后的对应文件。 |

## 变化数量

| 运行时 C# 文件状态 | 数量 |
| --- | ---: |
| 新增 | 32 |
| 修改 | 4 |
| 改名且修改 | 14 |
| 仅改名 | 0 |
| 删除 | 0 |
| 字节未变 | 1 |

| 类型状态 | 数量 |
| --- | ---: |
| 新增 | 41 |
| 修改 | 13 |
| 删除 | 0 |
| 规范化声明一致 | 11 |

文件范围为 `Assets/Demon/Scripts/Runtime/**/*.cs`，包含 Domain。文件身份先按对应 `.meta` GUID 匹配，字节相同依据 Git blob ID。类型统计包含 class、interface、struct 与嵌套类型，partial 声明合并为一个类型，因此与文件数不同。

“规范化声明一致”将旧类名映射为新类名，忽略空白、注释和单独的 MovedFrom 迁移属性，再比较该类型的完整语法 token。这个指标说明声明/实现文本的稳定程度，不能单独证明其所调用对象的行为一致。全仓库实际 Git 文件状态，包括编辑器、测试、场景、美术和文档，另存于 [version-manifest.json](version-manifest.json) 的 comparisons.repositoryChanges。

## 稳定部分与必须说明的边界

- `AttributeOperations`、`GameClock`、`GameSession`（原 `SliceSession`）与 `FinaleTimeWindow` 的规范化声明一致，保留统一时钟、属性交换和午夜时间窗口。
- 文件级字节完全相同的运行时代码为 `Domain/FinaleTimeWindow.cs`。`GameClock` 等类型与其他类型共用原 `SliceRules.cs`，该文件改名为 `GameRules.cs` 且其中其他类型有改动，因此不标为“整个文件不变”。
- `RuneBoardModel` 有实际变化，例如 T07 对称 EXCHANGE 接受 BOX/LAKE 的两种排列。场景控制器也增加模块组装和重建接点，不能宣称本次只增加新类。
- 接口化确实缩小了部分依赖。若干组件仍保存具体场景控制器，保留这一真实耦合。
- 旧 Slice 前缀的重命名与玩法扩展分别记述。阶段 1 图保留原名，阶段 2 图使用实际新名。

## PPT 可用的代码与验证证据

- 旧玩家持有具体控制器。[SlicePlayer](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SlicePlayer.cs#L7)
- 新玩家消费接口。[PlayerController](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/PlayerController.cs#L7)
- 新近景/石板契约。[ICloseViewInteraction](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/SceneContracts.cs#L37)
- 独立提示选择器。[ExplorationGuidance](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/ExplorationGuidance.cs#L34)
- 当时的接口/引导重构和九天回归见 [TASK-016](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Docs/Tasks/TASK-016-solid-refactor.md)。F8 的会话重建、81 项规则及原九天路线结果见 [TASK-022](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Docs/Tasks/TASK-022-day-jump-menu.md)。

以上是相应版本提交内保留的历史验证记录。本次执行源码提取、类型关系与差异核查，没有重新打开三个 Unity 版本运行整段九天。

## 完整运行时脚本清单

| 状态 | Stage 1 文件 | Stage 2 文件 |
| --- | --- | --- |
| 新增 | — | [BeamDefinition.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/BeamDefinition.cs) |
| 新增 | — | [BeamTether.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/BeamTether.cs) |
| 新增 | — | [CabinDoorInteraction.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/CabinDoorInteraction.cs) |
| 新增 | — | [ChaseController.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ChaseController.cs) |
| 新增 | — | [ChaseDefinition.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ChaseDefinition.cs) |
| 新增 | — | [CipherBox.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/CipherBox.cs) |
| 新增 | — | [CipherDefinition.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/CipherDefinition.cs) |
| 新增 | — | [CipherDigit.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/CipherDigit.cs) |
| 新增 | — | [CipherWheel.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/CipherWheel.cs) |
| 新增 | — | [CompletedDayReplay.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/CompletedDayReplay.cs) |
| 新增 | — | [DayJumpMenu.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/DayJumpMenu.cs) |
| 新增 | — | [Domain/BearingCipher.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/BearingCipher.cs) |
| 修改 | [Domain/DiscoveryProgress.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/Domain/DiscoveryProgress.cs) | [Domain/DiscoveryProgress.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/DiscoveryProgress.cs) |
| 新增 | — | [Domain/ExplorationGuidance.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/ExplorationGuidance.cs) |
| 新增 | — | [Domain/FinalTabletProgress.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/FinalTabletProgress.cs) |
| 字节未变 | [Domain/FinaleTimeWindow.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/Domain/FinaleTimeWindow.cs) | [Domain/FinaleTimeWindow.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/FinaleTimeWindow.cs) |
| 新增 | — | [Domain/FirstDayInspectionLog.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/FirstDayInspectionLog.cs) |
| 改名且修改 | [Domain/SliceRules.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/Domain/SliceRules.cs) | [Domain/GameRules.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/GameRules.cs) |
| 新增 | — | [Domain/PursuitRules.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/PursuitRules.cs) |
| 新增 | — | [Domain/RegionEntryLatch.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/RegionEntryLatch.cs) |
| 新增 | — | [Domain/ScheduledWorldAttributes.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/ScheduledWorldAttributes.cs) |
| 新增 | — | [Domain/StableBeamMotion.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/StableBeamMotion.cs) |
| 新增 | — | [ExplorationDefinition.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ExplorationDefinition.cs) |
| 新增 | — | [ExplorationFlow.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ExplorationFlow.cs) |
| 新增 | — | [FinalTabletDock.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/FinalTabletDock.cs) |
| 新增 | — | [FinalTabletRetrieval.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/FinalTabletRetrieval.cs) |
| 改名且修改 | [SliceFinaleSequence.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceFinaleSequence.cs) | [FinaleSequence.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/FinaleSequence.cs) |
| 改名且修改 | [SliceHud.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceHud.cs) | [GameHud.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/GameHud.cs) |
| 改名且修改 | [SliceSceneController.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceSceneController.cs) | [GameSceneController.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/GameSceneController.cs) |
| 改名且修改 | [SliceSettings.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceSettings.cs) | [GameSettings.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/GameSettings.cs) |
| 改名且修改 | [SliceInteractable.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceInteractable.cs) | [Interactable.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Interactable.cs) |
| 新增 | — | [ListeningChime.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ListeningChime.cs) |
| 新增 | — | [ListeningDefinition.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ListeningDefinition.cs) |
| 新增 | — | [ListeningTool.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ListeningTool.cs) |
| 新增 | — | [ListeningTrailProxy.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ListeningTrailProxy.cs) |
| 修改 | [NineDayDefinition.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/NineDayDefinition.cs) | [NineDayDefinition.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/NineDayDefinition.cs) |
| 修改 | [NineDayFlow.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/NineDayFlow.cs) | [NineDayFlow.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/NineDayFlow.cs) |
| 修改 | [NineDayTablet.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/NineDayTablet.cs) | [NineDayTablet.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/NineDayTablet.cs) |
| 改名且修改 | [SlicePlayer.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SlicePlayer.cs) | [PlayerController.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/PlayerController.cs) |
| 新增 | — | [PreparationRecord.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/PreparationRecord.cs) |
| 改名且修改 | [SliceBox.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceBox.cs) | [RecordBox.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/RecordBox.cs) |
| 改名且修改 | [SliceRuneSlot.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceRuneSlot.cs) | [RuneSlot.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/RuneSlot.cs) |
| 改名且修改 | [SliceTablet.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceTablet.cs) | [RuneTablet.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/RuneTablet.cs) |
| 改名且修改 | [SliceRuneTrayItem.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceRuneTrayItem.cs) | [RuneTrayItem.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/RuneTrayItem.cs) |
| 新增 | — | [SceneContracts.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/SceneContracts.cs) |
| 改名且修改 | [SliceFontBinding.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceFontBinding.cs) | [SceneFontBinding.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/SceneFontBinding.cs) |
| 改名且修改 | [SliceBed.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceBed.cs) | [SleepPoint.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/SleepPoint.cs) |
| 新增 | — | [StationObservation.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/StationObservation.cs) |
| 新增 | — | [ThreatPickup.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ThreatPickup.cs) |
| 新增 | — | [WorldAttributeConsequences.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/WorldAttributeConsequences.cs) |
| 改名且修改 | [SlicePresentation.cs](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SlicePresentation.cs) | [WorldPresentation.cs](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/WorldPresentation.cs) |

## 完整类型清单

| 状态 | 旧类型 | 新类型 |
| --- | --- | --- |
| 规范化声明一致 | [AttributeOperations](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/Domain/SliceRules.cs#L10) | [AttributeOperations](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/GameRules.cs#L10) |
| 新增 | — | [BeamDefinition](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/BeamDefinition.cs#L4) |
| 新增 | — | [BeamTether](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/BeamTether.cs#L7) |
| 新增 | — | [BearingCipher](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/BearingCipher.cs#L4) |
| 新增 | — | [CabinDoorInteraction](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/CabinDoorInteraction.cs#L6) |
| 新增 | — | [ChaseController](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ChaseController.cs#L7) |
| 新增 | — | [ChaseDefinition](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ChaseDefinition.cs#L4) |
| 新增 | — | [CipherBox](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/CipherBox.cs#L6) |
| 新增 | — | [CipherDefinition](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/CipherDefinition.cs#L5) |
| 新增 | — | [CipherDigit](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/CipherDigit.cs#L4) |
| 新增 | — | [CipherWheel](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/CipherWheel.cs#L6) |
| 新增 | — | [ClosePassGate](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/PursuitRules.cs#L4) |
| 新增 | — | [CompletedDayReplay](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/CompletedDayReplay.cs#L8) |
| 新增 | — | [DayJumpMenu](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/DayJumpMenu.cs#L8) |
| 规范化声明一致 | [DayPlan](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/NineDayDefinition.cs#L6) | [DayPlan](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/NineDayDefinition.cs#L6) |
| 修改 | [DiscoveryProgress](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/Domain/DiscoveryProgress.cs#L5) | [DiscoveryProgress](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/DiscoveryProgress.cs#L5) |
| 新增 | — | [ExplorationDefinition](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ExplorationDefinition.cs#L13) |
| 新增 | — | [ExplorationFlow](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ExplorationFlow.cs#L6) |
| 新增 | — | [ExplorationGuidance](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/ExplorationGuidance.cs#L34) |
| 新增 | — | [ExplorationGuideRule](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/ExplorationGuidance.cs#L17) |
| 新增 | — | [ExplorationGuideState](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/ExplorationGuidance.cs#L28) |
| 新增 | — | [FinalTabletDock](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/FinalTabletDock.cs#L4) |
| 新增 | — | [FinalTabletProgress](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/FinalTabletProgress.cs#L4) |
| 新增 | — | [FinalTabletRetrieval](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/FinalTabletRetrieval.cs#L6) |
| 修改 | [SliceFinaleSequence](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceFinaleSequence.cs#L8) | [FinaleSequence](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/FinaleSequence.cs#L9) |
| 规范化声明一致 | [FinaleTimeWindow](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/Domain/FinaleTimeWindow.cs#L7) | [FinaleTimeWindow](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/FinaleTimeWindow.cs#L7) |
| 新增 | — | [FirstDayInspectionLog](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/FirstDayInspectionLog.cs#L5) |
| 规范化声明一致 | [FixedTabletPlan](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/NineDayDefinition.cs#L12) | [FixedTabletPlan](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/NineDayDefinition.cs#L12) |
| 规范化声明一致 | [GameClock](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/Domain/SliceRules.cs#L35) | [GameClock](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/GameRules.cs#L35) |
| 修改 | [SliceHud](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceHud.cs#L6) | [GameHud](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/GameHud.cs#L7) |
| 修改 | [SliceSceneController](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceSceneController.cs#L7) | [GameSceneController](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/GameSceneController.cs#L8) |
| 规范化声明一致 | [SliceSession](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/Domain/SliceRules.cs#L151) | [GameSession](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/GameRules.cs#L157) |
| 修改 | [SliceSettings](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceSettings.cs#L6) | [GameSettings](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/GameSettings.cs#L7) |
| 新增 | — | [ICloseViewInteraction](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/SceneContracts.cs#L37) |
| 新增 | — | [IHudState](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/SceneContracts.cs#L22) |
| 新增 | — | [IPlayerSession](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/SceneContracts.cs#L13) |
| 新增 | — | [ISceneFeedback](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/SceneContracts.cs#L7) |
| 新增 | — | [ITabletProgram](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/SceneContracts.cs#L29) |
| 规范化声明一致 | [SliceInteractable](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceInteractable.cs#L5) | [Interactable](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Interactable.cs#L6) |
| 新增 | — | [ListeningChime](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ListeningChime.cs#L7) |
| 新增 | — | [ListeningDefinition](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ListeningDefinition.cs#L6) |
| 新增 | — | [ListeningTool](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ListeningTool.cs#L7) |
| 新增 | — | [ListeningTrailProxy](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ListeningTrailProxy.cs#L7) |
| 修改 | [NineDayDefinition](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/NineDayDefinition.cs#L19) | [NineDayDefinition](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/NineDayDefinition.cs#L19) |
| 修改 | [NineDayFlow](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/NineDayFlow.cs#L7) | [NineDayFlow](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/NineDayFlow.cs#L7) |
| 修改 | [NineDayTablet](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/NineDayTablet.cs#L5) | [NineDayTablet](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/NineDayTablet.cs#L5) |
| 新增 | — | [ObservationText](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ExplorationDefinition.cs#L6) |
| 新增 | — | [PickupAttempt](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/PursuitRules.cs#L28) |
| 修改 | [SlicePlayer](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SlicePlayer.cs#L7) | [PlayerController](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/PlayerController.cs#L7) |
| 新增 | — | [PreparationRecord](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/PreparationRecord.cs#L9) |
| 规范化声明一致 | [SliceBox](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceBox.cs#L5) | [RecordBox](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/RecordBox.cs#L6) |
| 新增 | — | [RegionEntryLatch](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/RegionEntryLatch.cs#L4) |
| 修改 | [RuneBoardModel](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/Domain/SliceRules.cs#L72) | [RuneBoardModel](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/GameRules.cs#L72) |
| 规范化声明一致 | [SliceRuneSlot](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceRuneSlot.cs#L5) | [RuneSlot](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/RuneSlot.cs#L6) |
| 修改 | [SliceTablet](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceTablet.cs#L6) | [RuneTablet](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/RuneTablet.cs#L7) |
| 规范化声明一致 | [SliceRuneTrayItem](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceRuneTrayItem.cs#L6) | [RuneTrayItem](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/RuneTrayItem.cs#L7) |
| 修改 | [SliceFontBinding](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceFontBinding.cs#L8) | [SceneFontBinding](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/SceneFontBinding.cs#L9) |
| 新增 | — | [ScheduledWorldAttributes](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/ScheduledWorldAttributes.cs#L4) |
| 规范化声明一致 | [SliceBed](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SliceBed.cs#L5) | [SleepPoint](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/SleepPoint.cs#L6) |
| 新增 | — | [StableBeamMotion](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/StableBeamMotion.cs#L5) |
| 新增 | — | [StationObservation](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/StationObservation.cs#L5) |
| 新增 | — | [ThreatPickup](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/ThreatPickup.cs#L7) |
| 新增 | — | [ThreeDigitLock](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/Domain/BearingCipher.cs#L25) |
| 新增 | — | [WorldAttributeConsequences](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/WorldAttributeConsequences.cs#L6) |
| 修改 | [SlicePresentation](https://github.com/TruthORZ/Demon/blob/a37036cd166d246270040bf7f843c9a6c221daa0/Assets/Demon/Scripts/Runtime/SlicePresentation.cs#L5) | [WorldPresentation](https://github.com/TruthORZ/Demon/blob/b31abc5c65524662a4448b4e0117d486da1d1fa7/Assets/Demon/Scripts/Runtime/WorldPresentation.cs#L6) |
