# FootPrint — Key AI Influence Evidence

Author folder: **PXY** · Repository: **TruthORZ/Demon** · Evidence prepared: **2026-11-13 (Asia/Shanghai)**

> Identify which requests or outputs influenced requirements, design choices and code changes.

本文件只摘录可在 [FullChat](FootPrint_FullChat.md) 定位的原话，再说明实际影响。**Human Evaluation** 只记录原聊天能够证明的用户判断、纠错和授权；AI 自动编译、截图审查和测试单列为验证证据，不写成用户手工完成，不声称用户亲自编写了 AI 生成的代码。

FullChat 保留同一聊天的全部可恢复文本及可见工具记录；本文件筛选影响 Demon 脚印需求和实现的关键部分。早期 Snow / GlobalSnow Demo 是研究背景，未实施的五文件 Terrain Shader 方案不是 Demon 成果。图像附件仅保留引用，未将二进制图片加入这两个 Markdown。

## 1. 用户实测纠正了“空格即可盖鞋印”的建议

### Original Prompt / Response

[FullChat 058](FootPrint_FullChat.md#entry-058)（L6835 起，2026-11-11T20:53:43.064000+08:00）

> 最简单就是用 Demo 自带的**空格盖脚印**：

[FullChat 059](FootPrint_FullChat.md#entry-059)（L6854 起，2026-11-11T20:58:10.566000+08:00）

> 按空格没有出现脚印， 小球运动有雪痕, 空格是控制小球跳跃的, 如果希望这个痕迹恰好是脚的形状的怎么办

[FullChat 062](FootPrint_FullChat.md#entry-062)（L7071 起，2026-11-11T20:58:47.406000+08:00）

> **小球留下的是 `Terrain Marks`（碰撞压痕），鞋底形状要用另一套 `Footprints` 系统。** 它不会根据碰撞物体的外形，自动压出鞋底。

[FullChat 062](FootPrint_FullChat.md#entry-062)（L7071 起，2026-11-11T20:58:47.406000+08:00）

> 刚才我把空格说得太确定了：Demo 里空格同时触发小球跳跃和脚印射线。如果画面中心对着小球，射线可能先打中小球，而不是雪地。

### Influence

明确目标是可辨识的鞋底形状，不能把球形碰撞压痕当作合格脚印。后续方案把触发位置/方向与鞋底遮罩、法线表现分开。这里的空格失败发生在 GlobalSnow Demo，**不是 Demon 的运行故障**；AI 对射线的解释是当时的诊断，不冒充用户已证实的根因。

### Human Evaluation

用户实际试按空格，报告只看到小球跳跃及雪痕，并追问如何获得脚的形状。这是明确的人工反馈，促使 AI 承认此前措辞过于确定并修正建议。没有证据表明用户随后按新步骤完整验收了该 Demo。

### Evidence

后续 Demon 使用独立鞋底遮罩/法线：[Assets/SnowSystem/FootPrint/Generated/SoleLeft.png](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/Generated/SoleLeft.png)、[Assets/SnowSystem/FootPrint/Generated/SoleLeftNormal.png](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/Generated/SoleLeftNormal.png)；生成逻辑在 [Assets/SnowSystem/FootPrint/Editor/FootprintSetup.cs](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/Editor/FootprintSetup.cs)。这些代码/资产在 d25a332901b556ae996844958e4afc64d1b964a9 中出现；不能反推原 Demo 是 Demon 成果。

## 2. 用户限定为 Unity 原生 Terrain，并明确不能改 GlobalSnow

### Original Prompt / Response

[FullChat 072](FootPrint_FullChat.md#entry-072)（L7656 起，2026-11-12T14:16:12.930000+08:00）

> 指的是unity terrain

[FullChat 080](FootPrint_FullChat.md#entry-080)（L7925 起，2026-11-12T14:31:48.573000+08:00）

> 那先帮我拆成这5个文件，放到单独的文件夹里"D:\MyProject\Demon\Assets\SnowSystem\FootPrint"，不要改原来的GlobalSnow，再告诉我怎么挂到Unity Terrain上测试。

[FullChat 084](FootPrint_FullChat.md#entry-084)（L8850 起，2026-11-12T14:32:36.231000+08:00）

> Demon 当前是 Unity **6000.6.0f1 / URP 17.6**，我会按它的接口制作，避免套用 Snow 工程的 URP14 代码。

### Influence

目标从含糊的 terrain 对象名称收敛为 Unity 原生 Terrain；写入范围明确为 Demon 的 FootPrint 目录。实现采用指定 TerrainCollider 的探地过滤，保留 Terrain 材质/数据及 GlobalSnow；接口核对使用 Demon 的 Unity 6 / URP17，而不是学习工程的 Unity2022 / URP14。

### Human Evaluation

用户主动纠正 AI 对目标地面的判断，并通过明确路径和“不要改原来的 GlobalSnow”限制写入范围。这证明需求确认和保护边界来自用户，不证明所有实现细节由用户编写。

### Evidence

[Assets/SnowSystem/FootPrint/TerrainFootprintEmitter.cs](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/TerrainFootprintEmitter.cs) 中的目标 Terrain、TerrainCollider 和雪层过滤；[Assets/SnowSystem/FootPrint/SOURCE.txt](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/SOURCE.txt) 明确独立实现且未复制 GlobalSnow 代码/美术。[Assets/SnowSystem/FootPrint/README.md](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/README.md) 记录 294 个受保护原文件的哈希检查，这是 AI 执行的检查，非人工测试声明。

## 3. 用户质疑复杂度，取消五文件 Terrain Shader 方案

### Original Prompt / Response

[FullChat 079](FootPrint_FullChat.md#entry-079)（L7869 起，2026-11-12T14:18:37.870000+08:00）

> **建议先拆成5个核心代码文件**，做成独立的 `TerrainFootprints` 模块：

[FullChat 086](FootPrint_FullChat.md#entry-086)（L9042 起，2026-11-12T14:52:24.812000+08:00）

> 目前只完成了环境核对：Demon 使用 **Unity 6000.6 / URP 17.6**。
> 
> **尚未创建5个文件，也未修改 GlobalSnow 或工程设置。**

[FullChat 087](FootPrint_FullChat.md#entry-087)（L9052 起，2026-11-12T15:14:30.802000+08:00）

> 这是最简单的方案了吗

[FullChat 088](FootPrint_FullChat.md#entry-088)（L9061 起，2026-11-12T15:15:02.159000+08:00）

> **不是。前面的5文件方案偏重了，尤其是改写 Terrain Shader，会增加地形层、远景和版本兼容的维护成本。** 对“只想在现有雪地上留下脚印”来说，可以更简单。

[FullChat 089](FootPrint_FullChat.md#entry-089)（L9098 起，2026-11-12T15:16:13.171000+08:00）

> 那就先做最简单的贴花版，放到 D:\MyProject\Demon\Assets\SnowSystem\FootPrint，不改原来的 GlobalSnow，做好后告诉我怎么挂到 Unity Terrain 上测试。

### Influence

这是实际技术路线的转折。放弃自建脚印 RenderTexture 和 Terrain Shader，改为 URP Decal Projector + Decal Shader Graph + 有限对象池，使用颜色与法线模拟凹陷。五文件方案只完成环境核对，未实施；不把它列为已交付代码。接受的范围不包含修改地形高度/碰撞，也不保证原插件的深度视差效果。

### Human Evaluation

用户提出“这是最简单的方案了吗”，要求重新比较成本；在 AI 说明替代方案后，明确选择“最简单的贴花版”。这是有原文支撑的设计评估和接受。用户没有要求完成最初五文件计划后再额外做第二套系统。

### Evidence

实际提交包含 [Assets/SnowSystem/FootPrint/TerrainFootprintEmitter.cs](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/TerrainFootprintEmitter.cs)、[Assets/SnowSystem/FootPrint/FootprintDecal.shadergraph](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/FootprintDecal.shadergraph) 和 [Assets/SnowSystem/FootPrint/Generated/FootprintProjector.prefab](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/Generated/FootprintProjector.prefab)。原生成代码见 [FullChat 098](FootPrint_FullChat.md#entry-098)（L9947 起，2026-11-12T15:19:00.884000+08:00）、[FullChat 099](FootPrint_FullChat.md#entry-099)（L10193 起，2026-11-12T15:20:07.039000+08:00）。[Assets/SnowSystem/FootPrint/SOURCE.txt](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/SOURCE.txt) 与 [Assets/SnowSystem/FootPrint/Unity-URP-License.txt](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/Unity-URP-License.txt) 保留 Unity URP Graph 来源；它不是 PXY 手写的完整 Shader，也不是直接拆出的 GlobalSnow 源码。

## 4. 提供独立可测入口，并修正批处理渲染验证错误

### Original Prompt / Response

[FullChat 089](FootPrint_FullChat.md#entry-089)（L9098 起，2026-11-12T15:16:13.171000+08:00）

> 那就先做最简单的贴花版，放到 D:\MyProject\Demon\Assets\SnowSystem\FootPrint，不改原来的 GlobalSnow，做好后告诉我怎么挂到 Unity Terrain 上测试。

[FullChat 100](FootPrint_FullChat.md#entry-100)（L10447 起，2026-11-12T15:20:33.465000+08:00）

> 脚本和资源生成工具已准备好：运行时最多保留48个脚印，并按时间淡出；Inspector 上有“盖一对测试脚印”按钮，不需要先接玩家。接下来会让 Unity 实际导入、编译并检查投射方向、材质和对象池。

[FullChat 120](FootPrint_FullChat.md#entry-120)（L12144 起，2026-11-12T15:26:12.942000+08:00）

> 文件和测试场景已经生成，C# 编译通过了。不过第一次运行发现贴花材质没有被 Unity 正确识别，我正在排查这个问题，并会用实际渲染截图确认脚印能显示。

[FullChat 135](FootPrint_FullChat.md#entry-135)（L13593 起，2026-11-12T15:27:36.511000+08:00）

> 实际渲染已经通过，原生 Unity Terrain 上能看到左右鞋印。走动留印、原地不重复、数量上限、淡出和清除也都通过了检查。刚才的问题出在批处理测试尚未初始化渲染管线，贴花材质本身正常；接下来整理挂载步骤并核对原文件未被修改。

### Influence

实现不依赖先挂接玩家：提供 Inspector 的测试双脚印按钮、显式菜单、独立测试 Terrain/Renderer。对象池上限与淡出让功能可控。首次材质识别检查失败后，验证过程增加真实渲染初始化和 On/Off 截图，避免只凭 C# 编译成功就宣布画面有效。

### Human Evaluation

用户要求“告诉我怎么挂到 Unity Terrain 上测试”，因此可操作的测试入口是验收需求。记录中修复渲染初始化、检查截图及对象池行为由 AI 完成；未找到用户逐项手测或逐行审阅这些代码的原始记录，不能填成“我亲自全部验证通过”。

### Evidence

[Assets/SnowSystem/FootPrint/Editor/FootprintSetup.cs](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/Editor/FootprintSetup.cs)、[Assets/SnowSystem/FootPrint/Editor/FootprintVerification.cs](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/Editor/FootprintVerification.cs)；原验证修正见 [FullChat 122](FootPrint_FullChat.md#entry-122)（L12333 起，2026-11-12T15:26:23.443000+08:00）、[FullChat 128](FootPrint_FullChat.md#entry-128)（L13050 起，2026-11-12T15:27:04.247000+08:00）。

本机历史结果 `Logs/Footprint-Verification.txt`：

```text
PASS: shader/material import; manual pair; terrain/layer rejection; bounded pool; clear; movement; stationary; teleport; expiry; enable lifecycle; rendered on/off captures.
```

这是 2026-11-12 的既有 AI 自动验证结果，本次证据整理仅重新读取，不声称重新运行 Unity。原命令/输出保留于 FullChat；Logs 截图在本机，未作为本次两个 Markdown 之外的文件上传。

## 5. 用户将通用脚印限定为“恶魔追踪”，明确玩家不留印

### Original Prompt / Response

[FullChat 144](FootPrint_FullChat.md#entry-144)（L13920 起，2026-11-12T15:33:07.192000+08:00）

> 恶魔追踪实现脚印, 玩家不需要, 接入恶魔实际代码

[FullChat 156](FootPrint_FullChat.md#entry-156)（L16533 起，2026-11-12T15:34:31.293000+08:00）

> 找到了：恶魔由 `ChaseController` 驱动 `SnowDisturbance_Target` 的 NavMeshAgent，已有踏雪音触发点。我会在这个落步点同步生成左右脚印，并处理冻结、停步和重新开局；玩家不挂脚印组件。正式雪地场景的引用和 Renderer 也会接好。

[FullChat 192](FootPrint_FullChat.md#entry-192)（L19620 起，2026-11-12T15:44:42.757000+08:00）

> 已接入**恶魔实际追踪代码**，玩家不留脚印，GlobalSnow 未改。

### Influence

需求从通用 Terrain 脚印转为正式游戏反馈。`ChaseController` 在已有踏雪音时机发布 `Stepped(position, forward)`，重置时发布 `PursuitReset`；`DemonFootprints` 订阅事件并调用通用 Emitter，追踪模块不反向依赖贴花实现。关闭 Emitter 的 `Automatic Steps`，由恶魔实际落步事件控制步频；玩家不挂 Emitter。

### Human Evaluation

“玩家不需要”是用户明确的删减与职责选择，覆盖此前 AI 提到的给玩家绑定 Follow Target 的通用示例。该指令证明用户选择了游戏接入对象，但不证明用户亲自实现了事件或完整人工走测。

### Evidence

[Assets/Demon/Scripts/Runtime/ChaseController.cs](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/Demon/Scripts/Runtime/ChaseController.cs)、[Assets/SnowSystem/FootPrint/DemonFootprints.cs](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/DemonFootprints.cs)、[Assets/SnowSystem/FootPrint/Editor/DemonFootprintIntegration.cs](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/Editor/DemonFootprintIntegration.cs)；代码原始新增/修改见 [FullChat 159](FootPrint_FullChat.md#entry-159)（L16688 起，2026-11-12T15:35:34.745000+08:00） 和 [FullChat 162](FootPrint_FullChat.md#entry-162)（L16932 起，2026-11-12T15:37:05.244000+08:00）。正式场景 [Assets/Demon/Scenes/DemonNineDay_Art.unity](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/Demon/Scenes/DemonNineDay_Art.unity)，渲染配置 [Assets/Settings/PC_Renderer.asset](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/Settings/PC_Renderer.asset)。这些是实际提交 d25a332901b556ae996844958e4afc64d1b964a9 的内容，而不是只有方案文字。

## 6. 游戏状态约束进入回归检查，保留测试条件和局限

### Original Prompt / Response

[FullChat 164](FootPrint_FullChat.md#entry-164)（L17315 起，2026-11-12T15:37:40.611000+08:00）

> 代码已接好：脚印与恶魔原有踏雪音同步，追踪和退开时都会留在地面上。正在用正式雪地场景运行验证，重点检查 D1 不留印、D3 开始留印、光束冻结后停印，以及跳天重开时清掉旧脚印。

[FullChat 176](FootPrint_FullChat.md#entry-176)（L18745 起，2026-11-12T15:39:45.936000+08:00）

> 正式场景中的验证已通过：恶魔沿真实导航路径留下左右脚印，旧脚印固定在地面；冻结、停用追踪和暂停时不会继续生成，跳回 D1 会清空旧脚印。渲染截图也已确认可见。现在做最后的差异检查，清理 Unity 保存时产生的无关改动。

[FullChat 185](FootPrint_FullChat.md#entry-185)（L19358 起，2026-11-12T15:43:09.800000+08:00）

> 最后的 81 项现有规则测试也全部通过了。现在正式场景打开即可测试：Play 后按 F8 选第 3 天，走出木屋就能观察恶魔追踪留下的脚印。我正在补齐使用说明和交付记录。

### Influence

验收覆盖 D1 无追踪印、D3 实际 NavMesh 追踪、左右交替、旧脚印固定、冻结停止新增、释放恢复、禁用/暂停不新增，以及重开清理。验证之外还检查尺寸保存重载和无关设置变更，避免效果只在临时 Play 状态存在。

### Human Evaluation

用户在前一项指定实际恶魔接入；具体状态测试矩阵和自动测试由 AI 提出并执行。本聊天没有后续用户对最终画面的明确验收回复，也没有完整实体键鼠九天通关记录。故这里只写“AI 自动验证通过”，不写“我已手工全部验收”。

### Evidence

[Assets/SnowSystem/FootPrint/Editor/DemonFootprintVerification.cs](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Assets/SnowSystem/FootPrint/Editor/DemonFootprintVerification.cs) 与 [Docs/Tasks/TASK-023-demon-footprint-decals.md](https://github.com/TruthORZ/Demon/blob/d25a332901b556ae996844958e4afc64d1b964a9/Docs/Tasks/TASK-023-demon-footprint-decals.md)。既有本机 `Logs/DemonCheckpoints/task023-footprints-20261112/verification.txt` 末尾为 `PASS`，`editmode.xml` 记录当时 81 项规则测试；输出原文在 [FullChat 184](FootPrint_FullChat.md#entry-184)（L19294 起，2026-11-12T15:43:02.523000+08:00） 附近，最终回复为 [FullChat 192](FootPrint_FullChat.md#entry-192)（L19620 起，2026-11-12T15:44:42.757000+08:00）。

冻结测试调用生产 `StableBeamMotion` 规则，并暂时关闭 BeamTether 输入轮询；这是自动状态检查，不是玩家手动瞄准测试。初次脚印接入没有新 Windows 构建或长时间人工试玩。后续主线合并的 89 项测试属于另外的验证轮次，不替换本聊天原话里的 81 项。

## 真实提交及后续证据边界

- 原始脚印实现提交：[d25a332901b556ae996844958e4afc64d1b964a9](https://github.com/TruthORZ/Demon/commit/d25a332901b556ae996844958e4afc64d1b964a9)，`Add demon footprint decals and update art scene`。该提交还含美术修改；本证据仅将脚印相关路径与本聊天关联，不把整个提交的所有工作都归给此聊天。
- 后续 art/main 合并：[8033386123f4c4269afc877cf41f7cbf988a56bc](https://github.com/TruthORZ/Demon/commit/8033386123f4c4269afc877cf41f7cbf988a56bc)，属于 03/05 的另轮验收。可查[合并验证记录](https://github.com/TruthORZ/Demon/blob/8033386123f4c4269afc877cf41f7cbf988a56bc/Docs/Tasks/DEMON-ART-MERGE-20261112.md)。89 项规则、九天回放及前台隔离虚拟输入等条件来自该记录，不伪装成 12 聊天内用户的验证。
- 后续主线曾清理旧占位脚印并增加重试清理验证；不把其他聊天的工作拼进本聊天，也不把动态 `main` 的最新文件当作原始输出。本文件代码链接固定到当时的 `d25a332`，避免后续变化造成证据错配。
- 早期坡度修改、GBuffer/Renderer Feature 学习、GlobalSnow Demo 的效果属于研究背景；FullChat 保留它们，Demon 交付采用独立 Decal 方案。没有把 GlobalSnow 第三方代码归为 PXY 原创。

## 隐私与完整性检查

- 本机用户名路径使用 `[LOCAL_USER]`；Unity 的内网/机器、会话、关联 ID、许可序列标识及有效期使用明确的 `[REDACTED: ...]` 占位符。脱敏说明及计数在 FullChat 中，未用总结替换原始对话。
- 检查私钥、常见 GitHub/API Token、密码/密钥赋值及私人网络/账户标识。公开的代码 GUID、提交 SHA、仓库、第三方署名和公开支持邮箱保留。
- 人工判断的证据来自原话；AI 测试输出与人类判断分开。缺少的人工验证没有补写。
- 本次交付只包含 `Docs/AI-Logs/PXY/FootPrint_FullChat.md` 与 `Docs/AI-Logs/PXY/FootPrint_KeyPart.md`，目标分支 `art`。
