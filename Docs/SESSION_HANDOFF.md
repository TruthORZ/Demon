# Session Handoff

Updated: 2026-10-28 Asia/Shanghai
Latest completed task: [TASK-005 无+1午夜结局](Tasks/TASK-005-no-plus-one-finale.md)。此前九天骨架见[TASK-004](Tasks/TASK-004-nine-day-flow.md)。
Repository/Unity project: D:\Unity\Projects\Demon。
Branch: main；本次发布前HEAD为7638ece33d2c2bd2e1d936de09c9cc62acd64955。用户于2026-10-28明确授权提交并推送当前成果；发布版本的准确HEAD以`git log -1`为准，远端核对结果见本轮交付回执。

## 当前状态

用户确认最新九天流程，并授权更新设计文档、开始下一步构建。GAME_SPEC已更新为2026-10-28基线，含完整日期表、T05正常搜寻/T06箱子A、轮盘物体方位密码、运动/声音/光顺序、无+1日期WHILE、风铃监听/光束交换设计，以及阅读后的午夜挂钟结局。

本轮实现范围：迁移现有九天灰盒与D8快速场景，删除+1与固化后的自动加速；T07在D8 22:00按时刻开放。首次交互保留午夜前窗口，错误尝试/长阅读不会抢先降临；Esc关闭箱内记录后短暂停留→挂钟倒计时→D9二次交换、箱盖猛合、湖光重亮→实体结局。主动睡眠释放窗口，允许完全不探索直接到D9。

快速试玩：Assets/Demon/Scenes/DemonSlice.unity，D8 22:00；Unity最终切到此场景，非Play状态。WASD/鼠标/E近景，EXCHANGE放中心、BOX下中、LAKE下右，Enter固化，再去左侧B箱按E阅读、Esc继续，R重开。九天入口仍是DemonNineDay，BuildSettings置首不变。

## 设计与实施边界

- 设计以[GAME_SPEC](GAME_SPEC.md)为准，代码与实际资产见[ARCHITECTURE](ARCHITECTURE.md)。T05/T06在当前场景仍只读记录，尚未制作风铃、补全、密码箱A/轮盘、手电牵制、追逐。前三块设施/声音/完整永夜能力也未接入；不要将设计文本当实际功能。
- 当前T07灰盒在近景交互时预约窗口；未来接入夜间争夺时应在该段入口调用同一预约逻辑，不能延后到读B箱才预约。
- IF一次性、日期WHILE每天一次为已确认规则；当前没有通用IF/ON语言解释器。RuneKind退役值5保留空缺，SEAL/BOX/LAKE原数值不变。槽6空且不显示，直接返回9→3；所有已有GUID保留。
- 程序与演出时间分开；SliceFinaleSequence组合显式连接，FinaleTimeWindow纯C#窗口。Session.Descended停止规则时钟，EndingPresented延后显示结局，以便看完钟与闭箱。

## 保存资产与调参

- NineDayDefinition：Days/WakeHour、Tablets记录、FinalTabletDay=8、FinalTabletHour=22。一天默认1440真实秒，在两套Settings中调整；快速场景StartHour=22。
- SliceComposition/SliceFinaleSequence：保留1游戏分钟、读后停留1.5秒、挂钟倒计时6秒、午夜/箱子展示各1.2秒；音效、灯强和视角引用可编辑。
- Finale_Authored：WallClock、ClockView、ClockLamp、LakeOpenGlow、TabletMidnightPulse和AudioSource均为保存的场景对象。ClockView在右柱前，已避开开箱盖；Box/ReadView与Environment/EndingView保持原作者化机位。
- Assets/Demon/Audio：PrototypeClockTick.wav与PrototypeBoxSlam.wav为合成占位，可替换。模型、音效、文字均未做最终美术与氛围验收。
- Windows新版：Builds/Windows/DemonSlice.exe快速入口；Builds/WindowsNineDay/DemonNineDay.exe九天入口。需整个输出目录，不能单拷exe。Data目录为本轮构建，原生exe文件可沿用引擎二进制旧时间戳。

## 验证

- 26项Unity EditMode通过：finale-tests-001。新增不自动推进、每日去重、长窗口/极晚操作不倒退、释放睡眠等规则测试。
- finale-normal-001：D1首夜、逐日发现、D8早间禁止/夜间开放、错误尝试、8秒阅读、挂钟、第二次交换、闭箱/湖光及延后面板通过。
- finale-tuning-001保存倒计时3.5秒和机位横移0.08米，实际Play采用，退出恢复；已核对日志DEMON_FINALE_TUNING_RESTORED及两场景6秒/原机位。查看修正后的钟、阅读与终局截图。
- finale-late-001午夜前0.01游戏分钟交互通过；finale-sleep-reserved-001窗口内主动睡到D9通过；finale-skip-001全睡眠0发现/0交换通过；finale-input-001实际虚拟W/E/鼠标/Enter/箱子E/Esc通过，退出恢复输入配置。
- finale-build-001九天Windows64成功173927554字节；finale-quick-build-001快速Windows64成功173914482字节。构建后finale-validate-001重载九天引用通过。
- git diff --check通过；既有meta哈希与任务基线相同、原文件无丢失、新文件都有meta。未做独立exe真人运行或完整九天真人试玩；自动化不证明恐怖感。

## 工作树与工具

本次发布纳入此前Assets/Demon功能及meta、Docs、URP/Graphics必要设置，以及本轮规则/演出/Editor工具/两场景/设置/占位WAV和TASK-005。构建产生的ProjectSettings.asset有效InputSystem Actions预加载引用一并保留。没有包/管线/输入切换。与玩法发布无关的UnityConnectSettings.asset服务启用差异仅保留本地，不提交；Builds、Temp、Logs与Library继续忽略。

提交检查额外发现58个新Unity序列化文件的空字段冒号后有生成空格，仅清理这些空白并校验非空白内容完全一致；GUID及资源含义未变。暂存文件检查通过，缓存/可执行构建未进入提交。

改动前完整备份Assets/Demon、Docs与EditorBuildSettings至忽略的Temp/Task005Baseline；不是Git提交。迁移已完成，FinaleAuthoring.Migrate遇已存在内容拒绝重跑，之后直接手调场景。第一次迁移预检查因末段编号误算而失败，未改场景；后修正成功。截图发现的钟遮挡已单独修正，详见任务。

没有Unity MCP，继续用固定白名单Temp/DemonSliceAuthoring.request.json操控现有Editor，不启动第二个Editor。新增finale-migrate/validate/sightline及nine-late/sleep-reserved/finale-tuning白名单。请求不打断已有Play；默认shell沙箱此前失败，require_escalated可用。日志仅筛必要错误与DEMON_SLICE_RESULT，不打印原始启动参数/完整进程命令行。

## 从这里继续

1. 让用户试玩D8快速新结局，收集阅读、视角与倒计时反馈；按保存源资产调整，不重新生成整个场景。
2. 下一切片：建立T05补全/风铃区域监听→箱子A与方位轮盘→T06稳定光束运动交换/失锁复位，先用最小雪地测试区验证后接T07夜间争夺。依当前已确认设计制作，无需重新提问已确定的工具方向；具体未定布局/密码场地在关卡任务中记录为可调占位。
3. 继续保护未提交工作和GUID，无新授权不提交/push。进入新的多模块任务前创建对应任务、建立基线并核对实际场景/差异。
