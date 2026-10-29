# TASK-013: D3–D4林间T02与洞外仪式T03
Status: done
Updated: 2026-10-29 Asia/Shanghai

## Goal and accepted scope
用户已验收TASK-012，并明确批准D3检查停转风向标、沿大型雪地痕迹找林间T02、体验已有追逐；D4山洞外仪式遗迹找T03、保留推理空间。不加强制打卡/伤害/额外追逐波次，仍可直接睡觉。本轮不做D5–D8新探索、正式美术、存档或Git发布。

## Acceptance criteria
- D1/D2无大型痕迹，D3才出现遗留大痕；它表示过去经过的路线，不替代实际移动雪雾/风铃的当前威胁位置。原Motion/声音/光日期保持。
- 原T02/T03移到可步行访问的新保存位置；分别D3/D4显示，提前仅可到空场地，不用看风向标/前块石板解锁。阅读/重复/追近不丢发现、不改日期程序；不能提前广播或失光。
- D3三站晚观察不补记D1，D4提示分明；沿现有林地进出，T01/原营地/密码342/湖岸边界/T07保持。复用Chase，不加新波次、生命值或死亡。
- 主场景+新DemonInvestigation，旧八个快速源场景不改。新原生InvestigationNavigation包含后续显示碰撞、保存重载完整可达；旧四份导航/meta/对象ID不破坏。
- 真实WASD/E/Esc搜寻往返、D3自然跨日/运动边界、D4/T03重复发现、D5/D7生效不依赖阅读、跳过/晚起/保存调参、旧D1–D2/九天工具回归、截图与Windows构建。

## Impact and plan
基线main@90ad3c2，TASK-009至012等大量既有本地修改保留。Temp/Task013Baseline包含本轮前Demon/Docs/ProjectSettings/BuildSettings。
扩展现有ExplorationFlow/Definition可选D3–D4字段，HUD复用已保存ExplorationStatus，不创建另一套任务系统。旧DemonExploration字段为空仍只显示D1–D2。NineDayFlow继续唯一拥有原T02/T03显示/发现和日期；Chase/Tether/SOUND/LIGHT规则不改。
布局仅利用现有雪林：大遗留痕迹从营地南侧穿通路向西岔到T02约(-2.5,1.2,-20.5)，朝北、站位(-2.5,0.04,-18.2)；T03位于洞西侧外部仪式遗迹约(3.4,1.2,-24.1)，朝北、站位(4.8,0.04,-21.7)。树与原墙不搬；路线以原物理输入与原生导航确认后可局部调整。新几何/痕迹/机位保存可编辑，运行不生成地图。
新导航保留原Surface范围，保守烘焙晚期显示物，保存独立资产，不覆盖Exploration/旧练习导航。新Quick从D3 08:00开始，不预给工具、不伪造发现。

## Progress and verification
- investigation-create-001保存/重载成功，营地→T02→T03→营地原生完整路径及342保持。
- investigation-check001-tests：78 passed/0 failed/0 skipped。boundary从D2 23:59.4自然跨D3，大遗留痕/T02/MOTION/Chase出现，T03/SOUND/LIGHT未提前，零发现。
- 首轮smoke真实WASD/E/Esc完成风向标、林间T02、D4洞外T03与往返，重复阅读不增加发现，71.63575m、3次既有追近反馈，睡至D5/D7既定属性通过。
- 截图排程修正：CaptureScreenshot延迟至帧末，原同一更新又按Esc使截图只见石板本体；改为捕获后等待0.25s再Esc，回营截图后等待再Sleep。断言阅读本身已通过，重跑用于真实阅读画面检查。
- 早期追近文案原提示使用尚未取得的风铃/光束；ChaseDefinition近身文字改为保持移动/观察/可返营，原速度/路径/退开/冷却完全保持。资产替换只接受已知旧文案或本轮新文案，拒绝覆盖其他手动定制。

- extra001的smoke/skip/late/tuning、旧exploration-smoke/nine-normal/nine-skip/chase-smoke/retrieval-late全部通过；旧1647主对象ID零缺失，8个旧Quick源场景/4个旧导航/旧meta无变化。
- 阅读画面复查：T03原机位/站位落入无碰撞树冠，调整保存的原ReadView到(4.9,1.55,-21.4)、站位到(4.8,0.04,-21.7)，朝原T03，不移动石板/树/碰撞/导航。北向牌面对朝南玩家，向西的岔路应显示右箭头，修正为“林间旧迹 →”。待重跑当前画面与步行验证。

- view001当前站位完整物理重跑通过（71.01067m、3次追近）、78规则再次通过；机位离开树冠后个别帧仍有UI字形缺失，故不能将其都归因于几何。已查安装uGUI源码Text.FontTextureChanged/FontUpdateTracker：动态图集在Canvas构建中可嵌套更新。FontBinding增加按textureRebuilt标记、LateUpdate一次延后调用原生FontTextureChanged的刷新，不每帧重建UI；正验证是否稳定，未预先宣称解决。
- 玩家T02/T03记录移除音频/日轮可替换占位等制作备注，保留日期与程序原文；制作状态仍在本记录/架构文档说明。

- font001的功能/晚起均通过，但部分首读截图仍缺字，延后FontTextureChanged没有充分解决，已撤回该实验。改在同一SliceFontBinding内分别创建UI32与世界字64两份OS动态字体，世界材质只监听世界字体图集；原字体家族、各Text字号/布局保持，销毁时两份均释放。新增Smoke断言两端Font资源不同且Overlay模式，正进行两次独立启动/重复阅读画面验证。没有切换UI框架或导入字体文件。

- 后续分离OS字体、每帧刷新记录和延长后台截图等待均未稳定消除自动截图缺字，字体/运行时HUD试验已全部撤回。实际材质为UI/Default、Canvas为ScreenSpaceOverlay，原文本内容/174字符缓存仍完整；不能据此断定根因。增加有界真人阅读预览，实际打开T03请用户辨认，避免继续无证据改UI。
- 用户继续时Unity已经重启，Temp下旧基线/报告/截图被清除；保存源资产和Docs仍在。新恢复检查点Logs/DemonCheckpoints/task013-resume-20261029-1727含当时Demon/Docs/设置，后续每次检查把JSON/PNG/XML备份Logs/DemonChecks/Task013；辅助脚本同目录有副本。前半段临时证据只保留任务中的既有记录，不能再链接不存在的Temp文件。
- 恢复后resume001-late真实D4搜寻/重复/返回/D5D7通过；该次仍出现截图缺字，运行时刷新实验撤回。live001-review正在显示T03，等用户核对；原设置会在退出时自动恢复。

## Resume checkpoint after Unity restart
- 原Temp报告/基线已被重启清理，保存源资产完整。恢复检查点Logs/DemonCheckpoints/task013-resume-20261029-1727，后续证据Logs/DemonChecks/Task013，辅助脚本同目录。前阶段通过数字是当时观察并记入本任务，不再引用已不存在的原始临时报告。
- 原FontBinding与HUD字体实验均已撤回。resume001-late功能通过，截图仍偶发缺字，不能认定解决；live001-review未获真人反馈，210s超时退出并恢复D3/08:00/3.3，此项不是通过的真人验收。现在预览上限600s，退出恢复规则保持。
- 新元数据/八个旧快速源场景/全部五份已保存导航与恢复基线一致，缺meta零、binary属性正确；最终构建/设置恢复后的审计待做。
- 当前Unity编译请求停在等待（无新编译错误日志）；final001-tests请求仍pending，后续串行测试可能超时。CUA helper启动失败，已请求用户切到Unity检查弹窗或Ctrl+R。不要覆盖pending请求，不开第二Editor、不清Library。

- 用户截图确认未看到阅读记录时已在Edit（顶部三角形），不能据该截图判断字体。编译在17:53恢复，final001-tests=78/0/0，已补归档。live002-review在17:57:51真实打开T03并最多保持600s，等待用户回答“文字完整/缺字/仍未看到”。只在收到反馈后用stop标记结束本测试；产品代码中不存在该预览路径。

- 真人显示确认：live002-review重新实际打开T03后，用户明确回复“文字完整”。此项阅读文字显示通过；自动PNG中缺字不能据此认定产品字体故障，所有字体/运行时HUD实验均已撤回。已通过stop标记结束本代理预览并恢复设置，接续最终流程与构建；不把此反馈扩大为整段D3–D4操作或独立exe试玩验收。

## Final verification and current delivery
- 阅读显示：用户在live002-review实际打开的T03窗口明确回复“文字完整”。预览收到stop后退出，源设置恢复D3/08:00/步速3.3。自动PNG偶发缺字仍无法定因，不以它宣称产品字体故障；FontBinding与HUD字体实验全部撤回，最终审计确认。
- investigation-final001-tests：78 passed/0 failed/0 skipped，报告/XML已归档。
- investigation-final002-boundary/smoke/skip/late/tuning全部通过。自然D2→D3运动/大痕/T02生效但T03/声音/光不提前；默认真实WASD/E/Esc完成两日搜寻往返70.32761m、4次原追近；重复阅读去重；D4不依赖T02、零发现可睡至D9且终局隐藏痕迹；保存文案/步速2.8生效并恢复。
- 同前缀exploration-smoke/nine-normal/nine-skip/smoke-input/retrieval-late/chase-smoke回归通过，包含原T01约75m往返、九天两交换/阅读/结局、真实鼠标符文输入、持E打断与回棚及冻结/冷却。
- investigation-build001-investigation-build：Windows64 Development成功174277599字节，Builds/WindowsInvestigation/DemonInvestigation.exe；nine-build成功174277591字节，Builds/WindowsNineDay/DemonNineDay.exe。未真人独立exe试玩，交付保留匹配_Data/UnityPlayer/Mono等整个目录。
- investigation-final003-investigation-validate再次保存重载验证通过，当前Unity打开DemonInvestigation，非Play。构建自动加入InputSystem预加载字段，仅在确认其余设置等同恢复基线后恢复；ProjectSettings/BuildSettings与恢复基线一致，原有本地差异保留。
- Logs/DemonChecks/Task013/final-audit.json：1842个恢复基线主场景ID零丢失、旧meta/全部GUID零变化、八个旧Quick/五份已保存导航零变化、缺meta零、无待处理请求、原FontBinding无diff/HUD无试验刷新。早期1647个前任务ID零丢失的历史审计见上文，原Temp已清理。新导航binary声明正确，git diff --check=0。
- 所有新验证JSON/PNG/XML与辅助脚本归档Logs/DemonChecks/Task013，避免再随Temp清理丢失。字体/采集失败试验保留为历史，不把预览超时当成功验收。main仍90ad3c2，无自动commit/push。

## Human editability and next action
打开Assets/Demon/Scenes/DemonInvestigation.unity，Play从D3 08:00开始：E看停转风向标、Esc放下；背向湖走南侧林径，大雪痕在岔路向西（面对南侧时右手边）去原T02。回营用睡袋到D4，再沿林径找山洞西侧外部石柱遗址T03，短按E阅读、Esc返回。可以直接睡觉，不强制检查/发现；本轮未加伤害或新追逐波次。
提示文案在ExplorationDefinition的Day3/Day4字段；原日期/石板正文在NineDayDefinition。场地/路牌/痕迹/柱与原板在Investigation_Authored，原ReadView可Inspector调整。ForestRoute/RitualRoute是布局/物理走行验证参考，不驱动玩家或NPC；改实际脚印仍编辑其保存GameObject。移动碰撞/地面后在Chase_Authored/NavMeshSurface重烘焙本轮InvestigationNavigation，勿覆盖旧练习资产。
2026-10-29用户回复“进行下一步吧”，同意结束本阶段并推进后续。自动验证和实际T03文字确认见上文；未收到完整路线/节奏或独立exe真人试玩的具体反馈，不把推进授权写成这些检查的通过记录。下一阶段见TASK-014，D5–D6具体路线待确认；正式美术/全九天真人体验/存档尚未完成。