# TASK-033: Demon AI conversation export through Tuesday midnight
Status: done
Updated: 2026-11-13 Asia/Shanghai

## Scope and acceptance
User confirmed exporting all chats under the Codex Demon project, through 2026-11-10 24:00 Asia/Shanghai (2026-11-11 00:00 +08:00). Filter individual message timestamps. Preserve original user prompts and user-visible assistant messages without summarising them. Produce an offline readable archive, machine-readable records and a ZIP, with provenance and a clear account of unavailable attachments. Do not publish to GitHub or change game files.

## Evidence and plan
- Enumerated all 58 active and 21 archived local Codex chats through the native app tools. Eight chats match Demon by project ID or exact workspace. None of the archived chats matched.
- Six matching chats began before the cutoff. The other two are later chats and will be listed as excluded. The app-provided titles are preserved verbatim.
- Read local original session records for complete untruncated messages. One source is compressed with Zstandard; the bundled Node 24 runtime supports reading it directly without altering the original.
- Export only user-visible conversation content. Internal reasoning and system/developer instructions are not conversation text and will not be copied into the deliverable.

## Delivery and verification
- ZIP: output/Demon-AI-Export/Demon_AI_Conversations_until_2026-11-10_2400_CST.zip (40,197,590 bytes, 159 members). Entry point after extraction: index.html. Also includes individual and merged HTML/Markdown/JSONL plus merged plain text, README, manifests and verification receipts.
- Six included chats, preserving app titles: 设计与初始化, 1, 2, 架构, 3, 4. 模型 and 5 began after the cutoff and contain no included messages.
- 620 visible source entries: 96 user messages, 388 assistant messages/commentary, 16 original interactive questions, 120 displayed-image events. Last included event: 2026-11-10 23:59:36.394 +08:00. No later events included.
- Independently re-read original source lines: all 500 textual entries match the app's visible message bodies exactly; all 620 eligible visible source IDs are present, without duplicate IDs. 388 underlying assistant replies also match after accounting solely for the app's machine citation container removal. Full underlying reply text remains separately preserved in JSONL.
- Initial strict underlying-output comparison failed because the app omits oai-mem-citation containers from visible text. No substantive dialogue differences were found. Recorded this explicit representation difference rather than weakening body checks or altering the exported conversation.
- Restored original embedded user and tool-output image bytes, including screenshots whose temporary files no longer exist. Multi-image tool calls preserve their original image groups without inventing a one-to-one file-path mapping. 130 unique attachment files included. The original 策划案.pdf and 恶魔语言系统 v0.2.pdf are unavailable at their recorded locations; their references remain and the missing files are declared. Current local copies of other files are distinguished from embedded historical images.
- Verified 880 offline links and all included attachment hashes. ZIP CRC and every packaged member's bytes verified. Credential-pattern scan found no hits in conversation text. The export excludes internal reasoning, system/developer/context injection and internal command/output traces.
- Headless browser initially lacked its default bundled browser; used the already-installed Edge in an isolated headless session without installing dependencies or touching the user's windows. Opened the index and architecture chat, tested keyword filtering, observed no horizontal overflow or page errors, and visually inspected both screenshots.
- Archive SHA256: c3d7203db14f0db40cb3707e9f0083693ffc63fca62b5f320ee239dadde65061. No game files changed and no Git commit/push performed.

## Use / next action
Download or copy the ZIP, extract it, and open index.html. For course delivery, place the reviewed logs in the appropriate student's branch and list that location in the assignment README; publication is not part of this export request. This export covers the accessible local account records and does not claim to include permanently deleted or other-account chats.
