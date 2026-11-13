# RenderPipeline — Key AI Evidence

原聊天：**15 渲染管线理论学习**。作者公开标识：PXY。整理日期：2026-11-13。

**来源不完整：仅取得 2026-11-04–05 的 5 轮原文，早期历史缺失，详见 FullChat 首页。** 本文件只依据可见片段，不补写早期需求/理论内容。已读内容是 Snow 的阅读练习与学习节奏调整，没有足够证据认定其直接改变了 Demon 的渲染代码。

本文件回答：*Identify which requests or outputs influenced requirements, design choices and code changes.*

摘录均可定位到同目录 FullChat 的稳定锚点。摘录外的 Influence / Human Evaluation / Evidence 为本次整理说明，不伪装成原始对话。Human Evaluation 只记录原文支持的选择、纠正或回答；AI 操作和自动验证另行标明。代码链接固定到核对过的提交，历史结论不代表当前全部功能重新验收。

## 1. 从整套代码阅读缩小到颜色传递的一至三行

**Original Prompt / Response**

[UserMessage · entry-001 · 2026-11-04T20:36:20.110000+08:00](RenderPipeline_FullChat.md#entry-001)

```text
这反映整体文件阅读跨度过大，不能把概念选择题答对当作会读完整实现。
```
[AgentMessage · entry-002 · 2026-11-04T20:36:41.622629+08:00](RenderPipeline_FullChat.md#entry-002)

```text
colorMaterial.SetColor("_ScreenColor", screenColor);
```
[UserMessage · entry-003 · 2026-11-04T20:41:07.006000+08:00](RenderPipeline_FullChat.md#entry-003)

```text
输出红色
```
[UserMessage · entry-005 · 2026-11-04T20:42:12.900000+08:00](RenderPipeline_FullChat.md#entry-005)

```text
蓝色
```
[UserMessage · entry-007 · 2026-11-04T20:43:20.051000+08:00](RenderPipeline_FullChat.md#entry-007)

```text
不会
```
**Influence**

教学设计从一次理解 Feature/Pass/Shader 三文件，缩小为观察颜色、追踪 SetColor 到 Shader 返回值，再解释 Pass 提交绘制。影响的是学习要求与验收方式，尚未证明生产代码改动。

**Human Evaluation**

原始用户回答依次为“输出红色”“蓝色”“不会”，可证明这些具体问题的作答。AI 对答对的评价不等于用户已经独立写出完整 Renderer Feature。最初“看不懂”由总调度转述，不能伪装成该聊天直接输入。

**Evidence**

实际本地阅读文件：`D:/NTUCourse/12/Snow/Learning/E01_FullScreenColor_Read/E01ColorFeature.cs`、`E01ColorPass.cs`、`E01Color.shader`。原消息明确未写入 Unity 工程、未编译或验证画面；没有对应 Demon 实现 Commit 或运行测试。

## 2. 调整顺序为 Graph → HLSL → Renderer Feature

**Original Prompt / Response**

[UserMessage · entry-009 · 2026-11-05T17:28:31.167000+08:00](RenderPipeline_FullChat.md#entry-009)

```text
原E01全屏Renderer Feature保留进度、放到Graph基础之后，不算已完成。
```
[AgentMessage · entry-010 · 2026-11-05T17:28:38.255515+08:00](RenderPipeline_FullChat.md#entry-010)

```text
- **进度状态**：E01 保留，但尚未确认完成；Shader Graph 资产也尚未修改或验证画面。
```
**Influence**

根据转入的用户选择，把 E01 保留到 Graph 理解之后，避免同时启动重复练习；理论解释围绕现有 Graph 和可见效果展开。这里只确认学习安排，未授权或实施全工程渲染迁移。

**Human Evaluation**

用户选择由总调度同步；本聊天 AI 简短确认，不再额外发题。没有新增操作结果，不能将学习顺序变化说成 Demon 渲染器已改造。

**Evidence**

与 [SnowEffect_KeyPart.md](SnowEffect_KeyPart.md) 的 Graph/HLSL 学习关联可核对。Demon 存在同源 WorldSnow Graph 只能证明资源进入仓库，不能建立本聊天直接改码的因果链。更早历史未取到，因此不推断其完整影响范围。

## 证据边界与隐私

FullChat 已注明原文来源、时间规则、脱敏类别和附件限制。本次仅整理 Markdown 与核对 Git/本地文件，未启动 Unity、未重新测帧率或发布游戏构建。文件入库证明资源/代码存在，不自动证明用户独立编写、完整学会或实际用于最终画面。
