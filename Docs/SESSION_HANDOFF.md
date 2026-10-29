# Session Handoff
Updated: 2026-10-30 Asia/Shanghai
Active task: [TASK-014 D5–D6声音与风铃搜寻](Tasks/TASK-014-signal-search.md)，实现/自动验证/Windows构建完成，用户于2026-10-30确认没问题并授权Git发布。TASK-013用户已授权推进，文字实际确认完整；不把推进授权当成独立exe完整试玩记录。
Repository/Unity project: D:\Unity\Projects\Demon
Branch and HEAD: main@90ad3c2c04261e00ea0599bfda09df3a3123d753；147个工作树条目（本轮前137个），保留TASK-009至013等本地内容，无commit/push。原ProjectSettings空preloadedAssets与UnityConnectSettings差异保留。

## Current state
用户批准D5静电电台→循湖中原广播找湖岸T04；D6沿林径找原T05，补全后看雪痕触铃，再绕木墙听高低音。可直接睡觉，无强制任务/伤害/新追逐波次。
主DemonNineDay与新DemonSignalSearch保存SignalSearch_Authored：原T04(4.5,1.2,4.6)/Y0，原T05(12,1.2,-12)/Y90朝西；原板身份/子板/托盘/日期引用保持。原低铃(3.4,.15,-14.3)、高铃(9.5,.15,-14)、原墙(8,1.35,-15.7)，原监测目标仍是Chase唯一Agent。低铃按实际西侧绕行路径设置，能在木墙遮挡时触发。声音/光/封印/补全规则保持。可选D5/D6提示扩展ExplorationFlow/Definition/HUD至D6，旧Quick边界保持。详细结构/可调位置见[ARCHITECTURE第15节](ARCHITECTURE.md)。
新原生SignalSearchNavigation主/Quick共享，旧9个Quick和5份Nav无改动。新SignalSearchSettings从D5 08:00，步速3.3，无预给工具/发现。T04去掉制作备注但未发明实体函数，签名/来源仍按GAME_SPEC未决。

## Verification
- final001-tests：78 passed/0 failed/0 skipped。
- check001-boundary：自然D4→D5原声音转移/静电/T04，D6原T05开放，无发现前置/提前LIGHT/自动监听。
- chime002-smoke/late/tuning/skip全部通过：真实WASD/E/Esc与鼠标错误/正确补全；完整D5–D6往返86.74223m、8次原近身；D6不读T04也能启用并监听49.07915m；真实Agent分别触发可见和ListeningOccluder遮挡的原生空间音源；零发现可睡到D9；保存2.8/提示前缀采用后恢复D5 08:00/3.3/原提示。
- 新地图first-search/old-search分别复用D1–D2与D3–D4真实输入fixture通过（77.98295m/71.14587m），并非只测旧场景。final001-nine-normal/nine-skip/smoke-input回归通过。
- final001 Windows64 Development：SignalSearch 174286453字节、NineDay 174286449字节，Builds/WindowsSignalSearch和WindowsNineDay；两份_Data中运行程序集一致并包含新接口。未真人独立exe试玩，保留匹配整个目录。
- final002-signal-validate在构建/设置恢复/空白清理后保存重载通过，当前Unity为DemonSignalSearch、非Play，无pending。
- final-audit：旧1842个主ID零丢失，当前1903；旧meta/GUID/9个Quick/5个Native Nav无变化，缺meta零，设置/BuildSettings与本轮基线完全一致。全量旧Demon仅7个预期文件变化；Native Nav精确binary，git diff --check=0。原字体/其他素材无变更。
- PNG已查看T04全文、T05完整布局、可见原雪痕和木墙遮挡；未进行真人耳机定位/节奏/独立版验收。

## Resume here
1. 打开Assets/Demon/Scenes/DemonSignalSearch.unity，▶从D5 08:00开始。E看电台/湖岸T04，Esc放下；睡袋到D6，背湖沿南侧通路入林向东找“旧监听站”与T05。E近看，RING放中心、CHIME放下中，Enter启用；木墙北侧看雪痕，绕南侧听高低音。仍可跳过搜寻直接睡。
2. 提示在ExplorationDefinition，日程/原文NineDayDefinition，场地/原板/两铃/墙/标牌在SignalSearch_Authored。Route/Visible/Hidden只是布局/验证参考，不驱动玩家或NPC。改障碍后Bake本轮SignalSearchNavigation，保留旧练习资产。
3. D5–D6阶段已由用户确认，下一步对齐D7–D8。D7–D8具体新探索尚未对齐，正式音频/美术、全九天真人体验、存档/暂停/设置范围仍有未决，本批Git已获明确授权，正进行发布；不含构建/临时证据和两处原本本地设置。

## Evidence and tools
恢复基线Logs/DemonCheckpoints/task014-baseline-20261029-2207包含本轮前Demon/Docs/ProjectSettings/EditorBuildSettings；先前TASK-013恢复基线仍保留。报告/XML/PNG/审计/RunTask014Checks.ps1和AuditTask014.ps1在Logs/DemonChecks/Task014，均被Git忽略。不要依赖Unity重启会清理的Temp作为唯一证据。
无Unity MCP；固定Temp/DemonSliceAuthoring.request.json，一次一请求，报告Temp/DemonSliceReports。先检查pending/Play，编译回执不等于编译完成，不在Smoke Play时改Assets脚本，不开第二Editor或清Library。当前Shell require_escalated可用，本轮无需用户手动刷新。
signal-create/validate/smoke/boundary/late/skip/tuning/build；first-search/old-search在新Quick做早期回归。create拒绝既有输出；finish/settings/low-chime仅修复已知初次状态，拒绝覆盖其他手动定制。当前所有probe已恢复。旧investigation/exploration等操作仍保留，见各任务；其旧快速场景独立，不是最新主地图。
