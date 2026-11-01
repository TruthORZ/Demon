# TASK-002 首个可玩样例与三周实施路线

Status: awaiting verification
Updated: 2026-10-25 Asia/Shanghai

## 目标与授权边界

用户已确认Windows并明确要求开始，用简单图形建立石板、箱子、湖岸雪地；授权实施本任务的首个灰盒可玩样例，不扩展到完整九天或正式美术，不自动提交/push。

本轮基线：HEAD为7638ece33d2c2bd2e1d936de09c9cc62acd64955，跟踪main/origin/main；已有未跟踪Assets/Demon.meta及各预留目录meta，是Editor导入产生的前置内容，保留原GUID。无已跟踪人工作业差异。Unity 6000.6.0f1的本项目Editor已打开，不启动竞争实例。

当前没有Unity MCP。采用项目内显式菜单工具与一次性请求文件，让现有Editor创建资产；请求限定操作、目标目录与执行次数，消费后删除，拒绝覆盖已有场景。不是Play Mode自动建图或无条件初始化重生成。若Editor不导入，保留菜单供用户手动执行并如实标记验证缺口。

样例暂定E交互、鼠标选取/放置、Enter确认固化、Esc退出；初始布局、中文揭示文本和简易实体为可编辑占位，不把这些选择升级为正式谜题设计。选择已装uGUI与Input System，近景采用场景机位锚点，不添加依赖。先实现可测试的纯C#规则，再通过保存的场景接入。

首个样例只做一个能走动的简单空间、一张石板、一个箱子、一个湖封印状态和最小结局。用D8晚间到D9零点验证核心因果，不先制作九套场景、通用语言或大量惊吓。完整设计见 [GAME_SPEC.md](../GAME_SPEC.md)；当前工程证据见 [ARCHITECTURE.md](../ARCHITECTURE.md)。

## 首个样例验收条件

- 第一人称键鼠可移动观察，靠近石板交互后镜头拉近，退出后恢复输入和视角。
- 石板、槽位、箱子、湖状态对象和灯光已保存为Scene/Prefab，离开Play Mode仍存在，编辑器可调。
- 可以移动预置符文并补全；错误组合可重试且不触发世界变化；只有目标固化。
- D8当天较晚固化立即执行一次，箱子SEAL 1->0、湖0->1；抠符文不改变固化结果。
- +1经过统一时钟到D9零点，执行下一轮且不被每日调度重复；T04占位状态赋予实体后，湖开放触发终局。
- 未修复T07时睡至D9，也能以原本开放的湖触发终局；不伪造隐藏OPEN。
- 日长、起始时间、醒来时间、交互距离、近景机位、加速和环境效果有可编辑配置。
- 终局阻断D10推进和重复交换；重开清理事件订阅与旧运行状态。
- 规则自动检查、Unity编译、实际Play Mode与代表性人工调参验证分别记录结果。

## 拟影响范围与公共契约

计划资产位于 `Assets/Demon`，样例场景暂名 `Assets/Demon/Scenes/DemonSlice.unity`。不改写或移动SampleScene及URP模板资产；若要将新场景加入Build Settings，只在样例可打开后做明确的小范围修改。

计划复用现有Input System Actions；其Interact是Hold，需先确定单击还是按住，不能直接假定。动作映射与UI输入切换属于兼容边界。新配置通过序列化字段/静态数据资产提供，运行时状态分离。

拟模块与依赖：

| 模块边界 | 输入 输出与职责 | 约束 |
| --- | --- | --- |
| Clock与日期调度 | 当前时间、请求推进、跨零点通知、当日去重 | 所有推进共享路径；只有一个执行所有者接管最终循环 |
| WorldState与操作 | 属性值、容量；原子TRANSFER/EXCHANGE与提交结果 | 不依赖UI、灯光或玩家对象，便于规则测试 |
| Tablet | 固定模板、目标槽位、固化快照与执行进度 | 表面符文不是固化后的运行时真相来源 |
| Interaction与输入 | 聚焦对象、进入/退出近景、选择放置符文 | 探索和交互有明确输入所有权，避免全局查找 |
| Presentation | 订阅状态，驱动箱子、湖、灯光、音频与实体 | 不反向用动画或材质决定规则状态 |
| Finale | 阅读过渡、时间加速、终局接管 | 通过Clock推进，不再维护一套独立日期 |
| Scene composition | 在保存场景的一个显式位置绑定上述引用 | 不引入万能GameManager、服务定位器或全局事件总线 |

这些是拟实施边界，不要求每行建一个接口、Manager或程序集。先用少量职责明确的组件，只有真实耦合需要时才抽象。数据源、实际类名、接口和asmdef在实现时根据最小需求确定。

## 引擎能力选择建议

| 需求 | 优先评估现有能力 | 建议与理由 |
| --- | --- | --- |
| 第一人称移动 | CharacterController及现有Input System | 需求简单，先评估小型控制器；不默认引入完整Starter Assets依赖 |
| 靠近石板与镜头 | 场景中Camera/Transform锚点与小型过渡脚本 | 首个样例无需新摄像机包，拉近机位可由美术调整 |
| 文本与提示 | 已安装uGUI，或引擎UI Toolkit | 建议首版uGUI Prefab，便于场景/世界空间提示；尚未实施或切换框架 |
| 场景与开箱动画 | 保存的场景、Prefab、Animation/Animator | 静态内容不在Awake/Start里临时搭建 |
| 结局演出 | 已安装Timeline或少量状态过渡 | Timeline负责作者化轨道，游戏规则仍由Clock/WorldState决定 |
| 声音混音 | 已有音频组件与AudioMixer | 资产在Editor可编辑，不先造音频框架 |
| 雪地追逐 | 配置路线/触发区、脚印Prefab与音源 | 第二阶段先做受控样例；已装Navigation不代表必须做复杂AI |

当前不增加依赖。若后续引入包，先说明版本兼容、费用许可、平台影响和替代方案。查API时用本项目6000.6与已装包版本，不能复制旧教程代码后凭文本检查称编译成功。

## 第一阶段施工顺序

1. **工程基线和样例资产**：确认Editor、Console、实际平台和版本控制选择；通过Editor建立可安全重跑的样例创建入口，只生成明确归属的新资产，不自动覆盖美术改动。
2. **时钟与属性规则**：先验证日长、自然/睡觉/+1的跨日一致性；交换守恒、来源/容量错误不半次提交；每日资格与循环接管去重。
3. **最小交互**：第一人称、聚焦交互、拉近机位与退出；建立保存的石板和槽位，锁定输入所有权。
4. **目标与固化**：目标槽位快照、错误重试、固化反馈；表面符文移除与内部程序分离。
5. **箱子到终局**：第一次交换、阅读占位、加速到D9、实体与湖状态共同触发、无修复跳日支路。
6. **人工编辑路径**：在Inspector改一个日长、一个镜头锚点、一个表现参数，验证运行尊重它们，再还原检查值并重载场景。
7. **更新实况**：把实际类、资产、引用与通过的步骤写入ARCHITECTURE；任务保留具体验证和未完成项，交接指向下一步。

## 三周建议路线

相对周安排，不擅自推定提交截止日。第一阶段验收通过后再增加内容。

| 阶段 | 程序交付 | 策划与技术美术配合 | 退出条件 |
| --- | --- | --- | --- |
| 第一周 | 首个D8到D9样例；交互、时钟、目标固化、交换与结局 | 确认最终槽位与文本结构；一块石板、箱子和湖岸样例 | 正常与跳过两条路线可跑通，资产可手工改 |
| 第二周 | 九天事件、七块石板内容、监测诱导、受控雪地追逐 | 锁定工具条件与复位；制作主要区域、脚印、声音、关键文本 | 从D1到D9完整可玩，无主线阻塞 |
| 第三周 | 节奏调参、输入/时间边界修复、展示构建与重置 | 未参与设计者试玩，优化提示和氛围，替换重点占位 | 目标设备可运行，展示流程可重复 |

保存继续、正式菜单、复杂设置和追加惊吓按确认优先级排入，不挤占核心链路。先追求有始有终的简单样例，再追求九天内容量与最终画面。

## 验证方案

- EditMode：交换两次恢复、容量失败原子性、日期方向、D8补触发、睡眠跨多日、重复零点回调去重、固化快照不受槽位移除影响、终局不会继续D10。
- PlayMode/人工：靠近与退出石板、预置符文移动、错误重试、开箱阅读与提前/延迟退出、加速后输入所有权、两条降临路径、反复重开清理。
- 资产检查：Editor保存重载、GUID引用、场景/Prefab值、人工调参被尊重、离开Play Mode仍可编辑。
- 构建：目标设备确认后做一次代表性独立运行验证；不以Editor成功代替构建。

不要为每个简单getter写测试；优先覆盖会破坏剧情因果或产生重复交换的规则。没有运行的检查必须写not run，不能把计划当结果。

## 风险与待确认

首个样例直接相关：目标槽位预置、参数对调是否等价、目标通过自动固化还是确认键、最终阅读过渡，以及正式展示平台。可先在计划中使用清楚标记的占位文本和实体状态，但不把占位方式写回GAME_SPEC当正式设定。

后续相关：实体来源、工具条件与复位、投影能力边界、无伤追逐结束方式、具体惊吓与完整文本。详见GAME_SPEC待定项，不在多个文档复制完整问题清单。

用户已明确选择GitHub并授权初次push，版本库建立与基线发布见[TASK-003](TASK-003-github-initial-push.md)。进入玩法实施前复查本地与远端状态，保留人工作业；授权初次push不等于授权后续每次自动提交或开始游戏实施。

## 进度与下一步

2026-10-25实施检查点：

- 已由现有Editor创建保存DemonSlice场景、三个预制体、配置、材质和uGUI源对象；没有Play Mode运行时建图。
- Unity原生编译已生成Domain、Runtime、Editor、Tests四个项目程序集；EditMode规则14项通过。
- 首次正常路线真实Play Mode通过，帧从1推进到11522，D9零点、两次交换和终局停止正确；没有把仅进入Play当作通过。
- 画面检查发现OS字体经fontsettings保存后没有显示中文/符文。补充SliceFontBinding，在Windows运行时解析Inspector中定义的系统字体名称并绑定已保存的文本对象；字体修复待Editor再次刷新和复验。未复制Windows字体二进制到项目。
- 重要失败：使用6000.6旧objectReferenceInstanceIDValue导致预编译错误，已改EntityId接口；全源码本机预编译通过，实际Unity随后编译通过。背景Editor未导入字体修复前，一次fonts请求被旧程序集拒绝，日志中的Unsupported bounded authoring operation是该次工具失败，不是规则测试失败。为后续增加显式refresh操作，仍然只消费请求，不自动轮询重建资源。
- 官方CLI存在，但项目没有com.unity.pipeline，status/recompile不能连接；未擅自安装Pipeline或升级CLI，没有启动第二个Editor。首次用户切回Editor后创建成功；字体修复需要再次导入，已向用户请求Assets/Refresh。

后续实测检查点：

- `Temp/DemonSliceReports/tests-001.json`及XML：真实Unity EditMode 14 passed / 0 failed / 0 skipped；Domain无后续修改。
- 字体修复已刷新生效，截图确认中文、符文和终局文字显示。`normal-002.json`：正常路线再次通过，帧1到10927，D9零点、两次交换、实体与湖状态、终局停时正确。record截图是在退出记录后的画面，尚不能证明阅读页完整布局。
- `sleep-001.json`：不修复时睡到D9通过，交换0次，原本开放的湖触发终局。
- 输入失败调查：后台输入受焦点路由干扰，测试改用临时虚拟键鼠并在结束恢复InputSettings，不改项目输入配置。进一步发现近景镜头射线被玩家自身CharacterController挡住，已修复SlicePlayer交互查询，忽略自身子树并选择最近目标。
- `input-004.json`：真实Input System动作和物理射线链通过：W移动、E近景、保存的机位、六次点选补全三个符文、Enter只触发一次交换。这是自动输入验证，Esc/R真人操作及手感仍需试玩。
- 新源码/asmdef行尾检查、git diff --check通过，37个新增源资产都有meta。没有Packages改动、没有提交或push。
- Windows构建build-001在现有Editor调用，当前停在预处理后，尚无完成报告和exe。Performance Testing包创建Assets/Resources的两个PerformanceTestRun*.json；PC_RPAsset预筛选字段与URPProjectSettings被Unity更新。先保留运行中的状态，结束后检查临时输出，不还原人工作业。
- 可恢复的smoke-editability已补充源码：临时保存日长、近景机位和日光强度修改，在Play检查，退出后恢复原值。尚未导入/运行，需等构建结束刷新，不将源码当验证结果。
- computer-use窗口检查因本机sandbox helper setup refresh错误未能启动；已请用户查看Unity是否有弹窗或构建进度。未绕过安全设置或终止Editor。

最终检查点（以上等待状态已解除）：

- 构建等待原因是Unity保存对话框，用户点击Save后继续。`build-001.json`：Windows64成功，173882355字节，入口`Builds/Windows/DemonSlice.exe`。首次URP着色器编译耗时较长，未切换管线或弱化检查。独立exe人工运行未做，不把构建成功当独立运行验收。
- Performance Testing构建后自动清理了Assets/Resources及两个JSON，Input System临时preloadedAssets也恢复。Unity保存持久化了DefaultVolumeProfile、PC_RPAsset、UniversalRenderPipelineGlobalSettings、GraphicsSettings/URPProjectSettings的版本字段与缓存，以及UnityConnectSettings.m_Enabled；保留用户保存结果，不擅自改回。
- 最新Editor工具已由Unity实际编译。`tuning-001.json`通过：保存日长720秒、近景机位偏移和日光.37后，Play Mode确实采用新值；退出日志DEMON_SLICE_TUNING_RESTORED，资产日长恢复1440、日光1.15、近景x恢复.45。随后正常路线再次加载这些已恢复资产。
- `normal-003.json`再次通过：帧1到11653，D9零点两次交换；tablet/record/ending截图已检查，近景与阅读页文字完整且无裁切。该次还验证了近景退出返回、记录关闭与终局机位过渡。
- `validate-002.json`用于最后保存场景重载检查，结果见报告。后续Editor工具变更不影响已生成Windows玩家代码。

当前剩余验收：请用户亲自试玩整体手感、Esc退出/R重开和Windows独立运行；尚未将这些标为passed。首个灰盒实现已可交付试玩，任务暂为awaiting verification；下一阶段是根据反馈调整提示、机位和节奏，再扩展九天内容。不自动提交/push。

## 开箱空引用修复（2026-10-25）

- 用户实际按E开箱报NullReferenceException，日志定位SliceBox.Use第10行。先前normal测试直接调用SliceSceneController.ReadBox，绕过了箱子组件，旧检查结果不能证明开箱入口正常。
- 原因：SaveAsPrefabAssetAndConnect之后调用box.Configure，未记录Prefab实例属性覆盖，world连接在保存重载后丢失。修正生成器记录PrefabInstancePropertyModifications，并通过repair-box显式修复现有实例world字段；不重建场景或修改预制体/其他几何。
- 场景检查增加Box/Bed的world必须绑定当前Composition；过去只检查非零但失效的引用，会漏掉本应必填的空字段。
- box-repair-001通过保存、重新打开和引用检查。box-input-002通过W/E、六次符文点击、Enter、低头对准箱子后E阅读与Esc退出。一次输入测试失败是测试未低头，射线从矮箱上方通过；修正模拟鼠标观察后通过，没有更改玩家交互距离或箱子几何迁就测试。
- normal检查已改为调用SliceBox.Use。box-normal-001完整路线通过，帧1到12109，D9零点两次交换并停止；box-build-001修复版Windows64构建成功（173882355字节），已覆盖原Builds/Windows输出。Unity实际编译及git diff --check通过。规则Domain没有改动，不重复14项纯规则测试。保留现有人工作业，未提交或push。
