# TASK-011: D5声音交换与D7太阳失光/永夜
Status: done
Updated: 2026-10-29 Asia/Shanghai

## Goal and decisions
用户要求下一步，并明确选择原广播转移＋电台静电，音频占位可替换。SOUND指可辨认广播，原有设备无词静电是环境底噪，D1起已存在；D5 EXCHANGE(RADIO=1,DEMON=0)后原广播从湖中恶魔声源发出，电台只余底噪，不在D5前使用恶魔主动声音。D7 TRANSFER太阳LIGHT=1到恶魔0，太阳照明归零，环境持续夜色；正常日出时段只剩日轮白边。日轮残边/符文/锁定反馈不等于可用LIGHT。
沿GAME_SPEC，声音/光效果与读取T02/T03无关，同一Clock零点/睡眠/晚起场景加载均生效且只应用一次；不做通用解释器、正式人声、惊吓库或九天完整关卡。

## Acceptance
- D5前广播仅电台、恶魔声源不播放，静电底噪原已存在；D5后广播仅湖边源，静电不变；重复帧/跳日/禁用恢复不重复交换。MOTION冻结不停止声音。
- D7前正常日夜；D7零点太阳LIGHT移交一次，太阳DirectionalLight为0、Fog/Ambient保持夜间，预期日出显示白边黑心参照日轮。湖中既有异光由LIGHT和湖封闭控制，封湖熄灭、D9开湖复亮；原太阳根/密码342不变。
- 主/快速DemonRetrieval接线，新DemonConsequences从D4 23:50开始；旧D8/D6/D1/D7与练习源场景不重建。所有音源/Clip/材质/日轮/机位可Inspector编辑，保存后实际参数采用并恢复。
- 纯规则/实际音频路由与样本、D5/D7边界与跳日、真实右键MOTION独立性、T07双序与午夜/原工具回归、截图与Windows构建；真人听感/完整EXE明确区分。

## Impact and plan
基线main@90ad3c2；TASK-009/010及BOX/LAKE双序为已有未提交成果，全部保留；Temp/Task011Baseline保存Demon/Docs/meta/BuildSettings/ProjectSettings。现有本地InputSystem空预加载与UnityConnect差异不发布/回退。
新增纯ScheduledWorldAttributes与显式组合WorldAttributeConsequences。日期源复用NineDayDefinition T02/T03 DateNodeDay，不另建日期表；事件注册生命周期为Initialize→OnDestroy（禁用呈现不撤销日期规则），音源在禁用停止。世界时钟单一、不用第二Timer。
SlicePresentation仍唯一写太阳/Fog/Ambient，只读取新LIGHT状态；SliceFinaleSequence仍唯一控制已保存湖光Root，只从新LIGHT状态取可用性（旧未配置场景保留D7时段）。音频/日轮由新组件独立呈现，不与Tether锁定提示混用。原Sun_FixedPositionPlaceholder根不动，仅本轮太阳视觉切材质/射线/日轮边；无Collider变更，不重烘焙导航。
原生Editor添加保存音源/LineRenderer白边与独立材质，生成自创无词广播旋律/静电WAV占位；clip在新组件统一替换，音源本身拥有音量/距离/空间化参数，不新增依赖。DemonConsequences为同图快速副本，使用独立起始设置；DemonRetrieval同步新属性效果，其他源场景保留。

## Next action
用户试玩DemonConsequences声音迁移/永夜并验收听感；下一步设计九天探索路线、线索搜寻与值守日常。

## Checkpoint
consequences-create-001：保存主/原DemonRetrieval接线、新快速DemonConsequences及独立起始设置、两段原生WAV、三种太阳材质与LineRenderer白边，重新加载引用/密码342通过。太阳根与所有Collider/导航不改。
新实际运行检查会读取AudioSource.GetOutputData非零RMS（并非仅检查clip字段），验证原生空间播放与D5自然跨日路由；D6正午/D7零点及日出、真实右键冻结仍有声音、禁用恢复、T07双序封湖声音/光呈现暂退而不撤销属性、D9再开湖恢复及终局停止。正式人耳听感仍需用户验证。
- consequences-tests-001：73 passed / 0 failed / 0 skipped。
- consequences-smoke-001实际引擎音源输出radioRMS=0.03289407、lakeRMS=0.02864522；自然D5广播迁移/静电、D6正午→自然D7失光/日出白边/永夜、实际右键冻结仍有声音、禁用/恢复不撤销或重执行日期规则、逆序T07封湖隐藏声音/湖光但属性仍1、D9再开湖恢复及终局音频停止，全部通过。T07封闭验证用实际取得/放置/编译组件准备，不冒充本次完整真人拾取路线。

## Final checkpoint
- 截图发现球面前置白边造成中心轻微投影错位，consequences-solar-view-001只把本轮太阳参照盘厚度缩为0.03、白边前偏-0.025，所有根位置与Collider保持。最终D7白边/黑心和D6黄色参照截图已检查。
- consequences-skip-001：D4→D9睡眠经过D5/D7一次、零发现/零T07/零SEAL交换，终局音频停止。consequences-late-001：保存D8正午起始重建SOUND/LIGHT一次、永夜/白边/音源正确，睡眠终局停止。
- consequences-tuning-001：保存广播换为已有ChimeHigh、radio音量0.12、日出7.5后完整运行通过，源输出radioRMS=0.01158101/lakeRMS=0.04393752，两个端点确实采用替换clip；退出自动保存恢复原广播/0.35/6点/StartD4 23:50。
- consequences-retrieval-001：新夜色下实际持E/打断/重试/携带回棚/鼠标从空槽取符文/错误修正/LAKE-BOX逆序Enter/读B与午夜完整通过。consequences-nine-normal/skip-001与旧input/cipher/listening-001均通过。
- consequences-tests-final-001：73 passed / 0 failed / 0 skipped。
- consequences-build-001：Windows64 Development成功174160186字节，完整目录Builds/WindowsConsequences；consequences-nine-build-001：174160098字节，WindowsNineDay；consequences-retrieval-build-001：174160102字节，WindowsRetrieval。均未真人启动独立exe，不等于已完成听感/恐怖节奏验收。
- consequences-validate-001保存引用重载通过。原1140主场景对象ID无丢失、旧meta变化0、5个旧独立场景/3个旧导航/BuildSettings哈希保持、缺meta0；git diff --check通过。构建对InputSystem预加载的自动补入已按唯一字段差异核对，恢复本轮前空列表，原UnityConnect差异保留。
- HEAD仍90ad3c2，TASK-009/010/011与双序调整均本地保留，未提交/push。最终打开DemonConsequences，非Play。

## Human-editability and remaining work
- 广播/静电clip：SliceComposition的WorldAttributeConsequences一次替换，两端初始化采用同一广播；音量、空间化、衰减在Radio_FixedPosition两个原生AudioSource与Consequences_Authored/DemonBroadcast_Lake调整。
- 日期节点唯一源NineDayDefinition T02/T03 DateNodeDay；日出/日落在WorldPresentation的SlicePresentation，夜间Fog/Ambient在SliceSettings。太阳三材质/参照盘/射线/白边及Radio/Lake/SolarView机位均保存在场景，实际天空/广播人声仍为占位可替换。
- 月黑环境来自太阳失光，保留设备灯/手电/提示/环境填光；MOTION、SOUND、LIGHT与SEAL独立，封湖暂退不撤销已转移属性。
- 这轮规则/组合/自动验证完成，status awaiting verification指用户听感/键鼠验收；完整九天场地、日常、搜寻、惊吓及正式音画/存档仍未做，不宣称整款游戏完成。

- consequences-validate-final-002：文本空白清理后重新导入/加载通过；最终非Play、无待处理请求，旧meta变化0，diff检查退出0。

2026-10-29用户明确测试通过并要求进入下一步；此切片验收完成，完整九天/独立EXE节奏仍为后续。
