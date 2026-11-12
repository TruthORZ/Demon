# TASK-024 格子背包、左键手电与移除秒针
Status: done
Updated: 2026-11-11 Asia/Shanghai

## 目标和确认范围

用户明确要求：删除挂钟秒针（重复两次为同一项），手电改为左键点击切换，背包改为多格、每件纸条/道具占一格并可点击重读；说明时间流速的编辑位置。不新增容量、丢弃、组合或退出存档。

## 影响和计划

- 保留TASK-023全部未提交实现及已有本机设置。开工副本：`Logs/DemonCheckpoints/task024-before-inventory-controls-20261111`；现场dirty场景先由现有Editor请求工具留副本并保存，不能重新加载覆盖。
- `FinaleSequence`允许无秒针引用，主场景用Editor API删除该针；时针/分针及午夜导演保留。
- `BeamTether`点击锁存开关、近景/菜单隔离，快照保存开关状态；同步主线提示。
- `CompletePlaythrough`保留收录/暂停规则，将展示交给独立格子背包组件；保存GridLayoutGroup、格子模板/初始空格与右侧滚动正文，溢出扩展不构成容量限制。
- 时间设置仍由主场景显式引用的GameSettings.RealSecondsPerDay控制，不为说明调整现有值。

## 验收

- 保存重开后挂钟只有时针/分针，结局仍运行。
- 左键一次开、松开保持、再点关；右键不切换；背包/谜题点击不操作手电；普通手电不授予T06能力；重试还原开关。
- 空背包有多格；纸条/道具去重占格，点击分别显示对应正文；长文可滚动；Tab/Esc/关闭恢复暂停；重试同步收录；格子/布局在场景可编辑。
- Unity编译、针对输入/UI的分帧检查、89项规则和完整结局回归，Windows候选重建；记录实际完成及未运行检查。

## 进度

已读取当前交接、代码、场景引用、Unity6000.6.0f1和包版本。Editor当前已打开Art且dirty；正在读取现场状态。

- 现场经 `task024-live-save` 备份并保存，未重载覆盖。三份新组件/工具脚本由Unity生成meta，旧GUID保留。
- 格子背包已保存：24个初始格、模板、6列GridLayoutGroup、滚动格区和独立阅读区。主线独立MainRouteBeam.asset同步新提示；旧切片输入不改。
- 秒针对象及主场景引用已删除；FinaleSequence允许空秒针引用。主线左键锁存开关、关闭UI帧隔离、当天快照开关恢复已实现。
- 首轮虚拟鼠标点击用例未选中纸条，射线却命中正确格子。隔离测试UI Actions的硬件设备并先移动再按键后，真实InputSystem UI按下/松开选中纸条通过；生产输入配置未为测试修改。测试结束恢复设备过滤。
- `task024-input-grid-02`通过：实际格子点击/对应正文、背包隔离、左键开/松开保持/再点关、右键不改变、重试开关恢复，加原主线集成。界面截图已目视检查。
- `task024-rules`：89通过、0失败、0跳过。正在增加空背包/30物品溢出和关闭按钮隔离、格子布局保存重开/运行采用/还原的检查。
- 日长仍720，主线资产MainRouteSettings.asset的RealSecondsPerDay可调，中文Tooltip补充单位/快慢关系；验证工具不再强制固定720。

- `task024-grid-editability`通过：实际纸条格点击/正文、UI关闭点击不误开手电、24空格与30件溢出/去重、左右键与松开保持、开关快照恢复、两针时钟及主线集成；格子间距临时8→6后保存重开、运行采用，再还原8，纸条/任务布局/D3速度也恢复。恢复说明已复制。
- `task024-ending-route`通过：改为可选秒针后的主线D1–D9、日失败重试和完整结局通过。截图已查看格子背包与仅时/分针的挂钟。
- 成功报告、规则XML、截图在 `Logs/DemonCheckpoints/task024-inventory-controls-20261111/`。正在构建 `task024-windows-build`，旧候选将更新为本轮修改版本；尚未确认本次构建成功。

- `task024-windows-build`已成功：Windows64，2970828058字节，入口Art；仍使用 `Builds/WindowsCompletePlaythrough/Demon.exe`，同目录完整保留。开始启动检查。

- 新包已启动实看雪地入口、中文任务、左键提示和正常时钟。无托管异常，原导航初始化/DOF提示仍存在；本轮自启测试程序已关闭，原Unity Editor保留。
- 迁移工具再次执行时只验证，不覆盖后续Inspector修改。7份材质自动迁移、构建时自动加入的Input Actions预加载项和TimeManager自动格式迁移已审计备份后撤回；保留开工前设置和用户现场。
- 本任务完成。Windows原生实际键鼠连续九天、其余分辨率/音效等父任务验收仍见TASK-023；本次格子/开关行为由实际Unity InputSystem与uGUI事件验证，未冒称新增完整人工通关。

## 编辑入口与交付

- 场景 `Assets/Demon/Scenes/DemonNineDay_Art.unity`；UI `00_系统与界面/SliceHUD/CompletePlaythrough_完整流程界面/Journal_背包记录`。
- 格子在GridViewport/GridContent，GridLayoutGroup调整列数、格子大小、间距；Slot_01等保存按钮/标签/边框，SlotTemplate用于超出初始格数。
- `Assets/Demon/Data/CompletePlaythrough/MainRouteDefinition.asset`编辑纸条/道具正文；MainRouteBeam.asset编辑主线T06光束配置和提示。
- 时间流速：同目录MainRouteSettings.asset→Real Seconds Per Day，当前720。该值是现实秒/游戏24小时；360加快到2倍，1440减慢到一半。08:00到午夜仅占全天的2/3，720配置下约现实8分钟。建议退出Play后修改再开始试玩，构建包使用构建时保存值，需重新构建才带入。
- 最新Windows包 `Builds/WindowsCompletePlaythrough/Demon.exe`，完整文件夹一起保留。未提交/推送。

- 最终 `task024-saved-author-guard`验证通过；`final-integrity.json`确认新meta齐全、项目设置与本任务基线全部相同、供应商无差异、git diff --check通过。Unity空字段行尾空格仅作等价清理；没有更改序列化值。
