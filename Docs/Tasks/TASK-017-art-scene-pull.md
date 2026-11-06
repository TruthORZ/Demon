# TASK-017: 接收GitHub美术场景并保留当前代码
Status: done
Updated: 2026-11-01 Asia/Shanghai

## Goal and scope
用户授权拉取GitHub美术提交，明确代码以当前本地版本为准。接收独立Snow环境及完整依赖；保持TASK-015/016未提交源码、配置、场景、原九天入口和当前PC管线。不自动把美术场景接入九天、不发布Git、不提交本地工作。

## Acceptance criteria
- main快进到已检查的origin/main@94ed249，保留本地未提交代码与原玩法内容，已有GUID不变。
- 714个LFS路径（713个独立对象，约3.90GB）下载真实内容；无未展开指针或损坏对象。
- 原Demon代码/资源、Packages、ProjectSettings和当前管线资产保持本轮前字节；合并.gitattributes同时保留PreparationNavigation二进制声明。
- Unity导入编译；独立Snow场景能加载检查，旧核心规则与主九天回归通过。未接入玩家和完整玩法的环境必须明确标识。

## Impact and plan
基线main@213cfd751e766d5d6df696cb8c484240043432ac；副本Logs/DemonCheckpoints/task017-before-art-pull-20261101含Demon、Settings、Docs、ProjectSettings、Packages和.gitattributes及Git状态。Snow-main目录不动。
远端2237条差异：2225新增/12修改。玩法源码与Packages/ProjectSettings无远端变化；7音频与URP图标只迁移到LFS，payload SHA256与当前相同。新场景Assets/ImportAsset/SnowEnviroment/Nature Pack - Winter Environment/Scenes/Snow.unity，新附带7个模型/树雪风辅助脚本。
重叠仅.gitattributes、ARCHITECTURE和HANDOFF，先备份再暂还原这三份到HEAD以允许快进，然后恢复本地文档并整合美术信息。保留本地UniversalRenderPipelineGlobalSettings.asset，远端对其RuntimeSettings清空不带入原玩法管线。先下载LFS到Git缓存，再受控更新工作区，原生Editor导入与验证。

## Investigation
- 远端4提交至94ed249，来自另一台机器D:/MyProject/Demon。其上传说明和导入验证属于该机器，不能当成本机验收。
- 远端原Shader重复名称已隔离为SnowImported/LegacyLit，HighFidelity管线仅资产不激活；原环境有2处LOD renderer缺引用记录。

## Next action
拉取、地形恢复、临时工具清理与本机验证均完成。可在Unity打开Snow.unity检查环境；接入九天玩法不在本轮范围。当前代码/地形修复/保护规则尚未提交或推送。

- LFS首次fetch引用本地main（仍为旧213cfd7），因此没有下载新提交对象；fsck发现对象缺失，未进入工作区合并。改为显式origin/main（94ed249）下载，完整校验通过前不快进。

- art-preflight001确认现有Editor已保存/非Play，旧九天引用正常。为避免批量checkout中途导入不完整依赖，临时为现有Editor请求桥增加成对art-pause/resume（Disallow/AllowAutoRefresh），更新完成后恢复Runner本轮前原字节。

- 重叠文档一度出现内容哈希等于HEAD、diff为空但Git索引统计仍标M并拒绝快进。原生restore/refresh未清除；对已逐个验证等于HEAD的三个文件执行git add更新统计，确认cached diff为空后快进成功，没有暂存本地程序。当前HEAD=94ed249eca1611f9b09fbae3ed5600554d518ecc。
- LFS全部713对象（3,893,091,972字节缓存）与快进后fsck通过。art-resume恢复自动刷新成功，但第一次新场景检查发生在原生AssetDatabase尚未收录新文件时，OpenScene拒绝；未改场景。改为先显式同步Refresh，等完成导入再检查。

### Terrain integrity finding
- Unity完整导入后，Snow缺脚本/网格/材质槽与不支持shader均0，但Terrain.terrainData=null。GUID04159346b562ce4419274c703cd277b6指向Demo_Scene_02.asset：本机文件45,860,810字节，原生头声明45,860,812字节，Git属性为text/eol=lf，疑似CRLF规范化破坏二进制。该文件不是LFS对象，LFS校验不能覆盖它。
- 本地Snow-main有同GUID旧TerrainData（45,861,368字节、格式22），远端为格式23，不能直接用旧文件覆盖。先原生导入临时副本并SaveAssetIfDirty升级，输出到Logs进行完整内容比较；原始文件与收到的远端文件暂不修改。当前自动刷新已恢复，HEAD94ed249，暂存区为空，临时桥仍待清理。

- 原生升级旧副本可读，但树1237棵，与美术新版本记录1211不同；完整字节比较也不一致，因此未覆盖。
- 精确恢复证据：旧原始文件仅2处CRLF，两处前后各96字节在收到的新文件中（去CR后）各唯一匹配。收到文件头长度恰少2字节；只在这两个唯一锚点补回CR，新文件长度与头一致，去CRLF规范化后逐字节等于收到的远端版本。保留了所有新版数据及GUID，不复制旧地形内容。修复前原字节已备份到Logs；详见terrain-byte-anchors.json/terrain-recovery.json。
- 为该TerrainData添加精确路径binary规则，避免未来Git再次损坏，其他.asset仍维持文本策略。修复后原生加载检查待执行。旧81规则/nine-normal/nine-skip已在拉取后通过。

## Final verification and delivery
- Git main已快进至94ed249eca1611f9b09fbae3ed5600554d518ecc，保留上游历史，无新本地提交/推送，暂存区为空。
- 714个LFS路径/713独立对象，总路径字节3,900,613,883；两轮fsck通过，逐文件SHA256验证所有714份工作区payload与LFS oid相同，无指针残留或损坏。
- 地形恢复后art-terrain-fixed001与art-finalprobe001重复原生加载成功：2467对象、1527 Renderer、175份材质；缺脚本/网格/材质槽=0，不支持材质/Shader错误=0，TerrainData可读。2处空LOD Renderer槽为上游已有，未修改。场景未保存或改动布局。
- 旧副本仅作为两处唯一194字节锚点证据，未用其1237棵树版本覆盖新地形。恢复后只增加2个CR字节，长度45,860,812与原生头一致，规范化后完全等于收到的上游45,860,810字节。原Snow-main数据SHA256仍348f28c9461c155d0902f08d12eaf85835614a09c55c433dd5aa261922c4e950，零修改。恢复数据SHA256=575436ca2de76e96daf64176c19add85423d24694270ed75c91efc4d370d416b。特定TerrainData binary规则已用git check-attr确认text/diff/merge均unset。
- 拉取后81/0/0规则、nine-normal完整自动九天与nine-skip全睡眠通过。临时工具清理后再次81/0/0、12原保存场景arch-validate、nine-validate通过；最终Editor为DemonNineDay、非Play。
- 临时ArtReceiveProbe及meta、TerrainRecoveryProbe及meta均移除；原DemonSliceRequestRunner恢复本轮前字节。编译后的Demon.Slice.Editor.dll也不再含ArtReceiveProbe类型，非仅源码删除。
- receive-audit.json：381份保护文件（原Demon/Settings/Packages/ProjectSettings）零变化/零删除，原文案与玩法规格保持；新资源meta GUID零变化；全部LFS payload正确；无临时代码、无pending请求、无暂存内容。git diff --check通过。
- 本轮相对上游额外保留/修复：当前本地D7–D8与SOLID改动及文档；原GlobalSettings运行配置；PreparationNavigation binary规则；地形2字节修复及专用binary规则。其他新增美术资源与场景按上游接收。未进行新环境镜头美术验收、完整人工九天或独立exe试玩；环境尚未接入玩家/导航/九天规则。
- 编辑入口：Assets/ImportAsset/SnowEnviroment/Nature Pack - Winter Environment/Scenes/Snow.unity，在原层级调整地形/树林/模型/材质。当前主游戏入口仍Assets/Demon/Scenes/DemonNineDay.unity。
- 本机证据Logs/DemonChecks/Task017（回执、GUID预检、LFS校验、地形字节锚点/恢复/原生升级比较、receive-audit.json及脚本）；操作前副本Logs/DemonCheckpoints/task017-before-art-pull-20261101。
