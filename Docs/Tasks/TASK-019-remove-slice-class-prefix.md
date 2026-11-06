# TASK-019 运行时代码和工具类命名迁移
Status: done
Updated: 2026-11-05 Asia/Shanghai

## 范围与验收

将自有代码中含 Slice 的 18 个类改为职责名称，审查 Assets 中其余类的命名。脚本路径与类名同步，使用原 `.meta` 和 GUID。场景、预制体与配置需要能正常加载，核心规则与九天正常路线和全睡眠路线通过。既有程序集、命名空间、场景和资源路径作为兼容标识保留。

## 实施顺序

先保存当前工作区基线，记录 Unity 读取到的脚本和属性引用。迁移类和类型引用，保留序列化字段和资源身份，只定向迁移 YAML 中的脚本类标识。用 Unity 6000.6.0f1 重载并比较场景、预制体和配置，运行现有规则测试和 Play Mode 回归，最后更新类图、命名审查和交接记录。

## 改名对照

| 原名 | 新名 | 命名依据 |
| --- | --- | --- |
| `SliceSceneController` | `GameSceneController` | 组装场景依赖、推进游戏流程 |
| `SlicePlayer` | `PlayerController` | 玩家输入、移动与视角控制 |
| `SliceTablet` | `RuneTablet` | 可编辑符文石板，与只读 `NineDayTablet` 区分 |
| `SliceHud` | `GameHud` | 游戏中的状态与提示界面 |
| `SliceSettings` | `GameSettings` | 时钟、操作、石板、照明与文案配置 |
| `SliceBox` | `RecordBox` | 提供箱内记录的箱子 B，与 `CipherBox` 区分 |
| `SliceBed` | `SleepPoint` | 触发睡到次日的场景交互点 |
| `SliceInteractable` | `Interactable` | 可交互物体的抽象基类 |
| `SliceFinaleSequence` | `FinaleSequence` | 编排午夜终局演出 |
| `SliceFontBinding` | `SceneFontBinding` | 给场景内已保存的文字对象绑定字体 |
| `SlicePresentation` | `WorldPresentation` | 按规则状态表现箱盖、封印与环境光 |
| `SliceRuneSlot` | `RuneSlot` | 石板上的符文槽 |
| `SliceRuneTrayItem` | `RuneTrayItem` | 托盘内的可选符文 |
| `SliceSession` | `GameSession` | 维护一局游戏的纯 C# 规则与状态 |
| `DemonSliceAuthoring` | `PrototypeSceneAuthoring` | 旧灰盒场景的编辑器作者工具 |
| `DemonSliceRequestRunner` | `EditorRequestRunner` | 执行编辑器请求白名单 |
| `SliceSmokeRun` | `PrototypeSmokeRun` | 旧灰盒场景的冒烟验证 |
| `SliceRuleTests` | `CoreGameRuleTests` | 时钟、符文、封印等核心规则测试 |

## 命名审查

以 `Assets` 下的 C# 类声明为范围，包含嵌套类。共 110 个类，其中自有 100 个、外部资源与 Unity 模板 10 个。完整逐类结果见[命名清单](../Diagrams/class-naming-review.csv)。自有类全部符合 PascalCase，也就是单词首字母大写且没有下划线。

| 类别 | 数量 | 审查结果 |
| --- | ---: | --- |
| Unity 运行时组件与配置 | 39 | 组件、配置和记录类型有一致的职责词，例如 `ChaseController`、`ChaseDefinition`、`PreparationRecord`。本轮改名消除玩家、石板、箱子和睡眠入口的旧前缀。 |
| Domain 规则 | 17 | 名称指向规则或状态，例如 `GameClock`、`FinaleTimeWindow`、`ThreeDigitLock`。`GameSession` 明确表达一局会话。 |
| Editor 工具 | 33 | `Authoring` 表示制作或配置，`SmokeRun` 表示快速流程验证。内部 `Request`、`Report`、`TestCallbacks` 由外层 `EditorRequestRunner` 限定语义。 |
| 测试 | 11 | 均以 `Tests` 结尾，主体名称对应被验证的规则。 |
| 美术辅助和模板 | 10 | 7 个美术包类、3 个模板类。`LC_Door` 与项目的 PascalCase 风格不同，沿用供应方命名。 |

几个现名需要一些玩法背景才能读懂。`ThreatPickup` 实际是按住键持续拾取，可考虑 `HoldPickupInteraction`。`ClosePassGate` 控制近身通过与冷却，可考虑 `ClosePassTrigger`。`ObservationText` 是观察站的多状态文案，可考虑 `StationObservationText`。`WorldAttributeConsequences` 把声音和光属性变化应用到表现，可考虑 `WorldAttributePresentation`。这四项为命名审查建议，当前沿用现名。

## 迁移与验证

- 基线在 `Logs/DemonCheckpoints/task019-before-class-rename-20261105`，包含原有未提交改动。Roslyn 精确迁移 62 份源码中的类型标识。18 对脚本和 `.meta` 同步改名，包含保存 `GameSession` 等规则的 `GameRules.cs`。原 `.meta` 逐字节保留。
- 13 个 Unity 组件或配置类保留旧类名的 `MovedFrom` 迁移信息。27 个文本资产共迁移 267 行 `m_EditorClassIdentifier`，限定原脚本 GUID 与类型对应。场景、预制体、设置路径、程序集、命名空间、菜单和编辑器请求协议沿用原标识。`FinalTabletAuthoring` 的类型查询改用 `nameof(GameSettings)`。
- Unity 6000.6.0f1 原生加载前后分别记录 9,983 条脚本对象、序列化引用和场景/预制体信息，快照 SHA256 一致。加载 12 个场景与 3 个预制体，无缺失脚本。检查工具初次使用的旧 InstanceID 属性在该 Unity 版本已不可用，改用 EntityId 后完成采集。
- `rename-001` 的 81 项 EditMode 测试、九天场景验证、九天正常流程、全睡眠和预约后睡眠均通过。`rename-002-smoke-normal` 通过旧 D8 场景的石板、箱子和 D9 结束流程。
- 批处理模式下 `smoke-input` 无法锁定光标。常规编辑器首次运行与资源索引重叠，W 移动距离未达到测试阈值。索引完成后同一代码、同一测试 `rename-004-smoke-input` 通过 W/E、六次符文射线点击、Enter、开箱、阅读和 Esc。
- 重新从源码生成 UML。映射新旧类名后 69 个自定义类型及 241 条关系一致。见[现行图与索引](../Diagrams/runtime-classes.md)。
- 最终文件审计覆盖 367 个工程文件。代码符合预先生成的标识迁移，资产逆映射后与原件字节一致，无额外修改、丢失 `.meta` 或残留探针。证据在 `Logs/DemonChecks/Task019`。本轮验证覆盖 Unity 自动流程与输入，人工连续九天试玩和独立 Windows 包未重新执行。
