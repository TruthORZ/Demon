# TASK-038 Formal snow scene English text, weather, music and day selection
Preserve current scene edits, inspect live text and references, run the formal route after wiring.

正式入口为 DemonNineDay_Art。已保存英文玩家界面、交互提示、日程、巡查日志、石板、背包、结局与跳日菜单，场景关联的 UI、世界文字与玩法字段扫描剩余汉字字符串为 0。制作对象名保留。收音机与风向标等头顶标签已停用；锁盘、时钟和准星交互仍可用。
D1 18:00 前关闭发射并清除存量雪花；雪粒子预制体的 Stop Action 原为 Destroy，所以不对该实例使用 Stop。D2 醒来后播放随附 MP3，重开 D1 停止。追逐圆盘仅移除网格，脚步音源静音。日长 720 → 480 秒。
回归曾发现从菜单跳到 D8 夜/D9 时，跳转代码自己设置的菜单输入锁挡住 WaitUntilNight。移除此同步重放期间的锁，保留真实等待和交接门槛。F8 输入、所有目的地、倒跳、重复跳转、Esc 和鼠标命中通过。EditMode 89/89、完整路线和输入回归通过；修正粒子生命周期后另验证 D1 早/晚、D2 早和重开四个雪/音轨状态。本机报告与菜单图在 Logs/task038。未重新构建 Windows 包。
