# TASK-009: 导航追逐与拾取打断
Status: done
Updated: 2026-10-28 Asia/Shanghai

## Goal and scope
用户要求开始下一步，制作会跟随玩家并绕障的雪痕追逐，接入现有风铃/光束；接近时可打断正在进行的拾取，无生命值/伤害/死亡。主九天场景替换固定路线；独立DemonChase场景用练习碎片验证拾取基础，不改变已验收的A/T07正式取得流程。T07夜间争夺留下一切片。
设计依据GAME_SPEC第2/3/6节。拾取被打断指当前未完成尝试取消重试，已取得记录、已解箱、已固化程序保留，不退关或重置。距离/冷却/时长为本轮可调灰盒参数。

## Acceptance criteria
- D3起追逐玩家，玩家换位置后重规划，绕实体障碍、不穿墙或水边界；停在不可达侧不能隔墙打断。追近后可见退开/短暂冷却，避免连续钉住玩家；不移动/传送玩家，不加入伤害。
- MOTION冻结真正停止NavMeshAgent，失锁恢复；风铃仍读同一目标区域进入。保持10m/1m/0.5s已确认光束配置和342密码。
- 练习拾取实际按住E，松开/Esc取消；近景停用光束，威胁逼近可打断。成功只收一次；中断不清已有收集、锁/Board/日期/结局状态。禁用/场景切换不遗留拾取或导航状态。
- 导航数据为Editor烘焙保存资产；主体/速度/追近距离/冷却/退开点/拾取时长/机位可编辑。代表参数保存后实际运行采用并恢复；编译、规则、真实键鼠、导航绕障/动态跟踪/牵制/打断、旧工具与结局回归、Windows构建及截图检查。

## Impact and plan
基线main@90ad3c2c04261e00ea0599bfda09df3a3123d753，已有ProjectSettings.asset预加载移除与UnityConnectSettings启用差异保留。本轮完整备份Demon/Docs/BuildSettings/两项设置/meta哈希于Temp/Task009Baseline，不提交/push。
已安装AI Navigation2.0.14，使用官方NavMeshSurface/Agent，不新装包或重建寻路。当前AgentType0半径0.5/高度2/台阶0.75；按PhysicsColliders烘焙，排除玩家/动态目标，保存独立主场景和练习场景数据。Editor程序集只新增现有Unity.AI.Navigation引用；Runtime仅用引擎AI模块。
新增ClosePassGate和PickupAttempt纯规则；ChaseController显式引用已有雪痕NavMeshAgent、玩家、Tether与保存退开点。保留ListeningTrailProxy组件但在新组合停用，旧独立场景原路径不改。Player新增Pickup近景/持E状态与明确打断API；ThreatPickup拥有局部尝试及完成事件，不创建全局物品管理器。
练习碎片及绕障墙仅在DemonChase测试区保存，标为测试内容，不冒充正式T07。主地图原物体和旧GUID保留；日期、B/湖、A与T07规则不改。

## Progress and verification
已读设计/交接/核心源码及已装导航包源码、NavMeshAreas；当前场地无导航烘焙或追逐AI，固定路线仅为工具教学代理。继续现有Editor固定白名单，不打断Play/不开第二Editor。

## Next action
用户在DemonChase或完整WindowsChase目录真人试玩验收；随后接D8夜间T07正式争夺/取得与回棚补全。

## Checkpoint and investigation
- chase-create-001通过：主/独立场景原生导航已烘焙、保存、重载；NavMeshData实为Unity二进制，已添加两条精确.gitattributes binary规则以防换行转换损坏。
- chase-tests-001通过：53 EditMode passed / 0 failed / 0 skipped。
- 首次chase-smoke-001不是通过结果：运行揭示NavMeshPath不能在MonoBehaviour字段初始化器创建；退开CalculatePath因此空引用。已改在显式Initialize中创建。测试已到冻结/拾取/打断阶段，但未完成退开验证。
- 此次Play期间源脚本重载使内存测试runner丢失，首次报告未产生。refresh-005被既有Play保护拒绝；未绕过保护为通过。computer-use两条入口均因本机sandbox helper启动失败不可用；尝试仅限本次测试token的Editor恢复脚本，但后台未导入，已删除；用户手动退出Play后继续。后续测试运行期间不改脚本，等待报告退出后再修复。
- chase-smoke-002完成打断/重试后在禁用停止检查失败：isStopped仍有当前速度残留；Stop增加velocity清零。chase-smoke-003实际绕障、墙后不可打断、换目标、右键冻结/叶片/T05、持E/松开/Esc/物品禁用/打断重试/取得去重、控制器禁用与D9清理全部通过。
- chase-tuning-001在保存2.2m/s及1.8s后运行采用，冻结/打断均通过；退开距离断言只给1秒，在较慢速度绕障时过早失败。参数已自动恢复3.6/1.2。测试调整为持续校验冷却去重，并等待离开近身范围，3秒内仍未离开则失败且输出位置/目的地/阶段；不改变运行规则，也不跳过退开检查。
## Verified checkpoint
- chase-tuning-002/003：保存2.2m/s与1.8s后完整实际输入检查通过，退出Play自动恢复3.6m/s与1.2s；现有BeamDefinition保持10m/1m/0.5s。将加速度16m/s²、转向540度/s也集中到ChaseDefinition，避免隐藏的硬编码覆盖且保持既有行为。
- chase-tests-final-001：53 passed / 0 failed / 0 skipped。
- chase-nine-normal-001正常九天：D8夜间/保留长阅读/挂钟机位/第二次交换/箱盖/湖光/延迟结局通过；chase-nine-skip-001全睡眠D1→D9、零发现/零交换通过。
- chase-old-input-001：实际W/E/符文鼠标射线/Enter/箱子/阅读/Esc通过。chase-cipher-regression-001：D1真实密码/轮盘/错误重试/独立T06取得/重复发现/朝向不改密码/无B-SEAL时间副作用通过。chase-listening-regression-001：独立D6真实输入/T05/A-B空间音源进出停留/快照/遮挡通过。
- chase-build-001与chase-nine-build-001：Windows64构建分别174119642与174113994字节；补充可编辑加速度/转向后刷新最终构建，最终结果另附。chase-validate-final-001保存导航/单移动者/退开点/组合重载通过。
- 已检查冻结、打断、拾取画面。ScreenCapture在帧末取图，原collected截图被同帧Sleep替换为D9；测试已将拍摄移到成功取得后等待帧，最终截图以新运行文件为准，不将旧误标截图作为成功视觉证据。
- 原1066个主场景对象ID无丢失，旧meta哈希改变0，四个原独立D8/D6/D1/D7源场景及EditorBuildSettings哈希保持，资源缺meta为0；二进制导航资产Git text/diff/merge均unset。

## Remaining acceptance
本轮实现与自动验证不代替真人键鼠或独立EXE试玩。练习碎片不是正式T07；D8夜间争夺/回棚取得接线、D5声音交换与D7永夜、九天完整地图与日常仍是后续任务。当前源场景均为灰盒，脚印/踏雪音/蓝光是可替换占位。

## Final delivery
- chase-smoke-004（最终默认3.6m/s、1.2s）与chase-tuning-003（2.2m/s、1.8s）均完整通过；后者自动恢复并保存原值。最终collected图已检查：D7、练习碎片已取得、空台保留、未误入D9。
- chase-tests-final-002：53 passed / 0 failed / 0 skipped。
- chase-build-002：Builds/WindowsChase/DemonChase.exe及完整依赖目录，174120178字节；chase-nine-build-002：Builds/WindowsNineDay/DemonNineDay.exe及完整依赖目录，174114530字节。均为Windows64 Development构建成功；未启动独立exe，完整真人试玩未做。
- chase-validate-final-003保存引用重载通过。最终打开DemonChase，退出Play，无待处理请求；不提交/push，HEAD仍为90ad3c2。
- 状态awaiting verification指真人操作/手感验收尚未完成，实现与上述自动检查已完成。演示入口DemonChase，正式主入口DemonNineDay。布局源为保存场景；速度/加速度/转向/距离/冷却在ChaseDefinition；练习持E时长/近景/itemVisual在ChasePractice_Authored/PracticePickup的ThreatPickup。

用户在本轮后回复没问题、下一步，接受该切片并进入TASK-010；完整九天/独立EXE人工测试仍属后续，不改写为已完成。
