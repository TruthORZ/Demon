# Unity Game Development — Agent Working Guide

This file defines durable working rules for Codex and other coding agents in a Unity repository. Place it at the repository root, alongside `Assets`, `Packages`, and `ProjectSettings`. If repository and Unity project roots differ, record the Unity project path in `Docs/ARCHITECTURE.md`.

**Objective:** deliver working game features with a stable architecture, recoverable project context, and assets that designers and artists can edit in Unity without rewriting code.

`MUST` means required. `SHOULD` means the default; document a concrete reason for an exception. These are project working agreements, subject to higher-priority instructions and the user's current authorized scope.

## 1. Start each task with evidence

Before changing files:

1. Read the applicable repository instructions, then follow the document routing in section 2. Do not assume another conversation's memory is complete.
2. Inspect Git status and the relevant diff. Identify existing human changes and preserve them. If Git is unavailable, state that limitation and avoid destructive edits.
3. Inspect the actual implementation, relevant scenes/prefabs, installed packages, and existing tests before proposing replacements.
4. Establish the Unity version from `ProjectSettings/ProjectVersion.txt`, packages from `Packages/manifest.json` and its lockfile, and the target platform from documented requirements and project settings. Check the existing render pipeline, input system, UI framework, and assembly boundaries.
5. State the intended behavior, acceptance criteria, smallest affected area, and verification approach. For a small fix, a brief update is sufficient. For substantial work, create or update a task record before implementation.

Use APIs and package documentation matching the installed versions. Do not assume the newest Unity documentation applies. Never invent tool access, Editor state, package availability, or successful test results.

Proceed autonomously with reversible work inside the request. Ask a focused question only when missing information would materially change gameplay, compatibility, cost, or an irreversible action; continue independent work meanwhile. Routine implementation choices do not need repeated approval. Explicit authorization already given remains valid.

## 2. Document ownership and retrieval

Keep the original five-document division below. Each fact has one authoritative home; other files link to it instead of copying it.

| File | Owns | Update trigger |
| --- | --- | --- |
| `AGENTS.md` | Durable work rules, coding constraints, validation policy, prohibited practices | The working agreement changes; not after every task |
| `Docs/GAME_SPEC.md` | Intended gameplay, core loop, terminology, scope/MVP, accepted design decisions, non-goals | A gameplay/design decision changes |
| `Docs/ARCHITECTURE.md` | Current implemented module responsibilities, dependency direction, key contracts, composition, asset locations, project setup and verified commands | Implemented structure, contracts, setup, or commands change |
| `Docs/Tasks/<task-id>-<slug>.md` | One substantial task's scope, acceptance criteria, plan, decisions, important failed attempts, touched areas, validation and remaining work | Start, meaningful checkpoint, blocked state, or completion |
| `Docs/SESSION_HANDOFF.md` | Latest resumable state: active task links, working-tree state, blockers, next concrete action and verification gaps | End of meaningful work, pause, thread change, or before a context limit |

If `HIERARCHY.md` exists, retain it as a short index pointing to these owners. It is not a second rulebook and does not need a separate change history. Do not create more tracking files unless the current structure has a demonstrated limitation.

### Reading order

- Every new session: applicable instructions → `HIERARCHY.md` if present → `SESSION_HANDOFF.md` if present → linked active task(s).
- Gameplay changes: read the relevant `GAME_SPEC.md` sections.
- Implementation changes: read relevant `ARCHITECTURE.md` sections and the actual code/assets. Use the project profile and verified commands there.
- Search task titles/headings and affected paths before repeating an investigation. Read the matching records, not the entire archive.

### Disagreements and missing documents

- Current explicit user decisions supersede older project decisions within the authorized scope; update their owning document. Do not silently edit this guide to bypass a rule.
- `GAME_SPEC.md` describes intended behavior; repository code/assets show actual behavior. A difference is a discrepancy to resolve, not permission to rewrite the spec to match a bug.
- `ARCHITECTURE.md` must describe implemented structure. Keep proposed changes in the active task until implemented and checked.
- Handoff notes and task proposals do not silently override accepted design or architecture. Flag material conflicts and resolve them using the current request and evidence.
- Create missing documents only when needed. Mark unknowns as unknown and distinguish **observed**, **decided**, and **proposed** information. Do not invent a game design or migrate an established folder layout to fit this guide.

## 3. Preserve architecture through scoped changes

Before a substantial change, identify the affected module, its public contract, incoming/outgoing dependencies, and serialized assets that consume it. Record this impact assessment in the task file.

- Extend the existing feature/module before creating a parallel system. Search for equivalent code, tools, and assets first.
- Preserve unrelated behavior and established dependency directions. Do not replace the architecture, introduce a framework, or conduct a broad cleanup as a side effect of a feature request.
- Keep refactoring local and necessary. If a wider change is essential, explain the concrete limitation, affected consumers, migration, and rollback before proceeding within authorization.
- Treat public APIs, serialized fields, asset GUIDs, prefab hierarchies, persistent UnityEvent bindings, scene identifiers, and save-data schemas as compatibility surfaces. A successful C# compile does not prove these survived.
- Use an explicit composition point for wiring dependencies. Avoid a growing global manager, hidden service locator, or global event bus as a shortcut for all system communication.
- Preserve useful existing patterns. If the existing architecture is harmful, improve the smallest relevant boundary and record the remaining debt; do not reproduce a clear defect simply for consistency.

### Apply SOLID practically

| Principle | Required practice | Review question |
| --- | --- | --- |
| Single Responsibility | Give each class/component a coherent reason to change. Separate game rules, presentation, persistence, and input when they evolve independently. | Does changing UI layout require editing combat or save logic? |
| Open/Closed | Put expected variation into configuration, composition, or an established extension point. Refactor repeated branching when real variation warrants it. | Can the next similar content item reuse the system without unrelated edits? |
| Liskov Substitution | Implementations preserve the contract's preconditions, outcomes, and lifecycle assumptions. Avoid subclasses that reject normal base operations. | Can callers use each implementation without subtype-specific exceptions? |
| Interface Segregation | Expose small capability contracts based on actual consumers. | Is a consumer forced to depend on methods it never uses? |
| Dependency Inversion | Put abstractions at meaningful boundaries between policies and replaceable implementations, such as game rules and save storage. Wire implementations explicitly. | Can a gameplay rule be verified without constructing unrelated UI or infrastructure? |

Prefer composition over deep inheritance. Keep testable rules in plain C# where useful and use `MonoBehaviour` for engine integration. This is a default, not a requirement to wrap every Unity component or remove all `UnityEngine` value types from game logic.

Do not create an interface for every class, a DI framework, a universal event system, or speculative support for features outside the agreed scope. An abstraction must solve a current coupling, testing, or variation problem. SOLID does not require needless indirection.

## 4. Choose engine features before custom replacements

Use this selection order, considering suitability rather than blindly choosing the first option:

1. Existing project components, prefabs, tools, and installed packages.
2. Suitable Unity components and supported official packages compatible with the project.
3. A vetted third-party tool when it materially reduces maintenance or production effort.
4. Custom code for missing behavior or where the alternatives are unsuitable.

This order is an evaluation process, not authorization to download or install packages. Use already approved tools directly. Before introducing a dependency outside the authorized scope, present its purpose, version compatibility, license/cost, runtime/platform implications, and a fallback. Do not switch the render pipeline, input system, UI framework, or Unity version merely because another option is easier to generate.

Typical choices to evaluate include Colliders/Rigidbody for physics, Animator/Animation Clips for animation, Timeline for authored sequences, the existing camera package, AudioMixer for mixing, and the project's navigation solution. A small custom behavior is reasonable; rebuilding a mature subsystem needs a concrete justification.

## 5. Deliver editable Unity content

**Default: code implements behavior; saved assets and exposed data own authored content.** A teammate must be able to open, inspect, adjust, and save the result through the project's normal tools.

| Content | Preferred authoring surface | Appropriate runtime code |
| --- | --- | --- |
| UI | Existing uGUI scenes/prefabs or UI Toolkit UXML/USS assets editable in UI Builder | Present data, bind actions, change state, instantiate reusable item templates |
| Levels and encounters | Saved scenes, prefabs/variants, layout and spawn configuration | Run encounter logic; spawn from authored definitions |
| Balance and reusable definitions | Serialized component fields, ScriptableObject assets, or the established data pipeline | Read definitions; hold per-instance runtime state separately |
| Animation and sequences | Animation Clips, Animator assets, Timeline where appropriate | Select states, trigger actions, coordinate gameplay |
| Audio and visuals | Assigned clips, mixers, materials, particles and other editable assets | Trigger playback and parameter changes |

Follow the existing UI system. For a new project without one, choose based on requirements and version compatibility, and record the reason; this guide mandates no universal UI framework.

### UI and content constraints

- Do not build an entire static screen, level, or asset library in `Awake`, `Start`, or a hidden bootstrap script solely because text generation is convenient.
- Avoid hardcoded layout coordinates, colors, text, timings, balance values, and asset paths when people need to tune them. Expose the appropriate values with useful names, sensible defaults, and units/tooltips where helpful.
- Do not overwrite Inspector values or authored transforms at startup without a documented gameplay reason. Make the ownership of animated/runtime-driven properties clear.
- Dynamic lists, procedural gameplay, generated meshes, pooling, and temporary debug views may require runtime generation. Prefer authored templates and data; document the exception and its editable controls.
- Use ScriptableObjects for shared definitions where appropriate. Do not accidentally use a shared definition asset as mutable per-character state or assume runtime asset mutations are a player-save system.
- Keep content references explicit and inspectable. Separate placeholder/debug content from deliverable assets and identify placeholders in the task record.

### Editor automation is encouraged when it preserves authorship

Codex may write Editor tools or use an available Unity integration to create scenes, prefabs, UI, and configuration assets. Generated assets must be saved to disk and editable outside Play Mode.

- Keep Editor tooling separate from player code through the appropriate `Editor` folders and/or assembly definitions.
- Prefer Unity Editor APIs for Unity-serialized assets. Do not fabricate scene/prefab YAML when a supported Editor route is available. If a focused text edit is necessary, preserve IDs and validate it in Unity.
- An authoring tool must use explicit targets and safe rerun behavior: no duplicate creation, silent deletion, or overwriting human edits. For intentional regeneration, identify the owned output and overwrite behavior before running it.
- Use Undo for interactive scene edits where supported; save intended assets/scenes and preserve prefab links and overrides. Do not use `ExecuteAlways` or automatic initialization as an unbounded regeneration mechanism.
- The generator is a tool, not an invisible source of truth that destroys subsequent manual changes. Keep it only if it serves a useful maintenance workflow.

If Unity/Editor automation is unavailable, implement what can be verified locally and give exact remaining Editor steps: asset paths, hierarchy locations, components, field assignments, and expected results. Prefer a safe Editor utility over a long sequence of repetitive manual actions. Label the feature **awaiting Editor integration/verification** rather than claiming completion.

## 6. Unity implementation and asset integrity

- Preserve `.meta` files and GUIDs. Move/rename assets with their metadata, preferably through Unity. Never regenerate metadata or delete `Library` as routine problem-solving.
- Preserve scenes, prefab variants, and intentional instance overrides. Do not apply/revert all overrides or resave unrelated scenes. Check the correct source asset or instance before editing.
- Prefer private serialized fields for designer configuration and explicit APIs for controlled runtime changes. Unity's standard serializer does not directly serialize interface fields, ordinary properties, or arbitrary dictionaries; use a supported representation and validate it rather than assuming it works.
- When renaming serialized fields, use an appropriate migration such as `FormerlySerializedAs` where applicable. Class/type/assembly changes and save schemas may need separate migrations. Verify representative existing assets, values, and bindings after migration.
- Use explicit serialized references for stable authored dependencies. Local component lookup is acceptable when ownership is clear; avoid global name searches and repeated discovery in hot paths. UI Toolkit element queries are normal; keep their names/contracts stable and validate required elements.
- Pair event subscriptions with appropriate cleanup and handle enable/disable, destruction, scene reload, and pooled-object reuse. Do not rely on accidental script execution order or stale static state.
- Fail clearly for missing required references. Do not hide incorrect setup behind blanket exception handling, silent no-ops, or runtime creation of substitute managers.
- Keep runtime assemblies free of `UnityEditor` dependencies. Respect existing assembly definitions; avoid circular references and broad assembly reorganizations for a local feature.
- Follow existing naming and formatting. Use comments for intent, constraints, and non-obvious engine behavior. Profile before adding complex optimization; avoid obvious repeated allocations/searches in frequently executed code.
- Keep source assets, metadata, and relevant project/package settings under the established version-control policy. Do not add caches, machine-specific files, credentials, or unrelated generated IDE output.

## 7. Keep durable records without creating paperwork

Create a task record for multi-step/cross-system features, architectural or serialization migrations, investigations with meaningful failed attempts, or work likely to span sessions. A small self-contained fix needs no separate task file unless its context is difficult to recover.

Records must be concise and searchable: use stable task IDs, descriptive headings, repository-relative links, exact class/asset paths, and concrete next actions. Avoid full chat transcripts, chain-of-thought, duplicate specs, and large raw log dumps.

- Record decisions that a future agent might otherwise reverse: the chosen approach, concrete reason, affected boundary, and rejected alternative if consequential.
- Record significant failed attempts with symptoms, action/command, observed result, and what it ruled out. Do not record every harmless typo.
- Before a risky edit, establish a recoverable baseline using the repository's normal version-control/checkpoint practice. Protect pre-existing uncommitted work. Do not create commits automatically unless the user or repository workflow authorizes them.
- Keep the handoff as a current snapshot, linking to task history. Update it after a meaningful checkpoint and before ending a substantive session, including when blocked. A no-change/read-only session needs no artificial handoff update.
- Never claim a checkpoint, commit, test, or migration exists unless verified. Record the actual current commit when available and separately describe uncommitted changes; a commit hash alone is not a complete working-state record.

### Task record template

Use only the sections that materially help the task; add debugging detail when needed.

```markdown
# TASK-<id>: <behavior or problem>
Status: planned | in progress | blocked | awaiting verification | done
Updated: <date with timezone when needed>

## Goal and scope
Expected behavior; explicit non-goals; links to design/architecture owners.

## Acceptance criteria
- Observable behavior and important regression boundaries.
- Required Editor/content editability checks.

## Impact and plan
Affected modules/contracts/assets; dependencies; smallest implementation steps.
Migration and rollback approach if relevant.

## Decisions and investigation
- Decision or failed attempt: evidence/result; reason; affected paths.

## Progress and verification
- Implemented changes and relevant file/asset links.
- Exact check/command; environment; observed result; evidence path if retained.
- Checks not run and why; remaining Editor setup or verification.

## Next action / blocker
One concrete resumable action; prerequisite or decision needed.
```

### Session handoff template

```markdown
# Session Handoff
Updated: <date/time and timezone>
Active task(s): <relative links>
Repository/Unity project: <paths when not obvious>
Branch and HEAD: <verified values, or unavailable>
Working tree: <relevant uncommitted changes; identify pre-existing human edits>

## Current state
Implemented and verified behavior; partial work; missing integration.

## Important constraints
Links to decisions/contracts; any new assumption still awaiting confirmation.

## Verification and blockers
Last relevant checks/results; tests not run; current reproducible error.
Link to detailed investigation instead of copying it.

## Resume here
1. First concrete action, with scene/asset/file or command.
2. Next verification needed to satisfy acceptance criteria.
```

## 8. Validate behavior, integration, and human editability

Use the smallest meaningful checks for the change, plus existing required repository checks. Add automated coverage for important game rules, defects, and public contracts when useful; do not create tests that merely mirror trivial implementation.

| Change | Relevant evidence |
| --- | --- |
| Game rules / algorithms | Focused automated cases for expected, boundary, and failure behavior |
| Component interaction / lifecycle | Unity compilation and applicable EditMode/PlayMode checks; repeated enable/disable or scene transitions where relevant |
| UI / content authoring | Open saved assets; inspect layout and wiring; interact in Play Mode; check representative resolutions/input modes |
| Serialized structure / asset changes | Load representative old scenes/prefabs/data; inspect values, references, overrides, and persistent event bindings |
| Packages / platform / build configuration | Package resolution and a representative target build when available |

For an authored feature, verify a practical human-editability path: identify the saved source asset, change a representative intended tuning/layout property, confirm the change is respected, and restore any temporary verification edit. Check that the authored result persists after leaving Play Mode and reloading the asset when applicable.

Use repository commands where verified. Discover the actual Unity executable, project path, test tooling, and supported runner options before constructing commands. Do not launch a competing Editor against a project already open in Unity. A standalone .NET build or text check is not a substitute for Unity import, serialization, or visual verification.

Check the diff for accidental deletions, unrelated reserialization, missing metadata, temporary assets, dependency changes, and broken links. Run `git diff --check` when applicable. Distinguish baseline failures from new regressions; do not suppress or weaken checks to make the task appear successful.

Report verification precisely: **passed**, **failed**, or **not run**, with the reason. If runtime, visual, or build validation is unavailable, state the remaining checks and leave the relevant acceptance criteria open.

## 9. Finish with a reviewable result

A feature is done only when its behavior meets acceptance criteria, integration is complete, relevant checks have passed, editable assets are saved, and affected documentation is current. Otherwise report its actual partial/blocked status.

Final delivery must state:

1. What changed and where it can be tested: scene, prefab, asset, or entry point.
2. Where a human adjusts layout, content, balance, or other intended controls.
3. Verification performed and any remaining limitations/setup.
4. Handoff/task record location when maintained.

Keep the response proportionate. Do not claim that these guidelines guarantee architecture quality; apply their impact, compatibility, and verification checks to the actual repository.

## 10. Initial project documentation

On first substantive implementation, populate the relevant portions of `Docs/ARCHITECTURE.md` from inspection. Keep project-specific facts there rather than expanding this general guide:

- Unity project location, exact Editor version, platform targets and constraints.
- Render pipeline, UI/input systems, important packages and version sources.
- Existing folder/assembly boundaries, module responsibilities and dependency direction.
- Startup scene/composition entry points and important scenes/prefabs/data assets.
- Verified setup, compile/test/build commands or Editor procedures; explicitly unknown commands.
- Serialization/save compatibility constraints and concise consequential decision records.

Create/update `GAME_SPEC.md` from supplied requirements and confirmed decisions. Put unresolved design choices in a clearly marked questions/proposals section. Start with the smallest playable slice and validate it before multiplying content or building generalized infrastructure.

## Reference documentation

These references explain tool/engine behavior; the project policies above are working agreements. Use the Unity documentation version that matches the repository; the links below use Unity 6.0 as examples, not a mandated project version.

- [Codex project instructions with AGENTS.md](https://learn.chatgpt.com/docs/agent-configuration/agents-md)
- [Unity UI authoring: uGUI and UI Toolkit](https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-Transitioning-From-UGUI.html)
- [Unity prefab instance overrides](https://docs.unity3d.com/6000.0/Documentation/Manual/PrefabInstanceOverrides.html)
- [Unity serialization rules](https://docs.unity3d.com/6000.0/Documentation/Manual/script-serialization-rules.html)
