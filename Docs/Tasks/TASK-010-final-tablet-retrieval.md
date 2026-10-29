# TASK-010: D8夜间T07拾取、带回与补全
Status: awaiting verification
Updated: 2026-10-28 Asia/Shanghai

## Goal and scope
用户验收TASK-009并要求下一步。沿用现有ThreatPickup、Chase、Tether、T05、RuneBoard和FinaleTimeWindow，把正式主场景T07改为湖岸夜间出现、持E取得、带回工具棚放置后补全。新增独立DemonRetrieval快速场景，保留九天主入口和旧独立测试源场景。D5声音/D7永夜/完整九天地图不在本轮。
设计依据GAME_SPEC第6/7/8节；不改SEAL循环/日期规则，不强制先取得T05/T06，允许睡到D9。

## Acceptance criteria
- D8指定夜间前不能拾取或编辑；夜间只出现实体T07，不直接显示可编辑板。首次合法拾取即保留午夜前窗口，松开/Esc/威胁打断后保留窗口供重试，时间不倒退。主动睡眠仍释放窗口并到D9。
- 实际持E过程中关闭光束；雪痕逼近打断当前尝试并退开；已解A/取得T06/固化T05不丢。拾取成功只发生一次。
- T07取得后携带，回工具棚交互放置后才开放原九槽板；未取得/仅携带时不能通过world.TryCompile绕过。放置/修改/失败尝试不交换SEAL；固化只交换一次，读B→挂钟→午夜第二次交换与实体顺序保留。
- 保存场景/导航/源机位/布局/持E时长/提示均可编辑；参数保存后Play采用并恢复。真实键鼠/追逐/打断/放置/拼板/阅读/结局、晚到与睡眠、旧工具回归、Windows构建及截图检查。

## Impact and plan
main@90ad3c2。本轮基线Temp/Task010Baseline包含已验收但未提交的TASK-009完整Demon/Docs/meta哈希/BuildSettings；保留UnityConnectSettings本地差异，不自动提交/push。
新增FinalTabletProgress纯规则与显式组合FinalTabletRetrieval、FinalTabletDock；ThreatPickup新增局部开始策略/取得事件，由组合点绑定并清理，不创建物品管理器。NineDayFlow仍拥有最终板可见性，按放置状态显示；SliceSceneController正式重构入口额外检查携带/放置状态。旧场景没有该组件时保持原有单板行为。
将主场景原T07板（保留组件、槽位、GUID、持久绑定）移动缩放到工具棚侧边，新增可编辑空台/拾取石板。湖岸T07约(8.4,0,4.2)，棚内板约(-5.3,1.5,1)，朝东/板面向西；实际Editor检查后可调整本轮新增位置。NavMeshSurface原生重烘焙到新共享RetrievalNavigation资产，不改旧ChaseNavigation，快速场景仅配置起始时间/出生点变化。

## Progress and verification
已读交接、现有规则、组件及序列：当前T07直接由NineDayFlow.FinalAvailable显示，BeginFinalInteraction才保留窗口；正式物理拾取/带回尚缺。修复这两个边界，不重写终局规则。

## Next action
用户打开DemonRetrieval或WindowsRetrieval完整目录试玩验收；随后制作D5声音交换与D7光转移/永夜。

## Checkpoint
- retrieval-create-001：原生Editor保存主/快速场景、湖岸实体拾取、棚内放置台与原板移动缩放、共享新导航数据、HUD与独立起始配置，重载验证通过。
- retrieval-tests-001：56 EditMode passed / 0 failed / 0 skipped。
- 运行测试新增FinalTabletSmokeRun：实际右键冻结→E伸手中断/重试→真实WASD带回工具棚→E放置→真实鼠标交换符文错误/修正Enter→步行到B/真实E读记录/Esc→挂钟午夜与二次交换。正确目标板作为对抗性准备，验证未拾取/仅携带不能用TryCompile绕过；旧九天流程测试在新组合中实际经过取得/放置组件准备，不跳过正式入口检查。- 首次retrieval-smoke-001未通过：快速场景SliceSceneController.settings保存为fileID0，Awake明确报缺引用；主场景settings/player/tablet/presentation/HUD/endingView完整。CopyAsset/OpenScene后需重新LoadAssetAtPath赋值，而非沿用早先创建的quick实例（与现有ChaseAuthoring一致）。Create原验证仅覆盖T07接线，漏查基础settings；将增强完整组合验证并定向修复null字段，保留其余作者内容。等待本次测试超时退出后修复，不在Play中改脚本导致runner丢失。
- retrieval-repair-001通过，快速settings改为重新从资产路径加载，增强基础引用校验。
- retrieval-smoke-002已通过冻结、真实E伸手/打断/重试/携带及窗口保留；步行到棚路线在(-6.59,0.08,-0.91)被原箱A的碰撞边缘挡住。仅将自动化路线终点从x=-6.65改到其外侧-6.3，不移动/削弱箱子或跳过真实步行。旧meta变化0，原ChaseNavigation哈希不变。
- retrieval-smoke-003完整实际输入通过：夜间实体/未取得与仅携带正确目标板均不可编译、右键牵制转持E被打断/重试、WASD实走回棚、E放置、鼠标错位再修正Enter、步行到B并E长阅读/Esc、挂钟与D9二次交换。未用传送代替取得后的返回路线。
- 原Finale午夜pulse是独立位置灯，石板移到棚内后未随动。定向把原Light父级迁入原石板、保留引用/ID与现有效果参数，局部前偏0.5；之后作者移动石板会自动跟随。纯灯光父级变化不影响已烘焙导航；不改旧独立测试场景。
- 截图人工检查发现旧近景从棚西侧看石板时，箱A及棚柱遮挡右侧符文盘；仅槽位互换通过不足以证明可玩。保持地图/箱子/棚柱不动，仅把该板CloseView本地x改-0.45、z改-2.4并朝向共同构图点，避开遮挡。强化测试：验证全部四个tray的屏幕范围与物理射线；late/tuning从实际初始空槽用鼠标取EXCHANGE/BOX/LAKE、逐个放置，再故意错位并修正，不用预填板替代正常玩家流程。
- retrieval-late-001从临近午夜真实持E开始，含初始空槽的真实符文盘取符文/放置/错误修正全过程，通过。retrieval-tuning-001保存1.8s后同流程通过并恢复1.2s；retrieval-sleep-001被打断后主动睡眠到D9，未取得/未修复/零交换通过。
- nine-normal/skip/sleep-reserved/late、原chase/键鼠/cipher/listening回归（retrieval-*-001）全通过；retrieval-tests-final-001为56/0/0。WindowsRetrieval及NineDay首轮构建174125474/174125470字节成功。
- 近景复查仍有棚柱遮住符文盘文字右端：保留全部地图/棚柱/Collider/原导航，四项RuneTray的Label与TrayTitle本地x移到1.9，仍在各自原面板内部；机位构图目标y微降到-0.08以露出底部按钮。真实点击验证改用可见标签位置，并检查命中其原Collider，避免中心点可点但文字被挡的漏检。最终图与构建将刷新。

## Final delivery and verification
- retrieval-late-002：最终机位/全部标签位置下，从初始空槽真实点击EXCHANGE/BOX/LAKE文字取符文并放置，错误后修正Enter；实际右键转E被打断/重试、步行回棚/E放置、步行开B/长阅读/Esc、挂钟与D9第二次交换完整通过。临近午夜首次实际E保留窗口，逐帧检查时间未倒退。
- retrieval-tuning-001：保存1.8s持E后完整实际输入通过，退出自动恢复1.2s。检验时与最终版仅标签构图不同，规则/时长绑定相同。最终静态1.2s、Beam10m/1m/0.5s、Chase3.6m/s/1.6m/4s保持。
- retrieval-sleep-001及retrieval-nine-sleep-reserved-001分别验证被打断的未取得、已取得并放置但未固化时主动睡到D9，均零交换且窗口释放。retrieval-nine-skip-001全跳日零发现通过；nine-normal/late-001与旧chase/input/cipher/listening-regression-001全通过。
- retrieval-tests-final-001：56 passed / 0 failed / 0 skipped。
- retrieval-build-002：Windows64 Development成功174125474字节，Builds/WindowsRetrieval/DemonRetrieval.exe及完整依赖目录。retrieval-nine-build-002：WindowsNineDay同目标成功174125470字节。未启动独立exe或真人完整九天试玩。
- 最终取得、放置/全符文盘、B阅读等图已检查，标签完整可读，实际点击标签射线正确。1086个基线主场景对象ID无丢失；旧meta变化0；5个原独立源场景、旧Chase导航、BuildSettings保持；无缺meta。原灯引用保留并随板移动；二进制导航Git属性正确。
- git diff --check通过；代码/资产/文档保留本地，未提交/push，HEAD仍90ad3c2。TASK-009为本轮之前已验收的本地基线，未覆盖或回退。

## Authoring and remaining acceptance
- 正式主入口DemonNineDay；快速DemonRetrieval从D8 21:54开始，正常解A342再E取得T06；22:00右后湖岸出现T07，持E1.2s；被打断重试后带回工具棚西侧空台E放置，再原板补全/Enter、开B阅读/Esc。
- 保存布局源T07Retrieval_Authored及原Tablet prefab实例；时长/近景在LakeT07_HoldE的ThreatPickup与PickupView_Editable，出现时段在NineDayDefinition；文字在SliceComposition的FinalTabletRetrieval，追逐在ChaseDefinition。原板CloseView与Label本轮定向迁移后可继续人工调节，禁止自动重跑Create/view覆盖人工布局。
- 状态awaiting verification为待用户真人键鼠/手感验收；自动检查与整段接线完成。D5声音、D7永夜、九天完整地图/搜寻和独立exe人工试玩仍为后续范围。

- retrieval-validate-final-003：清理文本空白后保存重载通过，基础引用/导航/棚内原板/灯父级完整。最终非Play、无待处理请求；复查1086基线ID丢失0、旧meta变化0、diff检查退出0。

## Follow-up: 2026-10-29 T07对象顺序
用户明确要求第8/9槽BOX、LAKE不分先后。只对目标为EXCHANGE且对象对为BOX/LAKE的板按无序对象对匹配，其余槽位逐槽严格匹配；编译快照保留玩家实际顺序，SEAL交换/午夜循环继续原规则。缺对象、重复对象、OPEN代替对象、函数/属性/比较错误仍拒绝，不放宽T05或其他板。
旧测试把BOX/LAKE反序当错误，已改为错误函数排布后修正并反序对象，实际鼠标/Enter验证反序固化和D9第二次交换。规则测试增加双序/错误对象对/其他槽位/非交换目标覆盖。箱内记录改称末端参数槽，避免把第9槽写死为湖。
基线Temp/T07OperandOrderBaseline；保留前两轮所有本地修改，不提交/push。验证进行中。
- t07-order-tests-001：66 passed / 0 failed / 0 skipped。双序均首轮交换一次、重复提交不交换、D9第二次交换；快照保留原排列；错误/缺失/重复对象与其他槽位错误拒绝。
- t07-order-play-001：真实鼠标将第8槽放LAKE、第9槽放BOX后Enter固化，显式检查CompiledAt保持LAKE/BOX；开B/阅读/挂钟/D9二次交换通过。t07-order-normal-001原BOX/LAKE顺序九天流程回归通过。
- t07-order-wording-001使用Unity原生API更新7个SliceSettings箱内文本，均改称末端参数槽；未重排保存模板、场景、线路或对象位置。
- t07-order-build-001与t07-order-nine-build-001成功，WindowsRetrieval/WindowsNineDay分别174125534/174125530字节。t07-order-validate-001保存重载通过；本次未真人独立exe试玩。
- 保留本轮之前ProjectSettings.asset空preloadedAssets差异（构建仅自动补入InputSystem引用，字段对比后恢复原文件），原UnityConnect差异保留。未提交/push。
