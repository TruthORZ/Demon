# DEMON-ART-MERGE-20261112: art into main
Status: verified locally; publication recorded by Git merge commit
Updated: 2026-11-12 Asia/Shanghai

## Inputs and resolution
User authorized merging art into main and publishing. Parents: main 0685e2146673b2cd3f22bb5344e823b4a7659dfc and art d25a332901b556ae996844958e4afc64d1b964a9; common base b31abc5c65524662a4448b4e0117d486da1d1fa7.
Unity 6000.6.0f1 native UnityYAMLMerge combined DemonNineDay_Art.unity. Both branches' additions and deletions are preserved: 3534 unique scene records; no dangling local fileID references. Shared font bindings and parent children include both branches' new references. Native Unity save/reload changed only equivalent formatting/order.
ChaseController auto-merge retains main detection/audio/safe-zone behavior and art step/reset events. Architecture retains main section 22 and adds footprint section 23. GAME_SPEC retains main's D3 apparition timing plus art footprint constraints. Historical handoffs are retained below the current summary.

## Verified
- Scene loads/saves/reloads: 2827 GameObjects, zero missing scripts.
- EditMode rules: 89 passed, zero failed/skipped.
- Nine-day route and daily failure/retry replay passed.
- Main control scene and merged scene both pass original complete-integration assertions in the foreground editor with runtime test-device isolation: E pickup, inventory slot click/close, empty and 30-item overflow, left-click flashlight latch/right-click ignored, retry snapshots, D3 safe zone, D7 night, D8 T wait, T07 window and two-hand clock.
- Real NavMesh pursuit produces alternating stationary decals; freeze, release, disable, pause and restart cleanup passed. On/Off screenshots inspected and decal shader compilation passed.
- Final scene SHA256 F47E725CD5F982C8EB92D421E909462E1688A1A2D18DAACD7766D15B4B51AA30. Chase SHA256 B20BA8016DBDA8568C5FFB3A98B99F0FA48A59EF61C2AD5A8401F355DF52399D.
- Test Editor exited; temporary tools removed; automatic TimeManager formatting restored to the staged baseline. Source resources and metadata retained.

## Conditions and limits
Earlier batch/background input checks failed. Foreground editor and isolated virtual devices passed without changing game code or weakening original assertions. The main scene control used merged runtime code; Chase differs from main only by step/reset events. Footprint verification used a legal outdoor NavMesh setup because main now excludes pursuit inside the cabin; it did not force chase phase/tracking. This does not prove background input stability or full manual playthrough.
Two pre-existing Mountain_03 (11)/(12) LOD0 renderer references and the existing shadow atlas warning remain. Native Unity files retain whitespace diagnostics. No Windows build, complete manual nine-day playthrough, performance benchmark or teammate-machine validation was run for this merge.
Evidence on the integration machine: D:/NTUCourse/12/Snow/Integration/ArtMerge20261112, and dispatch/handoffs/03_DEMON-ART-MERGE-20261112.json. Recovery inputs contain all three original scenes and the validated merged scene.

## Continue
Open Assets/Demon/Scenes/DemonNineDay_Art.unity. Main-route settings and UI are described in ARCHITECTURE section 22; footprint tuning and resources in section 23. Use git log -1 and git status for the publication commit and current branch.