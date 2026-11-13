# TASK-041 选天菜单操作和开场标题

正式雪地场景的开场标题和 Player Settings 的 Product Name 改为 Demon Principles。DayJumpMenu 打开后把焦点交给当前天，11 个按钮在 Inspector 的 Navigation 中显式相连。支持鼠标、方向键加 Enter、数字直达，以及 F8 / Esc 退出。关闭时恢复之前的界面焦点和玩家输入状态。

旧按钮关闭了导航。原来的日程测试判断鼠标是否命中按钮后直接调用 onClick，跳过了鼠标按下和释放。修改前单独测试鼠标点击已通过，未能稳定复现玩家遇到的鼠标失灵。另发现主流程集成测试清理时把无限制设备列表转换成空设备列表，修复了其恢复逻辑。

Unity 6000.6.0f1，先运行 complete-integration，再运行 day-jump-smoke。均通过。后者验证 F8 打开并暂停，方向键选 D2 后 Enter 进入，模拟鼠标按下并释放进入 D8，覆盖全部目的地、重复跳转与 Esc 恢复。开场标题和产品名称通过 Unity 保存并复核。未重新导出 Windows 包。
