# SnowEffect — Key AI Evidence

原聊天：**石头积雪教程**。作者公开标识：PXY。整理日期：2026-11-13。

这是 Snow / 雪景水体中的教学聊天。原 Graph 资产确已进入 Demon，可证明资源关联；本聊天中的 HLSL 填空尚无提交结果，不能称为已实现的新 Demon Shader。初始 AI 对视频内容的把握及 Lerp 假设均保留了后续纠正。

本文件回答：*Identify which requests or outputs influenced requirements, design choices and code changes.*

摘录均可定位到同目录 FullChat 的稳定锚点。摘录外的 Influence / Human Evaluation / Evidence 为本次整理说明，不伪装成原始对话。Human Evaluation 只记录原文支持的选择、纠正或回答；AI 操作和自动验证另行标明。代码链接固定到核对过的提交，历史结论不代表当前全部功能重新验收。

## 1. 三轮学习与视频来源核查

**Original Prompt / Response**

[UserMessage · entry-001 · 2026-10-27T20:31:14.935000+08:00](SnowEffect_FullChat.md#entry-001)

```text
[https://www.youtube.com/watch?v=IC9g5hlfV6o&list=PLaQGGY7vp6WfpPyg-qlcNrBuNaDYa3_n9&index=19](https://www.youtube.com/watch?v=IC9g5hlfV6o\&list=PLaQGGY7vp6WfpPyg-qlcNrBuNaDYa3_n9\&index=19) 1. 看看这个教程 按照我的学习方法, 第一轮讲述核心原理 第二轮讲基本框架 第三轮讲具体每一步操作 2. 教程用的是2019.1.2f1, 我用的是2022.3.39f1c1  有些操作在新的版本没有了或者换了名字位置, 告知我
```
[UserMessage · entry-003 · 2026-10-27T20:40:37.113000+08:00](SnowEffect_FullChat.md#entry-003)

```text
现在能看到视频画面吗
```
[AgentMessage · entry-004 · 2026-10-27T20:40:50.923118+08:00](SnowEffect_FullChat.md#entry-004)

```text
目前**还不能直接看到这条 YouTube 视频的实际播放画面**。
```
[AgentMessage · entry-004 · 2026-10-27T20:40:50.923118+08:00](SnowEffect_FullChat.md#entry-004)

```text
之前展示的图片是参考示意图，**不是视频截图**。
```
**Influence**

需求是从原理、框架到具体操作，并处理 Unity 2019 教程与本地 2022.3 的差异。用户追问视频可见性后，AI 承认不能逐帧核对；因此原教学只能作为技术解释，不能宣称准确复刻视频。

**Human Evaluation**

用户直接核查 AI 是否看过视频，促成来源纠错。没有用户实际完成所有节点或每步迁移的证据。

**Evidence**

FullChat entry-001/004 保存教程链接与纠正。无该项独立 Demon 代码提交或运行测试；教程/第三方素材与用户新增实现应分别归属。

## 2. 根据实际 Graph 纠正 Lerp 为 Add，保留转换语义

**Original Prompt / Response**

[UserMessage · entry-007 · 2026-11-05T17:27:56.610000+08:00](SnowEffect_FullChat.md#entry-007)

```text
Lerp节点完全未连接；Dot另连Clamp但Clamp输出未使用。因此目前是原颜色加亮的效果，不能误说已实现岩石/雪两套外观的Lerp混合。
```
[AgentMessage · entry-008 · 2026-11-05T17:28:21.506942+08:00](SnowEffect_FullChat.md#entry-008)

```text
根据总调度提供的文件解析结果，目前你的 Shader 使用的是 `Add`（加法），而不是 `Lerp`（材质混合）。
```
**Influence**

实际节点资料推翻早期通用框架中的 Lerp 混合假设：贴图颜色加遮罩亮度才是现有输出。后续转 HLSL 应保持 Add，并逐步核对坡度、噪声和坐标，避免改了算法却声称等价。

**Human Evaluation**

结构信息由总调度作为用户角色消息转入，属于 Codex 静态解析结果，不是 ChatGPT 直接读盘，也不是用户手工核验。用户此前只选择了学习方法；参数观察建议没有反馈验收。

**Evidence**

学习源 `D:/MyProject/Snow/Assets/SnowSystem/SnowTest/WorldSnow_test/WorldSnow.shadergraph` 与 Demon [`Assets/SnowSystem/SnowTest/assets/WorldSnow.shadergraph`](https://github.com/TruthORZ/Demon/blob/c12560b6896a5bf46423fe69a9408b79cbe25864/Assets/SnowSystem/SnowTest/assets/WorldSnow.shadergraph) 本次逐字节 SHA256 相同：`55e7908c08968a997346759749542e613d43edca18aa73754fb51000d294c9c7`；入库提交 [358340bb26db](https://github.com/TruthORZ/Demon/commit/358340bb26dbe5e08c0a019544e273425eec7a1f)。相同文件证明资产关联，不证明最终场景使用情况或个人原创全部节点。

## 3. 用户把学习起点改为节点转 HLSL，小范围代码验收待完成

**Original Prompt / Response**

[UserMessage · entry-009 · 2026-11-05T17:33:39.849000+08:00](SnowEffect_FullChat.md#entry-009)

```text
用户最新反馈覆盖上一轮学习起点：“我已经理解节点效果，现在直接开始转化为脚本，新建一个21批阅代码。”
```
[AgentMessage · entry-010 · 2026-11-05T17:34:23.061666+08:00](SnowEffect_FullChat.md#entry-010)

```text
这只是独立的 HLSL 函数练习，暂时不能直接作为完整 Unity Shader 运行。
```
**Influence**

教学由 Graph 参数观察改为 `Normal Vector → Dot Product → Smoothstep → Multiply` 的手写函数。保留 SnowOpauqe 原拼写、0.5/0.85 阈值，要求亲写两行并解释输入输出；不立即扩展到完整管线或自动生成完整雪系统。

**Human Evaluation**

该选择由总调度转述用户原话，属于学习偏好与自述理解；完整原聊天没有填空答案或 21 的批阅结果，不能记录成已通过 HLSL 编程验收。

**Evidence**

FullChat entry-010 原样保留 `GetSnowMask` 的空白代码块；无本聊天可归因的手写 Shader Commit、Unity 编译或画面通过记录。Demon 已有 Graph 资产与尚未完成的 HLSL 练习是两件事。

## 证据边界与隐私

FullChat 已注明原文来源、时间规则、脱敏类别和附件限制。本次仅整理 Markdown 与核对 Git/本地文件，未启动 Unity、未重新测帧率或发布游戏构建。文件入库证明资源/代码存在，不自动证明用户独立编写、完整学会或实际用于最终画面。
