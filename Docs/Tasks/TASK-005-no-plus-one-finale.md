# TASK-005: 无+1的九天流程与午夜挂钟结局
Status: done
Updated: 2026-10-28 Asia/Shanghai

## 目标与范围
用户确认新版流程并授权同步文档及下一步构建。设计权威为[GAME_SPEC](../GAME_SPEC.md)。本轮先迁移已有九天灰盒和D8快速验证场景：删除+1玩法、T07夜间开放、保留解谜/阅读时间、挂钟至午夜后二次交换。T05风铃、箱子A密码、T06光束及雪地追逐为后续可玩切片，本轮只同步其设计与记录内容，不宣称完成。

## 验收
- IF一次性、WHILE跨日每天最多一次；固化不会推进时间，不在同帧反复交换。
- T07在D8夜间开放；交互前允许自然错过或睡到D9，进入重构后为拼图与阅读保留午夜前时间。
- 任意长阅读、立即退出、错误拼图重试都不会误触午夜；关闭箱内记录后展示挂钟、第二次交换与降临。
- 原D8场景键鼠与实际Box.Use回归；九天跳日路线保留；新配置、机位、时钟、灯光和音频为保存资源，可编辑、重载。
- Unity编译、规则测试、两条Play Mode、代表Windows构建及截图检查分别记录，不以自动化代替完整真人试玩。

## 影响与计划
- 保留现有Domain→Runtime→Editor程序集方向。SliceSession既有每日去重可复用；新增小型纯C#时间窗口规则验证不回退和午夜保留。
- SliceSceneController移除固化后加速；独立SliceFinaleSequence负责演出及时间窗口，显式组合引用。Player增加不可Esc中断的演出视角；HUD将规则终局和结局面板显示分开。
- NineDayFlow新增夜间可用性；日程与记录更新至新身份/能力顺序。T05目前仍只读占位，未实现补全。
- Editor以明确目标原地迁移两套Settings、两个Scene的T07槽位/返回线路，保留Prefab/GUID及其他Transform；只新增本任务拥有的Finale_Authored内容。迁移已存在时拒绝重跑，以后直接编辑资产。
- 无包/管线/输入切换，无自动Git提交/push。基线main@7638ece33d2c2bd2e1d936de09c9cc62acd64955，既有未提交功能/URP差异保留。已备份Assets/Demon、Docs及BuildSettings至忽略的Temp/Task005Baseline。

## 进度与验证
- 已核对6000.6.0f1、URP17.6.0、InputSystem1.20.0、uGUI2.6.0及现有实现。没有Unity MCP；使用既有白名单一次性Editor请求，不启动竞争Editor。
- GAME_SPEC已同步用户确认的九天表、工具/密码/循环规则及剩余待定项。脚本已移除+1与固化加速，新增独立演出与纯规则时间窗口，尚待Editor资产迁移及运行验证。
- finale-refresh-001实际编译通过。finale-migrate-001在修改前预检查失败：误将旧八段连线末尾编号当作7/8，实际Prefab为0至7。已核对Prefab及两场景哈希未变，修正为将6改为直接返回、停用7，重新编译后迁移。
- finale-migrate-002保存并重载两个场景及两套Settings成功。finale-tests-001：26 passed / 0 failed / 0 skipped。
- finale-normal-001：真实Play Mode逐日发现、D8早间禁止编译/22:00开放、错误尝试/保留窗口、8秒阅读不自动越日、挂钟机位、D9两次交换、箱盖闭合/湖光恢复、延后结局通过。截图额外发现开箱盖遮住挂钟下半部；新增限范围Editor修正将挂钟/机位移到右侧柱前，不移动既有箱子或棚子。需要修正后复验画面。
- finale-sightline-001保存修正。finale-tuning-001实际保存倒计时3.5秒/ClockView横移0.08米，完整Play验证采用保存参数与机位，通过；退出日志DEMON_FINALE_TUNING_RESTORED，已核对两场景倒计时均恢复6秒，钟/机位x=-2.5。查看修正后的挂钟、箱内记录与终局截图，表盘不再被箱盖遮挡。
- finale-late-001从D8午夜前0.01游戏分钟首次交互，保留时间、长阅读和完整结局通过，没有倒退或抢先跨日。与Temp基线比较：原文件无丢失，既有meta全部哈希相同。
- finale-sleep-reserved-001：进入T07操作后主动睡眠可释放窗口、不修复也正常到D9；finale-skip-001：全程睡眠0发现/0交换，止于D9零点。finale-input-001：旧D8场景实际虚拟W/E、六次符文鼠标射线、Enter、E开箱、Esc继续通过。虚拟输入和焦点设置退出恢复。
- finale-build-001：九天Windows64构建成功，173927554字节；finale-quick-build-001：D8快速Windows64成功，173914482字节。Data目录level0/globalgamemanagers均为2026-10-28本轮输出，原生exe复制时可保留引擎二进制旧时间戳，不以exe时间戳单独判断是否新构建。完整目录分别为Builds/WindowsNineDay、Builds/Windows。
- finale-validate-001：构建后重载九天场景，无+1布局/文字、演出引用检查通过。git diff --check通过（GAME_SPEC只有Git换行转换提示），所有Assets/Demon源文件均有meta。构建产生ProjectSettings.asset的InputSystem预加载Actions引用差异，保留有效包行为，不恢复其他既有URP设置。

## 下一步
本轮范围完成，交付用户试玩。下一制作切片为T05风铃监听、箱子A/轮盘与T06光束的最小可玩验证，再接入T07雪地争夺。T05/T06目前仍是只读占位，不将设计记录写成已实现功能。

## 用户与美术测试入口
- 快速结局：Assets/Demon/Scenes/DemonSlice.unity，从D8 22:00开始；进入Play点击Game，WASD/鼠标靠近T07按E。依次点右侧EXCHANGE→中心、BOX→下中、LAKE→下右，Enter固化。去左侧箱子B按E读记录，Esc继续看挂钟与午夜结局，R重开。
- 九天入口：Assets/Demon/Scenes/DemonNineDay.unity。左侧睡袋推进次日，T07在D8 22:00开放，早间不会出现；暂时不必等待整段时间测试结局，使用快速场景。
- 策划调日程：NineDayDefinition的FinalTabletHour/Days/WakeHour；日长在两套Settings的RealSecondsPerDay。
- 调演出：SliceComposition的SliceFinaleSequence可改保留分钟、读完停留、挂钟倒计时/午夜停留/闭箱展示时长、音效/灯强。Finale_Authored内的WallClock/ClockView/ClockLamp/LakeOpenGlow等为保存场景内容。Box/ReadView、Environment/EndingView、SliceHUD仍按现有方式可编辑。
- 音频、挂钟几何与实体均为灰盒占位。尚未完成真人完整九天或独立exe键鼠试玩，自动Play/输入检查不能代替节奏与恐怖感验收。
