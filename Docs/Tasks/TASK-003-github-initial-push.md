# TASK-003 初始化Git并推送GitHub

Status: in progress
Updated: 2026-10-25 Asia/Shanghai

## 目标与范围

用户明确授权将当前项目推送到 `https://github.com/TruthORZ/Demon.git`。建立本地版本库、main分支、origin、Unity忽略规则与初始提交，普通push后核对远端提交。游戏制作任务[TASK-002](TASK-002-playable-slice.md)仍为planned。

不修改Unity资源、版本、包或玩法，不强制推送，不改全局Git设置，不使用用户密码或显示凭据。

## 验收条件

- 缓存、日志、用户设置和生成的IDE工程不进入提交。
- Assets及现有meta、Packages、ProjectSettings、AGENTS与Docs进入提交。
- 运行`git diff --cached --check`，新增/修改的文档和规则通过；原模板问题单独记录，不为清理空格改写Unity资产。提交中无意外大文件。
- 远端main与本地HEAD一致，upstream设置正确，最终状态检查清楚。

## 检查与决定

- 本地原本无Git仓库。默认身份已配置，沿用现有身份，不改全局配置。
- 初次沙箱网络连接GitHub失败；通过获准联网的`git ls-remote --symref`成功，输出为空，远端没有HEAD或现有提交。初始化main不覆盖历史。
- 当前源文件最大约41 KB，不需要新加Git LFS依赖。
- `.gitignore`只排除本机生成内容；`.gitattributes`维护文本与二进制类型，初始提交按属性规范化。
- Git不跟踪预留空目录。本机目录保留，目录归属记录在[ARCHITECTURE](../ARCHITECTURE.md)；不为推送生成伪造meta或成批占位文件。

## 进度与验证

已初始化main与origin，并选择性暂存79个文件，其中23个meta；Library、Temp、Logs、UserSettings及生成工程文件没有进入暂存区。

全量`git diff --cached --check`在首次导入模板时返回2，发现Unity原文件的行尾空格，涉及序列化空字段、模板脚本与布局。保留这些原始内容，不修改检查规则或清理无关Unity资产。文档原有4处Markdown双空格换行已去除，新增/修改内容另作检查。待提交、推送与远端验证。

Unity运行测试不属于版本发布的验证，本次不声称已执行。

## 下一步

推送并核对main，然后更新工程档案与交接，说明接下来执行TASK-002的最小样例。
