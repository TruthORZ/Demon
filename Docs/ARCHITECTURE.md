# Demon 当前工程档案

Updated: 2026-11-11 Asia/Shanghai
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
| 启动场景 | TASK-023后 `Assets/Demon/Scenes/DemonNineDay_Art.unity` 置首；原灰盒九天、DemonSlice与SampleScene保留 |
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
| Test Framework | 1.8.0 | 项目EditMode程序集Demon.Slice.Tests，TASK-023后89项规则用例已通过 |
| AI Navigation | 2.0.14 | NavMeshSurface保存导航，主场景及DemonChase由NavMeshAgent追逐，见第10节 |
| Visual Scripting | 1.9.12 | 已安装，尚未用于玩法 |

没有安装或移除包，没有切换管线或输入。新项目原先没有玩法UI，首个样例选已装uGUI，便于团队编辑提示与阅读面板。Cinemachine未列入manifest，镜头过渡使用场景Transform锚点，不新增摄像机依赖。

`Assets/Settings/DefaultVolumeProfile.asset`静态可见部分`m_Script: {fileID: 0}`及Editor.Tests类型条目。尚未检查它们在Editor中的有效性或运行影响；属于原模板基线待查项，本次未修改或清理。

## 3 已有输入与程序集

`Assets/InputSystem_Actions.inputactions`包含Player与UI映射；Player中有Move、Look、Interact、Sprint等模板动作，也有Attack、Jump、Crouch等未获玩法批准的动作。Interact模板当前有Hold交互，不能不加检查就当成按一下立即交互。

Actions由 `ProjectSettings/EditorBuildSettings.asset` 的configObjects引用。样例玩家引用同一资产，在运行时克隆并只启用Player映射，销毁时清理；使用WasPressedThisFrame响应E的物理按下，不等待模板Hold完成。没有改写原Actions资源。Attack、Jump等动作未接入玩法。

新增四个asmdef：

- `Demon.Slice.Domain`：`Scripts/Runtime/Domain`，无Unity引擎引用，负责RuneBoardModel、GameClock、GameSession及原子属性操作。
- `Demon.Slice`：`Scripts/Runtime`，引用Domain、Unity.InputSystem、UnityEngine.UI，负责场景组件与表现。
- `Demon.Slice.Editor`：`Scripts/Editor`，仅Editor，引用项目运行时/Domain、UI、Input及测试Runner，负责明确的作者化与验证请求。
- `Demon.Slice.Tests`：`Tests/EditMode`，仅Editor测试程序集，引用Domain，不依赖玩法UI或场景。

模板Readme脚本保持原默认程序集，无程序集迁移。

## 4 目录与资源位置

保留模板的 `Assets/Scenes`、`Assets/Settings`、`Assets/TutorialInfo` 以及根Actions资源，未移动、覆盖或删除。新增项目内容统一预留在 `Assets/Demon`，避免混入模板与未来外部资源。

| 目录 | 当前状态与归属 |
| --- | --- |
| `Assets/Demon/Scenes` | DemonNineDay.unity为九天骨架，DemonSlice.unity保留D8结局，DemonListening.unity为D6风铃快速测试，DemonCipher.unity为D1密码快速测试，DemonBeam.unity为D7光束快速测试 |
| `Assets/Demon/Scripts/Runtime` | 场景组件与Domain规则，按实际边界分程序集 |
| `Assets/Demon/Scripts/Editor` | PrototypeSceneAuthoring和EditorRequestRunner，作者化和验证工具 |
| `Assets/Demon/Prefabs` | SlicePlayer、SliceTablet、SliceBox，均已连接场景实例 |
| `Assets/Demon/Data` | SliceSettings旧样例、NineDaySettings九天时间/交互配置、NineDayDefinition日程/阅读数据；运行状态单独创建 |
| `Assets/Demon/Art` | Materials中的简单URP占位材质，无正式美术资产 |
| `Assets/Demon/Audio` | PrototypeClockTick/PrototypeBoxSlam及PrototypeChimeLow/High为保存的合成WAV占位音效，可在Inspector替换；无正式音频/混音 |
| `Assets/Demon/UI` | WindowsChinese.fontsettings为字体名称配置，不含Windows字体二进制；运行时解析后已确认中文显示 |
| `Assets/Demon/Tests/EditMode` | 12份规则测试，含DailyProgressTests与原有规则测试，当前89项用例 |
| `Assets/Demon/Tests/PlayMode` | 空目录，无测试和asmdef |
| `Docs`与`Docs/Tasks` | 设计、工程档案、任务与交接，按AGENTS规定分工 |

Unity已经导入并生成这些目录与新增资源的meta，保留此前由用户Editor生成的目录GUID。没有手写meta或场景YAML。Git仍不跟踪剩余空目录；资源和meta应一起纳入后续提交，本轮未自动提交或push。

## 5 当前组合与兼容边界

场景组合点为 `SliceComposition` 上的 `GameSceneController`，显式引用Settings、Player、Tablet、Presentation、HUD、EndingView和FinaleSequence；九天场景另有NineDayFlow。它创建纯规则会话，按自然时间、主动睡眠或演出请求推进统一GameClock；石板不再推进时间，不持有全局静态服务。

Box和SleepPoint交互组件的world字段绑定同一场景Composition；Box是Prefab实例，场景连接必须保存为实例覆盖，不能写入可复用Prefab中的场景引用。作者工具及repair-box通过Unity序列化API持久化该连接，validate显式检查Box/Bed必填连接。开箱回归必须经过RecordBox.Use或实际E键，不能只调用控制器ReadBox。

`PlayerController`处理CharacterController、输入与探索/近景/阅读/返回/终局视角；`Interactable`是石板、箱子、睡袋共用的小型能力边界。石板的Slot和Tray组件保存可点选目标。`WorldPresentation`仅将规则状态映射为箱盖、封印、实体和时间驱动灯光；`GameHud`更新已保存uGUI对象。`SceneFontBinding`负责Windows字体的原生句柄解析，不创建UI或关卡。

日长、起始日时、醒来时刻、移动速度、交互距离、布局和文本在对应 `SliceSettings.asset` / `NineDaySettings.asset` 中调整。旧ReadingGraceSeconds/AccelerationSeconds/AccelerationCurve已移除。近景机位在Tablet/CloseView，箱子机位在Box/ReadView，终局机位在Environment/EndingView。午夜演出的时间窗口、过渡、倒计时、音效与引用在SliceComposition/FinaleSequence中调整；挂钟/机位/灯光在Finale_Authored。UI布局仍在SliceHUD下编辑。

程序固化在RuneBoardModel中保留不可变快照；表面符文可移除，快照不变。GameClock按经过的零点顺序通知，GameSession以日期集合避免重复交换，固化立即执行一轮但不改变时间，下一零点才执行下一轮，终局停止Clock、不继续D10。构造参数可指定初始湖封闭及最早重构日期。RuneKind明确保留SEAL=6、BOX=7、LAKE=8等既有序列化数值，已退役时间符文的值5不再分配。未实现通用IF/ON解释器，运动已有第9节的D3状态接点；前三块尚无通用程序解释器/完整剧情；D3运动已有接点，D5声音/D7失光与永夜见第12节。

九天场景在同一Composition上增加NineDayFlow，引用NineDayDefinition、六个NineDayTablet及T07根对象；GameSceneController显式配置该可选组件，未配置时沿用旧D8行为。NineDayFlow管理发现可用性、阅读计数、每日提示及醒来表，DiscoveryProgress为纯C#会话状态。普通阅读经ReadDocument进入同一阅读视角/面板，不设置HasReadBox。HUD阅读时隐藏日程提示，避免盖住正文。

NineDaySettings起始D1 08:00、日长1440真实秒、湖初始封闭；NineDayDefinition拥有T07开放日（8）及FinalTabletHour（22:00），Flow按日期与时刻判断可用，控制器拒绝早间编译。D1夜晚开封暂定22:00，可调且没有邪教徒/暴风雪演出。自然经过该时刻由Flow评估，睡眠跳过时在跨日检查中补上状态；没有通用任意时刻事件调度器。醒来表仍均暂定08:00，每日可调。

FixedTablets_Authored保留六块石板身份与发现入口，按D2至D7开放；T05已转接独立可补全石板；九天源场景T06旧陈列入口停用，由箱子A取得，其余四块仍只读。主场景T07在D8指定夜间出现在湖岸，需取得并回棚放置后重构；不要求先读前六块，仍可直接睡到D9（见第11节）。记录已同步运动/声音/光顺序及T05监听、T06光束机制，但前三块完整设施/演出仍未实现；T05补全/风铃见第7节，箱子A/轮盘与T06取得见第8节，光束牵制见第9节。原D6独立风铃场景保留旧陈列，用于单独监听测试，不是最新完整九天内容。T04由预定实体状态代替，没有确定其来源。已有灰盒导航追逐（第10节），没有正式九天关卡或存档。

FinaleSequence负责保留窗口与镜头表现，依赖独立的纯C# FinaleTimeWindow和显式World/Presentation连接。第一次T07交互即预约窗口，普通帧推进最多到午夜前一游戏分钟；很晚才操作时取当前时刻，绝不倒退。关闭真正的箱内记录后向前对齐保留时刻，短暂停留→挂钟6秒倒计时→D9状态变化/猛合箱盖与湖光→短暂箱子机位→EndingView。普通石板阅读不会触发结局。主动睡眠释放窗口，不进入这段导演流程。

规则终局Session.Descended和结局显示EndingPresented分开。Player有Sequence机位模式，此时不接受Esc/交互；HUD延迟显示结局面板，避免遮挡钟与箱盖。普通未修复睡眠路线仍立即进入结局。箱盖机械开合与BoxSeal仍分开，Presentation.SlamBox只处理表现。LakeOpenGlow为D7起且湖开时显示的占位光，不代表声音/完整光能力系统已实现；运动在第9节仅为当前代理与风向标的日期接点。

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

菜单 `Demon > Slice > Create Graybox Scene`仅用于首次创建；场景存在时拒绝覆盖。外部自动化可写一次性 `Temp/DemonSliceAuthoring.request.json`，仅接受源码白名单操作，字段为id和operation，报告在Temp/DemonSliceReports。请求消费后删除，无请求不写资源。后台Editor未导入新工具时需人工Assets/Refresh；工具更新后可用refresh请求。流程记录见TASK-002与TASK-023。

已经验证：Editor创建保存、四个程序集编译、14项EditMode、正常及未修复睡觉路线Play Mode、中文显示、Input System的W/E/六次鼠标选择/Enter输入链。交互射线忽略玩家自身碰撞体，避免近景被身体挡住。测试临时使用虚拟设备和焦点配置，结束恢复，不改变正常输入配置。

Windows64构建已通过，入口`Builds/Windows/DemonSlice.exe`，完整输出目录需要一起保留。Performance Testing临时Resources和Input System临时预加载设置已由构建后处理清理。Unity保存/构建更新的URP、GraphicsSettings等版本字段保留，未替换管线或包。

smoke-editability已实际编译运行：保存日长、机位和日光修改，验证Play尊重，再还原资产并通过正常路线重载。normal-003的近景、阅读页与终局截图已查看。人体感受、真人Esc/R及Windows独立程序试玩仍待用户验收；报告详见TASK-002。

Editor运行入口：双击DemonSlice场景，进入Play；点击Game视图捕获鼠标，WASD移动、Shift奔跑、E交互、近景鼠标点选、Enter确认、Esc退出，结局R重开。箱子和睡袋需靠近并对准。占位交互和预置不是正式谜题定稿。

九天入口：双击DemonNineDay。相同键鼠操作；左侧睡袋推进天数，T01至T05按日期在洞内、林间、仪式遗址、湖岸和旧监听站出现（见第13–15节），D6的T05可补全；T06改由箱子A取得，允许D7前解箱，D8 22:00正前方开放T07。NineDayDefinition的Days编辑提示/醒来，Tablets编辑日期/文本，FinalTabletHour编辑夜间时段；NineDaySettings编辑日长、移动等。固定石板机位在FixedTablets_Authored。快速结局入口为DemonSlice，现从D8 22:00启动，使用同一无+1演出。修改源场景/配置在退出Play后仍保留，不在运行时生成地图。

TASK-004检查：19项EditMode、nine-skip-001、nine-normal-001、旧场景nine-old-input-001均通过。nine-tuning-001保存D2醒来时刻10:00后验证运行采用，且固定石板机位被尊重；退出恢复08:00，已核对资产和恢复日志。九天构建输出为Builds/WindowsNineDay/DemonNineDay.exe，最终报告与剩余人工试玩见[TASK-004](Tasks/TASK-004-nine-day-flow.md)。

TASK-005检查：finale-migrate-002保存重载两套场景/设置通过；finale-tests-001的26项EditMode通过；finale-normal-001的新夜间门槛、保留窗口/8秒阅读、挂钟、二次交换/闭箱/湖光及延后结局通过。挂钟遮挡由finale-sightline-001修正，已查看新截图。finale-tuning-001保存3.5秒/机位横移后实际运行尊重并恢复6秒/原机位；finale-late-001极晚交互、finale-sleep-reserved-001窗口内主动睡眠、finale-skip-001全睡眠及finale-input-001实际输入链均通过。

本轮两套Windows输出已更新：finale-build-001九天版173927554字节，finale-quick-build-001快速版173914482字节。构建后finale-validate-001重载引用检查通过；原有meta哈希无变化、源文件无丢失，新增文件都有meta，git diff --check通过。构建添加了ProjectSettings.asset中的有效InputSystem Actions预加载引用，保留该包行为。完整独立exe真人试玩及恐怖感/节奏未验收；详细证据在[TASK-005](Tasks/TASK-005-no-plus-one-finale.md)和忽略的Temp/DemonSliceReports。


## 7 T05监听切片（TASK-006）

`GameSceneController`显式引用`ListeningTool`；工具创建独立`RuneBoardModel`，用`ListeningDefinition.asset`的目标模板验证T05，不复用Session.Board。`RuneTablet`保留T07默认路径，仅在配置ListeningTool时读取工具Board/提交入口；`PlayerController`的Enter和HUD手持提示跟随ActiveTablet。现有RuneKind数值0至9（含退役5）不变，新增ON/ENTER/DEMON/REGION/RING/CHIME从10至15；没有通用ON解释器。

T05保留`NineDayTablet`的index=4和NineDayFlow发现门槛；其`T05_Editable`子对象提供真实近景和补全。原FixedStone/TabletLabel仅停用，旧ID保留。D6可用，完成不推进日期、不变B/湖、不预约T07时间窗口。成功固化后表面符文可移除而内部快照继续生效；会话重启重新补全，无存档。

`ListeningChime`明确引用监测目标Transform、音源、音效、悬挂几何。纯C#`RegionEntryLatch`只在区域外→内触发；停留去重、离开复位。开启/重新启用时采样占位基线，不凭空报警。范围是以组件Transform为中心的世界空间球体（米），选中可看Gizmo；默认半径1.55。AudioSource位于悬挂风铃处，spatialBlend=1、Linear衰减1至16米、Doppler=0；不根据玩家到目标距离调整响度，不显示精确目标坐标。低/高音WAV为合成占位。

旧D6快速场景的`Listening_Authored`包含A/B风铃、`ListeningOccluder`木墙、保存的观察机位、`SnowDisturbance_Target`与`ProxyRoute_Editable`。`ListeningTrailProxy`仅在T05启用后以默认2米/秒和1.5秒节点停留沿保存路线移动雪痕，不是恶魔追逐。目标位置属代理运行时驱动，路线和视觉初始布局由场景编辑；监听器可将monitoredTarget改接真正恶魔，不依赖路线脚本。测试区位于既有雪地内，没有修改旧边界/地图生成机制。

九天源场景D6入口现为第15节林间原T05；右后排位置只属于旧快速测试布局。快速场景`Assets/Demon/Scenes/DemonListening.unity`从D6 10:00开始，使用独立`ListeningSettings.asset`（日长/移动等）、共享`ListeningDefinition.asset`（T05布局/文字）与NineDayDefinition（发现门槛/日程）。快速场景为独立保存的测试布局，修改布局时需明确当前场景；不是Prefab自动同步副本。原D8场景未改，BuildSettings首场景仍是九天。

截图发现默认TextMesh字体穿透实体；仅九天与D6场景给`SceneFontBinding.worldTextMaterial`赋`WorldFontDepth.mat`，对应URP字体Shader采用ZTest LEqual。运行时克隆字体材质并同步动态字库纹理、销毁时释放，保留原D8字体默认路径。两个场景的已有uGUI文字增加保存的Outline，提升白色雪地上的可读性。

固定Editor白名单新增listening-create/validate/readability/smoke/tuning/build；Create拒绝覆盖已有场景、定义和标记；后续编辑保存内容，不重跑Create。首轮Unity规则测试`listening-tests-001`为33项通过；键鼠/范围事件及保存参数验证结果见[TASK-006](Tasks/TASK-006-listening-tool.md)，最终回归与构建以任务记录为准。

TASK-006新增构建：listening-build-001（D6）173986298字节，输出Builds/WindowsListening；listening-nine-build-001（九天）173986294字节，输出Builds/WindowsNineDay。原D8输入、九天正常与全睡眠回归通过；人耳音频定位与独立exe真人试玩仍待验收，详见TASK-006。


## 8 箱子A与方位轮盘（TASK-007）

`GameSceneController.cipherBox`显式连接场景`CipherBox`，在NineDayFlow初始化后初始化锁。`CipherWheel`引用`CipherDefinition.asset`、电台/太阳方位占位/风向标根Transform、十个保存数字标签和观察机位。位置转换到轮盘局部轴，忽略高度，以局部+Z固定北标为0，顺时针每36度一个扇区，按最近扇区取数字。`BearingCipher`纯C#验证有限非零水平位移和0至9各一次排列。数字标签在Initialize按数据刷新（数字归配置所有，不以手改TextMesh文字改密码）。默认数字排列4,7,2,9,1,6,8,0,5,3，电台/太阳/风向标在9/0/2扇区，灰盒答案342；实际答案从保存布局计算，不使用日期或固定答案常量。

风向标引用固定位置根对象，不引用`RotatingBlade_OrientationIsNotCode`叶片朝向；旧独立DemonCipher仅有固定方位太阳/电台几何占位；最新主/取回/属性快速场景已接第12节声音交换、太阳失光与永夜。地标和轮盘都在`Cipher_Authored`中可编辑，若移动地标、旋转轮盘北标或改数字配置，下次运行按对应布局计算。轮盘本体不是交互旋转机关，固定北标与其数字一同保存。

`ThreeDigitLock`持有独立三位输入/不可变答案副本/Unlocked/Acquired，错误可重试，数字9循环回0，开锁/取得各一次。`CipherBox`持有模型、保存的三枚`CipherDigit`按钮、LockView/ReadView、LidPivot和箱内T06视觉。实际鼠标点数、Enter尝试；Esc只退出近景保留输入；正确后机械箱盖打开，再靠近按E取T06。后续E重复阅读去重。模型不访问世界时钟、RuneBoard或B/湖状态。HUD封印提示明确写箱子B，避免A解开后的状态歧义。

Player新增Lock与Inspect视角，分别响应机械数字和只读轮盘观察，沿用现有相机过渡/输入，不接入石板提交。NineDayFlow有Cipher时始终隐藏旧T06陈列对象，ReadTablet也拒绝未取得的T06读取；六个旧身份和ID保留。`DiscoveryProgress.Acquire`明确登记取得，Available含已取得状态，因此D1解箱可阅读T06，不需要修改D7日程。其余日期发现与T05门槛不改；没有存档，场景重启复位。

主场景是DemonNineDay。新`Assets/Demon/Scenes/DemonCipher.unity`从D1 10:00开始，独立`CipherSettings.asset`控制日长/移动，共享CipherDefinition、NineDayDefinition。快速场景为独立保存副本，不自动同步主场景布局；旧D8/D6源场景不改，BuildSettings顺序不改。`Cipher_Authored`含BoxA、FixedBearingWheel、三个固定位置根。LockView/ReadView与数字、模型、字体引用都是保存的场景资产。

固定Editor请求新增cipher-create/validate/smoke/tuning/build；Create已有输出或标记时拒绝重建。cipher-tests-001为42项规则通过，cipher-smoke-001验证D1实际键鼠与独立取得；cipher-tuning-001保存8/9扇区数字互换和机位横移后运行采用542，并恢复默认342/原机位。最终回归、截图和构建见[TASK-007](Tasks/TASK-007-bearing-cipher.md)。

TASK-007 Windows构建：cipher-build-001密码版与cipher-nine-build-001九天版各174027510字节，分别输出Builds/WindowsCipher与Builds/WindowsNineDay。T05、旧D8输入、九天正常与全睡眠回归通过；最终资产/差异验证及人工验证边界见TASK-007。


## 9 T06稳定光束与运动交换（TASK-008）

`GameSceneController.tether`显式连接`BeamTether`，在Cipher/Listening初始化后构造纯C#`StableBeamMotion`。该规则只持有D3后MOTION成对状态（恶魔1/风向标0）、稳定累计、锁定及交换计数；稳定满足交换一次，连续照射不重复；失锁/Release再交换一次复位，重复Release不再交换。使用既有AttributeOperations容量检查，既不持有GameClock/RuneBoard，也不访问SEAL/实体。

`BeamTether`引用保存的BeamDefinition、T06_AimRegion、雪痕可见根、同一风向标叶片、手电Light/LineRenderer/几何、锁定区域灯与ObservationPoint。玩家只能在Explore且鼠标已捕获时用右键；取得状态来自真正CipherBox.Acquired，无D7装备门槛。射线从视角向目标中心检查真实遮挡，忽略自身和目标视觉子物体；沿视线偏移在RegionRadius内并且在Range内才累计稳定，不要求精准单像素点。松键、转向、遮挡、超距、近景/阅读/演出与禁用都会复位。

BeamDefinition当前RangeMetres=10、RegionRadiusMetres=1、StableSeconds=0.5（用户确认发布参数；此前运行/构建验证采用16/0.85/1）、SpotAngle=26、LightIntensity=14、叶片100度/秒，可在Inspector调节。手电/蓝色辅助线与目标发光仍为灰盒表现，可替换保存资产；不创建运行时静态UI或地图。右下BeamStatus为保存的uGUI，只显示装备/稳定/锁定说明，不显示目标坐标；关闭组件、阅读或结局时隐藏。

为遵守D3才有恶魔运动，当前组合读同一世界时钟与DemonMotionDay=3：D3前风向标转，雪痕根不可见/不移动；D3起风向标停，独立DemonBeam代理沿保存路线移动；主场景现由第10节NavMeshAgent跟随玩家。日期边界独立于是否取得T06/启用T05。它是T01运动后果的小型状态接点，不是通用石板调度器或完整T01剧情。提前D1取得T06可照明，但没有可辨认恶魔区域，不会创造D3前运动；D3即可牵制，不要求等D7。

ListeningTrailProxy可选引用Tether：配置后由ProxyCanTravel/DemonCanMove控制路径，冻结时不消耗节点停留计时，失锁后继续既有路径；未配置的旧D6场景保持T05启用后移动的独立测试路径。T05只观察目标位置，不参与牵制/交换，不因目标停留重复响。叶片是唯一旋转属性，风向标根位置及密码答案保持。

九天源场景增加Beam_Authored、目标子区域、玩家相机下保存的T06_Flashlight与BeamStatus。新DemonBeam从D7 10:00开始，仍需解A取得T06，没有自动给予；使用独立BeamSettings，共享BeamDefinition/NineDayDefinition。观察点位于湖岸，可看雪痕与风向标；现有木墙可验证遮挡。原D8/D6/D1密码测试源场景未改；独立快速场景不是最新主场景的自动同步副本。

固定Editor请求新增beam-create/view/validate/smoke/tuning/early/build。Create拒绝已有输出/标记；view仅修正本轮观察点/区域底层，不动旧钟/密码。beam-tests-001为48项规则通过；beam-smoke-001实际右键、部分/稳定照射、演员停止/叶片转动、T05静止去重、转向/超距/木墙/近景/禁用/松键/睡眠复位（14次MOTION交换）通过。beam-tuning-001保存6m/1.1m/0.4s，运行采用后恢复16m/0.85m/1s。最终早期边界、截图、回归与构建见[TASK-008](Tasks/TASK-008-beam-tether.md)。

TASK-008 Windows构建：beam-build-001光束版174076962字节、beam-nine-build-001九天版174076978字节，分别输出Builds/WindowsBeam和Builds/WindowsNineDay。密码/T05/九天正常/全睡眠/旧D8输入均回归通过；早期取得与D3能力边界已通过真实右键验证，真人手感及完整独立试玩仍待验收。

## 10 导航追逐与拾取（TASK-009）

主场景DemonNineDay和独立DemonChase通过GameSceneController.chase显式组合ChaseController。使用已装AI Navigation 2.0.14的NavMeshSurface（Editor程序集引用Unity.AI.Navigation）及引擎NavMeshAgent；运行时不烘焙导航，不使用自建寻路。Chase_Authored保存PhysicsColliders/Volume范围、0.08m voxel与四个RetreatPoints_Editable；Data/ChaseNavigation.asset、ChasePracticeNavigation.asset为Unity原生二进制导航数据，.gitattributes精确声明binary，不能按文本改写。

旧ListeningTrailProxy仍保留在Listening_Authored但在新两个场景禁用；NavMeshAgent是雪痕唯一移动者。旧DemonListening/DemonBeam等独立场景仍由旧固定路线验证，不自动同步主场景。Tether继续拥有MOTION和日期，Chase只读取ProxyCanTravel/DemonCanMove；D3前休眠、D3后追逐，冻结时isStopped并清速度，叶片转动仍由原Tether控制。T05继续观察同一Target的位置/区域进入；空间雪脚步是环境踏雪占位音，不是D5前主动说话。

ChaseController每0.25s把玩家实际位置投影到保存NavMesh并重规划；物理视线排除墙后打断。ClosePassGate只拥有距离/冷却/次数；D9实体和LakeSeal关闭均停止追逐。追近调用Player.InterruptPickup，只取消当前尝试，再选远离玩家且完整可达的保存退开点；退开未完成前不再次追逐，冷却未结束则等待。ChaseDefinition.asset当前可调速度3.6m/s、加速度16m/s²、转向540度/s、追近1.6m、冷却4s、退开最小3.5m、目标投影1m、脚步间隔0.45s及反馈文字；无生命值、伤害或玩家传送。大范围改动障碍/地板/日期显示物需在对应场景Chase_Authored的NavMeshSurface重新Bake并检查数据引用；原始烘焙保守包含晚期T07占位碰撞，避免出现后穿行。

DemonChase从D7 10:00开始，ChasePractice_Authored只含练习碎片、保存的拾取近景和独立绕障墙。ThreatPickup使用纯规则PickupAttempt；按住E计时，松开/Esc/物品或玩家禁用/被追近则取消未完成尝试，已完成状态不变，成功事件只派发一次。成功先退出近景再触发序列化UnityEvent，便于后续内容接线；本轮没有正式物品栏或T07事件绑定。Player新增Pickup近景模式，工具停用；GameHud复用保存的focusText显示百分比。拾取holdSeconds默认1.2s、closeView、itemVisual、itemName及完成事件均在练习物组件Inspector编辑。该独立练习场景保留原A/T07单板流程；主场景正式T07已在第11节接入，玩法意图仍归GAME_SPEC。

创建工具ChaseAuthoring拒绝覆盖已有输出；chase-create/validate/smoke/tuning/build沿固定白名单请求，报告与截图在Temp/DemonSliceReports。53项规则、默认/保存调参真实输入、绕障/动态跟随/打断重试/清理、旧工具/九天回归、Windows64两版构建与保存引用重载均通过；详细证据和失败修复见[TASK-009](Tasks/TASK-009-chase-pickup.md)。真人及独立exe试玩仍待验收。

## 11 T07正式取得与回棚（TASK-010）

GameSceneController.retrieval显式组合FinalTabletRetrieval、湖岸ThreatPickup、棚内FinalTabletDock与原RuneTablet。FinalTabletProgress只持有本次游戏Acquired/Placed；拾取进度仍由PickupAttempt负责，SEAL/时钟/符文快照仍归原规则。

ThreatPickup新增单一局部BindAttempt开始策略/完成回调，由Retrieval.Initialize绑定、OnDestroy清理。开始策略检查FinalAvailable/未取得，并调用现有Finale.ReserveTime；松开、Esc或被追近不会释放窗口，重试与步行/拼图/阅读继续受原LimitNaturalTime限制。主动Sleep仍经ReleaseForSleep放弃，不锁住D8。普通练习拾取未绑定策略，保持TASK-009行为。

NineDayFlow仍唯一拥有原板的显示写入；FinalAvailable只表示日期满足，RefreshFinalTablet在配置Retrieval时还要求Placed。Retrieval负责湖岸拾取及空台可见性，取得后只携带；E交互FinalTabletDock才Place、显示原板并进入原近景。GameSceneController.BeginFinalInteraction/TryCompile拒绝未取得或仅携带时的重构，因此正确目标也不能绕过实际取得/放置。未配置Retrieval的旧独立场景保持原单板测试流程。

主场景新增T07Retrieval_Authored/LakeT07_HoldE、ShedPlacementDock。原T07板保留原组件/槽/线路/prefab及GUID，位置改为工具棚西侧(-5.3,1.5,1)，Y旋转90度、缩放0.6；拾取点(8.4,0,4.2)。两端视觉/碰撞/近景/时长/反馈可编辑，ThreatPickup默认1.2s，HUD新增保存的右上T07Status。T07状态提示字符串在Retrieval组件编辑。

NavMeshSurface按新布局重烘焙到Data/RetrievalNavigation.asset，主场景与同布局DemonRetrieval共享该原生二进制数据；旧Chase导航资产不改。快速场景独立RetrievalSettings从D8 21:54开始，仍需正常解A342/取得T06和补T05，不自动给予工具。作者工具拒绝覆盖已有输出，特殊repair只补丢失的快速settings引用；固定请求retrieval-create/repair/validate/smoke/late/sleep/tuning/build。实际结果和已知失败见[TASK-010](Tasks/TASK-010-final-tablet-retrieval.md)。

TASK-010机位修正：原板CloseView本地x=-0.45、z=-2.4，朝向板内(.45,-.08,0)；避原箱A/棚柱遮挡，几何与导航不因机位变化重建。原TabletMidnightPulse已作为板的子物体保存，本地(0,0,-.5)，原Light引用/参数保留并随人工移动石板。
四项RuneTray Label及TrayTitle本地x=1.9，保持面板/Collider/地图原位置，完整文字位于棚柱左侧；真实点击按可见文字位置验证命中原面板。56项规则、正式拾取/带回/全部符文盘真实输入、晚到/被打断重试/主动睡眠/保存调参、旧工具与九天回归、两版Windows64构建通过。最终截图已检查；用户试玩与完整九天/独立exe验收仍开放。

2026-10-29 T07对象顺序：RuneBoardModel.MatchesTarget仅在目标中心为EXCHANGE、第8/9槽目标对象对为BOX/LAKE时接受两种对象顺序，其余7槽严格对照模板。CompiledAt保存玩家实际排列，GameSession仍按原状态执行一次SEAL交换及D9再交换。规则不适用于T05/非EXCHANGE目标。箱内记录改称末端参数槽，线路仍是9→3，不将第9槽固定称为湖；Inspector中的原目标模板可保留BOX/LAKE，无需改场景或布局。

## 12 D5声音交换与D7太阳失光（TASK-011）

GameSceneController.consequences显式组合WorldAttributeConsequences，初始化在Tether之后、Finale之前。纯ScheduledWorldAttributes只持RADIO/DEMON的SOUND与SUN/DEMON的LIGHT值和两次单日程序账本；日期读取NineDayDefinition.Tablets[1/2].DateNodeDay（5/7），不复制日期表。Initialize按当前Day追赶，Clock.Midnight按日推进，重复同日不再EXCHANGE/TRANSFER；没有Rune解析器、第二时钟或发现门槛，不触碰SEAL/MOTION/实体。

组件从Initialize订阅Clock，到OnDestroy解绑；禁用只停止音频呈现，日期账本仍属于该场游戏，开启后不重执行属性。D5前广播在Cipher_Authored/Radio_FixedPosition的Broadcast_TransferredContent，Static_DeviceEnvironment从D1起持续极低底噪。D5后广播在Consequences_Authored/DemonBroadcast_Lake（0,0.85,9），电台只余静电；源为原生3D/Linear/无多普勒的loop AudioSource。广播clip在WorldAttributeConsequences统一替换一次，Initialize镜像到两端；音量/距离/空间化由各AudioSource Inspector拥有，运行不覆盖。广播迁移保留播放采样位置，停用/封湖后恢复沿用采样；终局停止所有源。MOTION冻结不停止湖中声音，湖封闭只遮蔽恶魔呈现，不撤销SOUND值。

WorldPresentation仍唯一写Directional Sun、Fog、Ambient与相机背景。新增保存的sunriseHour=6/sunsetHour=18控制正常日夜及预期日出；D7后读取SunLight=0，太阳intensity=0且Fog/Ambient保持Night参数，日期/钟表仍向前走。原太阳参照根不动（密码342保留），原SolarReferenceDisk使用薄盘厚度0.03、正常暖黄/失光黑色独立Unlit材质；新LineRenderer白边在局部(0,4.2,-0.025)，64个保存顶点，不生成实际照明。太阳/射线/白边仅在正常预期日出至日落时段显示；D7后无日光，黑心白边是灰盒参照，尚非最终天空美术。

FinaleSequence仍唯一控制原LakeOpenGlow：有Consequences时读DemonLight=1，无该组件的旧测试场景仍保留D7日期路径。湖关闭隐藏已有光，D9开湖复亮，和原箱盖/挂钟/实体顺序兼容。光束锁定蓝色辅助反馈不是LIGHT属性。

主DemonNineDay与DemonRetrieval已加入上述接线；新DemonConsequences使用独立ConsequencesSettings从D4 23:50、箱A附近开始，可自然等到D5或睡到次日，再睡到D7观察永夜。旧DemonSlice/Listening/Cipher/Beam/Chase源场景不改。音频PrototypeRadioBroadcast（4秒原创无词短旋律）/PrototypeRadioStatic（2秒设备底噪）是可替换WAV；NormalSolar/EmptySolar/SolarRim为独立材质，未改共享Paper。没有Collider/导航变更。

原生作者工具ConsequencesAuthoring拒绝覆盖已有输出；consequences-create/solar-view/validate/smoke/skip/late/tuning/build沿固定请求白名单。solar-view仅定向校正本轮参照盘和白边，避免球面前后错位，不在运行时生成外观。保存volume、替换clip、日出时刻的probe自动恢复；证据、原生输出RMS、截图与回归详见[TASK-011](Tasks/TASK-011-sound-light-consequences.md)。

## 13 D1–D2可选观察与山洞（TASK-012）

GameSceneController.exploration显式组合ExplorationFlow，初始化在Campaign/Tether/Consequences之后。FirstDayInspectionLog仅持D1三站去重计数；其他日期观察不补记D1，不读写时钟/SEAL/发现进度。StationObservation复用原Interactable的物理E和ReadDocument近景，生成当时日期/时刻及设施状态的文本快照，Esc沿原相机路径返回。文本/提示由Data/ExplorationDefinition.asset编辑，三个观察点/近景/站位均保存为显式引用。GameHud新explorationStatus只在D1–D2显示，使用保存的uGUI Text。

主DemonNineDay及新DemonExploration保存Exploration_Authored：南侧x1..5通路、雪林地面x[-6,17]/z[-28,-10]、林木、山洞约(10,0,-23)、小脚印、路线节点、观察点和机位。原SouthBoundary根与Collider ID保留，Collider左段改中心/尺寸，新增右段；不移动密码设施根，不改342。原T01移动到洞内(10,1.2,-24.6)/Y180，身份与发现日仍由NineDayFlow唯一控制；D1可进入空洞，D2原板与小人类脚印出现，D3恶魔雪痕沿原Tether日程。脚印是保存的几何，不在运行生成地图。树、洞口、灯与刻痕均为可替换灰盒内容。原12个扁平SnowBank的SphereCollider因最大轴缩放阻挡电台射线，主/探索场景保留原组件并禁用，新增使用原球mesh的静态MeshCollider，使碰撞贴合可见椭球；外形和根位置不变。

新Data/ExplorationNavigation.asset为原生二进制烘焙数据，.gitattributes精确binary。主/探索场景Surface Volume中心(2,1,-11)、大小(30,8,34)，烘焙完整营地↔洞路径；原三份导航资产及七个旧快速源场景保持，不自动同步新地图。移动障碍或扩大地面后须在对应场景NavMeshSurface重新Bake并验证，运行时不重烘焙。

DemonExploration从D1 08:00开始，使用独立ExplorationSettings；完整九天入口与BuildSettings保持。固定请求exploration-create/validate/smoke/skip/late/tuning/build，Create拒绝已有输出，probe恢复起始日/时、步速及观察文本。保存引用/重载、78规则、真实三站/洞内T01重复阅读与75m往返、D3原生追逐进洞、跳过/晚起/保存调参和旧流程回归、Windows64探索/主流程构建均已通过。用户体验尚待确认，具体报告与截图以[TASK-012](Tasks/TASK-012-first-days-exploration.md)为准。
## 14 D3–D4林间与洞外探索（TASK-013）

复用ExplorationFlow/ExplorationDefinition的可选扩展：largeTrail、forestRoute/ritualRoute、原secondTablet/thirdTablet和四条D3/D4搜寻/已发现提示。配置任一扩展引用就必须完整接线，Initialize明确验证；旧DemonExploration无扩展引用，仍只显示D1–D2。HUD沿用已保存ExplorationStatus，D3按T02发现、D4按T03发现切换文案，无D5–D6扩展的旧快速场景在D5退场；当前主场景延长至第15节。大遗留痕迹仅按原Tether.MotionReady/D3显示，表示已留下的路线，不控制或模拟当前威胁。Clock.Midnight更新日期呈现，Session.Changed负责终局隐藏两类遗留痕迹；OnDestroy对称解绑。NineDayFlow继续唯一拥有石板日期显示/发现，不要求检查风向标或发现前块石板。

主DemonNineDay与新DemonInvestigation保存Investigation_Authored。原T02移至ForestTabletSite_Editable的(-2.5,1.2,-20.5)，原T03移至山洞西侧外部OutsideCaveRitual_Editable的(3.4,1.2,-24.1)，均Y180、保留原对象和ReadView引用。大遗留脚印、林间路牌、两条路线、石柱/无发光刻痕均为保存灰盒。路牌从北侧看时向西岔路为右箭头。T03站位(4.8,0.04,-21.7)、原ReadView(4.9,1.55,-21.4)朝原板，避开树冠；树、原板、营地设施根不因机位改变。

新Data/InvestigationNavigation.asset为Unity原生二进制，精确binary声明；Surface范围沿用原(2,1,-11)/(30,8,34)，保守包含晚期显示碰撞。旧四份导航和八个快速源场景不覆盖。完整D1入口不变，独立InvestigationSettings从D3 08:00开始，不预给工具或发现。Chase沿用原Agent/近身/退开/冷却，无新波次/伤害；仅ChaseDefinition近身提示改成前期也适用的行动文案。T02/T03记录保留程序/日期，移除制作占位备注；音画仍为灰盒资源。

InvestigationAuthoring固定请求investigation-create/validate/smoke/boundary/skip/late/tuning/build，Create拒绝已有输出；定向ritual-view/records/chase-hint仅用于本轮已知目标。investigation-review是有界的真人阅读预览：D4自动步行到T03并打开记录，结束后恢复起始设置，不是产品传送/新流程。Route节点是布局/步行验证参考，不控制玩家或追逐；脚印是独立保存的可编辑几何。78规则、实际两日输入/旧流程/调参/跳日和Windows快速/主流程构建已通过，用户实际确认T03文字完整；全段玩家节奏与独立exe仍待验收。字体排查试验已撤回，最终沿原FontBinding。具体证据见[TASK-013](Tasks/TASK-013-middle-days-investigation.md)。
## 15 D5湖岸T04与D6林间风铃（TASK-014）

当前DemonNineDay及DemonSignalSearch在原ExplorationFlow/Definition上接可选D5–D6引用：fourthTablet/fifthTablet、lakeRoute/listeningRoute、visibleListeningPoint/hiddenListeningPoint。任一引用存在就要求整组完整且有D3–D4扩展；错误接线明确报错。ShowStatus延长至D6，D5按T04发现切换提示，D6按未发现/已发现未启用/监听已启用切换，D7退场。旧快速场景未配置新字段，仍保持原显示边界。没有新增任务完成条件、资源或阻止睡觉。

SignalSearch_Authored保存原T04于湖岸(4.5,1.2,4.6)/Y0、原T05于林径东侧(12,1.2,-12)/Y90朝西；T05_Editable的原板/槽/托盘/近景整体随父对象移动，不重建身份。湖岸路线终点(4.5,.04,2.5)，监听路线终点(9.8,.04,-12)。路线/Visible/Hidden节点仅用于编辑布局与走行验证，不驱动玩家或敌人；实际场地、指示牌、铃架和墙均为保存GameObject。T04去除制作备注，实体函数签名/来源仍属GAME_SPEC未决项，不增加新函数或取消D9节点。

原ListeningTool/ListeningChime继续唯一拥有T05固化/进入报警；原两只铃及空间AudioSource移到ForestListeningSite_Editable，低音(3.4,.15,-14.3)、高音(9.5,.15,-14)，原半径1.55m、音色/音量/衰减保持。原ListeningOccluder木墙在(8,1.35,-15.7)，旧观察锚点保留，实际步行可见点(9.5,.04,-15)、墙后点(8,.04,-17)。监测目标仍是Chase原NavMeshAgent；原ListeningTrailProxy保持禁用，没有测试路线驱动敌人或额外波次。低音位置根据实际西侧绕墙路径调整，使进入事件在玩家视线被木墙遮挡时触发。D5湖中广播/电台静电继续由WorldAttributeConsequences拥有，音源/素材不变。

Data/SignalSearchNavigation.asset为新的原生二进制保存数据，显式binary；当前主/新快速场景共用。Native NavMeshSurface保守包含按日开放板体和T07碰撞，完整路径覆盖T01–T05、T07岸边与返营。原九个快速场景和五份导航不改；移动场地碰撞后应在本场景对应Surface重Bake本轮导航，不在运行生成。SignalSearchSettings从D5 08:00开始、步速3.3，无预给工具/发现；日期/原文NineDayDefinition，五条搜寻状态文案ExplorationDefinition，布局SignalSearch_Authored均可在Inspector修改并保存。

定向SignalSearchAuthoring使用原生Editor APIs，Create拒绝既有输出。固定请求signal-create/validate/smoke/boundary/late/skip/tuning/build；first-search/old-search在新地图分别复用D1–D2、D3–D4真实输入fixture。初次布局/空配置恢复和低音铃路径调整为有严格前置的定向操作，不是自动覆盖手工资产。Probe备份日期/时刻/步速/提示并在退出恢复。实际证据与历史失败见[TASK-014](Tasks/TASK-014-signal-search.md)，最终验证状态以任务记录为准。

## 16 D7日出观察、光束练习与D8交接准备（TASK-015）

主DemonNineDay和新DemonPreparation复用ExplorationFlow/Definition的可选末段扩展：solarRecord、handoverRecord、practicePoint，任一存在必须整组完整且已有D5–D6扩展。PreparationRecord继承原Interactable，显式保存owner/kind/CloseView/AimTarget/Approach/Prompt，沿用物理E、原ReadDocument面板与Esc返回；没有第二阅读UI、背包或任务管理器。

日出记录从原T03的DateNodeDay开放，交接记录从D8开放；纯阅读仅设置本场探索呈现记忆，不登记石板发现、授予工具、推进时钟、交换属性或预约T07窗口。记录附带当前D/时刻，D8交班未到的原因留白，未提前教授WHILE。终局隐藏两份记录。记录不是睡眠、早期取得T06或夜间取T07的前置条件。

D7提示按日出记录/实际Cipher.Acquired/当前锁定/已见锁定后释放选择；ExplorationFlow.Update只观察原Tether锁定，呈现“已验证恢复”，不控制Agent、移动或复制MOTION。D8提示按交接阅读和实际T06/T05状态选择，已完成工具不要求重做。准备完毕的开放时刻由NineDayDefinition.FinalTabletHour格式化；到原T07夜间门槛后该探索提示退出，由原FinalTabletRetrieval状态接管。旧场景没有配置该扩展，维持原2/4/6天提示边界。共享日程文案不指向旧练习场景没有的新记录。

Preparation_Authored保存SolarObservation_Editable(-5.1,1.1,-2.7)、HandoverDesk_Editable(-4,1.1,-4.4)及原湖岸观察位置的BeamPracticePoint_Editable。两张记录牌、支架和标题为占位几何；各自ReadView_Editable与Approach_Editable可直接调节。实际文字在Data/ExplorationDefinition的D7–D8字段，日程在NineDayDefinition；原密码设施根、原观察点、两铃/墙和唯一Chase目标保持。

Data/PreparationNavigation.asset是独立Unity原生二进制，主/新快速共用；烘焙保守包含按日显示石板、两张新记录与T07碰撞。旧十个快速源场景与六份旧导航不覆盖。改新布局碰撞后在对应Surface重Bake该新资产；运行不烘焙。PreparationSettings从D7 08:00、步速3.3开始，无预给工具/发现；主NineDaySettings仍D1入口，BuildSettings未改变。

PreparationAuthoring仅显式Create创建，已有输出即拒绝；无自动重建/覆盖手工布局。prep-create/validate/build及prep-smoke/late/skip/boundary/tuning/early/night/first-search/old-search/signal-search沿固定单请求工作流运行。Probe保存并恢复起始日/时、步速、交接正文及近景位置；Early从营地实际走到A，不沿用旧练习场景的出生假设。新场景真实输入、保存调参和旧流程验证以[TASK-015](Tasks/TASK-015-final-days-preparation.md)为准；工具不是完整九天真人体验或正式美术/音频验收。

## 17 SOLID局部重构与九天兼容（TASK-016）

当前工作区的 UML 类关系快照（2026-11-05）见[总览、完整图与源码索引](Diagrams/runtime-classes.md)。该图只解释已有实现，不增加运行时系统；后续修改类关系后需要重新生成。

本轮实现记录与最终验证状态见[任务](Tasks/TASK-016-solid-refactor.md)。保留现有MonoBehaviour类型、序列化字段、GUID、Configure入口和场景/Prefab结构；不增加新玩法、运行时场景生成、DI容器或全局事件总线。

- `SceneContracts.cs`定义按消费者划分的运行时契约。`IPlayerSession`仅提供玩家所需会话/配置/结局状态和阅读关闭通知；`IHudState`只提供消息/正文/结局显示状态；`ISceneFeedback`提供消息与文档显示。`GameSceneController`仍是显式组合点及流程协调者，向Player、HUD、Chase、Beam、Listening、Exploration传入所需依赖，避免这些组件经总控制器寻找其他系统。其旧Getters保留供已保存引用及Editor验证使用，其他尚存控制器依赖不是本轮全面架构替换。
- `ICloseViewInteraction`拥有Confirm/Click两种能力；RuneTablet与CipherBox实现相同契约。PlayerController统一转交近景输入，不再在Enter/鼠标分发中按两类谜题写分支。旧EnterTabletView/EnterLockView、InspectingTablet/InspectingLock/ActiveTablet兼容入口保留；拾取的按住、打断和相机模式仍沿原行为。
- `ITabletProgram`提供Board/BeginInteraction/TryCompile；SceneController拥有T07会话规则，ListeningTool拥有独立T05规则。RuneTablet只呈现/操作被注入的Board，不再自行经总控制器决定程序来源。原listeningTool序列化字段及作者化连接保留，避免破坏现有资产。
- 九天场景的运动开放日期唯一读取`NineDayDefinition.MotionDay`，实际来自Tablets[0]（T01）.DateNodeDay。组合点将该值传给BeamTether，Chase/探索继续读取Tether的运动状态。`BeamDefinition.DemonMotionDay`保留为无Campaign的独立场景回退，Inspector Tooltip已明确；默认日仍为D3。
- `Domain/ExplorationGuidance.cs`是无Unity引用的纯文本选择策略：有序GuideRules读取状态快照，返回文本Key，不写时钟、发现、工具或SEAL。配置保存于原`Data/ExplorationDefinition.asset`；旧文案字段保持。每条规则的Day为日期（0表示通用回退）、RequiresPreparation限制为配置两份准备记录的场景、Condition选择已有状态条件、Key选择原文案字段。首个匹配生效；必须保留无条件Day0回退。GuideKey/Condition枚举值属于保存资产协议，新增项追加，不得重排。调整既有条件的日期、顺序或文案Key无需修改ExplorationFlow。默认21条规则保持原D1–D8优先级及旧场景退场边界，不是任务完成系统。
- `ExplorationFlow`继续拥有观察、记录和场景痕迹显隐，向纯策略提供状态快照；不再承载每日提示分支。记录日期/阅读、D7练习记忆及ShowStatus原边界保持。
- Editor验证器共用`ISmokeRun.Tick/Cleanup`生命周期，RequestRunner仅保存一个当前验证实例，统一轮询和退出清理；原D8验证实现已移入独立PrototypeSmokeRun，调度器由本轮前676行降为414行。`SmokePlayerDriver`集中D5–D8验证重复的临时键鼠、瞄准、NavMesh路径/WASD行走与设置恢复；仍由生产CharacterController实际移动，不传送玩家或操纵敌人。原各场景路线、断言和30/35秒行走超时保留。

新增固定请求`arch-save-guides`通过原生Unity序列化保存现有引导规则；`arch-validate`加载全部12场景检查缺脚本/组合与规则，并恢复原活动场景；`arch-runtime`临时保存冲突运动日期与一条提示Key，验证日程权威、规则编辑生效及独立近景实现的真实Enter/鼠标/Esc，退出自动恢复。重构回归通过`Logs/DemonChecks/Task016/RunChecks.ps1`顺序发送现有单请求；详细回执与XML/PNG归档于同目录。不要在Play期间改脚本或同时运行多个请求。


## 18 接收Snow美术场景（TASK-017）

从 origin/main@94ed249 接收的源环境是 `Assets/ImportAsset/SnowEnviroment/Nature Pack - Winter Environment/Scenes/Snow.unity`，以及 ArtAsset、ImportAsset、SnowSystem 等依赖。整合试玩场景及其导航见第 20 节。
保留本机PC管线及UniversalRenderPipelineGlobalSettings原配置；新增URP-HighFidelity资产仅随环境接收，不激活。Git LFS用于贴图、模型和音频，保留PreparationNavigation原生二进制声明。
另一台机器的源环境验证记录见[导入记录](Tasks/SNOW-SCENE-PACKAGE-IMPORT-20261101.md)与[上传记录](Tasks/DEMON-GITHUB-20261101.md)，其中D:/MyProject/Demon等路径不是本机工作目录，不替代本机验收。当前本机拉取、保留与验证状态见[TASK-017](Tasks/TASK-017-art-scene-pull.md)。

本机接收检查已完成：LFS 714路径逐个payload校验通过；Snow原生加载缺脚本/网格/材质槽与Shader错误均0，保留上游两处空LOD Renderer槽。TerrainData `Scenes/Demo_Scene_02.asset` 曾被 `.asset text` 规范化丢失 2 个 CR 字节，已按原数据字节锚点恢复。对应完整带空格路径在 `.gitattributes` 用引号声明 binary。详见 TASK-017 证据。
原有Demon代码/资源、当前Settings、Packages和ProjectSettings逐字节保持本轮前状态。81规则及主九天正常/全睡眠回归通过。临时检查代码均已移除，当前程序版本与接收前一致；新环境自带的7份模型/树林辅助脚本随资源新增。环境加载验证不等于玩法集成或完整美术/真人验收。

## 19 类名与兼容标识（TASK-019）

场景组装使用 `GameSceneController`，玩家控制使用 `PlayerController`，可编辑符文石板使用 `RuneTablet`，纯规则会话使用 `GameSession`。18 个类的旧名对照和 110 个类的命名审查见 [TASK-019](Tasks/TASK-019-remove-slice-class-prefix.md)。[运行时类图](Diagrams/runtime-classes.md)已从迁移后的源码重新生成。

`Demon.Slice` 系列程序集与命名空间继续作为兼容标识。旧场景、预制体、设置文件及编辑器请求路径仍可按原名称查找。Unity 脚本随原 `.meta` 移动，序列化字段、引用和实例覆盖保持。类名迁移在 12 个场景、3 个预制体和配置的原生加载中完成前后比对，九天设置仍在 `Assets/Demon/Data/NineDaySettings.asset` 编辑，启动场景仍为 `Assets/Demon/Scenes/DemonNineDay.unity`。

## 20 Snow 玩法整合副本（TASK-020、TASK-021）

`Assets/Demon/Scenes/DemonNineDay_Art.unity` 是 Unity 内的雪地整合试玩入口。当前层级按用途分为七个根组。原灰盒九天场景继续作为已配置的构建入口。

| 根组 | 当前归属和编辑入口 |
| --- | --- |
| `00_系统与界面` | `GameSystems_系统接线` 上的流程组件、WorldPresentation、Finale 音频和保存的 uGUI。 |
| `01_玩家` | 原 Player 预制体及其 PlayerCamera、T06 手电。Player 的位置是开局位置。 |
| `02_室内交互` | SleepPoint、Radio_FixedPosition、FixedBearingWheel、BoxA、BoxB_Record、T07_Table_Editable、SolarObservation_Editable、HandoverDesk_Editable、WallClock 及照明。移动每个父物体可带动其交互和镜头子物体。 |
| `03_逐日点位` | `D02_T01_点位`、`D03_T02_点位`、`D04_T03_点位`、`D05_T04_点位`、`D06_T05_监听站`、`D07_光束练习`、`D08_T07_湖岸`。公共_室外设施包含风向标、太阳参照和湖岸观察点。 |
| `04_湖面事件与结局` | LakeSeal、LakeOpenGlow、DemonBroadcast_Lake、D09_DescentFigure_Editable、EndingView。 |
| `05_路线与导航` | 路线参考下保存脚印、探索参考节点和 CampReturn。Chase_Authored 保存 NavMeshSurface/退开点；SnowDisturbance_Target 是实际追逐对象，门槛和楼梯导航连接单独可选。 |
| `06_美术环境` | 原雪地 Terrain、树木、岩石、Water、we/LogCabin、围栏、飘雪、全局美术设置、灯光、Port 和 Temple。 |

每日点位组的枢轴已放在对应石板附近。移动点位组会带动模型、交互与近景；路线参考在 `05_路线与导航` 中另行调整。T05 监听站还包含两只风铃、声音遮挡墙和观察参考点。T07 的空台和放好后的拼板仍由 `02_室内交互/T07_Table_Editable` 一起移动。

BeamTether.ObservationPoint 显式引用 `03_逐日点位/D07_光束练习/BeamPracticePoint_Editable`。已删除无用途的旧教学代理、旧代理路线、停用相机、旧镜头参考、分隔线、冗余空组和 T06 旧陈列几何。T06_RecordIdentity 保留原 NineDayTablet 身份和日程引用，位于 BoxA 下。SleepPoint 使用已有交互碰撞，床的可见外观来自美术木屋。

追逐沿用独立的 `Assets/Demon/Data/SnowIntegrationNavigation.asset`。改道路、障碍、门槛或目标的可站立范围后，在本场景的 Chase_Authored 对应 Surface 重烘焙，并检查玩家和敌人的实际通行。

整合版独立引用 `SnowIntegrationExploration.asset` 和 `SnowIntegrationSchedule.asset`，分别编辑探索文案和日程/石板内容。移动轮盘、电台、风向标或太阳参照后应重测密码。屋顶挡雪保留 Particle_Snow 对专层的精确检测。结构整理与启动修复的检查记录见 [TASK-021](Tasks/TASK-021-hierarchy-recovery.md)，布局和流程历史见 [TASK-020](Tasks/TASK-020-snow-gameplay-integration.md)。

## 21 天数调试菜单（TASK-022）

整合场景 `00_系统与界面/SliceHUD/DayJumpMenu_天数快捷菜单` 保存 DayJumpMenu、菜单 uGUI、按钮、顶部中央快捷键提示及 MenuEventSystem。F8 开关，1–9 跳到对应天的开局，0 跳到 D8 日程设定的夜间开放时刻，Esc 继续。D9 是零点终局。菜单位置和尺寸在 MenuPanel_菜单面板/Card_菜单布局 的 RectTransform 编辑，按钮/说明为保存的 Text；DayJumpMenu 的 toggleKey 与 availableInPlayer 可在 Inspector 调整。改热键时同步修改提示文字。后者默认允许独立游戏内使用。

DayJumpMenu 调用 GameSceneController.Restart，再由 CompletedDayReplay 从 D1 沿原 GameClock 的每个零点推进，调用既有观察、阅读、符文盘操作、密码解锁、工具取得、T07 拾取安放、箱 B 阅读与 FinaleSequence。目标当天不代完成，D8 夜间额外补齐交接记录并停在湖岸拾取前。正常开局不重放。回退重新创建 GameSession/规则模型，各组件复用 Initialize 配对解绑旧事件、重绑新模型并恢复自身状态；玩家恢复开局位置/镜头，追逐恢复起点，地图/字体/UI 对象保持。R 重新开始也通过 IPlayerSession.Restart 使用同一路径。不会倒拨旧时钟，不需要把整合场景加入构建列表。

重初始化显式恢复两个符文盘的监听/线材质、箱 A 模型/盖子/工具视觉、风铃计数/摆动、光束状态、追逐冷却/路径、T07 拾取尝试/放置、箱 B 盖子、结局阶段/音频、观察记忆。关卡布置和共享数据不更改。当前版本整图异步重载曾两次在 GPUResidentDrawer 的 ObjectDispatcher 原生工作任务崩溃，所以使用局部玩法重置，未禁用 GRD、修改管线或修改包缓存。失败调查留在任务记录。

菜单打开保存/恢复 Time.timeScale、AudioListener.pause 与鼠标状态。PlayerController.MenuOpen 隔离移动/交互，关闭帧隔离 Esc；BeamTether/ChaseController 暂停期间不更新瞬时光束锁定/追近判定。RefreshClockState 和 RefreshPracticeProgress 是既有刷新逻辑的显式入口，不增加新的规则状态。菜单的 EventSystem 使用原有 InputSystem_Actions.inputactions 的已保存 UI 引用，中文标签加入 SceneFontBinding。

显式 Editor 工具 DayJumpMenuAuthoring 仅对当前整合场景添加菜单，存在即拒绝覆盖。EditorRequestRunner 的 day-jump-inspect/create/input/hint/validate/editability/smoke 请求用于检查、创建、接线、保存重开、编辑性探针及回归。验证状态见 [TASK-022](Tasks/TASK-022-day-jump-menu.md)。

## 22 完整九天主线实现 TASK-023

本节描述2026-11-11已接入并通过规则/组件检查的新增实现；新版玩法目标由GAME_SPEC第10–12节维护。第5–21节中的旧灰盒/轻量切片行为仍说明各自历史版本，新雪地主线以本节为准。Windows构建与人工验收的最新结果见[TASK-023](Tasks/TASK-023-complete-playthrough-plan.md)。

`DemonNineDay_Art.unity` 的 `00_系统与界面/GameSystems_系统接线` 保存 `CompletePlaythrough` 与 `SkySunPresentation`，组合根 `GameSceneController.playthrough` 显式引用它。未配置该组件的旧切片继续沿用旧规则，不被每日主线门槛影响。

| 数据或对象 | 已实现职责和编辑位置 |
| --- | --- |
| `Assets/Demon/Data/CompletePlaythrough/MainRouteSettings.asset` | 独立日长720秒、速度/交互/阅读/结局基础配置；原NineDaySettings保留。 |
| `MainRouteDefinition.asset` 同目录 | 九天关键事项ID清单、纸条正文、开场/提示时长、夜间门槛、D3–D8速度、发现/脱离距离、丢失视野时限、误导笑声间隔与偏移。只读共享定义，不存本局收集状态。 |
| `MainRouteExploration.asset` 同目录 | 主线观察/探索文案，包括D5啸叫与D8等待提示；不改旧切片共享文本。 |
| `MainRouteSchedule.asset` 同目录 | 独立主线日期/石板文本，移除旧可选巡查文案并同步T02啸叫；原SnowIntegrationSchedule留给旧版本。 |
| `MainRouteBeam.asset` 同目录 | 主线专用光束数值/左键点击开关提示；旧机制切片继续使用原共享配置。 |
| `SliceHUD/CompletePlaythrough_完整流程界面` | 保存的今日任务、中央提示、全屏黑色开场/等待、失败重试面板和带ScrollRect的记录背包。布局在各RectTransform，字体仍由SceneFontBinding绑定。 |
| `02_室内交互/D01_桌面线索` 与D02–D06点位组下 `Record_*` | 保存的纸条与手电占位模型/交互/阅读机位引用；纸条随对应点位组移动。淡黄色纸条带标题，按日可见、收录去重，重试同步还原可见状态。 |
| `03_逐日点位/公共_室外设施/ForestInspection_森林巡查` | 新增森林巡查交互，位置当前位于T02附近，原三站Transform未移动。 |
| `06_美术环境/.../LogCabin/CabinSafeZone_木屋安全范围` | 保存的BoxCollider触发范围及NavMeshObstacle carving；Inspector调整安全范围，改动后检查门开关与实际追逐。玩家使用CharacterController，不受导航carving限制。 |
| `GameSystems_系统接线/CabinExit_敌人停留边界` | 恶魔误入安全范围时的显式室外回退位置，需位于安全区外的有效导航上。 |
| `WorldPresentation/SkySun_可编辑日轮` | 保存的日轮/黑心占位；SkySunPresentation按相机远裁剪范围、太阳方向放到远景，D7与固定密码太阳方位一致。 |
| `GameSystems_系统接线/MisleadingVoice_误导笑声占位` | 独立3D声源，其偏移不改变恶魔和风铃数据。音量与传播范围在AudioSource。 |

`Domain/DailyProgress` 管理当日任务去重、重置与睡眠资格；`RecordCollection` 管理本局已收录ID。原 `ObservationSite` 三值及 `FirstDayInspectionLog` 契约保留，森林使用 `StationObservation.forestPoint` 单独标记；主线进度由DailyProgress记录，旧D1三站统计不承担新版门槛。

`GameClock.CanEnterDay` 在任何零点副作用前裁决是否允许跨日。允许后先通知原Midnight监听，再通知DayEntered，最后在本次推进达到目标时通知Advanced。主线据此先建立新日快照，再在睡眠抵达醒来时刻后更新为实际入口；大步进不能重复使用上一日任务。失败仅暂停当前会话并显示缺项，直到点击重试，不先执行次日属性变化。

`RuleSnapshots.cs` 提供GameSession、符文盘、发现、密码锁、光束、拾取等显式捕获/恢复；`CheckpointState.cs` 保存玩家姿态、观察记忆、风铃、追逐目标/速度/冷却、风向标角度、广播采样位置、箱盖及门的值状态。`CompletePlaythrough.RetryDay` 重建规则会话、解绑旧事件、恢复实际入口快照并重绑表现；不倒拨旧GameClock，不重载地图，不用CompletedDayReplay补全玩家历史。R重新开始也创建新的D1重试点，避免只有首次Start建点。快照仅存内存，不是退出后存档。

木屋主线门由 `CabinDoorInteraction` 独立管理可恢复的开关/插值/音效；只在整合场景实例禁用旧LC_Door协程组件，保留供应商脚本、资源和原UnityEvent。旧切片没有主线标记时仍调用原事件。门口敌人导航连接不因开门启用，安全范围另外由导航carving与追逐目的地/近身判定共同保护。

`BeamTether.HasLamp` 和 `Equipped` 分开表示普通手电与T06。普通照明不会交换运动；没取手电但已取T06会提示回屋拾取。追逐按配置速度和视线/距离侦测，丢失目标停止读取实时目的地；木屋安全不会交换运动属性。D5电台换为占位啸叫，笑声独立定位，监听风铃继续采样真实目标。新占位WAV为 `Assets/Demon/Audio/PlaceholderRadioHowl.wav`、`PlaceholderMisleadingLaugh.wav`，均为本地生成素材。

背包默认Tab开关、Esc关闭；暂停世界和音频、隔离玩家操作，关闭恢复先前倍率。`JournalInventory`展示当前收录，`JournalSlot`绑定独立物品的按钮/标题/类别/选中标记。主场景保存6列×4行初始空格、GridLayoutGroup、溢出模板和右侧滚动正文；超出初始格数只克隆保存模板，不构成容量限制。每件物品独占一格，点击只重读该物品，不调用现场开箱/固化或午夜导演。编辑入口为 `CompletePlaythrough_完整流程界面/Journal_背包记录/GridViewport_格子滚动区/GridContent_格子布局`；模板在同背包根下 `SlotTemplate_溢出格模板`。

主线 `BeamTether.LampSwitchedOn` 保存左键点击锁存状态；松开不关闭，再点切换。只有探索且可用工具时接收点击，菜单关闭帧也隔离输入；近景有效照射仍中断。当天快照同时保存并还原开关。旧无CompletePlaythrough的机制切片保留原右键按住输入。主场景挂钟的SecondHand已用Editor API删除，FinaleSequence的秒针引用成为可选，时针/分针与导演不变。

D8检查交接后在屋内按T或点按钮，短黑屏等待后通过原时钟推进到22:00；T07有效拾取后仍使用FinaleTimeWindow保护，现场箱B记录关闭才交还午夜导演。F8前置重放补齐新任务并建立目标入口，失败重试与调试重放分开。

时间流速在 `Assets/Demon/Data/CompletePlaythrough/MainRouteSettings.asset` 的 `RealSecondsPerDay` 调整，当前720现实秒/游戏24小时。数值越小越快；运行时GameSceneController以此换算GameClock推进，不启动时覆盖Inspector数值。编辑器检查接受>=10秒的自定义日长，不再限定720。StartHour/WakeHour另管开始/醒来时刻；Inspector中新增中文单位说明。

新增EditorRequestRunner操作为 `complete-inspect/create/content/sky/labels/presentation/finalize-content/reachability/repair-records/inventory-controls/inventory-validate/save/validate/smoke/route-smoke/integration/editability/build`。请求文件仍为 `Temp/DemonSliceAuthoring.request.json`，字段仍只有合法id/operation。create拒绝覆盖，后续操作明确针对已保存的主线对象；操作前若场景有延迟序列化变脏，会先在 `Logs/DemonCheckpoints/task023-work` 保存副本并保存当前现场，绝不以磁盘旧图覆盖现场。

已验证：89项EditMode；主线D1–D9及各日重试组件回归；分帧Tab/Esc/右键/T输入、背包按钮射线、木屋安全、逐日速度、D7日轮/密码、T07保留窗口；纸条/布局/速度修改保存重开后运行采用并还原。真实连续键鼠通关不在这些自动检查的证明范围内，不能据此宣称已通过人工完整验收。

Windows64候选输出为 `Builds/WindowsCompletePlaythrough/Demon.exe`（完整目录2970752238字节），已实际启动雪地入口和中文任务界面；实体键盘F8由用户确认。规则/最终集成/构建的原工具结果已保留到 `Logs/DemonCheckpoints/task023-final-20261111/recovered-tool-observations.json`，当前资产与包校验见同目录 `resume-integrity.json`。独立包日志仍有初始化导航和GaussianDepthOfField提示，影响与完整连续键鼠验收留在TASK-023，不据自动检查标记整体完成。

TASK-024最新候选仍在同一Windows路径，构建报告2970828058字节；89项规则、格子/左键/快照/两针时钟集成、格子布局保存重开还原、完整结局回归均通过。证据在 `Logs/DemonCheckpoints/task024-inventory-controls-20261111/`。该候选已实看启动；连续实体键鼠通关仍归TASK-023验收。

## 23 恶魔追踪贴花脚印（TASK-023）

当前工作副本位于 `D:/MyProject/Demon`，Unity 6000.6.0f1 / URP 17.6。正式入口仍为 `Assets/Demon/Scenes/DemonNineDay_Art.unity`。在GameSceneController/Chase所在对象下保存 `Demon Terrain Footprints`，显式绑定真实ChaseController和唯一原生Terrain。玩家不绑定Emitter；D2静态路线保持原样。

`ChaseController.Stepped(Vector3 position, Vector3 forward)`在现有踏雪音实际触发点发布，`PursuitReset`在Initialize/Restart重置后发布。位于`Assets/SnowSystem/FootPrint/DemonFootprints.cs`的表现适配器订阅/解绑这两个事件，交替左右脚调用通用Emitter.StampAt；跳跃导航链接、极小位移、传送跳变和未命中指定Terrain过滤。Demon.Slice不依赖贴花程序集；通用Emitter仍不依赖Demon或GlobalSnow。

正式场景的Emitter关闭Automatic Steps，脚印0.4×0.85米、左右偏移0.3米、寿命35秒、池上限48。步频由现有ChaseDefinition.FootstepSeconds控制，与声音共用时机。光束冻结/停步/暂停不发布步事件；会话重启和当天重试清空旧轨迹。静态鞋印的世界位置不跟随恶魔。两个旧 SnowPrint 移动占位对象已删除，目标根、导航、音源、风铃和T06可瞄准区域保留。

PC_Renderer增加DBuffer/AlbedoNormal Decal Renderer Feature，保留原渲染模式和SSAO。地形使用原有URP Terrain材质；不替换TerrainData/材质、导航或GlobalSnow。贴花只模拟颜色和法线，没有真实高度变形。

Editor工具`DemonFootprintIntegration.Connect`显式接入Art场景，存在则验证并保留调参。`DemonFootprintVerification.RunBatch`包含首次接线、尺寸保存重开和实际追踪验证；`VerifyExistingBatch`只加载已有接线并运行验证，不保存场景。批处理入口需在同工程没有Unity实例时使用。当前验证与边界见 [TASK-023](Tasks/TASK-023-demon-footprint-decals.md)，使用说明见[FootPrint README](../Assets/SnowSystem/FootPrint/README.md)。
