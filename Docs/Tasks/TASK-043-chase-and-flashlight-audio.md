# TASK-043 雪地追逐脚步声与手电筒开关声
正式场景引用 Assets/Demon/Audio 中的 footsteps-on-snow-335509.mp3（8.10 秒）和 HG_sound_flashlight_switch.mp3（1.03 秒）。复用敌人脚步空间音源，连续移动时循环原录音，避免逐步叠放。手电筒声源与玩家相机一起启用，实际开灯、关灯时从头播放。

Unity Play Mode 验证通过：D4 追逐开始移动时播放，回屋后停止。实际左键开、关各检测到开关音源播放。主流程输入回归通过。未人工试听混音。
