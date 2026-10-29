# TASK-014: D5湖岸T04与D6林间风铃搜寻
Status: done
Updated: 2026-10-30 Asia/Shanghai

## Goal and accepted scope
用户明确批准：D5检查静电电台，循湖中原广播到湖岸找原T04；D6沿林径找原T05、补全启用已有高低音风铃，先看雪痕进入、再绕遮挡听铃辨位。沿用现有追逐，无强制任务/伤害/新波次，仍可直接睡觉。不做D7–D8新探索、正式音频/美术、存档或Git发布。

## Acceptance criteria
- 原T04/T05身份、GUID、发现日期D5/D6、T05不可变固化快照及T07规则保持；D5声音不依赖阅读，D7失光不提前。
- 主场景和新DemonSignalSearch保存湖岸/林间布局、原石板/两铃/遮挡/观察与路线节点，旧九个快速场景和五份导航不改；原密码342与T01–03/T07可达。
- 真实WASD/E/Esc走到T04、T05，错误补全不生效，鼠标正确补全不跳日/不改封印；原追逐目标实际进入已启用区域，视觉与遮挡报警可验证。
- 日期边界、晚到不要求前块发现、跳过搜寻睡至D9、保存调参恢复、已有规则与关键九天回归。保存重载/人工可调路径、Windows构建检查。

## Impact and plan
基线main@90ad3c2c04261e00ea0599bfda09df3a3123d753，137个已有工作树条目全部保护。Logs/DemonCheckpoints/task014-baseline-20261029-2207包含本轮前Assets/Demon、Docs、ProjectSettings与EditorBuildSettings；其后仅先记录TASK-013推进授权。
扩展原ExplorationFlow/Definition可选D5–D6引用与提示，不再建任务系统。NineDayFlow仍拥有发现/日期，ListeningTool/Chime仍拥有补全/区域报警，Chase仍唯一移动目标，WorldAttributeConsequences仍拥有SOUND/LIGHT。原板/铃/墙只通过定向Editor工具重定位并保存；新导航单独烘焙，不覆盖旧练习。新Quick从D5 08:00，无预授工具/发现。

## Progress and verification
- 已读取现有实现：T05原身份子级为T05_Editable，两只铃同一原目标；旧ListeningTrailProxy在主场景已禁用。已有D5湖声源不移动，仍用现有可替换无词旋律/静电。
- 本轮前git diff --check通过，Unity6000.6.0f1/URP17.6.0/Input1.20/uGUI2.6/Navigation2.0.14由本地版本与锁文件核实。

## Next action
实现/自动验证/Windows构建已完成，具体结果见下方最终检查点。下一步在DemonSignalSearch从D5开始真人试玩；独立exe真人试玩不与自动构建混淆。

### Authoring checkpoint
- signal-create001首次原生导航检查失败：候选T05位于x14/z-14.5，原东边界与板体分隔了北侧站位。定向signal-finish仅接受这个已知初次布局，把原板改到(12,1.2,-12)、Y90朝西，路线终点(9.8,.04,-12)。CopySerialized更新本轮原生导航并保留其GUID，保存重载全部路径通过；旧边界/旧导航不改。
- 初次Quick复制/重开场景后临时settings对象引用失效。CreateQuick改为从AssetDatabase按确定路径重新读取；signal-settings仅补空引用，拒绝覆盖既有配置；Validate已补设置非空检查。修复重载通过，源Quick为D5 08:00/步速3.3。
- layout001-tests：78 passed/0 failed/0 skipped；check001-boundary通过自然D4→D5声音/原T04，D6原T05开放，无发现前置/提前LIGHT/自动工具。
- 实际T04两次E阅读/返回与T05鼠标错误及正确摆放已运行至风铃阶段。自动PNG中T04文字完整、T05布局清楚；尚不等同真人音频定位/独立版验收。
### Real-input verification checkpoint
- 可见检测最初误把玩家自身CharacterController当遮挡；fixture改为排除玩家/原目标自身碰撞。原游戏碰撞/追逐不改。
- 原低音候选(5.4,.15,-14.6)没有覆盖真实绕墙路径；移到(4.4,.15,-15)虽报警，但触发时射线已经绕过木墙。记录实际进入坐标与遮挡射线后，最终定位(3.4,.15,-14.3)，保留半径1.55/速度/原音源和目标；Native GUID保持，保存重载验证。
- chime002-late通过：直接D6零前块发现，实际T05鼠标错误/正确补全，原追逐目标可见/木墙遮挡的进入报警，返营49.07915m。该早期报告文字固定前缀提及电台/T04，实际Late分支跳过这两项；不把该前缀作为这轮D5证据，完整D5证据见smoke。
- chime002-smoke通过：电台静电原广播迁移、原T04两次阅读、原T05错误/正确鼠标补全、真实可见及ListeningOccluder遮挡报警、实体未提前/不改SEAL/不预约T07，完整往返86.74223m，原近身8次。原追逐目标未被fixture移动，Chase始终开启。
- chime002-skip通过：D5→D9零发现/零检查/未启用工具可正常睡至降临，SOUND/LIGHT各一次，终局清理。
- chime002-tuning通过：保存步速2.8和Day5Guide前缀实际采用，同路线86.73788m/7次近身，退出恢复原D5 08:00/3.3/提示。
- chime002-first-search通过：当前新地图复用旧D1–D2真实WASD/E/Esc，三站观察去重、洞内原T01两次阅读/返营77.98295m、无提前运动/封印、D3 Agent进洞完整路径。
- PNG已检查：T04阅读全文、T05完整补全布局、可见原移动雪痕、木墙遮挡的游戏画面。音源isPlaying/原生3D配置与进入计数通过，不替代真人耳机音色/定位体验。
## Final automated delivery checkpoint
- chime002-old-search：当前新地图D3停转风向标/原林间T02、D4原洞外T03，真实输入/重复去重/返营71.14587m、4次原近身通过；新地图D1–D4回归均非只在旧Quick运行。
- final001-tests：78 passed/0 failed/0 skipped；nine-normal两交换/钟/箱盖/湖光/延迟结局、nine-skip零发现全睡眠、smoke-input真实W/E/六次鼠标符文/Enter/箱内记录/Esc均通过。
- final001-signal-build：Windows64 Development总计174286453字节，Builds/WindowsSignalSearch/DemonSignalSearch.exe。nine-build总计174286449字节，Builds/WindowsNineDay/DemonNineDay.exe。exe本身是Unity通用启动程序，旧时间戳不代表_Data未更新；两份_Data/Managed/Demon.Slice.dll SHA256相同且包含新增get_HasSignalSearch，打包内容已核实。交付须保留匹配的整个_Data/UnityPlayer/Mono等文件夹，未真人独立exe试玩。
- 构建仅自动加入InputSystem preloadedAssets；确认排除该单字段后其余ProjectSettings与基线完全相同，恢复精确基线内容，保留原有本地差异。BuildSettings未改。final002-signal-validate在清理/恢复后重新打开保存Quick，配置、342及T01–05/T07岸边完整路径通过。
- final-audit.json：1842个旧主对象ID零丢失，当前1903；旧meta/GUID无变化，9个旧Quick/5份旧导航无变化，缺meta零，设置/BuildSettings基线一致，无pending。全量比较基线Demon，原文件仅预期7处变化：两份Definition资产、主场景、RequestRunner/InvestigationSmokeRun、ExplorationFlow/Definition；无其他旧文件变化。新Native Nav精确binary，git diff --check通过。
- 以上报告/XML/PNG/审计/辅助脚本在Logs/DemonChecks/Task014；Logs被Git忽略。HEAD仍90ad3c2，无commit/push。

## Human editability and remaining acceptance
Unity当前打开Assets/Demon/Scenes/DemonSignalSearch.unity，非Play，无pending。点击▶从D5 08:00开始：E检查电台、Esc放下，沿湖岸找T04；回睡袋到D6，从营地南侧通路入林，向东沿“旧监听站”牌到原T05。E近看，鼠标把RING放中心、CHIME放下中，Enter启用。先在木墙北侧看移动雪痕经过，再绕到南侧听高低音；可自由返营/直接睡觉。没有剧情传送、强制教学完成或新敌人波次。
文案改Data/ExplorationDefinition，日期/原文改NineDayDefinition；布局在SignalSearch_Authored，低高铃/各音源/半径仍在原ListeningChime和AudioSource Inspector，广播替换仍由WorldAttributeConsequences统一配置。Route/Visible/Hidden为编辑与验证参考，不会移动玩家。改碰撞/地面后重Bake本轮SignalSearchNavigation；旧快速场景独立保存，不自动同步。
自动及保存可编辑性检查完成，待用户实际D5–D6走行/高低音定位和节奏体验；正式音频/美术、完整九天真人体验与D7–D8新探索仍未做。当前下一步是用户试玩反馈，不把自动播放/音源isPlaying等同真人音频验收。
## User acceptance and Git authorization
2026-10-30用户回复“没问题 push到git吧”，确认当前阶段并明确授权提交/推送本批已验收改动。当前实现验收关闭；不扩大为未执行的独立exe真人试玩或完整九天体验。后续D7–D8新探索仍需对齐。发布范围为TASK-009至014、T07对象双顺序及必要资产/文档；排除原有两处ProjectSettings本地差异、Builds/Temp/Logs与缓存。