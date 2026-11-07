# TASK-021 Play Mode 修复与 Hierarchy 整理
Status: done
Updated: 2026-11-07 Asia/Shanghai

## 目标与验收
修复当前 DemonNineDay_Art 场景因删物体而出现的初始化错误。按玩家、室内、逐日可移动点位、路线导航和美术环境归组，清理无用途灰盒和过期验证物体。保存重开检查组件与引用、世界坐标/旋转/缩放、预制体链接、地形数据。正常与睡眠跳过分支及现有规则测试通过，检查代表性交互和点位父组移动随动。

## 影响与方案
仅修改整合场景和文档。保留当前用户布局和已删除的美术占位物。将 BeamTether 缺失的 observationPoint 指向已有 BeamPracticePoint_Editable。按实际引用决定删除范围，仍承担声音遮挡、追逐瞄准、封印反馈和结局显示的对象保留其功能。分组用 Unity 原生父子关系操作并保持世界姿态，现有序列化引用继续使用同一对象。

## 基线与调查
HEAD main@d23cfa5381d477d1537808033a7107dabbf307f0。原有场景、导航、文档、TagManager 和 ProjectSettings 差异留存。Logs/DemonCheckpoints/task021-hierarchy-recovery-20261107 保存本轮基线。
日志报错为 BeamTether.Initialize 缺失引用，实际场景 observationPoint 为 0。用户删除了旧 Beam_Authored/ObservationPoint 及多组岩石、刻痕和路牌。先修接线，再清理其余对象，禁止恢复整份旧场景覆盖当前布局。

## 完成结果与验证
- 原 BeamTether 观察点改接现有 BeamPracticePoint_Editable，正常初始化和九天正常分支已通过。没有改动运行时代码。
- Unity 原生分组为 00_系统与界面、01_玩家、02_室内交互、03_逐日点位、04_湖面事件与结局、05_路线与导航、06_美术环境。D02–D08 的点位组可独立移动。箱 B 改名 BoxB_Record，降临模型改名 D09_DescentFigure_Editable。
- 清理停用的 ListeningTrailProxy 与9个旧路线对象、旧观察镜头、停用相机、分隔线、空作者组和旧 T06 陈列几何。睡眠点去除重复灰盒外观，保留实际交互碰撞。风铃声音遮挡墙、追逐雪痕、湖封印和结局表现仍承担玩法功能，保留其对象。
- 重建字体绑定列表，去掉用户删除物体留下的空标签引用。T06_RecordIdentity 保留原日程身份和引用，仅剩组件与阅读锚点，归于 BoxA。
- 比较2866个保留物体世界姿态和883个预制体根关联通过。点位组枢轴有意移到对应石板附近，子物体世界姿态不变。T01、T05、T07三组临时平移后检查全部子物体随动，检查结束恢复。保存重开后无缺脚本、仅一台玩家相机，T06和文字接线均持久化。
- 保留组件的序列化对象引用逐项对比，除 BeamTether.observationPoint 修复和字体清理外一致。TerrainData、导航、原九天场景和构建设置与开工前 SHA256 相同。
- Play Mode 九天正常分支与连续睡眠跳过分支通过。正常分支使用组件调用验证，含真实箱 B 交互组件、D8时段、拾取/安放、拼板、长阅读、挂钟镜头、午夜交换与延迟结局。为避免编辑器帧延迟导致误判，临时 runner 按实际镜头到位判定。
- 临时工具和快照清理后，81项 EditMode 规则通过，0失败、0跳过。新增场景差异与文档空白检查通过。证据在开工备份目录，重点 retry.txt、validate.txt、reference-audit.json、nine-normal.txt、nine-skip.txt、final-tests.json。
- 作者工具首次运行在清理旧物体后访问了已删除 Transform，第二次发现旧观察锚点尚未归类。两次均在保存前停止，从本轮已备份的当前场景重开后纠正工具再执行，未恢复用户已删的旧灰盒。

## 剩余验证
人工连续键鼠九天与独立 Windows 包本轮未执行，仍沿 TASK-020 的验收安排。用户继续布局时按 ARCHITECTURE 第20节选择点位父组；改障碍物后分别检查玩家碰撞和追逐导航。
