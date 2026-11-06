# TASK-018: 运行时类关系图
Status: done
Updated: 2026-11-03 Asia/Shanghai

## Goal and scope
按用户提供的四类 UML 符号梳理当前工作区运行时代码，交付清晰、可放大的关系图及对应源码依据。以实际代码为准，不修改游戏逻辑、资产、项目设置或现有未提交工作；Editor、Smoke、Tests 与外部美术辅助代码明确区分。

## Acceptance criteria
- 继承/实现使用空心三角，关联/依赖使用普通箭头；实线与虚线准确区分。
- 类名、接口、字段引用和临时使用关系能追溯到当前源码。
- 主图按模块组织；简化、重复节点和省略范围明确标注，不将调用流程误写成继承。
- 生成后检查图中文字、箭头和覆盖范围；不需要启动或重测游戏。

## Impact and plan
读取 Assets/Demon/Scripts/Runtime（包含 Domain）及新增美术辅助脚本；输出文档图表与关系清单。沿用 ARCHITECTURE 中已实现的边界。保存本轮前 Git 状态及源文件哈希，完成后比对保证现有工作不被改动。

## Progress and verification
- 已核对 AGENTS、交接、TASK-016、程序集和 Unity 6000.6.0f1；当前 main@94ed249 上有此前未提交的重构、D7–D8和资源修复，全部保留。
- 实际读取 55 份运行时 C#（游戏 48 + 外部辅助 7）：69 个自定义类型，游戏 56 类/5 接口/1 struct，外部 7 类。按源码字段/存储属性、基类/接口与成员使用建立 241 条去重关系。
- 本机 Roslyn 最终静态诊断错误 0。首次混用 .NET Core 与 Unity mscorlib 产生预定义类型冲突；改为读取现有 Unity csproj 引用集后消失。此处是制图工具的离线解析验证，不是游戏编译失败或重新运行 Unity 的证据。
- [交付与说明](../Diagrams/runtime-classes.md)：6 区总览（79 条主要关系）、13 区完整图（241 条关系），PNG/SVG、Graphviz 源、CSV 和包含全部依据位置/源哈希的 JSON。关系类型与示例已按用户提供的四种符号复核；跨区同名节点明确说明为同一类型。
- 已查看总览与完整图，并放大接口区域核对字体与空心三角/普通箭头。修复制图转义造成的字面 `\n`，最终接口/static/abstract/struct 标注分行正常。
- 69 个类型按起点分区全部覆盖；完整图边数与关系清单一致。原有 Assets/Demon、Packages、ProjectSettings 共 367 文件逐字节哈希比对通过，无删除或新增；没有运行游戏测试，因为未改动可执行内容。详细检查在 Logs/DemonChecks/Task018。

## Next action
本次制图完成；用户可先按总览讲解，再放大完整 SVG 或用 CSV 查源码。后续代码改变后重新生成快照。未提交或推送 Git；TASK-017 的玩法集成/人工验收边界仍保持。
