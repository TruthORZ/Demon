# TASK-008: T06稳定光束与运动交换
Status: done
Updated: 2026-10-28 Asia/Shanghai

## Goal and scope
用户确认TASK-007测试通过，接入T06稳定光束牵制：箱子A取得后，探索持右键稳定照住可辨认雪痕区域，交换恶魔代理/风向标运动一次；失锁再交换复位。九天主场景接入，新增D7快速场景。设计见[GAME_SPEC](../GAME_SPEC.md)第2/3/6节；当前实现与调参见[ARCHITECTURE](../ARCHITECTURE.md)第9节。
雪痕仍是固定路径代理；未制作真正追逐/打断、T07夜间争夺、完整T01剧情/通用解释器、声音/完整光/永夜。手电、蓝线、区域发光为可替换灰盒表现。

## Acceptance and impact
- 已实现：未取得不能用，允许D1从实际A取得；按同一Clock的D3运动边界，D3前风向标转/目标不可见，D3起风向标停/雪痕移动，D3即可牵制，无D7硬锁。早期手电可照明但不会创造D3前恶魔运动。
- 已实现：部分稳定不足不交换，中断清零；满足后只交换一次，持续照住不按帧反复交换。雪痕停止、同一叶片转；松键/转向/超距/真实遮挡/近景/阅读/禁用/睡眠终局均释放并再次交换恢复。重复Release不泄漏。
- 新增纯C#StableBeamMotion，复用AttributeOperations容量检查；MOTION与SEAL/实体/日期分开。BeamTether显式连接区域、叶片、灯/线/模型/观察点；Player仅暴露CanUseTools，HUD新增可选状态Text，无精确目标坐标。
- ListeningTrailProxy可选接Tether控制运动；未配置的旧D6沿原T05路径。T05继续只监听位置进入，冻结/停留不重复响；冻结时节点等待也暂停，复位沿已有路径继续，不重置到起点。
- 初始验证BeamDefinition：D3、16m距离、0.85m识别半径、1s稳定、26度Spot、14灯强、100度/秒叶片。射线检查真实遮挡并忽略自身/目标视觉子物体，区域允许偏离准星一定范围，不要求单像素点。
- 叶片旋转归MOTION表现，风向标根位置及342密码不变。T01记录/D3提示通过beam-records同步为当前观察内容，日期不变；未将剧情/其他能力写成已完成。
- 九天源场景新增Beam_Authored、T06_AimRegion/雪痕底层/锁定灯、玩家相机下T06_Flashlight、保存的BeamStatus。新DemonBeam从D7 10:00开始，仍需真实解A取得，无自动装备。独立BeamSettings，共享BeamDefinition/NineDayDefinition；主场景/快速场景布局分别保存。
- 基线main@a37036c，保护全部T05/密码未提交成果与UnityConnectSettings既有差异。Temp/Task008Baseline完整备份Demon/Docs/BuildSettings/meta哈希。未提交/push、未安装包/改管线/输入或启动第二Editor。

## Verification and investigation
报告/截图：Temp/DemonSliceReports（忽略产物）。本轮实际执行：
- beam-create-001保存重载组合/模型/区域/HUD引用通过；九天原1020对象ID全部保留。原D8/D6/D1密码源场景哈希与本轮基线一致。
- beam-tests-001：Unity EditMode 48 passed / 0 failed / 0 skipped（原42+新增6），XML保留。
- beam-smoke-001/002：实际右键/鼠标、短照/中断、稳定保持一次交换、雪痕停/叶片转、T05静止去重；转向/超距/木墙遮挡/近景/禁用/松键/睡眠结局均释放，14次MOTION交换，B/湖/SEAL计数/日期/T07窗口/密码不泄漏。
- beam-tuning-001：保存距离6m、识别半径1.1m、稳定0.4s后完整Play实际采用；DEMON_BEAM_TUNING_RESTORED，已核对恢复16m/0.85m/1s。不是仅改运行内存。
- beam-early-001：主场景D1从真实A取得T06；D3前雪痕不可见/不牵制、叶片转，同一Clock推进D3后可见/运动切换，实际右键在D7前锁/失锁通过。
- 截图发现观察点靠近旧挂钟、新区域底层遮住脚印。beam-view-001只移动本轮观察点/标记/说明至(-1,0.04,3.3)，区域本地y由0.03压到-0.07；不动旧钟/密码。002重跑全部检查通过，新锁定/观察点/遮挡截图已看，脚印露出且钟不再占观察画面。
- beam-records-001同步T01观察文字/D3提示，去掉运动尚未制作的旧说明，未改日期/其他日程。
- beam-cipher-regression-001：原D1密码/轮盘实际W/E/鼠标/Enter、错误重试/Esc保留、取得去重与早期路线通过。
- beam-listening-regression-001：原D6 W/E/补全、区域监听/音源播放/遮挡与快照通过。
- beam-nine-normal-001：发现/T05/A取得T06、D8门槛/长阅读/挂钟/D9二次交换/闭箱/湖光/延后面板通过；beam-nine-skip-001全睡眠0发现0交换到D9；beam-old-input-001旧D8 W/E/六次符文点选/Enter/Box E/阅读Esc通过。
- beam-build-001：D7光束Windows64成功174076962字节，Builds/WindowsBeam/DemonBeam.exe；beam-nine-build-001：九天Windows64成功174076978字节，Builds/WindowsNineDay/DemonNineDay.exe。两套level0数据均为本轮输出；须保留完整输出目录，不能单拷exe。
- 构建后InputSystem预加载引用由包恢复，ProjectSettings.asset与HEAD一致；只保留既有UnityConnectSettings差异。没有Packages/BuildSettings变更。
- 未做独立exe/完整九天真人试玩，瞄准手感、距离/时间平衡和恐怖节奏仍需用户确认；自动化与截图不能代替真人体验。

- beam-validate-final-001：构建与仅空白清理后重载D7场景，保存引用检查通过；git diff --check通过。旧meta改变0、缺meta0、旧对象丢失0；D8/D6/D1密码源场景及BuildSettings与本轮基线哈希一致。Editor最终为DemonBeam、非Play，无待处理请求。

## Delivery and next action
Unity打开Assets/Demon/Scenes/DemonBeam.unity，Play后点Game；E进A，当前342，Enter解锁后再E取T06，Esc放下。到湖岸蓝色观察点，转向雪痕并按住右键约1秒；雪痕停、叶片转。松开、移开、绕木墙或进入近景，观察恢复。WASD/Shift仍可同时移动；超距需靠近。
BeamDefinition调距离/识别范围/稳定时长/灯/叶片速度；Beam_Authored调观察点，玩家相机下T06_Flashlight调外观/灯/线，Listening_Authored/SnowDisturbance_Target下调识别点/视觉。移动风向标根会影响密码，改叶片朝向不影响；两套场景布局各自保存。
固定Editor白名单beam-create/view/records/validate/smoke/tuning/early/build；Create已有输出拒绝重建，后续手调保存内容。不打断Play，不启动竞争Editor。
下一步制作实际追逐/打断与T07夜间争夺，把风铃区域判断和光束争取时间接成可玩流程；不将当前固定路线代理称为完整AI。

## Git delivery
2026-10-28用户明确授权push；本次一并发布TASK-006/007/008的Assets/Demon源资源/meta与Docs。发布前HEAD为a37036cd166d246270040bf7f843c9a6c221daa0，远端main预检与其一致，暂存区原先为空。保留本地UnityConnectSettings服务启用差异，不提交缓存/备份/Windows构建。发布后SHA与远端核对结果见本轮交付回执，恢复时以git log -1确认，不在提交内记录其自身SHA。

暂存检查发现15个新入库meta的空字段尾空格；仅清理空白，逐个校验GUID及全部非空白内容一致。该格式整理发生在功能验证后，不改变资源含义；此前meta哈希零变更结果对应功能验证时点。发布继续排除UnityConnectSettings及缓存/构建。

用户随后明确确认发布到https://github.com/TruthORZ/Demon.git的main，包含全部已提交功能及最新BeamDefinition参数10m/1m/0.5s。追加提交仅含参数与文档；ProjectSettings两项本地差异继续排除。此前自动审批阻塞已由明确确认解决，仍按普通push及远端SHA核对执行。现有Windows构建对应先前验证参数，本次未重建或重新声称新参数的Play结果。
