# TASK-001 项目目录 文档初始化与构建规划

Status: done
Updated: 2026-10-25 Asia/Shanghai

## 目标与范围

按根目录 [AGENTS.md](../../AGENTS.md) 建立必要目录、设计规范、实际工程档案、后续任务与交接。当前请求仅授权整理与规划，不实现玩法、不修改已有场景或包、不初始化版本库。

设计由 [GAME_SPEC.md](../GAME_SPEC.md) 唯一维护；实际工程由 [ARCHITECTURE.md](../ARCHITECTURE.md) 唯一维护；后续方案由 [TASK-002](TASK-002-playable-slice.md) 维护。已有 AGENTS.md 不改写，不复制一份规则手册或旧技术策划案进项目。

## 验收条件

- 必要文档存在，设计和实现状态明确区分，链接可用。
- 新建资源目录与既有模板目录分离，不移动或覆盖已有资源。
- 已确认设计完整进入 GAME_SPEC；未知项不冒充已决定内容。
- 版本、包、URP 质量配置、输入和入口场景以实际文件为据。
- 提供三周路线与首个可玩样例的步骤、可编辑资产、边界和验证方案。
- 校验原 AGENTS、Assets、Packages、ProjectSettings 文件未被修改。

## 影响与计划

仅新增 Docs 文档与 Assets/Demon 空目录。对既有脚本公共接口、序列化字段、GUID、场景、预制体、输入资源、包依赖和构建设置没有修改。目录由文件系统创建，元数据由 Unity 下次导入生成，不手工编造 meta 或场景 YAML。

1. 只读检查指令、版本、包、质量与图形设置、输入资源、示例场景和版本控制。
2. 建立项目专用目录，填写设计与实际结构文档。
3. 建立 TASK-002 的实施计划，写入最新交接。
4. 校验内容、相对链接、空白格式和原文件 SHA256。

## 检查发现

- `git status --short` 返回 not a git repository；分支和 HEAD 不存在，不能使用 Git diff 声称工作树干净。未初始化、暂存或提交。
- 新建文档前无 Docs、HIERARCHY 或项目玩法脚本，只有模板 Readme 编辑器脚本；无需迁移既有架构。
- `Temp/UnityLockfile` 存在，不据此声称 Editor 正在运行或已连接；本会话未启动第二个 Editor。
- 当前没有可用的 Unity MCP 工具；本次不需要 Editor 自动化。没有做 Play Mode、编译或构建。
- 修改前已对 AGENTS.md、Assets、Packages、ProjectSettings 的70个原有文件计算 SHA256，供新增后比对。

## 进度与验证

已创建 GAME_SPEC、ARCHITECTURE、SESSION_HANDOFF、本任务和TASK-002，共5份项目文档；Assets/Demon下创建10个叶目录及其父目录。保留原AGENTS，不新增HIERARCHY或第二份追踪总表。

验证结果：

- **passed**：5份Markdown可用UTF-8读取、代码围栏闭合、无冲突标记和非预期行尾空白；17个相对链接有效。
- **passed**：关键6个包的manifest版本与lock版本一致；实际输入资源有Player与UI映射。
- **passed**：原70个文件SHA256逐一相同，无修改或丢失；Assets、Packages、ProjectSettings没有新增资源文件。Assets/Demon中玩法脚本数量为0。
- **not run**：Git diff/check，原因是项目不是Git仓库；已以内容校验和Markdown静态检查覆盖本次新增边界，但不冒称Git验证通过。
- **not run**：Unity导入、meta生成、编译、Play Mode、测试与构建。本次仅目录与文档工作，没有启动Editor或实现玩法。

## 下一步

用户确认开始制作后转交 [TASK-002](TASK-002-playable-slice.md)。游戏实施保持planned，本任务完成不代表游戏功能完成。下一步先确认版本控制与Editor状态，按TASK-002的第一阶段建立可玩样例。
