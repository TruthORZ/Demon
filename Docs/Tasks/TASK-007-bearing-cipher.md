# TASK-007: 箱子A与固定方位轮盘
Status: done
Updated: 2026-10-28 Asia/Shanghai

## Goal and scope
用户确认T05没有问题并要求下一步。本轮完成箱子A的三位机械密码锁、固定方位数字轮盘、三个可观察地标，以及开箱取得/阅读T06；接入九天主场景，新增D1快速密码场景。设计见[GAME_SPEC](../GAME_SPEC.md)第3/6节，实际架构和调参见[ARCHITECTURE](../ARCHITECTURE.md)第8节。
T06光束/运动交换/失锁复位、真正追逐、前三块能力与完整永夜未制作；T06目前只有取得/阅读。太阳是固定方位几何占位，不宣称太阳异变已实现。

## Acceptance and impact
- 已实现：固定北标轮盘，电台/太阳/风向标三个显式位置引用；按箱面顺序取数，转动风向标叶片不变答案。十个36度扇区按最近方向对应数字，忽略高度，不读日期/573常量。
- 已实现：点击数字0至9循环，Enter尝试，错误可重试，Esc退出保留输入；正确只开一次，另一次E取出T06，重复阅读不重复发现。不改B/湖、不推进日期、不预约T07窗口。
- 已实现并验证D1提前解开：NineDayFlow有Cipher时始终停用旧T06陈列，拒绝未取得的旧读取接口；DiscoveryProgress.Acquire独立登记取得，允许早于D7。其余发现门槛、T05和T07不改。
- 新增纯C#BearingCipher/ThreeDigitLock、CipherDefinition/Wheel/Box/Digit；Player增加Lock/Inspect视角，复用已保存镜头过渡，不接入石板Board。HUD封印状态明确标箱子B。
- 场景Cipher_Authored保存箱子/轮盘/地标/机位/模型，数据控制数字和说明；轮盘标签运行时按配置刷新，数字应改定义。默认排列4,7,2,9,1,6,8,0,5,3；三地标在9/0/2扇区，当前灰盒答案342，由位置计算，不硬编码答案。太阳/电台/风向标视觉均为可调占位。
- 快速DemonCipher与主场景为独立保存布局，共享CipherDefinition/NineDayDefinition，使用独立CipherSettings（D1 10:00）。原D8与D6快速源场景完全不改；六块旧身份/对象ID保留，不重建Prefab/边界。旧D6仅为独立T05测试，非最新完整内容。
- 基线main@a37036c；保护全部上轮T05未提交成果及既有UnityConnectSettings服务启用修改。Temp/Task007Baseline完整备份Demon/Docs/BuildSettings及meta哈希。本轮不提交/push、不安装包或切换管线/输入。

## Verification
报告与截图在Temp/DemonSliceReports（忽略产物）。本轮实际执行：
- cipher-create-001：Editor原生保存/重载箱子A、轮盘、三个地标、数字与组合引用通过。九天原764对象ID无丢失；旧meta哈希不变、无缺失meta。
- cipher-tests-001：真实Unity EditMode 42 passed / 0 failed / 0 skipped（原33+新增9），XML保留。
- cipher-smoke-001/002：实际W/E进入A、数字鼠标射线、错误密码重试、Esc保留输入、正确开锁后再E取得T06、轮盘E观察、标签与配置一致、叶片转137度不变答案；D1取得无隐藏门槛、B/湖/日期/窗口不变、重复取得/阅读去重、睡到D9通过。
- cipher-tuning-001：实际保存8/9扇区数字互换、LockView横移0.08；运行采用542和新机位，结束DEMON_CIPHER_TUNING_RESTORED，默认342及原机位恢复。不是只改运行时内存。
- 首轮T06截图同帧排队Esc拍到关闭后；验证器增加0.5秒游戏时间截图停留并保留阅读断言。002新锁/轮盘/阅读截图已查看，正文完整、三位数清楚；无玩法失败被掩盖。
- cipher-nine-normal-001：D1至D7发现、T05入口、D7实际A开锁/取得T06、D8夜间门槛/长阅读/挂钟/D9两次交换/闭箱/湖光/延后结局通过。
- cipher-listening-regression-001：原D6 W/E、补全、音源播放、区域进入/停留/重入/遮挡与快照通过；cipher-nine-skip-001：全睡眠0发现0交换到D9；cipher-old-input-001：旧D8 W/E、六次符文点选、Enter、Box E/阅读/Esc通过。
- cipher-build-001：密码Windows64构建成功174027510字节，Builds/WindowsCipher/DemonCipher.exe；cipher-nine-build-001：九天Windows64成功174027510字节，Builds/WindowsNineDay/DemonNineDay.exe。两套level0数据为本轮输出；须保留整个输出目录，不能单拷exe。
- 构建后InputSystem预加载引用已由包恢复，ProjectSettings.asset与HEAD一致；仅既有UnityConnectSettings差异保留。没有Packages/BuildSettings变更。
- 未做独立exe真人操作或正式谜题难度/空间线索验收，地标与文字/符号为灰盒；不能把自动化当完整真人九天试玩。

- cipher-validate-final-001：构建及仅空白清理后重载密码场景，保存引用检查通过；git diff --check通过。旧meta改变0、缺meta0、旧场景对象丢失0；D8/D6源场景及BuildSettings与Task007基线哈希一致。Editor最终为D1密码场景、非Play，无待处理请求。

## Delivery and next action
Unity打开Assets/Demon/Scenes/DemonCipher.unity，Play后点击Game。先观察箱旁轮盘和电台/太阳/风向标的物体方位，再E进箱子A近景，点击三位数、Enter尝试；当前灰盒342。Esc可返回保留输入，开箱后再E取T06，Esc放下记录。主九天场景同样可从D1解开。
策划改Data/CipherDefinition数字/说明、CipherSettings/NineDaySettings时间/移动；美术改Cipher_Authored中的地标位置、FixedBearingWheel北标、BoxA机位/模型。两份场景布局分别保存；新增Editor操作cipher-create/validate/smoke/tuning/build，Create已有输出拒绝重跑，后续直接手调。
下一步T06稳定光束锁定雪痕区域→交换运动一次→失锁再次交换复位，与风铃区域预警分工，再接T07争夺。
