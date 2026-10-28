# Demon 当前工程档案

Updated: 2026-10-28 Asia/Shanghai
本文件记录已观察或已创建的工程结构，不把建议组件写成已有架构。玩法以 [GAME_SPEC.md](GAME_SPEC.md) 为准；拟实施模块和选择理由见 [TASK-002](Tasks/TASK-002-playable-slice.md)。

## 1 已观察的工程配置

| 项目 | 当前证据 |
| --- | --- |
| 项目与工作目录 | `D:\Unity\Projects\Demon`，Unity根与工作根相同 |
| Editor | `6000.6.0f1`，revision `f7f8ed4d1e24`，来源 `ProjectSettings/ProjectVersion.txt` |
| 产品名 | `Demon`；公司名仍为 `DefaultCompany` |
| URP | manifest与lock均为 `com.unity.render-pipelines.universal` 17.6.0 |
| 质量设置 | `QualitySettings.asset` 当前索引1，名PC；Standalone默认质量索引1 |
| PC管线资产 | `Assets/Settings/PC_RPAsset.asset`，GUID `4b83569d67af61e458304325a23e5dfd`，由质量项引用 |
| Graphics全局管线字段 | `m_CustomRenderPipeline`为0，但质量项有URP覆盖；不能仅凭该0值判断项目使用Built-in |
| 输入 | Input System 1.20.0；`activeInputHandler: 1`；已有全局Actions资源 |
| 启动场景 | `Assets/Demon/Scenes/DemonNineDay.unity`置首；DemonSlice与原SampleScene保留 |
| 游戏模块 | 已创建九天流程骨架及D8到D9单独验证场景，复用规则/玩家/箱子/时钟/uGUI与占位终局 |
| 版本控制 | 已按用户授权初始化Git，分支main，origin为 `https://github.com/TruthORZ/Demon.git`；提交与推送验证见TASK-003 |
| 构建平台 | 用户确认Windows，StandaloneWindows64；旧样例构建已通过，九天版本结果见TASK-004 |

原模板场景仍有Main Camera、Directional Light和Global Volume，未改写。新增灰盒场景由现有Editor原生API保存，含可编辑几何、相机、灯光、碰撞、石板与箱子，不在Awake/Start生成地图。

## 2 包与资产来源

已核对 `Packages/manifest.json` 与 `Packages/packages-lock.json`，并看到相关 `Library/PackageCache` 目录。缓存存在不等于运行验证已通过。

| 包 | manifest和lock记录版本 | 当前用途 |
| --- | --- | --- |
| Universal RP | 17.6.0 | 已有渲染配置 |
| Input System | 1.20.0 | 已配置为输入后端 |
| Timeline | 6.6.0 | 已安装，当前样例采用少量显式状态过渡，无Timeline资产 |
| uGUI | 2.6.0 | 样例使用保存于场景的Canvas/Text/Panel，运行时只绑定数据与切换状态 |
| Test Framework | 1.8.0 | 项目EditMode程序集Demon.Slice.Tests，当前26项规则用例 |
| AI Navigation | 2.0.14 | 已安装，无项目导航或追逐实现 |
| Visual Scripting | 1.9.12 | 已安装，尚未用于玩法 |

没有安装或移除包，没有切换管线或输入。新项目原先没有玩法UI，首个样例选已装uGUI，便于团队编辑提示与阅读面板。Cinemachine未列入manifest，镜头过渡使用场景Transform锚点，不新增摄像机依赖。

`Assets/Settings/DefaultVolumeProfile.asset`静态可见部分`m_Script: {fileID: 0}`及Editor.Tests类型条目。尚未检查它们在Editor中的有效性或运行影响；属于原模板基线待查项，本次未修改或清理。

## 3 已有输入与程序集

`Assets/InputSystem_Actions.inputactions`包含Player与UI映射；Player中有Move、Look、Interact、Sprint等模板动作，也有Attack、Jump、Crouch等未获玩法批准的动作。Interact模板当前有Hold交互，不能不加检查就当成按一下立即交互。

Actions由 `ProjectSettings/EditorBuildSettings.asset` 的configObjects引用。样例玩家引用同一资产，在运行时克隆并只启用Player映射，销毁时清理；使用WasPressedThisFrame响应E的物理按下，不等待模板Hold完成。没有改写原Actions资源。Attack、Jump等动作未接入玩法。

新增四个asmdef：

- `Demon.Slice.Domain`：`Scripts/Runtime/Domain`，无Unity引擎引用，负责RuneBoardModel、GameClock、SliceSession及原子属性操作。
- `Demon.Slice`：`Scripts/Runtime`，引用Domain、Unity.InputSystem、UnityEngine.UI，负责场景组件与表现。
- `Demon.Slice.Editor`：`Scripts/Editor`，仅Editor，引用项目运行时/Domain、UI、Input及测试Runner，负责明确的作者化与验证请求。
- `Demon.Slice.Tests`：`Tests/EditMode`，仅Editor测试程序集，引用Domain，不依赖玩法UI或场景。

模板Readme脚本保持原默认程序集，无程序集迁移。

## 4 目录与资源位置

保留模板的 `Assets/Scenes`、`Assets/Settings`、`Assets/TutorialInfo` 以及根Actions资源，未移动、覆盖或删除。新增项目内容统一预留在 `Assets/Demon`，避免混入模板与未来外部资源。

| 目录 | 当前状态与归属 |
| --- | --- |
| `Assets/Demon/Scenes` | DemonNineDay.unity为九天骨架，DemonSlice.unity保留D8快速验证 |
| `Assets/Demon/Scripts/Runtime` | 场景组件与Domain规则，按实际边界分程序集 |
| `Assets/Demon/Scripts/Editor` | DemonSliceAuthoring和DemonSliceRequestRunner，作者化和验证工具 |
| `Assets/Demon/Prefabs` | SlicePlayer、SliceTablet、SliceBox，均已连接场景实例 |
| `Assets/Demon/Data` | SliceSettings旧样例、NineDaySettings九天时间/交互配置、NineDayDefinition日程/阅读数据；运行状态单独创建 |
| `Assets/Demon/Art` | Materials中的简单URP占位材质，无正式美术资产 |
| `Assets/Demon/Audio` | PrototypeClockTick/PrototypeBoxSlam为保存的合成WAV占位音效，可在Inspector替换；无正式音频/混音 |
| `Assets/Demon/UI` | WindowsChinese.fontsettings为字体名称配置，不含Windows字体二进制；运行时解析后已确认中文显示 |
| `Assets/Demon/Tests/EditMode` | SliceRuleTests、NineDayRuleTests、FinaleTimeWindowTests，26项用例 |
| `Assets/Demon/Tests/PlayMode` | 空目录，无测试和asmdef |
| `Docs`与`Docs/Tasks` | 设计、工程档案、任务与交接，按AGENTS规定分工 |

Unity已经导入并生成这些目录与新增资源的meta，保留此前由用户Editor生成的目录GUID。没有手写meta或场景YAML。Git仍不跟踪剩余空目录；资源和meta应一起纳入后续提交，本轮未自动提交或push。

## 5 当前组合与兼容边界

场景组合点为 `SliceComposition` 上的 `SliceSceneController`，显式引用Settings、Player、Tablet、Presentation、HUD、EndingView和SliceFinaleSequence；九天场景另有NineDayFlow。它创建纯规则会话，按自然时间、主动睡眠或演出请求推进统一GameClock；石板不再推进时间，不持有全局静态服务。

Box和SleepPoint交互组件的world字段绑定同一场景Composition；Box是Prefab实例，场景连接必须保存为实例覆盖，不能写入可复用Prefab中的场景引用。作者工具及repair-box通过Unity序列化API持久化该连接，validate显式检查Box/Bed必填连接。开箱回归必须经过SliceBox.Use或实际E键，不能只调用控制器ReadBox。

`SlicePlayer`处理CharacterController、输入与探索/近景/阅读/返回/终局视角；`SliceInteractable`是石板、箱子、睡袋共用的小型能力边界。石板的Slot和Tray组件保存可点选目标。`SlicePresentation`仅将规则状态映射为箱盖、封印、实体和时间驱动灯光；`SliceHud`更新已保存uGUI对象。`SliceFontBinding`负责Windows字体的原生句柄解析，不创建UI或关卡。

日长、起始日时、醒来时刻、移动速度、交互距离、布局和文本在对应SliceSettings/NineDaySettings中调整。旧ReadingGraceSeconds/AccelerationSeconds/AccelerationCurve已移除。近景机位在Tablet/CloseView，箱子机位在Box/ReadView，终局机位在Environment/EndingView。午夜演出的时间窗口、过渡、倒计时、音效与引用在SliceComposition/SliceFinaleSequence中调整；挂钟/机位/灯光在Finale_Authored。UI布局仍在SliceHUD下编辑。

程序固化在RuneBoardModel中保留不可变快照；表面符文可移除，快照不变。GameClock按经过的零点顺序通知，SliceSession以日期集合避免重复交换，固化立即执行一轮但不改变时间，下一零点才执行下一轮，终局停止Clock、不继续D10。构造参数可指定初始湖封闭及最早重构日期。RuneKind明确保留SEAL=6、BOX=7、LAKE=8等既有序列化数值，已退役时间符文的值5不再分配。未实现通用IF/ON解释器，固定前三块实际效果仍未接入。

九天场景在同一Composition上增加NineDayFlow，引用NineDayDefinition、六个NineDayTablet及T07根对象；SliceSceneController显式配置该可选组件，未配置时沿用旧D8行为。NineDayFlow管理发现可用性、阅读计数、每日提示及醒来表，DiscoveryProgress为纯C#会话状态。普通阅读经ReadDocument进入同一阅读视角/面板，不设置HasReadBox。HUD阅读时隐藏日程提示，避免盖住正文。

NineDaySettings起始D1 08:00、日长1440真实秒、湖初始封闭；NineDayDefinition拥有T07开放日（8）及FinalTabletHour（22:00），Flow按日期与时刻判断可用，控制器拒绝早间编译。D1夜晚开封暂定22:00，可调且没有邪教徒/暴风雪演出。自然经过该时刻由Flow评估，睡眠跳过时在跨日检查中补上状态；没有通用任意时刻事件调度器。醒来表仍均暂定08:00，每日可调。

FixedTablets_Authored保存六块只读占位石板及ReadView机位，按D2至D7开放；T07在D8指定夜间开放，可不读前六块直接重构或睡到D9。记录已同步运动/声音/光顺序及T05监听、T06光束机制，但实际前三块设施、T05补全/风铃、箱子A/轮盘、T06手电和追逐仍未实现。T06现在仍放在只读陈列区，不能把它当作已经完成密码箱取得方式。T04由预定实体状态代替，没有确定其来源。没有正式九天关卡、存档或追逐。

SliceFinaleSequence负责保留窗口与镜头表现，依赖独立的纯C# FinaleTimeWindow和显式World/Presentation连接。第一次T07交互即预约窗口，普通帧推进最多到午夜前一游戏分钟；很晚才操作时取当前时刻，绝不倒退。关闭真正的箱内记录后向前对齐保留时刻，短暂停留→挂钟6秒倒计时→D9状态变化/猛合箱盖与湖光→短暂箱子机位→EndingView。普通石板阅读不会触发结局。主动睡眠释放窗口，不进入这段导演流程。

规则终局Session.Descended和结局显示EndingPresented分开。Player有Sequence机位模式，此时不接受Esc/交互；HUD延迟显示结局面板，避免遮挡钟与箱盖。普通未修复睡眠路线仍立即进入结局。箱盖机械开合与BoxSeal仍分开，Presentation.SlamBox只处理表现。LakeOpenGlow为D7起且湖开时显示的占位光，不代表运动/声音/完整光能力系统已实现。

FinaleAuthoring在现有Editor中局部迁移两个场景与设置，生成保存的Finale_Authored和两条WAV，不修改共享石板Prefab。T07实例停用6号槽可视/碰撞及旧末段线，6号连线改为9→3，保留槽数组及GUID。迁移遇已有Finale拒绝覆盖；后续手调保存内容，不能再次生成。挂钟初次被开箱盖遮住，已通过限范围sightline工具将钟/机位移到右柱前；具体检查见TASK-005。

作者工具NineDayAuthoring以现有场景原生复制创建独立资产，拒绝覆盖已有输出。原DemonSlice场景SHA与TASK-004改动前备份相同；没有运行时生成关卡。新建配置经历资产导入后需重新LoadAssetAtPath获得持久化引用，不能继续使用已作废的创建句柄；nine-repair仅修复缺失settings连接并保存重载，不重建几何。

必须保留：已有meta/GUID、Actions引用、PC/Mobile管线资产与场景。以后采用哪个UI、怎样接入交互、如何划分程序集，应在功能实现并检查后记录到本文件；计划先放任务，不提前宣称已经落地。

## 6 已验证检查与未知命令

已执行并可复用的只读检查：

```powershell
Get-Content -LiteralPath 'ProjectSettings\ProjectVersion.txt'
Get-Content -LiteralPath 'Packages\manifest.json'
Get-Content -LiteralPath 'Packages\packages-lock.json'
Get-Content -LiteralPath 'ProjectSettings\EditorBuildSettings.asset'
rg -n 'activeInputHandler|productName' ProjectSettings/ProjectSettings.asset
rg -n 'm_CurrentQuality|customRenderPipeline' ProjectSettings/QualitySettings.asset
git status --short
```

首次目录初始化时，最后一条失败为not a git repository。之后用户提供GitHub地址并明确授权push，已初始化Git和main分支，设置origin。初始发布过程与结果见[TASK-003](Tasks/TASK-003-github-initial-push.md)。没有另外启用Unity Version Control。

版本控制文件：根`.gitignore`排除Library、Temp、Logs、UserSettings、构建产物和生成IDE工程；`.gitattributes`规定文本与二进制类型，未添加Git LFS依赖。源资源、meta、Packages及ProjectSettings纳入版本管理。Git身份沿用本机已配置值，未修改全局设置。

已观察到本项目Editor进程，程序路径为 `D:\unity\6000.6.0f1\Editor\Unity.exe`。没有启动竞争实例。真正Editor日志在项目 `Logs/Editor.log`，旧AppData日志只含启动转发，读取错误要以项目日志为准，不打印原始进程命令行或会话参数。

本机官方CLI路径为 `D:\Unity\Unity Hub\resources\unity.exe`，但项目没有Pipeline包，status/recompile无法连接。CLI提示可更新，未升级或安装依赖。当前通过现有Editor已编译的项目内工具执行，不借此启动第二个Editor或假装MCP可用。

菜单 `Demon > Slice > Create Graybox Scene`仅用于首次创建；场景存在时拒绝覆盖。外部自动化可写一次性 `Temp/DemonSliceAuthoring.request.json`，仅接受源码白名单的create/validate/refresh/fonts/tests/smoke/build等操作，目标固定在样例或忽略的Temp/Builds目录。请求消费后删除，无请求不写资源。后台Editor未导入新工具时需人工Assets/Refresh；工具更新后可用refresh请求。流程记录见TASK-002。

已经验证：Editor创建保存、四个程序集编译、14项EditMode、正常及未修复睡觉路线Play Mode、中文显示、Input System的W/E/六次鼠标选择/Enter输入链。交互射线忽略玩家自身碰撞体，避免近景被身体挡住。测试临时使用虚拟设备和焦点配置，结束恢复，不改变正常输入配置。

Windows64构建已通过，入口`Builds/Windows/DemonSlice.exe`，完整输出目录需要一起保留。Performance Testing临时Resources和Input System临时预加载设置已由构建后处理清理。Unity保存/构建更新的URP、GraphicsSettings等版本字段保留，未替换管线或包。

smoke-editability已实际编译运行：保存日长、机位和日光修改，验证Play尊重，再还原资产并通过正常路线重载。normal-003的近景、阅读页与终局截图已查看。人体感受、真人Esc/R及Windows独立程序试玩仍待用户验收；报告详见TASK-002。

Editor运行入口：双击DemonSlice场景，进入Play；点击Game视图捕获鼠标，WASD移动、Shift奔跑、E交互、近景鼠标点选、Enter确认、Esc退出，结局R重开。箱子和睡袋需靠近并对准。占位交互和预置不是正式谜题定稿。

九天入口：双击DemonNineDay。相同键鼠操作；左侧睡袋推进天数，右侧六块只读石板按天出现，D8 22:00正前方开放T07。NineDayDefinition的Days编辑提示/醒来，Tablets编辑日期/文本，FinalTabletHour编辑夜间时段；NineDaySettings编辑日长、移动等。固定石板机位在FixedTablets_Authored。快速结局入口为DemonSlice，现从D8 22:00启动，使用同一无+1演出。修改源场景/配置在退出Play后仍保留，不在运行时生成地图。

TASK-004检查：19项EditMode、nine-skip-001、nine-normal-001、旧场景nine-old-input-001均通过。nine-tuning-001保存D2醒来时刻10:00后验证运行采用，且固定石板机位被尊重；退出恢复08:00，已核对资产和恢复日志。九天构建输出为Builds/WindowsNineDay/DemonNineDay.exe，最终报告与剩余人工试玩见[TASK-004](Tasks/TASK-004-nine-day-flow.md)。

TASK-005检查：finale-migrate-002保存重载两套场景/设置通过；finale-tests-001的26项EditMode通过；finale-normal-001的新夜间门槛、保留窗口/8秒阅读、挂钟、二次交换/闭箱/湖光及延后结局通过。挂钟遮挡由finale-sightline-001修正，已查看新截图。finale-tuning-001保存3.5秒/机位横移后实际运行尊重并恢复6秒/原机位；finale-late-001极晚交互、finale-sleep-reserved-001窗口内主动睡眠、finale-skip-001全睡眠及finale-input-001实际输入链均通过。

本轮两套Windows输出已更新：finale-build-001九天版173927554字节，finale-quick-build-001快速版173914482字节。构建后finale-validate-001重载引用检查通过；原有meta哈希无变化、源文件无丢失，新增文件都有meta，git diff --check通过。构建添加了ProjectSettings.asset中的有效InputSystem Actions预加载引用，保留该包行为。完整独立exe真人试玩及恐怖感/节奏未验收；详细证据在[TASK-005](Tasks/TASK-005-no-plus-one-finale.md)和忽略的Temp/DemonSliceReports。
