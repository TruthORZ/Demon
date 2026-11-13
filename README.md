# Demon Principles

An AP6412 group assignment about designing an extensible game.

Demon Principles is a first-person horror puzzle game set at an isolated winter outpost. The player investigates anomalies, reads rune tablets, uses tools, and prepares for a lake-seal ritual across a nine-day story. The assessment focuses on the object-oriented model and how its design responds to new requirements.

## Submission information

| Item | Value |
| --- | --- |
| Course | AP6412 |
| Group identifier | **To be confirmed** |
| Route | Route A, three stages, using Unity 3D with lecturer approval reported by the team |
| Engine | Unity **6000.6.0f1**, revision `f7f8ed4d1e24` |
| Language | C# |
| Target | Windows 64-bit, keyboard and mouse |
| Repository | [TruthORZ/Demon](https://github.com/TruthORZ/Demon) |
| Documentation preparation date | 13 November 2026, Asia/Shanghai |

### Group members and individual evidence

The team will replace the following placeholders with its confirmed submission details. Git author names alone do not establish student identity or personal branch ownership.

| Student name | Student ID | GitHub account | Personal branch and contribution links | AI record location |
| --- | --- | --- | --- | --- |
| To be confirmed: current user | To be confirmed | To be confirmed | To be confirmed | [Current user's records through the approved cutoff](Docs/AI-Logs/Current-User/Through-2026-11-10/index.html) |
| To be confirmed: second member | To be confirmed | To be confirmed | To be confirmed | To be provided, or an explicit declaration of no AI use |

The group number, member identities and individual branch links remain pending by the user's instruction. Keep a separate, named reflection subsection and a separate generative-AI declaration for every member.

## Project and version status

The playable snow scene, pursuit spawns, audio and interactive props are included in [gameplay commit 8963e62](https://github.com/TruthORZ/Demon/commit/8963e62b723bead05ec183673f13f09e274c268c).

The three architecture checkpoints below describe fixed historical commits. Before final submission, the team must identify the actual final source revision, update any affected Class Map and report, and verify that the submitted project and demonstrated build use the same revision. This README does not claim that all submission items are already complete.

## Requirements and dependencies

Install Unity Hub and Unity **6000.6.0f1** with Windows build support. Use the engine revision recorded in [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt). The three selected historical checkpoints use this same Editor version.

Unity Package Manager restores dependencies from [manifest.json](Packages/manifest.json) and [packages-lock.json](Packages/packages-lock.json). Important packages are:

| Package | Version | Purpose |
| --- | --- | --- |
| Universal Render Pipeline | 17.6.0 | Rendering |
| Input System | 1.20.0 | Keyboard, mouse and UI input |
| uGUI | 2.6.0 | Authored in-game UI |
| AI Navigation | 2.0.14 | Navigation surfaces and pursuing enemies |
| Test Framework | 1.8.0 | EditMode rule tests |
| Timeline | 6.6.0 | Installed project dependency |

Use the complete manifest and lockfile for the remaining dependencies. Do not upgrade packages or change render/input systems as part of opening the assessment project.

Git and Git LFS are required when retrieving the project from GitHub. Many image, model, audio and other source assets use LFS. Repository access and an internet connection are required for a fresh clone and initial dependency retrieval. No runtime online service is required by the game's core loop.

## Setup and run

### Open the supplied project

1. Extract the submitted files to a writable local directory. The Unity project root is the folder containing `Assets`, `Packages`, `ProjectSettings` and this README.
2. In Unity Hub, add/open that folder with Unity **6000.6.0f1**. Allow asset import, package resolution and script compilation to finish.
3. Open **`Assets/Demon/Scenes/DemonNineDay_Art.unity`** for the current snow-environment main route.
4. Enter Play Mode and give the Game view keyboard/mouse focus. The route begins at the cabin on Day 1. Click inside the Game view if the mouse is not captured.
5. Follow the on-screen objectives. Exit Play Mode to stop the Editor session.

The original greybox nine-day scene is `Assets/Demon/Scenes/DemonNineDay.unity`. Other scenes include focused development demonstrations. They should not be mistaken for the current main-route entry point.

### Retrieve the published repository

Run these commands in a directory where a new project copy may be created:

```powershell
git clone https://github.com/TruthORZ/Demon.git Demon
cd Demon
git lfs install
git lfs pull
```

Then open the cloned project using the steps above. If textures, models or audio appear missing, check that LFS retrieved the actual asset data rather than only pointer files. A GitHub source ZIP should not be assumed to contain all LFS assets without checking it.

### Controls for the current main route

| Input | Action |
| --- | --- |
| WASD | Move |
| Mouse | Look around |
| Shift | Sprint |
| E | Interact with the focused object |
| Hold E | Complete a prompted hold-to-pick-up interaction |
| Left mouse button | Toggle the acquired flashlight during exploration; select runes, slots or UI controls in the relevant interaction view |
| Enter | Confirm/compile the active editable rune-board puzzle; also skip the opening when offered |
| Tab | Open or close the journal/inventory |
| Esc | Close the current reading, inspection or menu view; during exploration, release the cursor |
| T | On Day 8, use the prompted wait action inside the cabin after reading the handover record |
| R | Restart after the ending |
| F8 | Open the development day-jump menu |

Flashlight operation requires acquiring the lamp. Its ordinary illumination and the T06 motion-exchange ability have different unlock conditions. Follow context prompts for puzzle interactions. Earlier historical scenes may use different tool controls, including held right mouse input; the table above describes the current main route.

The day-jump menu supports choosing Days 1-9 and a final-night shortcut. It reconstructs earlier progress for demonstration. It is a debug facility, separate from the player's actual-entry retry checkpoint.

## Gameplay and reproducible demonstrations

The intended loop is to patrol the outpost, collect and reread records, investigate tablets and tools, complete the day's required work, and rest when allowed. Scheduled changes and the rune programs affect the same world clock. The story reaches its defined Day 9 ending after the final-night sequence.

| Demonstration | Procedure and expected result |
| --- | --- |
| Basic interaction | Start Day 1, move through the cabin, focus an interactable and press E. Open the journal with Tab and select a collected entry. |
| An invalid action | Attempt to sleep before meeting the day's rest conditions. The game should explain/block the action rather than silently advancing. |
| Tool extension | Demonstrate the listening/chime system or the T06 beam after reaching its unlock conditions. The tool affects its assigned rule without manually editing the clock. |
| Daily failure and retry | Leave required work incomplete until the day fails. Use the retry UI to restore that day's actual entry state while keeping the earlier days' progress. |
| Final-night sequence | Reach Day 8 normally, or explicitly disclose use of the F8 shortcut. Follow the handover, wait, retrieval and reconstruction prompts, then observe the defined Day 9 outcome. |

Day jumping demonstrates selected situations but does not establish that the whole route has been played continuously. For rules, objectives and accepted design decisions, see [GAME_SPEC.md](Docs/GAME_SPEC.md).

## The three Route A checkpoints

These Stage numbers group real historical commits for the presentation. The original brief's three-stage freeze and unknown-requirement process requires its own evidence. The team's reported permission to use Unity 3D does not, by itself, prove the dates on which requirements were released or versions were frozen.

| Stage | Fixed published commit | Main scene | Functional scope |
| --- | --- | --- | --- |
| Stage 1 | [`a37036c`](https://github.com/TruthORZ/Demon/commit/a37036cd166d246270040bf7f843c9a6c221daa0), 28 Oct 2026 | `Assets/Demon/Scenes/DemonNineDay.unity` | Basic nine-day clock and discovery rules, rune-board reconstruction/freezing, Box B/lake seal exchanges, final-night reservation and Day 9 ending. |
| Stage 2 | [`b31abc5`](https://github.com/TruthORZ/Demon/commit/b31abc5c65524662a4448b4e0117d486da1d1fa7), 7 Nov 2026 | `Assets/Demon/Scenes/DemonNineDay_Art.unity` | Operational listening, cipher and beam tools, pursuit and final-tablet retrieval, exploration, interaction interfaces, snow-scene integration and day-jump demonstrations. |
| Stage 3 | [`b94f864`](https://github.com/TruthORZ/Demon/commit/b94f864388d0dd524bdc07cfe165b7b70746711f), 12 Nov 2026 | `Assets/Demon/Scenes/DemonNineDay_Art.unity` | Required daily objectives and patrols, daily failure/retry snapshots, journal inventory, flashlight input, daily chase rules and merged art/footprint work. |

**Required submission tags remain pending:** `stage-1`, `stage-2`, `stage-3`. No local or remote tags were present when checked on 13 November 2026. The commit links above are valid checkpoint references, not tag links. The team must supply the actual frozen tags and explain the requirement-release evidence before claiming full Route A submission compliance. Do not move an earlier frozen tag or present a newly created retrospective tag as a historical freeze.

### Open each historical version independently

After cloning the repository and retrieving its LFS assets, the following commands create separate source folders without replacing the current project's files:

```powershell
git worktree add --detach ../Demon-Stage1 a37036cd166d246270040bf7f843c9a6c221daa0
git worktree add --detach ../Demon-Stage2 b31abc5c65524662a4448b4e0117d486da1d1fa7
git worktree add --detach ../Demon-Stage3 b94f864388d0dd524bdc07cfe165b7b70746711f
git -C ../Demon-Stage1 lfs pull
git -C ../Demon-Stage2 lfs pull
git -C ../Demon-Stage3 lfs pull
```

Open each folder separately in Unity Hub with the stated Editor version and the scene listed in the table. Each folder has its own imported project state. These Git commands require a real repository checkout; they do not work in a plain source ZIP without Git metadata. For an offline submission, provide independently runnable source copies and identify their locations in this README when packaging them.

## Class Maps and extensibility evidence

The [three-stage evidence index](Docs/Diagrams/Stages/README.md) contains full commit identifiers, functional boundaries, Class Maps and the change-analysis method.

| Evidence | Location |
| --- | --- |
| Stage 1 core Class Map | [PNG](Docs/Diagrams/Stages/stage-1-core.png), [SVG](Docs/Diagrams/Stages/stage-1-core.svg), [editable Mermaid](Docs/Diagrams/Stages/stage-1-core.mmd) |
| Stage 2 core Class Map | [PNG](Docs/Diagrams/Stages/stage-2-core.png), [SVG](Docs/Diagrams/Stages/stage-2-core.svg), [editable Mermaid](Docs/Diagrams/Stages/stage-2-core.mmd) |
| Stage 3 core Class Map | [PNG](Docs/Diagrams/Stages/stage-3-core.png), [SVG](Docs/Diagrams/Stages/stage-3-core.svg), [editable Mermaid](Docs/Diagrams/Stages/stage-3-core.mmd) |
| Stage 1 to Stage 2 change map | [Added, modified, renamed and unchanged code](Docs/Diagrams/Stages/stage-1-to-2-changes.md) |
| Stage 2 to Stage 3 change map | [Added, modified and unchanged code](Docs/Diagrams/Stages/stage-2-to-3-changes.md) |
| Current implemented architecture | [ARCHITECTURE.md](Docs/ARCHITECTURE.md) |

The model separates plain C# rules from Unity scene/input/presentation components. Later extensions reuse rules while adding shared interaction contracts or targeted state capture/restore. Reusing a modified class is distinguished from a byte-identical file. The maps intentionally show core relationships; omitted classes are not necessarily deleted. The historical Stage 3 map must be updated if later local changes become the final assessed version.

## GitHub development evidence

| Evidence | Link / status |
| --- | --- |
| Repository | [TruthORZ/Demon](https://github.com/TruthORZ/Demon) |
| Main integration branch | [main](https://github.com/TruthORZ/Demon/tree/main) |
| Existing art branch | [art](https://github.com/TruthORZ/Demon/tree/art) |
| Stage 1 to Stage 2 comparison | [a37036c...b31abc5](https://github.com/TruthORZ/Demon/compare/a37036cd166d246270040bf7f843c9a6c221daa0...b31abc5c65524662a4448b4e0117d486da1d1fa7) |
| Stage 2 to Stage 3 comparison | [b31abc5...b94f864](https://github.com/TruthORZ/Demon/compare/b31abc5c65524662a4448b4e0117d486da1d1fa7...b94f864388d0dd524bdc07cfe165b7b70746711f) |
| Verified art/main integration | [Merge commit 8033386](https://github.com/TruthORZ/Demon/commit/8033386123f4c4269afc877cf41f7cbf988a56bc) |
| Commit history | [Repository commits](https://github.com/TruthORZ/Demon/commits/main/) |
| Personal branches and individual PRs | **To be confirmed for each member** |
| Stage tag URLs | **To be supplied after the required tags exist** |

The existing `main` and `art` links document available branches. They do not replace identification of every student's own branch and contribution history. Preserve meaningful commits and merge/PR evidence. Ensure the assessor can access the repository and required LFS data.

## Complete AI communication records

The current user's complete available project conversations **through the explicitly approved cutoff of 10 November 2026, 24:00 Asia/Shanghai** are included in the project at:

**`Docs/AI-Logs/Current-User/Through-2026-11-10/`**

- [Offline reading directory](Docs/AI-Logs/Current-User/Through-2026-11-10/index.html): download/extract the submitted project and open this file in a browser.
- [All conversations in Markdown](Docs/AI-Logs/Current-User/Through-2026-11-10/all-conversations.md): readable in GitHub or a text editor.
- [All conversations as plain text](Docs/AI-Logs/Current-User/Through-2026-11-10/all-conversations.txt).
- [Full structured JSONL](Docs/AI-Logs/Current-User/Through-2026-11-10/all-conversations.jsonl): message IDs, exact text, timestamps, source references and underlying assistant reply text.
- [Export manifest](Docs/AI-Logs/Current-User/Through-2026-11-10/manifest.json) and [verification receipt](Docs/AI-Logs/Current-User/Through-2026-11-10/independent-verification.json).

The archive includes six conversations, **96 user messages, 388 AI replies/progress messages, 16 interactive questions and 120 image-display events**, with **130 unique attachment files**. The last included event is **10 November 2026, 23:59:36.394 +08:00**. Chinese dialogue remains in its original language. Messages have not been replaced by summaries or translated reconstructions.

Two originally attached files, `策划案.pdf` and `恶魔语言系统 v0.2.pdf`, could not be recovered at their original locations. Their references remain in the dialogue and the manifest identifies the missing originals. Embedded historical images were recovered where available; other local attachment copies are explicitly distinguished from historical embedded bytes.

The export preserves user-visible dialogue, interactive questions and available images. It excludes internal reasoning, system/developer instructions, automatically injected environment/context and internal command/output traces. The exported conversation is evidence of AI communication; actual implementation changes should also be traced through Git.

This is complete **within the approved cutoff**, not through the later final submission date. Later work and the chats created after that cutoff are outside this bundle. If the assessed submission includes later AI-assisted work, add its complete chronological record as an explicitly dated addendum. The second member's AI record or no-use declaration, and the mapping of `Current-User` to a named student/personal branch, remain **to be confirmed**.

## Build and verification

### Build a Windows player

1. Open the intended submitted source revision and `DemonNineDay_Art.unity` in the matching Unity Editor.
2. Open **File > Build Profiles**, choose Windows and the 64-bit architecture, and ensure the intended main scene is the first enabled scene for the build.
3. Build to a separate output folder. Launch the generated executable and verify the entry scene, input, UI and route.
4. Distribute the **entire build output folder**, including its data directory and required DLLs. An executable alone is insufficient.

An older local candidate exists at `Builds/WindowsCompletePlaythrough/Demon.exe`. Its filesystem date is 11 November 2026, so it must not be presented as a newly built copy of the later local source. Build outputs are ignored by Git and may not be included in a source clone. Rebuild the actual final submitted revision before demonstration.

### Run rule tests and inspect recorded checks

In Unity, open **Window > General > Test Runner**, select EditMode, and run the `Demon.Slice.Tests` tests under `Assets/Demon/Tests/EditMode`. Also load the intended saved scene and exercise the demonstration cases above.

Historical verification records include:

- [TASK-016: focused SOLID refactor and nine-day regression](Docs/Tasks/TASK-016-solid-refactor.md).
- [TASK-022: day-jump and session reconstruction](Docs/Tasks/TASK-022-day-jump-menu.md).
- [TASK-024: journal, retry and flashlight/input verification](Docs/Tasks/TASK-024-inventory-flashlight-clock.md).
- [Art/main merge verification](Docs/Tasks/DEMON-ART-MERGE-20261112.md).

These are dated evidence for their own revisions. Preparing this README did not rerun Unity tests or create a fresh game build. Automated rule/scenario checks do not establish a complete continuous manual nine-day playthrough or final Windows-player acceptance.

## Known limits and final submission checklist

The daily retry checkpoint is held in the running process. It is not a persistent save across application restarts. Some voice, audio or narrative presentation may remain explicitly identified placeholders. The formal game UI is in English. The AI records retain their original languages. Consult the [latest task handoff](Docs/SESSION_HANDOFF.md) for unresolved integration and human-verification items.

Before one member submits `AP6412_GroupXX.zip` through NTULearn, the group must complete the following:

- [ ] Replace `XX` and all member/name/ID/account/branch placeholders with confirmed details.
- [ ] Include the actual final runnable Unity source, scripts and essential assets with their `.meta` files. Preserve `Packages` and `ProjectSettings`.
- [ ] Provide independently runnable Stage 1, Stage 2 and Stage 3 versions and the required frozen tag URLs. Identify any offline source folders here when packaging them.
- [ ] Reconcile the historical stage evidence with the lecturer's actual requirement-release and freeze arrangements, and retain the lecturer's Unity 3D approval.
- [ ] Include **one group report in PDF**, covering the game overview, original design/assumptions, Class Maps, extensibility/change maps, implementation/testing, GitHub/AI evidence and one named Reflection subsection per student. The report PDF has not yet been supplied in this project.
- [ ] Include the group's signed **Declaration of Original Work**, using the AP6412 form.
- [ ] Include a **Declaration on Use of Generative AI Assistance for each student**, using the AP6412 form. The signed forms have not yet been supplied in this project.
- [ ] Confirm the AI-log cutoff covers the assessed work, add any needed later records, and place each student's records in that student's branch or a clearly linked repository location.
- [ ] Publish/review the intended final source and evidence, check assessor access and LFS retrieval, and make the final Class Map match that source.
- [ ] Rebuild and test the final submitted game; rehearse normal play, invalid-action handling, daily retry and the defined ending.
- [ ] Prepare the approximately 15-minute presentation and live game demonstration, with every member speaking about their own contribution. Open the actual repository evidence and at least one real AI prompt/response exchange.

The submission deadline is the one shown on the AP6412 NTULearn assignment page. Replace `GroupXX` with the confirmed group identifier. Include this README, `Assets`, `Packages`, `ProjectSettings`, the relevant `Docs` evidence and required report/declarations in the submission. Unity caches such as `Library`, `Temp`, `obj` and machine-specific `UserSettings` are not required to open a correctly packaged source project. Keep essential source assets and `.meta` files intact.

## Project documentation

- [Game specification](Docs/GAME_SPEC.md): intended gameplay and accepted decisions.
- [Architecture](Docs/ARCHITECTURE.md): implemented modules, contracts, assets and historical verified procedures.
- [Stage evidence](Docs/Diagrams/Stages/README.md): checkpoint maps and transition analyses.
- [AI evidence index](Docs/AI-Logs/README.md): scope, ownership placeholders and reading paths.
- [Submission README preparation record](Docs/Tasks/TASK-035-submission-readme-and-ai-records.md).
