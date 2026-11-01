# SNOW-SCENE-PACKAGE-IMPORT-20261101：Snow 环境场景打包与导入
Status: done (03执行验证完成，待00独立验收)
Updated: 2026-11-01 Asia/Shanghai

## 交付与边界
从 Unity 2022.3.39f1c1 使用原生 ExportPackage 导出 Snow.unity 及必要依赖，原生 ImportPackage 导入本地 D:/MyProject/Demon（Unity 6000.6.0f1 / URP 17.6）。这是可编辑环境场景交付；没有接入 Demon 玩家、任务、导航或九天玩法，没有恢复已暂停的整体降级。

- 包：D:/MyProject/Snow/Assets/ImportAsset/SnowEnviroment/Nature Pack - Winter Environment/Scenes/Snow.unitypackage
- 包大小：3,676,313,182 字节；SHA256：07be5d813dac153ce047001778ea098f74f88038896eded76bc278c74cae6dac。
- 目标场景：Assets/ImportAsset/SnowEnviroment/Nature Pack - Winter Environment/Scenes/Snow.unity。
- 证据目录：D:/NTUCourse/12/Snow/Integration/SnowSceneTransfer；包清单 package-manifest.json、逐项校验 package-audit.json。

## 实施与兼容修正
原生依赖收集后补入未被 GetDependencies 返回的五个 BGWater HLSL include。显式 ExportPackageOptions.Default 导出 1,077 项；压缩包全部 payload 哈希/GUID 与源一致，无额外条目，无目标路径冲突。没有复制 Packages/ProjectSettings 或缓存。
导入的 Assets/SnowSystem/SnowTest/assets/Lit.shader 原名与 URP 标准 Lit 重复。仅目标内将 Shader 名称改为 SnowImported/LegacyLit，GUID与其余着色器代码保持；29个现场材质继续引用同一 GUID。包保留源工程原始版本，再次覆盖导入该 shader 会覆盖此处名称隔离。
Unity 原生导入/保存升级了105个导入材质、导入的 URP-HighFidelity.asset 和 Snow.unity；加上该 shader，共108个包内文件与源包不同。全部1,077个导入meta与包一致，GUID零变化。详细清单 final-file-audit.json。导入的 HighFidelity 管线未设为当前管线。

## 实际验证
- C# 导入编译通过；实际渲染后的 ShaderUtil 检查无 Error、135个现场材质均 supported、材质贴图引用无变化（material-comparison.json）。
- 加载、保存、重载通过；Play后卸载再从磁盘加载、再次渲染及保存重载通过（target-reload-final.json）。
- 2,480个GameObject、1,536个Renderer；Missing Script/Mesh/Material槽位均0。所有对象transform/active、相机位置/FOV、地形与脚本列表与源一致。
- Terrain：1,211棵树、6个TerrainLayer、16个树/草原型、513高度图分辨率与600×200×600尺寸一致。天空盒、雾、环境光和颜色空间一致；Volume原有参数保持，仅新增版本默认字段和浮点打印尾差。
- 1600×900原生相机渲染：source-cabin01.png 对 target-cabin-final.png；source-lake02.png 对 target-lake-final.png。两对均由03目视检查：树木、小屋、地面雪、湖岸、水波、倒影与雾存在，无可见粉材质。机位/FOV/场景曝光相同，水波时间未锁定，非像素一致承诺。
- 默认保存相机 source-main01.png / target-main01.png 也已查看：原相机位于环境外侧/下方，构图不适合演示，源目标均有几何背面/水下遮挡；未替用户改机位。source-lake01 为遮挡试拍，不作验收图。
- Play 约8秒（09:55:44.028→09:55:52.718 UTC），正常进入/退出，未记录Error/Exception/Assert。沿用项目原有关闭domain/scene reload选项；这是环境短时运行，不代表玩家交互、通路或完整游戏验收。
- 原Demon 397个资产/Packages/ProjectSettings文件逐字节未变；BuildSettings、原渲染管线及玩法保持。源1,077项依赖及meta均未变化，源场景/meta哈希与00基线一致。

## 已知差异与限制
原场景的两个 LODGroup.m_LODs[0].renderers[0].renderer 缺失引用仍在 InnerMoutain/Mountain_03 (11)、(12)，不是本次新增；没有擅自删除。
Demon继续使用原 PC_RPAsset / PC_Renderer，源使用 URP-HighFidelity。目标画面略亮；阴影、抗锯齿和远景LOD等受原项目配置及URP代际影响，不承诺像素级相同。目标附加灯光阴影提示31张shadow map在2048图集内降分辨率；保留原配置，没有为追齐画面改全局管线或调色。
Unity6在导入旧TextureImporter版本时打印升级提示，实际贴图均完成导入，meta未手改。旧测试78项和Windows构建本次未复跑；没有双机或真人游玩验收。

## 重要失败与恢复
第一次启动因临时probe使用Unity6已移除的InstanceID属性编译失败，改为版本兼容反射后通过。第二次启动在包导入前发生原生QuickSearch LMDB索引合并崩溃，同时可用内存很低；target-editor2.log保留。其后临时关闭项目本地Search启动索引、限制job worker为2和import worker为1，并卸载源干净场景后，目标完成导入和两次启动验证。没有杀进程或更改全局偏好。
收尾已恢复UserSettings/Search.settings原文件；恢复后的默认索引启动未另行复测，不能宣称该Unity索引崩溃已修复。若再次出现，先检查内存与同版本索引问题，不能因此删Library或改工程版本。

## 清理、编辑入口与恢复点
目标编辑器已正常退出；两边临时SceneTransferProbe及目标SceneTransferTools已从Assets移除，无pending请求。源原编辑器已重新打开Snow.unity，非Play、无脏场景，未保存源场景；用户WinterWalkabilityAnalysis、PortHierarchyGrouping保留。源新增仅包及其meta，原有未提交内容保留。
在目标打开上述Snow.unity即可编辑层级、Terrain树与材质；氛围在 we/PostProcessing 的 Demo_Scene_02_ColdAtmosphere_URP.asset，湖面材质仍为原BGWater。场景未加入或替换构建入口，实际玩家接入另排任务。
操作前备份 Baseline/Demon 包含Assets/Packages/ProjectSettings/Docs，Baseline/SnowConfig保存源设置，Baseline/Snow.unity与meta保存场景；本地Demon无.git，不引用旧文档的Git HEAD为当前状态。原任务014说明仍完整保留在SESSION_HANDOFF下方。源Git main@753d697f有原先用户改动，未提交/推送/重置。
完成记录：D:/NTUCourse/12/Snow/dispatch/handoffs/03_SNOW-SCENE-PACKAGE-IMPORT-20261101.json。
