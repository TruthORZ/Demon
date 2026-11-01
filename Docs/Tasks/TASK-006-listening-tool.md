# TASK-006: T05 补全与风铃监听切片
Status: done
Updated: 2026-10-28 Asia/Shanghai

## Goal and scope
承接用户“进行下一步”，完成T05补全/风铃区域监听的最小可玩切片，接入九天场景D6，新增D6快速场景。设计依据[GAME_SPEC](../GAME_SPEC.md)第3/5/6/7节；实现归属与调参见[ARCHITECTURE](../ARCHITECTURE.md)第7节。
箱子A/方位轮盘、T06光束、真正追逐、前三块设施/完整永夜及正式美术为后续；雪痕代理仅是监听教学占位，不表示恶魔AI完成。

## Acceptance criteria and result
- 已实现并验证：D6可补全T05；错误不固化，成功当日启用风铃，不推进日期、不预约T07窗口、不修改B/湖。监听Board与Session.Board独立。
- 已实现并验证：区域外→内报警一次，停留不重复、离开重入重新报警；启用/重启时已有占位只建立基线，不凭空发事件。A/B独立、禁用重新启用正确；抠走表面符文不取消固化快照。
- 已保存并验证：不同位置/低高音空间风铃、可编辑球形范围/音效/摆动、雪痕路径/速度、近景机位；木墙遮挡目标时仍报警，无精确目标坐标HUD。响度由玩家到风铃的距离衰减，不以玩家到恶魔的距离冒充。
- 已完成：Unity编译、规则测试、真实Play/键鼠、保存参数修改后运行采用并恢复、旧路线回归、Windows构建和截图检查。人耳音量/方位感、独立exe真人试玩及完整九天恐怖节奏未做；合成音效/几何仍为灰盒。

## Impact and decisions
- 基线main@a37036c，仅ProjectSettings/UnityConnectSettings.asset服务启用差异为既有本地修改，保持不动。本轮未提交或push。改动前完整Assets/Demon、Docs、BuildSettings及meta哈希备份在忽略的Temp/Task006Baseline。
- 复用RuneBoardModel、SliceTablet、Player近景和HUD；Enter/手持符文显示跟随ActiveTablet；T07未配置ListeningTool时沿原路径。NineDayTablet保留index=4，T05子对象转接可补全近景，发现计数由NineDayFlow管理。
- 新增RegionEntryLatch纯规则；ListeningTool显式组合、ListeningChime显式注入目标Transform，监听器不依赖代理路线。新符文编号从10追加，旧枚举含退役5不变。
- T05目标模板暂为ON/ENTER/DEMON→RING/REGION/CHIME，补中心RING/下中CHIME。此为可调机制教学占位，不是通用ON解释器，不改变已接受的监听语义。
- Listening_Authored保存两个风铃、遮挡墙和路线；ListeningTrailProxy仅启用监听后以2米/秒、节点停留1.5秒移动雪痕。范围默认1.55米；AudioSource空间Linear衰减1至16米。未来可将monitoredTarget连接真正恶魔。
- 九天场景局部增加；原T05占位体/标签停用保留，旧对象ID无丢失；原DemonSlice场景哈希完全不变。DemonListening为独立保存的D6测试场景，布局不会自动同步九天；共享监听定义，独立ListeningSettings控制D6起点。
- 截图发现原字体穿透实体，九天与D6场景用保存的WorldFontDepth材质/URP Shader修复，字体动态字库材质在运行时克隆与清理；原D8字体默认路径不变。两场景uGUI加保存的Outline改善雪地可读性。
- Editor沿固定白名单新增listening-create/validate/readability/smoke/tuning/build；Create拒绝覆盖已有输出，之后手调保存内容，不重建地图或Prefab。不安装包/切换管线/输入，不启动第二Editor。

## Verification
证据目录：Temp/DemonSliceReports（忽略产物，报告ID可查）；不以旧历史报告代替本轮结果。
- listening-create-001：Editor原生保存并重载T05、两个空间风铃/路线/独立Board连接成功。旧九天491个原对象ID全部保留；原meta哈希无变化，源文件无丢失、新源文件均有meta。
- listening-tests-001：真实Unity EditMode 33 passed / 0 failed / 0 skipped（原26+新增7），XML保留。
- listening-smoke-001通过。截图发现默认TextMesh穿透、墙文字过大；listening-readability-001局部保存字体深度材质、描边、缩小/偏移墙说明和新增观察机位。
- listening-smoke-002第2帧W位移断言失败：Shader首次导入卡顿，测试按Editor墙钟等待0.3秒时游戏只推进一帧。改测试阶段为Time.timeAsDouble，保留W位移断言；listening-smoke-003通过。没有弱化玩法断言。
- listening-smoke-003：实际W/E、错误符文+Enter重试、鼠标射线/Enter正确补全T05；独立Board、日期/SEAL/窗口不泄漏；A/B进入/停留/重入与生命周期；AudioSource实际isPlaying；固化后取符文仍生效；木墙射线遮挡/区域报警通过。已查看新layout/area/occluded截图，远处标签不再穿板、墙不再显示后方标签。
- listening-tuning-001：将快速场景半径保存为2.25、代理速度1.25、近景横移0.08；Play采用，退出DEMON_LISTENING_TUNING_RESTORED。核对两场景半径1.55、速度2、原机位，演出仍6秒。
- listening-nine-normal-001：从D1逐日发现，D6进入T05补全视角，D8夜间门槛/错误尝试/长阅读/挂钟/D9二次交换/闭箱/湖光/延后面板通过。
- listening-nine-skip-001：全睡眠0发现/0交换，止于D9零点。
- listening-old-input-001：原D8 W/E、六次符文鼠标射线、Enter、Box实际E、阅读/Esc通过。
- listening-build-001：D6 Windows64构建成功173986298字节，Builds/WindowsListening/DemonListening.exe。
- listening-nine-build-001：九天Windows64构建成功173986294字节，Builds/WindowsNineDay/DemonNineDay.exe。完整输出目录需一起保留，不能单拷exe。
- 构建暂时移除的InputSystem预加载引用由包构建处理恢复，ProjectSettings.asset与HEAD相同；没有包/BuildSettings差异。UnityConnectSettings既有修改不动。

- listening-validate-final-001：构建与空白清理后重载D6场景，保存引用检查通过。git diff --check通过；旧meta改变0、新资源缺meta为0、旧九天场景对象丢失0；原D8场景和BuildSettings与基线哈希相同。Editor最终为D6快速场景、非Play，无待处理请求。

## Delivery and next action
快速入口Assets/Demon/Scenes/DemonListening.unity，D6 10:00。Play后点Game，E进入T05，点RING→中心，CHIME→下中，Enter。近景可Esc退出，WASD走到右后雪径观察A/B，再绕到木墙南侧听。程序固化后不能用Esc取消。九天源场景同样在D6开放。
策划改Data/ListeningDefinition（目标/文字）、ListeningSettings/NineDaySettings（日长/移动）；美术在FixedTablets_Authored/T05_FixedTablet/T05_Editable改板面/CloseView，在Listening_Authored改风铃/范围/音效/遮挡/代理路线。两场景布局分别保存。
下一步先收集试听/可读性反馈，再制作箱子A与方位轮盘，随后T06光束运动交换与失锁复位，最后T07争夺；不使用旧573密码或隐藏D7硬锁。完整真人试玩仍待验收。
