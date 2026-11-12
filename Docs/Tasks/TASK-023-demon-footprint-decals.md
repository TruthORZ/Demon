# TASK-023: Demon chase terrain footprint decals
Status: done
Updated: 2026-11-12 Asia/Shanghai

## Goal and scope
User requests footprints for the actual pursuing demon, not the player. Integrate the existing standalone URP decal emitter into DemonNineDay_Art. Preserve GlobalSnow, TerrainData/material, navigation data, movement/combat rules, player code and older practice scenes.

## Impact and decisions
- ChaseController publishes its existing audio step and session reset events. No dependency from Demon.Slice to the decal module.
- New DemonFootprints adapter under Assets/SnowSystem/FootPrint subscribes explicitly to the saved Chase reference; stamps alternate feet on the native Terrain, with Automatic Steps disabled. Reset clears old trails.
- Save explicit scene references using Unity Editor APIs; enable Decal Renderer Feature on the existing PC renderer without changing rendering mode or other features.
- Replace only the two moving SnowPrint placeholder renderers with stationary decal trails; keep target root, aim region, navigation, audio and chime bindings.
- Existing FootPrint sources are pre-existing untracked work from the previous authorized task. Preserve their GUIDs and materials.

## Acceptance
- Main scene reloads with valid references; player has no emitter; no GlobalSnow modification.
- Actual NavMesh pursuit produces alternating fixed prints; D1, frozen, stopped, paused and reset states do not create stray prints. Session restart clears trail.
- Unity compilation, runtime checks, actual scene visual capture and editable parameter save/reload check.
- Review diffs for unrelated scene reserialization and settings changes.

## Baseline
main at b31abc5c65524662a4448b4e0117d486da1d1fa7. Before this task only Assets/SnowSystem/FootPrint and its meta were untracked. Unity 6000.6.0f1, URP 17.6, Windows DX11. No Unity instance was running at start.

## Verification and evidence
- Unity batch entry `StandaloneTerrainFootprints.Editor.DemonFootprintVerification.RunBatch` compiled, authored/saved/reloaded the production Art scene and verified a saved footprint-size edit, restoring .4/.85 afterwards.
- Play Mode exercised real GameSceneController, DayJumpMenu D3 replay, ChaseController and saved NavMesh. Player movement in D1 produced zero prints. Actual D3 chase stamped alternating feet; prior prints stayed fixed.
- Production motion-rule lock drove ChasePhase.Frozen without added prints; release resumed. Disabled chase and paused menu produced no prints. JumpToDay(1)/Restart cleared the trail and remained silent. Freeze was tested through StableBeamMotion with BeamTether input polling temporarily disabled, not manual mouse aiming; no runtime overrides were saved.
- Actual PC renderer On/Off captures inspected: shoe decals visible on the original snow Terrain. Shader checks passed. Evidence: `Logs/DemonCheckpoints/task023-footprints-20261112/verification.txt`, `DemonFootprints-On.png`, `DemonFootprints-Off.png`.
- Unity whitespace noise was removed. Original scene IDs retained: only two legacy print Renderer flags and one parent child-list change, plus four Unity-created records. Original PC renderer/SSAO fields retained; only Decal subasset, reference and recovery map added. Final saved files loaded and runtime-verified again via `VerifyExistingBatch`: PASS.
- Existing EditMode rules: 81 passed, 0 failed/skipped. Result: `Logs/DemonCheckpoints/task023-footprints-20261112/editmode.xml`. `git diff --check` passed. Restored Unity's automatic TimeManager migration to its clean baseline; no gameplay timing change intended.
- Windows build and extended manual keyboard/mouse play were not run. Old practice scenes remain unmodified; this integration is for DemonNineDay_Art.

## Test and tuning
Open Art scene, Play, F8 → D3, then walk outside. Search Hierarchy for `Demon Terrain Footprints`; tune size, lateral offset, lifetime and capacity there. Automatic Steps must remain off; cadence comes from ChaseDefinition.FootstepSeconds. Player has no emitter. D2 static human tracks and T06 aim region remain.

## Recovery
Remove the new scene object, re-enable SnowPrint_0/1 Renderers and remove the added Decal Feature if unused by other content. Original scene and PC renderer snapshots are in the evidence folder. Chase events are optional and do not change unconnected practice scenes. No commit/push was performed.
