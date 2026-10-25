# TASK-003 初始化Git并推送GitHub

Status: done
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

全量`git diff --cached --check`在首次导入模板时返回2，保留566处Unity原文件行尾空格警告，涉及序列化空字段、模板脚本与布局。不修改检查规则或清理无关Unity资产。文档原有4处Markdown双空格换行已去除；`git diff --cached --check -- .gitignore .gitattributes .vsconfig AGENTS.md Docs`通过。

初始基线提交：`dd14cf209f47bd5cf02a620984673e7a7f1558a7`，79个文件。`git push -u origin main`成功，随后`git ls-remote origin refs/heads/main`与本地HEAD逐字符一致，main已跟踪origin/main，当时工作区无未提交内容。

本记录与交接完成后再随一个文档提交推送；该提交的自身SHA不写入提交内。最终远端匹配检查在会话工具结果中确认，恢复工作时仍重新读取HEAD与远端。

验证状态：忽略范围、元数据清单、作者新增内容空白检查、首次push及远端一致性均passed；首次全量空白检查为基线failed并已记录。未执行Unity导入、编译、Play Mode或构建。

Unity运行测试不属于版本发布的验证，本次不声称已执行。

## 下一步

用户开始制作后执行TASK-002；先检查Editor导入/Console和实际展示平台，再建立最小可玩样例。Git基线已建立，不重复初始化，不因后续制作而默认自动提交。
