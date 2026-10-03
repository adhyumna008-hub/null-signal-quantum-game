# Phase 3 — Astra-7 visual prototype

## Next Unity action
Run **Tools > NULL SIGNAL > Build Phase 3 Visual Prototype** outside Play Mode. The builder creates and opens `Assets/Scenes/VisualPrototype.unity` (a numbered copy if it already exists). Then press Play and click the Game view to give it keyboard focus.

Controls: Q SCAN; 1 MARK; 2 AMPLIFY; 3 LOCK; R RESET; F3 developer HUD. Numeric keypad 1/2/3 also work. WASD/arrows move, Space dodges, E inspects. Each new amplification iteration still requires MARK then AMPLIFY. HUD buttons remain available.

## Delivered
- Small input hotfix: explicitly enabled Input System actions capture presses and queue them for gameplay updates. F3 uses the same event path. Actions are disabled/disposed with their controller, and queued input clears on focus loss. No project input settings or packages changed. Existing Phase 2 button callbacks remain intact.
- New scene builder creates one real 3D laboratory with beveled floor/wall modules, copper jaali vents, trims, pillars, doors, consoles, railings, light strips, hologram pedestals and candidate platforms. Ten module prefabs plus shared meshes/materials are saved under `Assets/Art/Astra7/` when the menu runs.
- Rear apparatus and four readable candidate positions, floor circuits, cutaway foreground, bounded isometric follow and gentle encounter zoom.
- Nodes use actual signed amplitudes for direction, core size, wave height, density and intensity; actual probabilities for arcs; actual measurement for collapse and survivor feedback. No target-specific color or replacement quantum math.
- Compact ANVESH identity/objective/actions, live signed waveforms and per-state probabilities, iteration count and overshoot warning. F3 diagnostics are separate and initially hidden. No fabricated health/energy/system telemetry.
- Procedural humanoid silhouette with suit, harness, backpack, face/hair and wrist device; restrained stride animation.
- Shared URP materials, combined static module detail, reused meshes, at most 24 small low-poly motes per node, one shadowed directional and three shadowless practical lights. Small scene-local bloom, vignette and fog. No new packages or project-wide render changes.

## Verification and limits
The keyboard hotfix compiled before visual implementation. The completed runtime and editor assemblies were compiled against the project's installed Unity 6, Input System, uGUI and URP references. Static review covered references, prefab/scene serialization, mesh winding, lifecycle cleanup, input handling and read-only use of the original quantum system.

No Play Mode input simulation, screenshots, preservation hashes, automated gameplay tests or WebGL build were run in this phase. Visual composition, normal focused Game-view keyboard input and performance require the human Unity check. The scene and generated visual assets are created by the menu, rather than a batch editor session. Doors/consoles are visual kit pieces only. The character remains a stylized placeholder. Existing Phase 1/2 scenes and all quantum simulation files are untouched.

## File changes
Modified:
- `Assets/Scripts/Gameplay/AnveshController.cs`
- `Assets/Scripts/Presentation/HUDController.cs`

Created (each script also has its Unity `.meta` file):
- `Assets/Scripts/Presentation/QuantumNodePresentation.cs`
- `Assets/Scripts/Presentation/VisualCameraController.cs`
- `Assets/Scripts/Presentation/PrototypeHumanoid.cs`
- `Assets/Scripts/Presentation/ApparatusMotion.cs`
- `Assets/Scripts/Presentation/AmplitudeScopeGraphic.cs`
- `Assets/Scripts/Presentation/AnveshHUD.cs`
- `Assets/Scripts/Editor/Astra7VisualKit.cs`
- `Assets/Scripts/Editor/NullSignalPhase3HUDBuilder.cs`
- `Assets/Scripts/Editor/NullSignalPhase3Builder.cs`
- `Docs/ART_DIRECTION.md`
- `Docs/PHASE3_VISUAL.md`

Temporary compiler responses and DLLs are under `Temp/Phase3Compile/`; they are not game assets.

Phase 3 stops here. No enemies, combat, levels, cinematics or Phase 4 work.
