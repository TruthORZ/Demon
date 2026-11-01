# TASK-012: D1可选观察与D2脚印山洞探索
Status: done
Updated: 2026-10-29 Asia/Shanghai

## Goal and accepted scope
用户明确确认轻量方案：D1检查电台/风向标/湖岸，D2沿人类脚印找到山洞T01；本轮只做前两天可玩切片，日常不强制打卡、不加资源消耗、仍可直接睡觉。山洞无日期门锁，D1可探索空洞，T01/小脚印按原发现日D2出现。
GAME_SPEC仍为总玩法所有者；后续D3-D8具体搜寻、惊吓、正式美术/存档不在本轮。此前D5声音/D7永夜已由用户明确测试通过。

## Acceptance criteria
- 三处真实E观察并显示时间快照；D1首次记录各计一次，重复不刷进度，不推进时钟、不影响SEAL/日期/工具。未检查也可睡到D2/D9，晚检查不伪造D1完成。
- 保存雪林支路与山洞、可步行往返；D1无人类脚印/T01，D2小脚印引路到原T01，巨大恶魔雪痕仍D3才出现。T01阅读可重复、发现去重，不激活/推迟D3既定程序。
- 原营地/设施根位置与342保留。只拆分南边界开4m通路，原Collider/对象ID保留；新增地面、林木、洞口/仪式刻痕/作业灯可Inspector编辑，无运行时生成关卡。原T01移动到洞内，不创建第二块。
- 主/新DemonExploration接线；其他快速源场景保持。原生重新烘焙扩展NavMesh保存新资产，完整营地↔洞内路径及D3追逐可达；老导航不覆盖。
- 纯记录规则、真实WASD/E/Esc检查与跟脚印步行到洞、读取T01/返回、跳日与旧九天/工具回归、截图/参数保存恢复/Windows构建。灰盒自动通过不代替真人节奏验收。

## Impact and plan
基线main@90ad3c2，97项已有本地改动保留不提交/push。Temp/Task012Baseline为本轮前Demon/Docs/元数据/设置备份。
新增FirstDayInspectionLog、ObservationSite，配置文本ExplorationDefinition及ExplorationFlow/StationObservation，World显式可选组合，HUD新字段仅D1-D2显示。记录复用ReadDocument近景，不建背包/通用任务系统。
地图以原26x16营地为锚：南侧x1..5开口，旧SouthBoundary改左段Collider中心/尺寸、根Transform不动；右段新对象。新增雪林地面x[-6,17],z[-28,-10]、树与山洞约(10,0,-23)，路线(2.8,-7)→(3,-9.5)→(3,-12.5)→(5,-16)→(8,-18.5)→(10,-22.3)。旧湖岸不可涉水边界不动。
原T01根移到(10,1.2,-24.6)、Y180，原ReadView/身份/日期引用保留。新导航Volume中心(2,1,-11)、大小(30,8,34)，保存ExplorationNavigation。小脚印是静态人工痕迹/无光刻痕，不是D2恶魔运动。

## Progress and verification
- 原生Create保存/重载主及探索场景，扩展NavMesh营地↔洞完整路径与原342验证通过。78项EditMode规则全部通过。
- 1161个旧主场景对象ID全部保留，旧meta变化0，七个旧快速源场景/三份旧NavMesh哈希一致。
- 首次编译缺AI引用已修正，用户手动Refresh/API Updater后恢复。后续一次测试脚本局部变量编译错误已修正；编译状态须看新DLL/日志，Refresh回执不等于新代码加载完成。
- 物理步行初测卡在睡袋东侧(-3.26,-2.2)：Agent的0.75m爬阶路径越过0.4m睡袋，玩家stepOffset0.3不可直接跨过。测试改沿真实空道绕行，不改玩家/追逐规则。
- 电台南东侧站位的视线被箱A密码面板挡住，物理E实际打开箱A；调整观察站位至西南边(-9.1,0.04,-2.4)，保留设施/导航，重新验证。

- 再次物理E证据为SnowBank_6先命中、RadioBody在后；非均匀缩放SphereCollider按最大轴产生超出矮雪堆外观的碰撞。主/探索场景12个原球组件保留但禁用，新增引用原mesh的静态MeshCollider贴合外形，并重烘焙本轮ExplorationNavigation（保留GUID）；其他七个源场景/老三Nav不改。不是扩大交互距离或穿透碰撞选择。

## Final verification and delivery
- exploration-final001-exploration-smoke通过：三处真实WASD/E/Esc观察及重复去重，D2沿小脚印行走到原T01、两次E阅读仍FoundCount=1、实际走回营地，总行程75.06311m；D2不提前MOTION/SEAL。随后睡至D3，检查Agent绑定与完整路径；只在已证明返回后把玩家测试目标定位至洞内，生产ChaseController自主追逐穿过通路进洞（这次定位只属于测试fixture，产品没有玩家传送）。
- exploration-final001-tests：78 passed / 0 failed / 0 skipped。
- exploration-extra001-exploration-skip/late/tuning通过：不观察/发现也可睡D1→D9，D2起始不补记D1，保存文本前缀与walk2.7实际采用。退出后核对StartDay1/Hour8、walk3.3与原文字已恢复；未建立跨进程存档系统。
- exploration-extra001-nine-normal/nine-skip/smoke-input/cipher-smoke/listening-smoke/retrieval-late全部通过，保留原工具、实际持E/回棚/符文/B阅读与D9行为。
- 脚印、三站近景、洞内T01、返回截图在Temp/DemonSliceReports/exploration-smoke-*.png，已人工查看自动化捕获画面，文字与路径可见。此处人工查看指Codex检查截图，不是用户真人试玩。
- Windows64 Development：exploration-final001-exploration-build为174248394字节，Builds/WindowsExploration/DemonExploration.exe；nine-build为174248390字节，Builds/WindowsNineDay/DemonNineDay.exe。完整_Data/UnityPlayer/Mono依赖必须保留，未真人独立exe试玩。
- exploration-final001-exploration-validate再次重载通过，打开DemonExploration、非Play。Temp/Task012Audit.json：1161旧主场景ID零缺失、旧meta零变化、七个旧快速场景/三份旧导航均未变、缺meta零、BuildSettings未变、无待处理请求。新导航text/diff/merge均unset（binary）。git diff --check=0。
- 构建自动加入的InputSystem预加载引用，在确认只有该字段不同后恢复到Task012基线；保留原ProjectSettings空preloadedAssets和UnityConnectSettings既有差异。无自动commit/push，仍main@90ad3c2。

## Human editability and next action
打开Assets/Demon/Scenes/DemonExploration.unity后Play：D1在箱A左侧电台本体、风向标观察牌、湖岸观察牌按E，Esc返回；重复不增加右上记录。可直接用睡袋E到D2，背向湖走南侧林间小径，沿小脚印入洞，T01按E/用Esc退出并原路返营。山洞无日期门锁；D1板/人类脚印隐藏，恶魔巨大雪痕仍D3才出现。
文案/状态提示在Data/ExplorationDefinition.asset，步速/起始日时在ExplorationSettings；空间布局/小脚印/洞口灯/仪式刻痕在Exploration_Authored，三站与机位由ExplorationFlow/StationObservation显式引用。改障碍/地面后重新Bake对应Chase_Authored/NavMeshSurface的新ExplorationNavigation，勿覆盖旧练习数据。
待用户验证三处观察操作、脚印辨认、进洞/返营是否顺畅；正式美术、完整D3–D8探索/惊吓、独立exe人类体验与存档仍未验收/不在本轮范围。用户确认后再对齐下一段，不默认提交/push。
2026-10-29用户明确反馈没问题并要求下一步，D1–D2试玩验收通过。独立exe人类体验不据此推断。
