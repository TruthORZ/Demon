# Terrain 脚印贴花：独立测试版

适用：本工程 Unity 6000.6.0f1 / URP 17.6。完全不依赖 GlobalSnow。
这是贴花颜色＋法线模拟的凹陷，不改变 Terrain 高度、轮廓或碰撞，没有原插件的深度视差采样。

## 已接入恶魔实际追踪（TASK-023）

正式场景 `Assets/Demon/Scenes/DemonNineDay_Art.unity` 已保存接线，无需再给玩家挂组件。

1. 打开该场景，Play。
2. 按 **F8** 打开天数菜单，选 **第3天**（或按3）。D1/D2恶魔还没有运动能力，因此不会生成追踪脚印。
3. 走出木屋，恶魔的 `SnowDisturbance_Target` 沿真实 NavMesh 追踪时，会在 Terrain 上交替留下大脚印。脚印与原踏雪音同一触发点，玩家移动不会留印。
4. 旧脚印留在地面，默认35秒消退、最多48个。冻结/停步/暂停停止新增；跳天和Restart清除旧轨迹。

Hierarchy 搜索 **Demon Terrain Footprints**，即可调整 `TerrainFootprintEmitter`：
- `Footprint Size`：默认0.4×0.85米；`Lateral Offset`：左右各0.3米。
- `Lifetime` / `Capacity`：寿命和最多数量。
- `Automatic Steps` **保持关闭**，因为 `DemonFootprints` 订阅 `ChaseController.Stepped`，使用恶魔实际落步时机。`Step Distance` 在该接法中不控制步频。
- 步频沿用实际 Chase Definition 的 `Footstep Seconds`（默认0.45秒），与声音一致。

代码关系：`ChaseController.Stepped(position, forward)` → `DemonFootprints` → `TerrainFootprintEmitter.StampAt`；`PursuitReset` → 清空脚印。追踪代码不引用贴花模块；只有适配器引用 `Demon.Slice`。若把通用Emitter导出到没有Demon代码的项目，排除DemonFootprints及DemonFootprintIntegration/Verification三个接入文件。

本轮修改正式Art场景、`ChaseController.cs`和`Assets/Settings/PC_Renderer.asset`（增加Decal Feature）。仅关闭恶魔根下两个随移动漂移的 `SnowPrint_0/1` Renderer，保留原对象、导航、光束目标区域、风铃和D2静态人类脚印。GlobalSnow、玩家控制器、Terrain材质/数据与导航资产未改。

已通过实际场景追踪、左右交替、旧脚印固定、D1/玩家过滤、冻结/恢复、禁用追踪、暂停、Restart清除及渲染检查。可编辑尺寸已保存重开验证并恢复默认。证据在工程 `Logs/DemonCheckpoints/task023-footprints-20261112`。

## 最快挂到自己的 Terrain

1. 在 Project 找到当前相机使用的 **Universal Renderer Data**。注意不是 URP Pipeline Asset：从当前 Quality 的 Render Pipeline Asset 打开 Renderer List 中使用的 Renderer；若相机指定了独立 Renderer，选它。
   本次检查的 PC 配置是 `Assets/Settings/PC_RPAsset.asset` → `PC_Renderer.asset`。若你切换了 Quality 或相机 Renderer，以实际引用为准。
2. 选中该 Renderer，执行 **Tools → Terrain Footprints → Add Decal Feature to Selected Renderer**。弹窗确认后才会修改它。已有 Decal Feature 会保留；确认启用，Technique 选 **DBuffer**，Surface Data 选 **Albedo Normal** 或 **Albedo Normal MAOS**。不需要改成 Deferred。
3. 在 Hierarchy 选中真正带 **Terrain** 组件的地形，执行 **Tools → Terrain Footprints → Add Emitter to Selected Terrain**。它会创建已配置材质/预制体引用的 Emitter，位于地形中心上方0.15米；不会改变 Terrain 材质。
4. 将新建 Emitter 移到镜头附近的雪地上方约0.15米。其 Inspector 的 Target Terrain 已自动绑定。第一次保持 Follow Target 为空，Snow Layer Index = -1（允许整块地形）。
5. Play，选中 Emitter，点击 Inspector 底部 **Stamp Test Pair / 盖一对测试脚印**。贴花在 Emitter 下方出现，而不是在鼠标位置。移动它再点可继续测试。场景中另存你的配置。

如菜单中没有生成的预制体，先运行 **Create Assets and Test Scene**。此操作会提示保存当前场景，创建独立场景；已有生成资产不会被覆盖。

## 通用Emitter参考：跟随物体（正式场景不用于玩家）

- `Follow Target`：拖玩家根节点或脚底附近的定位物体；不推荐直接拖头部相机。
- `Character Controller`：拖实际控制角色的 CharacterController，让跳跃时不生成脚印。Rigidbody等其他控制器可通过脚落地事件调用 `StampAt`，并关闭 Automatic Steps。
- `Automatic Steps`：开启；每移动约 `Step Distance = 0.65` 米交替留印。
- `Max Ground Distance`：脚底节点约0.3～0.5；人物中心/相机要按离地高度增大，过大会让未接CharacterController的腾空物体也留印。
- 脚底偏移由 `Lateral Offset` 控制，默认左右各0.10米。
- 原地不重复盖印，超过 `Teleport Distance` 的位移不补路径。
- 若脚下是角色自己的 Collider，调整 Ground Layers 排除玩家层（并保留地形层）。射线只接受最近命中为指定 TerrainCollider，不会隔着屋顶在下面盖印。

落脚动画可调用：
```csharp
emitter.StampAt(footWorldPosition, walkingForward, true); // true 左脚，false 右脚
```
位置需在地形表面附近/上方，允许射线探地。该方法不添加自动左右偏移，传真实脚位置即可。

## 范围和外观

- 仅一个显式指定的 Unity Terrain；Mesh地面、其他Terrain不会接收新脚印请求。
- `Snow Layer Index` 是 TerrainLayer 的零起始下标；-1允许整个Terrain。指定雪层后用 Minimum Snow Weight 控制最低权重。
- 脚印大小默认0.16×0.32米；Projection Depth约0.16米。不要把投射体积加得太深，贴花体积内邻近物体也可能被投到。
- 贴花生成仅在雪层达标处触发，但鞋印边缘不会按雪层逐像素裁切。
- `Lifetime` 默认35秒，最后25%时间淡出；`Capacity` 默认48，超额复用最旧槽位。Capacity在退出Play后调节，下次启用生效。
- 颜色和遮罩在 `Generated/SoleLeft.png`、`SoleRight.png`；法线在对应Normal图。材质使用 `FootprintDecal.shadergraph` 的 Base Map、Normal Map、Normal Blend。均可单独替换并保存。
- 左右脚分别使用正确镜像生成的法线，不通过负缩放镜像投射器。
- URP DBuffer增加渲染成本；本版验证目标是Windows/DX11，其他平台需单独测试。
- 已验证原生 Terrain 配合 `Universal Render Pipeline/Terrain/Lit`。如果你的 Terrain 使用第三方自定义 Shader，需要它支持接收 URP DBuffer 贴花；仅有 Terrain 组件并不能保证任意自定义 Shader 都能显示。

## 自带独立测试场景

`Generated/FootprintTest.unity`：原生Terrain、6个静态预览鞋印、一个动态Emitter、相机和灯光。

临时测试时，在当前 Quality 的 Render Pipeline Asset 中选择 `Generated/FootprintTestURP.asset`。测试结束恢复原引用。
该资产只引用本目录的 `FootprintTestRenderer.asset`，已经含 Decal Feature；不会覆盖原项目 Renderer。
静态Preview Footprint用于直接观察材质；动态Emitter按钮、走动、淡出和Clear只管理它自己的运行时脚印池。

## 文件与可回退范围

- `TerrainFootprintEmitter.cs`：唯一运行时脚本；不依赖输入系统，不查询全局玩家。
- `FootprintDecal.shadergraph`：Unity URP17.6自带Decal图的独立副本。
- `Editor/FootprintSetup.cs`：显式资源生成、挂载菜单和Inspector测试按钮；不自动改场景。
- `Editor/FootprintVerification.cs`：仅显式批处理测试，普通编辑器会话不执行测试。
- `Generated`：Unity生成的可编辑贴图、材质、预制体与测试场景。

移除Emitter即移除运行时脚印；停止Play/禁用组件会清理其对象池。Terrain材质从未被替换。
独立版本通过菜单显式添加Decal Feature；本次恶魔接入已为PC_Renderer添加。若要回退，请只移除本功能添加的那个Feature，保留其他功能使用的Decal Feature。

源码/纹理来源见 SOURCE.txt；URP Graph版权与许可证见 Unity-URP-License.txt。没有复制或改写GlobalSnow源码及贴图。

## 独立版首次验证记录（2026-11-12，恶魔接入前）

在本机 Unity 6000.6.0f1 / URP 17.6 / DX11 批处理 Play Mode 中实际渲染，已检查有/无贴花截图：

- C#编译和Shader编译通过，贴花材质投射Pass有效。
- 手动左右脚印、地形外/无效雪层拒绝、自动走动、静止和传送过滤通过。
- 48个投射器池上限、清除、寿命结束、禁用后重新启用通过。
- 294个受保护原文件（原GlobalSnow、原ProjectSettings、Assets/Settings）哈希对比无变化。Git仅新增FootPrint目录及其meta。
- 原有主场景未接入玩家和Renderer；按上方步骤挂载测试。实际效果是鞋底颜色和法线贴花，没有真实几何下陷。

截图与原始结果：工程 `Logs/FootprintPreview/Footprints-On.png`、`Footprints-Off.png`、`Logs/Footprint-Verification.txt`。
批处理验证需关闭同工程的Unity实例后运行；入口 `StandaloneTerrainFootprints.Editor.FootprintVerification.RunBatch`，测试期间临时切换到独立测试管线，结束恢复原管线引用。

Unity官方设置说明：https://docs.unity.com/en-us/engine/6000.0/manual/visual-effects/decals/renderer-feature-decal/reference
