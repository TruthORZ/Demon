# Demon 当前工程档案

Updated: 2026-10-25 Asia/Shanghai
本文件记录已观察或已创建的工程结构，不把建议组件写成已有架构。玩法以 [GAME_SPEC.md](GAME_SPEC.md) 为准；拟实施模块和选择理由见 [TASK-002](Tasks/TASK-002-playable-slice.md)。

## 1 已观察的工程配置

| 项目 | 当前证据 |
| --- | --- |
| 项目与工作目录 | `D:\Unity\Projects\Demon`，Unity根与工作根相同 |
| Editor | `6000.6.0f1`，revision `f7f8ed4d1e24`，来源 `ProjectSettings/ProjectVersion.txt` |
| 产品名 | `Demon`；公司名仍为 `DefaultCompany` |
| URP | manifest与lock均为 `com.unity.render-pipelines.universal` 17.6.0 |
| 质量设置 | `QualitySettings.asset` 当前索引1，名PC；Standalone默认质量索引1 |
| PC管线资产 | `Assets/Settings/PC_RPAsset.asset`，GUID `4b83569d67af61e458304325a23e5dfd`，由质量项引用 |
| Graphics全局管线字段 | `m_CustomRenderPipeline`为0，但质量项有URP覆盖；不能仅凭该0值判断项目使用Built-in |
| 输入 | Input System 1.20.0；`activeInputHandler: 1`；已有全局Actions资源 |
| 启动候选 | Build Settings只登记并启用 `Assets/Scenes/SampleScene.unity` |
| 游戏模块 | 尚无自定义玩法脚本、玩家、石板或时钟系统 |
| 版本控制 | 已按用户授权初始化Git，分支main，origin为 `https://github.com/TruthORZ/Demon.git`；提交与推送验证见TASK-003 |
| 构建平台 | 未验证当前Editor活动Build Profile；不能从PC质量设置认定最终平台已决定 |

模板场景有Main Camera、Directional Light和Global Volume。只做过文本检查，未在Editor里确认对象显示、Console、材质或运行情况。

## 2 包与资产来源

已核对 `Packages/manifest.json` 与 `Packages/packages-lock.json`，并看到相关 `Library/PackageCache` 目录。缓存存在不等于运行验证已通过。

| 包 | manifest和lock记录版本 | 当前用途 |
| --- | --- | --- |
| Universal RP | 17.6.0 | 已有渲染配置 |
| Input System | 1.20.0 | 已配置为输入后端 |
| Timeline | 6.6.0 | 已安装，尚无项目演出资产 |
| uGUI | 2.6.0 | 已安装，尚未选定项目UI框架 |
| Test Framework | 1.8.0 | 已安装，无项目测试或测试程序集 |
| AI Navigation | 2.0.14 | 已安装，无项目导航或追逐实现 |
| Visual Scripting | 1.9.12 | 已安装，尚未用于玩法 |

没有安装或移除包，没有切换管线/输入/UI。Cinemachine未列入当前manifest，不假定可用；是否引入需有具体收益与审批依据。

`Assets/Settings/DefaultVolumeProfile.asset`静态可见部分`m_Script: {fileID: 0}`及Editor.Tests类型条目。尚未检查它们在Editor中的有效性或运行影响；属于原模板基线待查项，本次未修改或清理。

## 3 已有输入与程序集

`Assets/InputSystem_Actions.inputactions`包含Player与UI映射；Player中有Move、Look、Interact、Sprint等模板动作，也有Attack、Jump、Crouch等未获玩法批准的动作。Interact模板当前有Hold交互，不能不加检查就当成按一下立即交互。

Actions由 `ProjectSettings/EditorBuildSettings.asset` 的configObjects引用。后续若调整，应保留资源GUID、映射与动作契约，检查UI输入与玩家输入切换。

项目没有自建asmdef；现有 `.cs` 仅 `Assets/TutorialInfo/Readme.cs` 与其Editor辅助代码，使用Unity默认程序集。项目玩法程序集边界尚未建立，不虚构已经存在的运行时/测试引用。

## 4 目录与资源位置

保留模板的 `Assets/Scenes`、`Assets/Settings`、`Assets/TutorialInfo` 以及根Actions资源，未移动、覆盖或删除。新增项目内容统一预留在 `Assets/Demon`，避免混入模板与未来外部资源。

| 目录 | 当前状态与归属 |
| --- | --- |
| `Assets/Demon/Scenes` | 空目录，后续保存项目样例和正式场景 |
| `Assets/Demon/Scripts/Runtime` | 空目录，后续运行时代码；按实际功能逐步分子目录 |
| `Assets/Demon/Scripts/Editor` | 空目录，后续可重跑的显式Editor工具 |
| `Assets/Demon/Prefabs` | 空目录，后续玩家、石板、箱子与场景组件 |
| `Assets/Demon/Data` | 空目录，后续静态配置资产；运行时状态不能写回共享定义当存档 |
| `Assets/Demon/Art` | 空目录，项目美术资源，详细分类按资产到位再建 |
| `Assets/Demon/Audio` | 空目录，音频与混音资产 |
| `Assets/Demon/UI` | 空目录，后续所选框架的可编辑源资产 |
| `Assets/Demon/Tests/EditMode` | 空目录，无测试和asmdef |
| `Assets/Demon/Tests/PlayMode` | 空目录，无测试和asmdef |
| `Docs`与`Docs/Tasks` | 设计、工程档案、任务与交接，按AGENTS规定分工 |

这些目录由文件系统建立，未手写meta/GUID。Unity下次导入后应生成文件夹meta；当前没有验证导入。Git不跟踪空目录，本机预留目录与上表仍保留；放入实际资源后一起保存Unity生成的meta，不添加大量占位文件或伪造GUID。

## 5 当前组合与兼容边界

当前没有游戏启动组合点、公共游戏API、项目预制体、存档格式或正式入口场景。Build Settings仍保留SampleScene；新的 `DemonSlice.unity` 只是计划路径，尚不存在。

必须保留：已有meta/GUID、Actions引用、PC/Mobile管线资产与场景。以后采用哪个UI、怎样接入交互、如何划分程序集，应在功能实现并检查后记录到本文件；计划先放任务，不提前宣称已经落地。

## 6 已验证检查与未知命令

已执行并可复用的只读检查：

```powershell
Get-Content -LiteralPath 'ProjectSettings\ProjectVersion.txt'
Get-Content -LiteralPath 'Packages\manifest.json'
Get-Content -LiteralPath 'Packages\packages-lock.json'
Get-Content -LiteralPath 'ProjectSettings\EditorBuildSettings.asset'
rg -n 'activeInputHandler|productName' ProjectSettings/ProjectSettings.asset
rg -n 'm_CurrentQuality|customRenderPipeline' ProjectSettings/QualitySettings.asset
git status --short
```

首次目录初始化时，最后一条失败为not a git repository。之后用户提供GitHub地址并明确授权push，已初始化Git和main分支，设置origin。初始发布过程与结果见[TASK-003](Tasks/TASK-003-github-initial-push.md)。没有另外启用Unity Version Control。

版本控制文件：根`.gitignore`排除Library、Temp、Logs、UserSettings、构建产物和生成IDE工程；`.gitattributes`规定文本与二进制类型，未添加Git LFS依赖。源资源、meta、Packages及ProjectSettings纳入版本管理。Git身份沿用本机已配置值，未修改全局设置。

Unity启动、batchmode测试与打包命令均未验证：尚未定位可执行程序、确认活动平台与Editor占用，不提供可直接复制运行的猜测命令。本次无Unity MCP；`Temp/UnityLockfile`存在但未据此判定Editor运行状态，没有启动第二实例。

Editor后续检查入口：打开当前工程，等待导入，查看Project中Demon目录；检查Console；打开现有SampleScene和Quality、Graphics、Input设置。Play Mode、测试和构建的具体步骤在相应任务完成后记录，不能把本次静态检查当作游戏验收。
