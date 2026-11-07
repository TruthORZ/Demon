# TASK-022 天数快捷菜单与完整前置流程重放
Status: done
Updated: 2026-11-07 Asia/Shanghai

## 目标与验收
在雪地整合场景添加保存的 uGUI 调试菜单。F8 开关，按钮及数字键跳到 D1–D9，并提供 D8 夜间拾取入口。用户确认按完整路线补齐之前各天，目标当天内容留给玩家。跳转重置玩法会话并回到开局位置，支持往回跳和结局后重来。D9 是零点终局。

菜单打开暂停游戏并释放鼠标，关闭恢复原交互和暂停前时间倍率。不得穿透操作石板/移动玩家。布局、标签及热键可在场景/Inspector 调整。

## 影响与方案
沿用 GameSession/GameClock 的逐个零点推进、NineDayFlow 发现规则、ExplorationFlow 观察记录、ListeningTool/CipherBox/FinalTabletRetrieval 和 FinaleSequence。单独的调试重放服务调用已有交互/规则，不直接篡改私有状态，也不复制一套日期后果。GameSceneController 复用初始化路径重建模型，各组件配对解绑旧事件并清理自身瞬时状态，正常启动不自动补齐。

受影响为调试菜单/重放组件、玩家菜单输入隔离、光束/追逐暂停保护、流程组件重初始化和整合场景。Editor 工具显式创建并拒绝覆盖手工菜单。不改变原九天场景、构建列表、地形、导航或共享参数资产。曾为重载追加构建场景，放弃全场景重载后已从本轮备份恢复原构建列表。

## 基线
main@d23cfa5381d477d1537808033a7107dabbf307f0。现有整合场景、导航、文档、项目设置及未跟踪数据/恢复目录均为既有工作，保留。磁盘场景与构建设置备份在 Logs/DemonCheckpoints/task022-day-jump-20261107。

## 验证计划
Unity 导入编译与保存重开引用检查。实际 Play Mode 从菜单反复切换 D1–D9，对照各天发现、日期属性、工具、首夜、T07/SEAL/结局，验证回退/重复跳转和菜单输入隔离。运行现有 EditMode 规则，检查新旧差异与用户布局保留。验证修改菜单代表性 RectTransform 后保存重开并恢复。

## 调查与进展
- 菜单已用 Unity API 保存到整合场景。初次原生校验逐项检查所有原 Transform 的世界姿态一致。磁盘按序列化对象比对，原2870份对象记录均保留，仅 HUD 增子节点和字体绑定增标签两份原记录有差异。
- UI 模块默认生成的 InputActionReference 是场景内对象。改为原 InputSystem_Actions 的导入子资源，使用 SerializedObject 精确接线，保存重开检查引用。导入器为同一个 Action 暴露了旧名称和带映射名称两个别名，按 Action ID 选择保存引用，不要求别名唯一。无需新包或输入资产。
- 首次编译发现此版本禁止 GetInstanceID，验收器改用对象引用比较。源码随后在 Editor 编译通过。
- F8、暂停时间/移动/确认输入、恢复原时间倍率、Esc 关闭菜单而保留阅读均已通过。实际 Windows 鼠标点击 D8 成功，随后回退 D2 的前置状态通过。
- 原合成鼠标未可靠驱动 UI 模块，自动验收改为按钮 UnityEvent + 实际键盘，物理鼠标单独验证。重载后等待新菜单 Start 完成再检查状态。
- 连续重载时 Editor 原生崩溃，堆栈顶为 ParallelFilterChangedInstancesAndCreateScriptingArray 的对象扫描工作任务，报告保留在开工备份目录的 crash-editor.log。验收器原先在异步卸载期间反复 FindObjectsByType；现改为 sceneLoaded 回调取得明确场景根节点并缓存引用，等待初始化及短暂稳定后继续，正在重启 Editor 复测。堆栈指向原生对象扫描，扫描与崩溃的因果尚需复测确认，未将其认作已修复的引擎缺陷。
- 去除验收器的全局扫描后仍在第一次回退时原生崩溃，第二份堆栈明确到 GPUResidentDrawer.PostPostLateUpdate → WorldProcessor.Update → ObjectDispatcher.GetTypeChangesAndClear，而非验收器 FindObjects。保留 crash-editor-2.log。不改渲染设置、不修改包缓存，改为保留美术环境，只通过原初始化路径重建玩法模型及本模块拥有的瞬时状态。场景重载路线放弃，不能将前一假设写为已修复原因。
- 重初始化覆盖玩家开局姿态/镜头、旧时钟事件解绑、旧符文盘事件与线材质、密码箱盖/工具视觉、风铃计数/摆动、追逐起点/冷却、T07拾取尝试/放置、结局阶段/音频、探索记忆。地形、导航、UI、字体与环境对象不重建。

## 完成与验证
- 用户曾按物理 Esc 停止 Computer Use，随后明确继续。恢复后 Editor 已正常打开，使用现有实例完成以下检查。
- 最新原地重置版两轮连续切换均通过，无原生崩溃。最终 day-jump-final-smoke 覆盖 D8→D2→D7→D6→D5→D4→D3→D1→D8夜→D9→D9→D1→D8夜。D9 后的 D1 使用真实 InputSystem R 键路径，其他项使用 F8/数字键与按钮 UnityEvent。实际 Windows 鼠标点击已单独验证过保存的同一 UI；合成鼠标并未作为最终点击证据。
- 每个目的地对照发现数量、D1三站、监听启用、T06、太阳记录/光束练习/交接记忆、运动门槛、SOUND/LIGHT 日期后果、封印、T07/固化/拾取次数及结局。额外主动推进已废弃时钟，确认旧日期事件未影响新会话，检查监听符文盘文字重绑。重复 D9 恰为两次交换，R 后回到干净 D1。
- 暂停期间移动/确认/世界时间不推进；Esc 仅关闭菜单并保留原阅读。恢复先前时间倍率。T07 拾取中开菜单不取消或推进，关闭后可完成拾取和安放。
- IPlayerSession 增 Restart，GameSceneController 复用初始化；结局 R 使用同一安全路径，整合场景无须加入构建列表。菜单快捷键/面板文字及按钮保存引用有效，最终重开场景检查通过，布局截图人工目视检查通过。
- day-jump-final-editability：临时平移 Card 的 RectTransform，保存重开确认生效，再恢复并重开通过。场景最终保留原2870份序列化对象，新增147份菜单相关记录；仅原HUD子节点和字体绑定两份记录改变。原场景、构建设置不变。TimeManager 在本轮 Editor 重启自动迁移的无关序列化差异已恢复。
- day-jump-final-rules：81项 EditMode 通过，0失败、0跳过。day-jump-legacy-normal：原九天正常分支含 D8时段、拾取安放/拼板、长阅读、挂钟、两次交换、箱盖/湖光/延迟终局通过。day-jump-legacy-skip：原九天连续睡眠通过，0发现、0交换、D9零点终止。最终回到已保存 Art 场景，退出 Play Mode。
- 证据复制到 Logs/DemonCheckpoints/task022-day-jump-20261107，含最终 JSON/XML、菜单截图与序列化对象对比。源码/文档及本轮新增 UI 的空白检查通过。全局 git diff --check 剩一条开工前已有的灯光 YAML m_Name 尾随空格；baseline-whitespace.txt 和原对象1663982443逐字节对比确认未改，保留原有内容。没有提交或推送。

## 验收边界
逐天与原九天回归属于真实 Editor Play Mode 的组件/输入验证，不是人工连续键鼠九天游玩。独立 Windows 包本轮未构建或测试，仍沿 TASK-020 人工整体验收安排。
