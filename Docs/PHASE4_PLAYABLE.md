# Phase 4 — Connected playable story route

Run **Tools > NULL SIGNAL > Build Phase 4 Main Game** outside Play Mode. The menu generates and opens `Assets/Scenes/MainGame.unity`; if that name already exists it preserves it and creates a numbered copy. Press Play and focus the Game view. Previous prototype scenes are never saved over.

## Controls and fixes
- WASD/arrows move; Space dodges; E interacts with the nearby console/device.
- Q scans. Top-row **1/2/3** and **numpad 1/2/3** both mark/amplify/lock. R resets the current quantum encounter; F3 toggles separate diagnostics. HUD buttons use the same availability rules.
- Input fix: Unity's `digit1Key` property addresses control path `<Keyboard>/1`, not `<Keyboard>/digit1`. Corrected all three top-row bindings. No input settings or packages changed.
- Web/mobile URP render scale was 0.8; now 1.0 with 2x MSAA. MainGame has a scene-owned full-resolution forward URP profile, restored on exit, with no stacked post AA or dynamic resolution. Bloom is restrained; normal gameplay has no depth of field, motion blur or fog haze. The old Astra7 bloom profile is reduced too.
- HUD uses 1920x1080 scaling, pixel alignment and larger existing Unity dynamic-font text. Inspect at a native Game-view resolution/1x scale; an editor preview enlarged from a smaller resolution can still look soft.

## Route and teaching beats
Each chamber is 24x24 m. Centers are 28.8 m apart eastward, joined by physical service passages and animated gates. A continuous structural underdeck extends beyond the camera bounds. MainGame uses the Phase 3 kit, materials, procedural player and amplitude-driven nodes.

| Area | Player action | Result / checkpoint |
| --- | --- | --- |
| Maintenance | Walk to the cyan manual-release console and press E; avoid telegraphed falling panel with Space | Door opens; first hazard deals only 15 damage |
| ANVESH room | Recover the device with E; Q scans four power modules | SCAN unlocks; assisted real MARK, AMPLIFY and weighted LOCK restore power |
| Authentication | Q, then 1, then 2, then 3 | MARK, AMPLIFY and LOCK unlock successively; successful measurement opens bulkhead |
| Oracle Lab | Watch moving Ananya hologram and real phase demonstration while retaining movement; repeat Q then 1 | Phase marking opens the next route |
| Amplification Lab | Compare eight nodes; use MARK before each AMPLIFY; decide when to LOCK | Real successful measurement after amplification opens reactor access |
| Overshoot Reactor | Iterate through a probability decline and a subsequent rise; then LOCK | Successful weighted measurement stabilizes the reactor and ends Phase 4 |

Every AMPLIFY still requires a preceding MARK. The original simulation calculates everything. For N=8, target success naturally falls after the second iteration's peak; continue cycling to observe recovery. The reactor does not reset on overshoot. A missed measurement can be retried with R. Previously opened routes remain open. Checkpoints are local, in-memory room entrances; death restores health and the current encounter without replaying previous rooms.

Subtitles support speaker/text/duration, queued playback, named play-once triggers and non-pausing gameplay. ANANYA and LUBNA are simple gesturing, moving 3D holographic silhouettes. Door servo and device pulse audio are generated PCM placeholders, not voiceovers. Health is actual player state, with short invulnerability and quick local respawn.

## Verification scope
Input hotfix compiled first. Final verification is runtime/editor compilation and quick static review only, per request. No automated Play Mode run, input simulation, screenshots, preservation hashes or WebGL build. The menu has not been run automatically; it builds the scene and wires every reference in Unity. Human inspection is still needed for navigation, framing, pacing, audio levels and normal focused keyboard behavior. Target playtime has not been measured.

No Chhaya, combat, Vault, final cinematics, main menu, credits or Phase 5 implementation.

## Files
Modified: `Assets/Scripts/Gameplay/AnveshController.cs`; `Assets/Scripts/Presentation/AnveshHUD.cs`; `Assets/Scripts/Presentation/AmplitudeScopeGraphic.cs`; `Assets/Scripts/Editor/NullSignalPhase3HUDBuilder.cs`; `Assets/Scripts/Editor/NullSignalPhase3Builder.cs`; `Assets/Settings/Mobile_RPAsset.asset`; `Assets/Art/Astra7/Astra7 Atmosphere.asset`; `Docs/ART_DIRECTION.md`.

New scripts (each with `.meta`):
- `Assets/Scripts/Player/PlayerHealth.cs`
- `Assets/Scripts/Gameplay/DoorController.cs`
- `Assets/Scripts/Gameplay/StoryInteractable.cs`
- `Assets/Scripts/Gameplay/StationHazard.cs`
- `Assets/Scripts/Story/ObjectiveSystem.cs`
- `Assets/Scripts/Story/CheckpointSystem.cs`
- `Assets/Scripts/Story/StoryQuantumRoom.cs`
- `Assets/Scripts/Story/MainGameDirector.cs`
- `Assets/Scripts/Presentation/SubtitleController.cs`
- `Assets/Scripts/Presentation/StationFeedbackAudio.cs`
- `Assets/Scripts/Presentation/StationCamera.cs`
- `Assets/Scripts/Presentation/GameplayRenderSettings.cs`
- `Assets/Scripts/Presentation/HologramPerformance.cs`
- `Assets/Scripts/Presentation/StoryHudOverlay.cs`
- `Assets/Scripts/Presentation/StationFlicker.cs`
- `Assets/Scripts/Presentation/StationPowerState.cs`
- `Assets/Scripts/Editor/Phase4StoryContent.cs`
- `Assets/Scripts/Editor/NullSignalPhase4Builder.cs`

This document is `Docs/PHASE4_PLAYABLE.md`. Temporary compilation responses/DLLs are under `Temp/Phase4Compile/`. The menu creates MainGame and its two Phase 4 render/atmosphere assets, while reusing the existing Astra7 kit.
