# Phase 5.1 hotfix

Run **Tools > NULL SIGNAL > Build Phase 5 Combat** outside Play Mode. The builder backs up the previous canonical MainGame and saves the complete eight-room route to `Assets/Scenes/MainGame.unity`. It opens that scene automatically. No Inspector repair is needed.

## Progression findings and fix

Static inspection found that the Phase 5 scene (`MainGame 1.unity`) has its reactor exit and following Security/Chhaya rooms wired correctly. The older `MainGame.unity` is the six-room Phase 4 route: its reactor has no exit or following room. The previous unique-name builder left those different routes side by side under very similar names. Updating the canonical MainGame removes that ambiguity while archiving the prior scene.

The reactor also required both an observed overshoot and a later natural recovery before allowing LOCK and completion. RESET discarded an observed overshoot unless recovery had already happened. The hotfix retains lesson observations across resets and removes natural recovery as an extra gate. Observe an overshoot once; then either measure the current distribution, continue amplifying, or R/reset and rebuild a strong distribution. Successful real target measurement after at least one iteration completes the reactor. Two iterations after a reset give the normal calculated high-probability region; measurement remains genuinely random.

Room completion is reconciled from the current encounter result, objectives explicitly say EXIT OPEN and name the next sector, and a completed room keeps its exit open. Opening a door immediately disables its blocking collider while the leaves animate. Every generated intermediate room is checked for a wired exit. Crossing the connected eastern passage still advances the director, checkpoint, camera and active quantum context. Combat completion remains based on damaging the correctly measured, vulnerable enemy, with Chhaya expanding to eight before its final room completion.

## Strange object and interaction readability

The reactor's yellow footprint/falling plate is `Reactor service discharge`, a recurring `StationHazard`. It moves the plate through a scripted coroutine and never handles E; it was not ANVESH, a console or a loose Rigidbody decoration. It is removed from reactor scene generation. The intentional maintenance-corridor dodge hazard remains.

ANVESH is a clearly labeled, cyan-screen wrist-interface dock with attached copper cuff parts. The manual console is labeled RELEASE TERMINAL. Neither uses Rigidbody physics. Nearby interaction targets share the same range/selection logic for both E and the HUD prompt, including candidate inspection. Story props use a small cyan emissive proximity pulse. Interacting still unlocks ANVESH/opens the manual door, updates objectives/dialogue, or displays the candidate's actual phase status.

## Rendering and UI

The scene-owned forward URP asset uses render scale 1.0 and 4x MSAA; the camera keeps dynamic resolution and extra antialiasing disabled. Cached rendering assets are refreshed when rebuilding. Motion blur is zero and depth of field remains off. Bloom uses intensity 0.06, threshold 1.8 and scatter 0.2 to limit glow to bright emissive surfaces.

The analysis panel uses TextMeshPro directly, with separate operation/equation/state-change/result regions, four-decimal signed amplitudes and larger value text. Existing HUD bindings are preserved through small SDF-display adapters. Canvas scaling remains 1920x1080, pixel perfect, with no text glow or nonuniform scaling. Inter source font and SIL OFL license are copied from the already-installed Unity core package into the project. The builder automatically creates the SDF atlas and uses included TMP settings; no new Unity package or manual TMP resource import is required. Timed actions, real operators, graph transitions and first-use explanations remain intact.

## Files changed

- `Assets/Scripts/Story/StoryQuantumRoom.cs`
- `Assets/Scripts/Story/MainGameDirector.cs`
- `Assets/Scripts/Gameplay/DoorController.cs`
- `Assets/Scripts/Gameplay/StoryInteractable.cs`
- `Assets/Scripts/Player/PlayerInteraction.cs`
- `Assets/Scripts/Presentation/StoryHudOverlay.cs`
- `Assets/Scripts/Presentation/AnveshAnalysisPanel.cs`
- `Assets/Scripts/Editor/NullSignalPhase4Builder.cs`
- `Assets/Scripts/Editor/NullSignalPhase5Builder.cs`
- `Assets/Art/Astra7/Phase4 Full Resolution URP.asset`
- `Assets/Art/Astra7/Phase4 Restrained Atmosphere.asset`
- `Docs/PHASE5_COMBAT.md`

Created: `Assets/Scripts/Presentation/SharpHudText.cs`, `Assets/Scripts/Editor/NullSignalSharpUI.cs`, `Assets/Art/Astra7/Fonts/Inter-Regular.otf`, `Inter-LICENSE.txt`, `Fonts/Resources/TMP Settings.asset`, matching metadata, and this document. The font atlas, archived scene and rebuilt MainGame are generated when the human runs the menu.

Runtime and editor compilation passed with warnings treated as errors using Unity 6000.6.4f1's compiler and project references. Static review traced room completion, door collider release, next-room entry, combat phase completion, interaction selection and cached rendering configuration. No Play Mode, screenshots, simulated input or WebGL build were run. Scene generation and visual appearance remain for human inspection. No Phase 6 implementation.
