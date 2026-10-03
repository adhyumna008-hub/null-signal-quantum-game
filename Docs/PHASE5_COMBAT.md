# Phase 5 — ANVESH analysis and quantum combat

Run **Tools > NULL SIGNAL > Build Phase 5 Combat** outside Play Mode. The menu now backs up the existing canonical MainGame under `Assets/Scenes/Archive/`, then updates and opens `Assets/Scenes/MainGame.unity`. Other prototype scenes are preserved. Play the newly opened scene and focus Game view. See `PHASE5_1_HOTFIX.md` for the reactor and clarity fixes.

The connected route retains all six earlier rooms, then adds Security Lockdown, a corrupted passage and Chhaya. Controls remain WASD, Space dodge, E interact, Q scan, 1 mark, 2 amplify, 3 lock, R encounter reset, F3 diagnostics. Left mouse fires an aimed defensive pulse: click a visible manifestation; UI clicks do not attack. Range is 18 world units, damage 25, cooldown 0.38 seconds. Walls block damage.

ANVESH actions now resolve over 0.8 / 0.7 / 1.0 / 1.1 seconds. The original encounter operator runs once at 25% of SCAN, 45% of MARK/AMPLIFY, or 35% of LOCK. New quantum commands are rejected during resolution; movement, dodging and enemy attack timers continue at normal speed. LOCK briefly slows candidate presentation motion. Reset, death and room changes cancel pending operations. Existing assisted demonstrations are observed by the same analysis presentation. Normal candidates no longer expose E inspection; E remains for meaningful world devices.

The right panel displays captured real signed amplitudes, the actual diffusion mean, before/after transitions, and the actual weighted measurement result. Animated graph interpolation is presentation only. First-use explanations appear once per play session. F3 diagnostics temporarily hide the analysis panel. No Phase 1 operator or measurement implementation was changed.

Security has four moving drone states. Chhaya forms over three seconds through moving dark fragments and violet effects, then fights with four states followed by eight. Every active state schedules telegraphed ground strikes; leave the marked circle or dodge before impact. Correct measurement opens a five-second damage window. Wrong measurement creates an accelerated hostile decoy for 2.6 seconds, then resets the distribution automatically. Missed exposure windows restore the search while retaining damage. R or death restarts the combat room at full enemy vitality, including Chhaya phase A; player health follows the existing checkpoint rules.

Procedural drones share kit materials and meshes; each manifestation uses twelve small motes, a signed waveform and simple line telegraphs. Chhaya uses a different fragmented humanoid silhouette. Existing animated Ananya/Lubna holograms support the new rooms. Combat framing remains fixed isometric and slightly wider. No new packages or project-wide input settings.

## Files created

- `Assets/Scripts/Gameplay/AnveshOperationController.cs`
- `Assets/Scripts/Gameplay/QuantumCombatEncounter.cs`
- `Assets/Scripts/Gameplay/CombatManifestation.cs`
- `Assets/Scripts/Player/PlayerCombat.cs`
- `Assets/Scripts/Presentation/AnveshAnalysisPanel.cs`
- `Assets/Scripts/Presentation/AnalysisWaveGraphic.cs`
- `Assets/Scripts/Presentation/AnveshOperationPulse.cs`
- `Assets/Scripts/Editor/NullSignalPhase5Builder.cs`
- Matching `.cs.meta` files, plus this document.

## Files modified

- `Assets/Scripts/Gameplay/QuantumEncounter.cs` — read-only target index for analysis.
- `Assets/Scripts/Gameplay/AnveshController.cs` — optional timed action routing and busy gating.
- `Assets/Scripts/Presentation/AnveshHUD.cs` — resolving feedback.
- `Assets/Scripts/Presentation/PrototypeHumanoid.cs` — operation, attack and hit poses.
- `Assets/Scripts/Presentation/StationCamera.cs` — combat framing.
- `Assets/Scripts/Story/StoryQuantumRoom.cs` — optional combat lifecycle.
- `Assets/Scripts/Story/MainGameDirector.cs` — extended progression and cancellation on death.
- `Assets/Scripts/Editor/NullSignalPhase4Builder.cs` — shared optional eight-room construction.
- `Assets/Scripts/Editor/Phase4StoryContent.cs` — short combat entrance/completion cues.
- `Docs/ART_DIRECTION.md` — combat visual language.

## Verification and limits

Runtime and editor sources compile using Unity 6000.6.4f1's C# compiler, project response-file references and warnings treated as errors. Temporary compiler response files and outputs live under `Temp/Phase5Compile/`. Static review covered wiring, operator timing, state counts, collapse/target selection, reset/death, phase progression and door openings.

The editor menu has not been executed automatically. Scene generation, combat balance, mouse aiming, UI fit and final appearance need human inspection in Unity. No automated Play Mode, screenshots, input simulation, hash scans or WebGL build were run. Scope stops after Chhaya's eight-state encounter; no Prime, Vault or later phases.
