# 运行时类关系图：读图说明与源码索引

快照：2026-11-05，当前工作区（包括未提交代码），不是仅分析 Git HEAD。
本图已同步 TASK-019 类名迁移。旧名对照与命名审查见 [TASK-019](../Tasks/TASK-019-remove-slice-class-prefix.md)。当前架构仍由 [ARCHITECTURE.md](../ARCHITECTURE.md) 维护。

## 先看哪张图
- [总览 PNG](runtime-classes-overview.png) / [总览 SVG](runtime-classes-overview.svg)：6 个分区，适合课程讲解，保留主要关系。
- [完整图 SVG](runtime-classes-complete.svg) / [完整图 PNG](runtime-classes-complete.png)：13 个分区，展开全部 241 条去重的类型关系。SVG 可持续放大；在浏览器中查看更方便。
- [关系清单 CSV](runtime-class-relations.csv)：每条关系一行，附源码位置和字段/使用依据；UTF-8 BOM 可供 Excel 打开。
- [完整依据 JSON](runtime-class-relations.json)：包含每条关系的全部命中位置、类型索引和源码 SHA256。
- [Graphviz 源文件](runtime-classes.dot)：完整类型图的可编辑文本；与分区海报使用相同的节点/边，重新布局可能不同。

总览有意省略部分临时调用和重复的 Unity 基类连线，因此不能据其“没有连线”推断两个类型完全无关。完整图按**关系起点**归区；同名类跨区出现表示同一个类，重复节点用于减少跨图长线。

## 四种符号怎么理解

| 关系 | 画法 | 此项目真实例子 | 通俗解释 |
| --- | --- | --- | --- |
| Inheritance 继承 | 实线 + 空心三角 | `RuneTablet → Interactable` | 石板是一种可交互物体；三角指向父类。 |
| Realisation 实现 | 虚线 + 空心三角 | `RuneTablet → ICloseViewInteraction` | 石板承诺提供接口规定的操作；三角指向接口。 |
| Association 关联 | 实线 + 普通箭头 | `PlayerController → ICloseViewInteraction` | 玩家把当前近景对象保存在 `activeInteraction` 字段里。 |
| Dependency 依赖 | 虚线 + 普通箭头 | `CipherWheel → BearingCipher` | 轮盘临时调用方位算法，没有保存一个该算法对象。 |

关联并不要求字段由 Inspector 赋值：构造函数或 `Initialize` 接收后存入字段，也属于关联。具有真实存储的自动属性（如 `GameSession.Clock`）按关联处理。

仅在方法参数、返回类型、局部对象、类型判断、静态调用或通过另一个对象继续访问其成员时使用某类型，按依赖处理。接口中的只读属性只声明能力，并不自动意味着接口保存了对象，所以 `IPlayerSession → GameSession` 画为依赖。

同一有向类型对已经存在继承、实现或关联时，不再补一条泛化的“依赖”线。A → B 与 B → A 分别判断，不因为方向相反就合并。未引入聚合/组合菱形，也不把 `new` 自动等同于严格生命周期组合。

## 讲解顺序

1. **先看组装点 `GameSceneController`。** 它创建 `GameSession` 并连接场景里的玩家、界面、日程、工具和演出组件，属于依赖集中的位置。
2. **再看接口。** 控制器实现 `IPlayerSession`、`IHudState`、`ISceneFeedback`、`ITabletProgram`。玩家、界面与石板保存自己需要的接口；`ListeningTool` 也实现 `ITabletProgram`，使同一石板组件能连接不同程序。
3. **再看交互继承。** 10 种交互物体继承 `Interactable`；其中石板和密码箱还共同实现近景接口。这分别是“是什么”和“能做什么”两种关系。
4. **最后看规则层。** `GameClock`、`RuneBoardModel`、`StableBeamMotion` 等处理时间、符文和运动规则；Unity 组件把规则结果连接到输入、场景和表现。Domain 的 asmdef 明确没有 Unity 引擎引用。

这些只是实际结构，不代表已完全消除耦合。`NineDayFlow`、`CipherBox`、`WorldPresentation`、`FinalTabletRetrieval` 等仍保存具体控制器；玩家的兼容查询/入口仍临时引用 `CipherBox` 和 `RuneTablet`，完整图保留了这些虚线依赖。不要据总览宣称所有系统只依赖接口。

## 覆盖范围与数量

- 游戏自有：48 份运行时 `.cs` 中声明 **56 个 class、5 个 interface、1 个 struct**，类型数量与文件数量不是一一对应。
- 56 个类中：29 个 Unity 场景组件（含抽象交互基类和 10 个派生类）、7 个 ScriptableObject 配置、3 个普通序列化数据类、17 个 Domain 规则/状态/静态算法类。
- 额外列出美术包的 7 个辅助类；这些类都直接继承 `MonoBehaviour`。在本次限定的自有类型集合内，没有找到它们与游戏自有类之间的 C# 字段或直接调用关系。Shader 全局参数可能形成渲染层联系，不能由类图推断没有运行效果。
- 合计 **69 个自定义类型**；外部终点另含 `MonoBehaviour`、`ScriptableObject` 和 `IDisposable`。
- 去重关系：**43 条继承、8 条实现、103 条关联、87 条依赖，共 241 条**。8 条实现中包含 `GameSession → IDisposable`。
- 枚举、Unity 组件字段（例如 `Transform` / `AudioSource`）、框架通用类型、Editor、Smoke、Tests 及模板 Readme 未展开。`ExplorationGuideState` 明确标记为 struct，不当作 class。

## 验证边界

使用本机 Roslyn 与项目的 Unity 引用集重新提取类型和关系，静态分析诊断错误数为 0。按新旧类名映射比对，69 个类型及 241 条关系一致。Unity 原生加载与回归结果见 TASK-019。

以下索引方便从图中的英文类名找到用途和源文件。具体实现以当前源码为准，未来改代码后应重新生成这份快照。

## 类型索引

| 类型 | 种类 | 用途 | 源码 |
| --- | --- | --- | --- |
| `AGGlobalSettings` | Class | 外部美术包：全局树木着色参数 | `Assets/ImportAsset/SnowEnviroment/Nature Pack - Common/Scripts/AGGlobalSettings.cs:8` |
| `AGGlobalSnowGrass` | Class | 外部美术包：草的全局积雪着色参数 | `Assets/ImportAsset/SnowEnviroment/Nature Pack - Common/Scripts/AGGlobalSnowGrass.cs:9` |
| `AGGlobalSnowHeight` | Class | 外部美术包：全局积雪高度参数 | `Assets/ImportAsset/SnowEnviroment/Nature Pack - Common/Scripts/AGGlobalSnowHeight.cs:9` |
| `AGGlobalSnowProps` | Class | 外部美术包：物件的全局积雪着色参数 | `Assets/ImportAsset/SnowEnviroment/Nature Pack - Common/Scripts/AGGlobalSnowProps.cs:9` |
| `AGGlobalSnowTree` | Class | 外部美术包：树木的全局积雪着色参数 | `Assets/ImportAsset/SnowEnviroment/Nature Pack - Common/Scripts/AGGlobalSnowTree.cs:9` |
| `AGGlobalWind` | Class | 外部美术包：全局风着色参数 | `Assets/ImportAsset/SnowEnviroment/Nature Pack - Common/Scripts/AGGlobalWind.cs:9` |
| `AttributeOperations` | Class / static | 属性交换与转移的静态算法 | `Assets/Demon/Scripts/Runtime/Domain/GameRules.cs:10` |
| `BeamDefinition` | Class | 光束距离、稳定时间和提示等配置 | `Assets/Demon/Scripts/Runtime/BeamDefinition.cs:4` |
| `BeamTether` | Class | 光束输入、瞄准和运动牵制的 Unity 连接 | `Assets/Demon/Scripts/Runtime/BeamTether.cs:7` |
| `BearingCipher` | Class / static | 方位到密码的静态计算 | `Assets/Demon/Scripts/Runtime/Domain/BearingCipher.cs:4` |
| `ChaseController` | Class | NavMesh 追逐、冻结、退回等场景行为 | `Assets/Demon/Scripts/Runtime/ChaseController.cs:7` |
| `ChaseDefinition` | Class | 追逐范围、距离、速度等配置 | `Assets/Demon/Scripts/Runtime/ChaseDefinition.cs:4` |
| `CipherBox` | Class | 箱子 A 密码近景、开锁与取得工具 | `Assets/Demon/Scripts/Runtime/CipherBox.cs:6` |
| `CipherDefinition` | Class | 方位密码和箱子 A 的配置 | `Assets/Demon/Scripts/Runtime/CipherDefinition.cs:5` |
| `CipherDigit` | Class | 密码数字命中目标和显示 | `Assets/Demon/Scripts/Runtime/CipherDigit.cs:4` |
| `CipherWheel` | Class | 方位轮盘交互与密码提示 | `Assets/Demon/Scripts/Runtime/CipherWheel.cs:6` |
| `ClosePassGate` | Class | 近身通过及其复位的纯规则 | `Assets/Demon/Scripts/Runtime/Domain/PursuitRules.cs:4` |
| `DayPlan` | Class | 一天的标题、提示和醒来时间数据 | `Assets/Demon/Scripts/Runtime/NineDayDefinition.cs:6` |
| `DiscoveryProgress` | Class | 已发现/阅读石板的纯状态 | `Assets/Demon/Scripts/Runtime/Domain/DiscoveryProgress.cs:5` |
| `ExplorationDefinition` | Class | 探索文案、观察记录和有序提示规则配置 | `Assets/Demon/Scripts/Runtime/ExplorationDefinition.cs:13` |
| `ExplorationFlow` | Class | 探索进度、记录交互、提示状态和踪迹表现 | `Assets/Demon/Scripts/Runtime/ExplorationFlow.cs:6` |
| `ExplorationGuidance` | Class / static | 按状态与有序规则选择提示键 | `Assets/Demon/Scripts/Runtime/Domain/ExplorationGuidance.cs:34` |
| `ExplorationGuideRule` | Class | 一条提示匹配规则的数据类 | `Assets/Demon/Scripts/Runtime/Domain/ExplorationGuidance.cs:17` |
| `ExplorationGuideState` | Struct | 提示选择使用的临时状态快照，struct 而非 class | `Assets/Demon/Scripts/Runtime/Domain/ExplorationGuidance.cs:28` |
| `FinaleSequence` | Class | 午夜窗口、阅读后倒计时和终局演出 | `Assets/Demon/Scripts/Runtime/FinaleSequence.cs:9` |
| `FinaleTimeWindow` | Class | 终局时间预约与限制的纯规则 | `Assets/Demon/Scripts/Runtime/Domain/FinaleTimeWindow.cs:7` |
| `FinalTabletDock` | Class | 工具棚空台的 T07 放置交互入口 | `Assets/Demon/Scripts/Runtime/FinalTabletDock.cs:4` |
| `FinalTabletProgress` | Class | T07 已取得/已放置的纯状态 | `Assets/Demon/Scripts/Runtime/Domain/FinalTabletProgress.cs:4` |
| `FinalTabletRetrieval` | Class | T07 取得、带回、放置和可重构状态的连接 | `Assets/Demon/Scripts/Runtime/FinalTabletRetrieval.cs:6` |
| `FirstDayInspectionLog` | Class | 首日观察进度的纯状态 | `Assets/Demon/Scripts/Runtime/Domain/FirstDayInspectionLog.cs:5` |
| `FixedTabletPlan` | Class | 固定石板的发现日、日期节点和记录数据 | `Assets/Demon/Scripts/Runtime/NineDayDefinition.cs:12` |
| `GameClock` | Class | 统一游戏时间与跨午夜通知 | `Assets/Demon/Scripts/Runtime/Domain/GameRules.cs:35` |
| `GameHud` | Class | 把状态显示到已保存的 uGUI 对象 | `Assets/Demon/Scripts/Runtime/GameHud.cs:7` |
| `GameSceneController` | Class | 场景组装点：创建会话、注入系统依赖并推进时间 | `Assets/Demon/Scripts/Runtime/GameSceneController.cs:8` |
| `GameSession` | Class | 封印交换、固化、睡眠和降临等会话规则 | `Assets/Demon/Scripts/Runtime/Domain/GameRules.cs:157` |
| `GameSettings` | Class | 起始时间、移动、交互、符文和文本等共享配置 | `Assets/Demon/Scripts/Runtime/GameSettings.cs:7` |
| `ICloseViewInteraction` | Interface / abstract | 近景确认与点击的统一契约 | `Assets/Demon/Scripts/Runtime/SceneContracts.cs:36` |
| `IHudState` | Interface / abstract | 界面需要的消息、正文和结束状态契约 | `Assets/Demon/Scripts/Runtime/SceneContracts.cs:21` |
| `Interactable` | Class / abstract | 交互提示和 Use 操作的抽象基类 | `Assets/Demon/Scripts/Runtime/Interactable.cs:6` |
| `IPlayerSession` | Interface / abstract | 玩家需要的会话、配置及阅读关闭契约 | `Assets/Demon/Scripts/Runtime/SceneContracts.cs:13` |
| `ISceneFeedback` | Interface / abstract | 发送消息、打开记录阅读的契约 | `Assets/Demon/Scripts/Runtime/SceneContracts.cs:7` |
| `ITabletProgram` | Interface / abstract | 石板的规则盘、开始操作和固化契约 | `Assets/Demon/Scripts/Runtime/SceneContracts.cs:28` |
| `LC_Door` | Class | 外部美术包：小屋门的辅助行为 | `Assets/ImportAsset/LogCabin/Source/Scripts/LC_Door.cs:4` |
| `ListeningChime` | Class | 进入监测区域后的响铃与摆动 | `Assets/Demon/Scripts/Runtime/ListeningChime.cs:7` |
| `ListeningDefinition` | Class | 监听符文和相关配置 | `Assets/Demon/Scripts/Runtime/ListeningDefinition.cs:6` |
| `ListeningTool` | Class | T05 监听石板程序与风铃激活 | `Assets/Demon/Scripts/Runtime/ListeningTool.cs:7` |
| `ListeningTrailProxy` | Class | 独立教学场景使用的监测目标移动代理 | `Assets/Demon/Scripts/Runtime/ListeningTrailProxy.cs:7` |
| `NineDayDefinition` | Class | 九天日程与固定石板的共享配置 | `Assets/Demon/Scripts/Runtime/NineDayDefinition.cs:19` |
| `NineDayFlow` | Class | 九天日程、发现状态、阅读与可用性 | `Assets/Demon/Scripts/Runtime/NineDayFlow.cs:7` |
| `NineDayTablet` | Class | 九天流程中的固定石板交互入口 | `Assets/Demon/Scripts/Runtime/NineDayTablet.cs:5` |
| `ObservationText` | Class | 观察点记录文字的数据类 | `Assets/Demon/Scripts/Runtime/ExplorationDefinition.cs:6` |
| `PickupAttempt` | Class | 持续拾取进度、中断和完成规则 | `Assets/Demon/Scripts/Runtime/Domain/PursuitRules.cs:28` |
| `PlayerController` | Class | 玩家移动、输入、射线交互和各类视角 | `Assets/Demon/Scripts/Runtime/PlayerController.cs:8` |
| `PreparationRecord` | Class | D7–D8 准备记录的交互入口 | `Assets/Demon/Scripts/Runtime/PreparationRecord.cs:9` |
| `RecordBox` | Class | 箱子 B 的阅读交互入口 | `Assets/Demon/Scripts/Runtime/RecordBox.cs:6` |
| `RegionEntryLatch` | Class | 区域进入边沿检测，避免连续重复触发 | `Assets/Demon/Scripts/Runtime/Domain/RegionEntryLatch.cs:4` |
| `RuneBoardModel` | Class | 符文盘操作、匹配和固化快照 | `Assets/Demon/Scripts/Runtime/Domain/GameRules.cs:72` |
| `RuneSlot` | Class | 石板槽位命中标识和显示引用 | `Assets/Demon/Scripts/Runtime/RuneSlot.cs:6` |
| `RuneTablet` | Class | 可编辑石板的近景、符文命中与程序调用 | `Assets/Demon/Scripts/Runtime/RuneTablet.cs:7` |
| `RuneTrayItem` | Class | 符文盘物品命中标识和显示引用 | `Assets/Demon/Scripts/Runtime/RuneTrayItem.cs:7` |
| `SceneFontBinding` | Class | 运行时字体解析与绑定 | `Assets/Demon/Scripts/Runtime/SceneFontBinding.cs:9` |
| `ScheduledWorldAttributes` | Class | 按日期应用声音/光属性的纯规则 | `Assets/Demon/Scripts/Runtime/Domain/ScheduledWorldAttributes.cs:4` |
| `SleepPoint` | Class | 主动睡眠交互入口 | `Assets/Demon/Scripts/Runtime/SleepPoint.cs:6` |
| `StableBeamMotion` | Class | 稳定照射、锁定和运动属性规则 | `Assets/Demon/Scripts/Runtime/Domain/StableBeamMotion.cs:5` |
| `StationObservation` | Class | 电台、风向标、湖等观察点的交互入口 | `Assets/Demon/Scripts/Runtime/StationObservation.cs:5` |
| `ThreatPickup` | Class | 按住拾取的场景交互与回调 | `Assets/Demon/Scripts/Runtime/ThreatPickup.cs:7` |
| `ThreeDigitLock` | Class | 三位密码锁的纯规则 | `Assets/Demon/Scripts/Runtime/Domain/BearingCipher.cs:25` |
| `WorldAttributeConsequences` | Class | 声音属性与太阳光属性的场景表现 | `Assets/Demon/Scripts/Runtime/WorldAttributeConsequences.cs:6` |
| `WorldPresentation` | Class | 把会话状态映射到箱盖、封印、灯光等场景表现 | `Assets/Demon/Scripts/Runtime/WorldPresentation.cs:6` |
