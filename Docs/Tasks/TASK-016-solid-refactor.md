# TASK-016: 保持九天流程的 SOLID 架构改进
Status: done
Updated: 2026-11-01 Asia/Shanghai

## Goal and scope
用户授权按只读架构评估改进代码，现有玩法、输入、完整D1–D9与全跳过路线必须保持。不增玩法、依赖、框架，不发布Git，不重建关卡。设计仍由[GAME_SPEC](../GAME_SPEC.md)拥有。

## Acceptance criteria
- 现有场景/Prefab/GUID/UnityEvent/Inspector配置兼容；可编辑内容继续由保存资产拥有。
- 日程中T01运动日期作为九天流程权威来源；独立旧练习保留显式回退。
- 玩家近景输入面向小型契约；场景组合点显式注入所需依赖，缩小通过总控制器访问其他模块的范围。
- 探索引导选择与交互/显隐分离，默认D1–D8文本与优先级保持。
- 合并验证工具的共用输入/行走实现，保持实际输入与原断言。
- Unity编译、核心规则与新增边界测试、主九天正常/跳过/晚到/睡眠、分段真实输入、保存调参恢复和Windows64构建通过；区分自动证据与人工完整体验。

## Impact and plan
基线main@213cfd751e766d5d6df696cb8c484240043432ac，当前D7–D8全部未提交内容和两处原ProjectSettings差异属于本轮前状态。恢复副本：Logs/DemonCheckpoints/task016-baseline-20261101（Assets/Demon、Docs、Packages、ProjectSettings及Git状态）。
保留现有MonoBehaviour类型、序列化字段和资产；优先改变内部协作契约。触及SceneController/Player/Tablet/Cipher/Exploration及必要消费者，Domain新增展示选择规则与测试，Editor抽共用验证支持并增加只读审计。每阶段检查编译/场景与旧调用兼容，最后跑完整回归。
依赖方向维持Editor→Runtime→Domain；Domain无Unity引用。已有公共Configure/Getters为场景/旧验证兼容保留，避免无关迁移。回滚从本轮基线逐文件恢复，不使用reset覆盖用户工作。

## Progress and verification
- 已复核Unity6000.6.0f1、URP17.6.0、Input1.20.0、uGUI2.6.0、Navigation2.0.14与TestFramework1.8.0。
- 当前Editor存在，沿用单请求Temp/DemonSliceAuthoring.request.json；不启动第二Editor，不在Play中改脚本。
- 默认沙箱启动故障，使用已获自动审核的workspace内命令；不改Snow-main或原项目设置。

## Next action
本轮实现与自动验证完成。打开DemonNineDay从D1试玩完整连贯体验；人工完整九天/独立exe验收尚未执行。未授权Git提交或推送。

- 首轮81/0/0规则、12保存场景、nine-normal/skip/late/sleep-reserved与smoke-input通过。后续扩展验证工具时Unity提示SessionState.GetInt必须提供defaultValue；旧程序集因此拒绝arch-runtime（未进入Play/未改探针资产）。补明确缺失值检查，待新编译后继续。

### Implementation checkpoint
- SceneContracts按实际消费者拆出IPlayerSession/IHudState/ISceneFeedback、近景ICloseViewInteraction及石板ITabletProgram；保留原序列化字段与兼容调用。Player/Tablet/HUD/Chase/Beam/Listening/Exploration不再持有具体SceneController作为跨模块入口。
- 九天运动日期读取T01.DateNodeDay；BeamDefinition字段仅独立场景回退。原默认D3保持。
- 探索显示使用原资产内21条有序规则及原文案字段，纯Domain选择器；新增3项测试包含9日期×4096状态组合的原行为对照（36864组）、配置扩展及错误配置边界。
- Editor共用ISmokeRun生命周期和D5–D8 SmokePlayerDriver，保留实际输入/碰撞、原断言与各自超时。
- arch-full002-arch-runtime通过：保存T01=D4、回退=D2后D3不运动/D4原生追逐；保存提示Key覆盖生效；第三种独立近景实现实际Enter/碰撞点击/Esc通过，未改变发现/SEAL/预约。退出恢复参数。
- arch-full002-tests为81/0/0；prep-validate及自然D6→D7、D8边界通过。其余分段输入与最终构建/审计进行中，尚不标记完成。

- arch-full002全部20个操作通过（含完整九天正常/跳过/晚到/睡眠、D1–D8分段实际输入、D1提前T06、T07打断重试/带回/午夜、保存调参、旧监听/追逐生命周期与12场景检查）。已查看T03及交接正文截图，显示完整。
- 最后从RequestRunner提取原D8 SliceSmokeRun，保持原断言/输入与截图ID，通过回调汇报；调度器不再拥有该验证场景实现。提示选择改索引遍历，避免每帧接口枚举器分配。待针对性复验及构建。

### Asset review checkpoint
- 第一次严格审计发现DemonPreparation.unity重存以及ExplorationDefinition除新增GuideRules以外的变化。逐项diff显示均为Unity写回空字段的行尾空格；需在最终验证/构建后以语义相同前提恢复原场景字节并清理规则资产行尾空格，再保存重载检查，不把审计失败略过。
- 最后代码整理后的arch-final001-tests再次81/0/0通过，针对性近景/调参/九天回归与双Windows构建进行中。最终状态尚待回执及基线审计。

### Final verification and delivery
- arch-full002：20个操作全部通过；最后代码提取后arch-final001再次81/0/0规则、arch-runtime、原smoke-input/editability、主nine-normal、12保存场景通过。
- arch-final001-nine-build：Windows64 Development成功，总计174308727字节；prep-build成功174308731字节。Builds/WindowsNineDay/DemonNineDay.exe和Builds/WindowsPreparation/DemonPreparation.exe均保留完整_Data/依赖目录。build-audit.json确认两份Runtime/Domain DLL SHA256各自一致，包含新契约/ExplorationGuidance，level0包含Preparation_Authored；未运行真人独立exe。
- 最后旧smoke-editability保存DemonSlice时，Unity补入原有脚本的默认空引用、6/18日出日落字段及两盏灯的默认URP附加组件；严格比对仅此变化后恢复本轮前原场景字节（RestoreLegacyResave.py）。Preparation场景仅空格重存，也恢复原字节。构建仅补入InputSystem预加载引用，排除该段后其余设置相同，恢复原本ProjectSettings字节。不是删除功能组件或覆盖未知人工变化。
- 最终asset-audit.json通过：所有原场景、Prefab、导航、音画资产及旧meta字节均与本轮基线一致；无删除、缺meta、丢失序列化字段，全部ProjectSettings/Packages不变。唯一修改的非代码资产为ExplorationDefinition新增GuideRules；排除该段后原内容完全相同。git diff --check通过。
- arch-reload001 refresh后，arch-reload002-arch-validate重新加载12场景通过；smoke-input再次真实W/E/6符文点击/Enter/B阅读/Esc通过；nine-validate通过。最后Editor在DemonNineDay、frame=0、无pending请求。恢复字节后已做实际重载，不只依赖刷新回执。
- Initialize的内部组合调用签名随依赖收窄调整，已搜索更新所有现有调用；未发现场景/Prefab的持久UnityEvent绑定这些方法。Configure、已有交互入口/Getters及序列化字段保持。
- Scripts由71份变78份：新增2份运行时文件（小型契约/独立引导策略）与5份Editor文件（共用生命周期/驱动、独立旧切片验证、架构审计及边界验证）；测试目录新增1份有意义的重构对照测试。未为每个类建立接口或新增玩法管理系统。RequestRunner由676行降414行。
- 可编辑路径：原Data/ExplorationDefinition文案与新增GuideRules；九天运动日期在NineDayDefinition.Tablets[0].DateNodeDay；原场景布局/机位和Beam等参数保持原编辑路径。arch-runtime已保存/重载修改Key与日期并恢复，prep-tuning和smoke-editability已验证原参数/机位调节尊重Inspector。
- 自动实现验收已完成，不扩展为用户完整九天连续游玩、听觉/恐怖节奏或独立exe的人工验收；尚未Git提交/推送。恢复基线及日志在Logs/DemonCheckpoints/task016-baseline-20261101与Logs/DemonChecks/Task016。
