# TASK-015: D7永夜工具练习与D8交接准备
Status: done
Updated: 2026-10-30 Asia/Shanghai

## Goal and accepted scope
用户选择继续制作D7–D8，并确认通过可阅读交接记录呈现D8接班未到：原定D8交班、目前无人到达，原因留白。D7观察永夜、解原箱A取得T06、在湖岸练习光束；D8阅读交接记录、核对已有线索与工具，接上原22:00取回T07和午夜结局。探索可选，保留直接睡觉路线。
不增加伤害/资源/新追逐波次、接班者缺席原因、通用任务系统或新的时间跳跃能力。本轮不提交或推送Git。

## Acceptance criteria
- 主DemonNineDay及新快速场景保存D7–D8内容；交接记录在D8开放，E阅读/Esc放下，重复阅读不改变日期、发现/工具、T07时间窗或SEAL。
- D7提示尊重实际箱A取得状态与光束锁定/恢复；允许提前解A、跳过T05/记录，不设教学完成门槛。
- D8白天指向记录及可选准备，指定夜间接原拾取/带回/补全/读B/午夜流程；不提前泄露WHILE真相。
- 场景、近景、提示、记录内容和参数在Inspector可编辑；保存重载、实际键鼠与旧场景/九天正常及跳过回归，Windows构建。

## Impact and plan
基线main@213cfd751e766d5d6df696cb8c484240043432ac。仅两处既有设置差异：ProjectSettings.asset的空preloadedAssets、UnityConnectSettings.asset服务启用，完整保护。
复用ExplorationFlow/Definition可选扩展与原SliceInteractable/ReadDocument；新增局部记录交互与保存的D7–D8锚点。NineDayFlow继续拥有日期/发现，原Cipher/Tether/Listening/Chase/Retrieval/Finale继续拥有各自规则。新快速场景从D7开始，无预给工具/发现；旧十个快速场景和旧导航资产不覆盖。新增几何如不影响通行则沿用现有导航，若碰撞改变则使用独立原生导航资产。
先备份Demon/Docs/ProjectSettings，再实现最小运行时扩展、原生Editor定向作者化和验证工具；保存主/快速场景，编译/真实输入/调参恢复/规则与旧流程回归，更新架构与交接。

## Progress and verification
- 已核对Unity6000.6.0f1、URP17.6.0、Input1.20.0、uGUI2.6.0、Navigation2.0.14；沿用当前Editor和固定单请求工作流，无Unity MCP。
- 已读取TASK-007/008/010/014及当前代码；D7失光/光束、D8原取回与结局已有实现，本轮补内容衔接。
- 默认沙箱进程仍报setup refresh错误；require_escalated命令可用。未启动第二Editor。

## Next action
用户已确认本阶段测试通过，TASK-015关闭。下一轮范围尚未授权；建议先做D1→D9完整人工连贯体验与节奏检查，再确定实际场景资产接入批次。本轮未提交或推送。

### First authoring checkpoint
- 恢复基线位于Logs/DemonCheckpoints/task015-baseline-20261030；prep-preflight001通过，原Editor可接单，无需第二实例。
- 原生保存主场景与DemonPreparation、独立PreparationSettings/PreparationNavigation；主旧对象保留，未重建原密码/工具/敌人。prep-layout001-create与78/0/0规则、自然D6→D7及D8记录开放检查通过。
- 首轮实际WASD/E/鼠标/Enter已走通日出记录与箱A取得T06；在锁定同帧读取Agent.isStopped导致验证器失败。Tether在LateUpdate写锁定，Chase下帧Update停Agent；验证器改为等待一次Update后检查停止/位置/风向标，未改玩法或放宽停止断言。
- 已查看日出记录截图，中文正文和退出提示完整。共享每日提示保持旧练习场景也适用，新记录位置提示仅放可选探索扩展。
- prep-input002的完整两日真实输入70.02267m/5次原近身、直接D8两次阅读及全跳过均通过；交接记录截图完整。prep-check001-tuning保存2.8步速/正文前缀/机位偏移0.08后同路线通过并恢复。
- 初次prep-early复用CipherSmokeRun失败：旧夹具假定从A前方起步，但新快速场景从营地起步。没有改变出生点或密码；改用新两日夹具的真实步行解箱路径，从D1取得后睡到D7，检查已有T06跳过重复获取。
- 第二次早期检查等待HasNavigation，但D1的目标根按设计未启用。应仅在已开放运动的阶段等待活动Agent，不能为了测试提前激活D1敌人；保留有界超时自动退出，不在Play中改脚本。
- 早期直达A的自动最短路径在(-3.27,-2.18)被原营地低矮障碍阻挡，下一Nav角点高0.42m；改沿已经真实走通的日出记录站位绕到A，不挪动原箱/障碍或放宽CharacterController碰撞。导航角点不等同玩家可直跨的路面。

### Integration checkpoint
- prep-regression001-night：在新Quick保存地图复用原retrieval-late夹具，真实右键牵制→持E打断/重试→WASD返棚/E放置→鼠标错误/修正且对象反序→B阅读/Esc→挂钟/D9二次交换通过。跳过新增记录也可走完整结局。
- prep-regression001-first-search：新地图D1三站/重复观察、D2原洞内T01/返营80.58817m、D3原生Agent进洞通过。
- prep-regression001-old-search：新地图D3原T02/D4原T03、返营71.1354m、4次原近身及日程回归通过。
- prep-regression001-signal-search：新地图D5原广播迁移/T04、D6原T05鼠标错误/正确补全、真实可见/木墙遮挡两铃与返营89.31782m、7次原近身通过。
- 已查看正常/调参交接全文、日出全文、原光束锁定和D8准备提示截图；文字完整，未改旧UI布局或字体素材。

### Final automated checkpoint
- prep-regression001-early：D1从营地实际步行到A/鼠标342/另E取得，睡到D7后原工具保留、日出阅读后提示直接练习光束；接D8两次阅读/原T05/返营，76.46506m/4次原近身通过。
- prep-final001-nine-normal、nine-skip与smoke-input通过：主九天正常/全睡眠及旧D8真实键鼠回归；新记录不成为隐性门槛。
- prep-final001-prep-build与nine-build：Windows64 Development总计174301446/174301442字节，输出Builds/WindowsPreparation/DemonPreparation.exe和Builds/WindowsNineDay/DemonNineDay.exe及各自完整依赖目录。两份_Data/Managed/Demon.Slice.dll SHA256相同，均包含HasFinalPreparation/PreparationRecord/HandoverRead；见build-content-audit.json。未运行真人独立exe。
- prep-final001-validate保存重载通过。构建自动补入InputSystem预加载引用；规范化换行并排除仅该字段后与本轮前文件完全相同，已恢复本轮前原字节，原UnityConnect差异保留。
- postbuild审计：1903旧主ID零丢失，现1949；仅字体绑定/探索/Nav/根列表四个预期旧对象变化，旧10个Quick/6份Nav/Prefab/meta均零变化，缺meta零，全部ProjectSettings与本轮前本地基线一致。
- 清理6个本轮Unity文本文件的行尾空白，校验非空白内容不变，git diff --check通过。随后最后一次refresh回执成功但Editor在360秒内未消费重载请求；窗口控制同受sandbox setup refresh错误影响，已请求用户点击Unity执行Assets > Refresh。仅取消了本轮超时的单次请求，最终pending=false，未触碰Editor进程或其他请求。该步骤不代表此前编译/运行/构建失败，当前待完成清理后重载确认，记录见final-reload-pending.json。
- 最终final-audit通过，设置与原本本地基线完全一致。build-scene-audit额外确认两份构建的level0均包含Preparation_Authored及太阳/交接记录根，不只是程序集更新。

### User refresh checkpoint
- 用户2026-10-30确认“已刷新”。prep-user-refresh001在实际Editor重新打开保存的DemonPreparation后通过，frame=0，引用、原342、两记录、唯一追逐目标与D1–D8路径均正常。
- final-user-refresh-audit通过：1903旧对象零丢失、旧meta/10 Quick/6 Nav/Prefab无变化、全部ProjectSettings与原本本地基线相同，pending=false；git diff --check通过。
- 最后刷新阻塞已解决，final-reload-pending.json仅保留历史排查记录。当前Editor为DemonPreparation、非Play，等待用户试玩；不把用户完成刷新当作内容手感验收。

## Human editability and delivery
打开Assets/Demon/Scenes/DemonPreparation.unity，Play后点击Game捕获鼠标，从D7 08:00开始。轮盘北侧日出记录E/阅读/Esc；按轮盘方位解A（当前342），另按E取T06；原湖岸蓝色观察点右键稳定约0.5秒，观察雪痕停/风向标转，松开恢复。睡袋E到D8，棚外交接台E/重复阅读/Esc；可选返林间启用T05，到22:00接原T07流程或直接睡觉。
文案在Data/ExplorationDefinition的D7–D8字段，日期与原记录在NineDayDefinition，起始日/时/步速在PreparationSettings；布局、牌面/支架/机位在Preparation_Authored，光束参数仍在BeamDefinition。想单测夜间可在非Play时将PreparationSettings设StartDay=8、StartHour=21.9（21:54），测试后恢复7/8；没有新增产品跳时按钮。
实现与自动检查已交付，用户2026-10-30回复“测试通过”，确认本阶段，TASK-015关闭；该确认不扩大为完整九天或独立exe验收。正式音频/美术和完整体验仍开放。

## User acceptance
2026-10-30用户明确确认本阶段“测试通过”，随后询问完整流程剩余步骤及实际场景资产介入时机。本轮仅记录验收和提供后续建议，不视为授权新玩法、资产替换或Git发布。
