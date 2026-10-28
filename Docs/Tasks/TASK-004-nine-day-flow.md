# TASK-004 九天流程骨架

Status: done
Updated: 2026-10-25 Asia/Shanghai

## 目标与授权

用户验收当前开箱修复并要求下一步；明确选择先做九天流程，前三块能力效果暂缓。只实现日期、发现/阅读、睡眠、D8重构到D9终局，不编造工具条件或惊吓。设计权威见GAME_SPEC。

## 影响与计划

- 复用SliceSession/GameClock、玩家近景、保存的HUD与箱子。现有配置增加初始湖封闭，默认维持旧样例；最早编译日由九天日程的FinalTabletDay唯一提供。通用阅读内容由控制器提供给现有阅读面板。
- 新增NineDayDefinition配置、NineDayFlow及六个只读石板交互。发现日与日期节点分开；日期节点仅作流程标记，不宣称实际能力已经执行。
- 新建DemonNineDay场景及独立设置资产，原DemonSlice与其配置保留。Editor用原生复制/保存创建，生成器拒绝覆盖。美术可改保存的模型/机位/位置，策划可改日程/文本/醒来时间。
- D1夜间封印开启用可调时刻的流程占位，无邪教徒演出；睡觉跳过夜晚也保留开封结果。允许连续睡到D9；T07在D8开放，不要求读完前六块。
- 无新增包/程序集、无战斗/追逐/自由语言解释器、无提交或push。Git HEAD仍7638ece；既有改动保留。改动前备份Scripts/Docs/旧场景/BuildSettings到忽略的Temp/Task004Baseline，不称Git提交。

## 验收

- D1启动，按D2至D7开放T01至T06阅读，登记发现且重复查看不重复计数；T07在D8开放，早期编译拒绝。
- 自然/睡眠日期一致，日程醒来时间可调，D9午夜停止；跳过探索不阻塞。
- D8重构、实际箱子交互和原D8测试场景均回归；普通石板阅读不设置已读箱子标记。
- 配置、引用、标签、近景保存重载；规则EditMode、Unity编译、两条九天Play Mode、Windows代表构建与截图检查分别记录。

## 当前进度

现有Unity中完成保存场景和配置。新增NineDayAuthoring与NineDaySmokeRun，沿用固定白名单请求，没有Unity MCP连接，不启动第二实例。

- nine-create-001第一次检查失败：新配置创建后经过场景复制/导入，创建时的settings对象句柄已失效，保存后字段为0。已改为在场景打开后重新LoadAssetAtPath获取持久资产；nine-repair-001只修复缺失连接、保存重载，验证通过。未删除或重建内容。
- nine-tests-001：真实Unity EditMode 19 passed / 0 failed / 0 skipped，包含原14项和发现去重/可用性、初始封湖、提前编译拒绝、完整跳日边界。
- nine-skip-001：从D1连续睡到D9，0发现、0交换，终局停在D9零点。
- nine-normal-001：D1自然到夜间开封、D2至D7逐一发现且重复查看不重复计数、D8固化/真实箱子组件阅读、D9两次交换终局均通过。
- nine-old-input-001：原场景W/E、六次符文物理点选、Enter、E开箱与Esc阅读退出通过。原DemonSlice场景SHA与Temp/Task004Baseline一致。
- nine-tuning-001：将配置中D2醒来时间保存为10:00，实际Play采用，石板阅读镜头到达保存的ReadView；退出日志DEMON_NINE_DAY_WAKE_RESTORED，资产九个WakeHour均恢复8。tuned-record截图已查看，正文完整且日程提示隐藏。
- 查看D1起点、D2/D7普通记录、箱内记录、结局截图。新增资源meta完整，git diff --check通过。日长默认24分钟、醒来暂均8点、首夜开封暂22点，均为可调整的流程占位。
- nine-build-001：Windows64构建成功，173903219字节，输出Builds/WindowsNineDay/DemonNineDay.exe。旧Builds/Windows的D8版本保留。未运行独立exe的人工试玩；未做完整九天的真人键鼠路线，不把组件自动化当真人体验。

## 下一步

nine-validate-001重载最终场景，检查九天组合与箱子引用；结果在Temp/DemonSliceReports。已交付本阶段骨架，下一步由用户试玩九天节奏与可读性。前三块效果、工具交互、正式文本、美术、暴风雪演出与追逐继续暂缓，不将流程骨架写成完整游戏。
