# Session Handoff
Updated: 2026-11-07 Asia/Shanghai
Completed task: [TASK-022 天数快捷菜单](Tasks/TASK-022-day-jump-menu.md)。整合场景历史见 [TASK-020](Tasks/TASK-020-snow-gameplay-integration.md)、[TASK-021](Tasks/TASK-021-hierarchy-recovery.md)。
Repository/Unity project: D:/Unity/Projects/Demon
功能发布提交: main@487c8942008c886e7494cdb433abb311a9c50ff4。已推送 origin/main，服务器返回同一 SHA。后续发布记录的文档提交见 Git 最新历史。

## 当前状态
- 雪地试玩入口 Assets/Demon/Scenes/DemonNineDay_Art.unity。层级按00–06分组，点位调整见 ARCHITECTURE 第20节。Terrain、导航、原九天场景及构建入口未因本轮菜单修改。
- 菜单位于 00_系统与界面/SliceHUD/DayJumpMenu_天数快捷菜单，F8开关、1–9跳天、0到D8夜间、Esc关闭。鼠标按钮同样可选。D9直接进入零点终局。
- 用户确认按完整路线补齐之前各天，目标当天留待体验。跳转回到开局位置；D8晨间有T05监听和T06，T07仍待当晚拾取，D8夜间入口可直接开始拾取。可回退、重复切换、结局后F8或R重来。
- 最终使用 GameSceneController.Restart 重建玩法会话，各模块配对解绑/重置自身状态，CompletedDayReplay沿现有规则/交互补齐。保留环境、字体、UI对象，不反复卸载地图。原整图异步重载曾触发GPUResidentDrawer/ObjectDispatcher原生崩溃，已放弃，不能恢复为最终实现。调查及两份日志见TASK-022。
- 菜单位置/样式在 MenuPanel_菜单面板/Card_菜单布局调整。toggleKey和availableInPlayer在DayJumpMenu Inspector调整；改热键同步改提示文案。详细结构见ARCHITECTURE第21节。

## 验证
- 最新原地重置版两轮逐天切换通过。最终含所有日期、D8夜、回退/重复D9、R回D1、旧时钟事件隔离、监听文字重绑、拾取次数清理、暂停/输入隔离及拾取恢复。没有再次崩溃。
- 81项EditMode规则通过，0失败、0跳过。原九天正常分支与全睡眠分支通过。菜单RectTransform临时修改、保存重开及恢复通过，保存引用重开检查和截图目视检查通过。
- 原2870份场景记录保留，新增147份UI相关记录；原记录仅HUD子节点及字体绑定改变。源码/文档与本轮新增UI空白检查通过，全局diff check仍有开工前灯光m_Name尾随空格，原记录保持不变，已记录证据。
- Logs/DemonCheckpoints/task022-day-jump-20261107 保存开工/完工场景、最终JSON/XML、菜单截图、对象对比和崩溃调查。当前已回到Art场景并退出Play Mode。
- 人工连续九天键鼠游玩及独立Windows构建本轮未进行，仍沿TASK-020安排。

## 工作树与保护边界
- 本次发布37份文件，包括此前雪地场景/导航/分组/挡雪修复、独立SnowIntegration数据、SnowBlocker层、菜单及流程重置代码与文档。恢复了本轮临时构建列表追加及TimeManager自动序列化迁移。
- ProjectSettings/ProjectSettings.asset、UnityConnectSettings.asset 的本机差异，以及 Assets/_Recovery、Snow-main 仍留本地，未纳入提交。新增脚本和资源的meta一并提交，未重新生成既有GUID。
- Git LFS fsck通过。暂存导航数据与磁盘逐字节相同，新增场景引用均在跟踪资产或已锁定Unity包中解析。本机两份设置的SHA256在提交前后相同。

## 下一步
1. 用户可在Art场景Play后F8选天试玩。若继续调整布局，沿保存Hierarchy父组移动；改障碍需检查玩家通行与重新烘焙导航。
2. 后续整体验收从D1连续键鼠到D9，再制作Windows包。当前功能已按用户授权发布，继续开发以本次提交为基线。
