# NULL SIGNAL — Phase 2 playable amplitude amplification

Phase 2 implements one four-candidate quantum encounter. It adds no enemies, Chhaya, combat, levels, cinematics, voiceovers, final art or final UI. All original project assets, Phase 1 code, documents, packages and Unity settings are preserved.

## One next Unity action

Double-click **AmplitudeAmplificationPrototype** in the Unity Project window's **Assets/Scenes** folder. The scene has already been generated and verified. Its player, encounter, candidate visuals, camera, HUD and Input System UI references are wired.

When playing, focus the Game view. Controls:

| Input | Action |
| --- | --- |
| Q | SCAN the active encounter; enable search operations |
| 1 | MARK: use the existing Oracle to invert the target's amplitude sign |
| 2 | AMPLIFY: use the existing diffusion operator; complete one iteration |
| 3 | LOCK: sample the actual distribution, collapse it, report success or miss |
| R | Restart the encounter with equal amplitudes and a newly chosen target |
| F3 | Hide/show development amplitude and probability values |
| WASD / arrows | Existing Phase 1 movement |
| Space | Existing Phase 1 dodge |
| E | Inspect a nearby candidate without selecting or measuring it |

Repeat **1 then 2** for every iteration. At N=4 the first complete iteration reaches 100% target probability. A second complete iteration naturally overshoots to 25%. LOCK is also allowed immediately after SCAN or MARK, using the unchanged probabilities. Measurement never selects the highest-weight candidate deterministically.

The five HUD cards have button handlers for the same operations. In the normal Unity editor, focus the Game view for input. The existing new Input System is used; no settings changes, input-asset setup or package installation are required.

## Implementation

- `QuantumEncounter` owns encounter lifecycle, SCAN gating, legal operation order, iteration reporting, measurement outcome, reset and read-only snapshots. It delegates every quantum operation to the original `AmplitudeAmplificationSystem`, including its `MeasurementSystem` sampling and collapse. Configuration supports N=4, 8 and 16; this phase's world scene has four candidates only.
- `AnveshController` connects temporary keyboard actions to that encounter. It processes one quantum action per frame and uses the existing Input System.
- `CandidateVisualizer` reads actual signed amplitude. Absolute magnitude controls column height, emission, opacity, wave height and flow-particle size. Negative amplitude mirrors the waveform and reverses particle travel. Every candidate uses the same cyan materials; there is no target-index/color branch. Immediately after MARK, color and magnitude stay equivalent. After LOCK, all unmeasured manifestations/waves fade away; empty sockets remain as room fixtures.
- `HUDController` shows action availability, encounter state, iteration count and result feedback. F3 hides raw development values, leaving candidate identifiers and the simple action HUD. After LOCK, the target-probability diagnostic explicitly shows the distribution **before** measurement, while candidate snapshots reflect the collapsed state. Debug display is enabled for this prototype, not a final game UI.
- `NullSignalPhase2Builder` adds **Tools > NULL SIGNAL > Build Amplitude Amplification Prototype**. It creates a separate scene at a unique path and reuses its own shared primitive materials. Repeated generation does not overwrite existing scenes/materials. The original foundation builder remains unchanged.
- `NullSignalPhase2Validation` adds **Tools > NULL SIGNAL > Validate Playable Quantum Encounter** for math/encounter checks. Its explicit batch runner was used for native Play Mode verification; it does not automatically run when the project opens normally.

Bootstrap, FoundationPrototype, SampleScene, build settings, active build target, render-pipeline assets and installed packages were not changed. No scene was added to the build list. There is no runtime Python, native plugin, filesystem or threading dependency.

## Verification completed

Unity version: **6000.6.4f1**, existing URP and Input System packages.

1. Compiled quantum, runtime and editor code against the installed Unity .NET Standard 2.1 assemblies with C# 9 and warnings treated as errors. Runtime compilation included WebGL and new Input System defines. Native Unity subsequently imported/compiled the project without C# errors or warnings.
2. Re-ran all original Phase 1 validation: normalization, Oracle sign inversion, diffusion, probability sums, all supported counts, amplification, overshooting and weighted sampling.
3. Tested the encounter wrapper for all 28 target positions across N=4/8/16, including 12 completed iterations each, order guards, snapshot isolation, collapse and reset. Also performed **9,000 encounter LOCK trials** on uniform states, in addition to the Phase 1 sampling checks.
4. Entered Unity Play Mode and drove actual controller updates with injected Q/1/2/3/R/F3 keyboard events. Verified equal states, unchanged MARK probability/color, mirrored marked waveform, amplification, overshoot, successful and missed weighted measurement, collapse and repeated reset.
5. Checked HUD hit testing and dispatched pointer down/up/click handlers to each of the five buttons; verified their actual encounter effects.
6. Exercised W movement, Space dodge and E proximity inspection in Play Mode. Inspection did not measure the state.
7. Captured and visually inspected representative states. The evidence shows equal cyan columns, a phase-inverted wave without a magnitude/color reveal, a taller amplified signal, natural weakening after overshoot, and only the measured manifestation remaining after LOCK.
8. Compared SHA-256 hashes of **all 130 original files** in Assets, Docs, Packages, ProjectSettings and AGENTS.md: every original file remained byte-for-byte unchanged.

The batch runner temporarily cloned input settings in memory to let injected keyboard input reach a hidden Game view. It restored the original settings before exit and saved no input-setting changes. A controlled random sample of 0.99 was used **only by the editor test fixture** to reproduce both success and miss cases through the real sampler. The playable scene uses normal `System.Random` and randomizes the target on restart.

**Limits:** physical mouse interaction was not verified; native mouse-event injection was unreliable in the hidden batch editor, so the HUD checks used real UI raycasts and pointer-handler dispatch. A WebGL player build was not run. Compilation with WebGL defines does not establish browser behavior or performance.

## Evidence

- `00_equal.png` — equivalent candidates before SCAN/MARK.
- `01_scan.png` — acquired search space and available actions.
- `02_mark.png` — phase inversion with unchanged magnitude/probability/color.
- `03_amplify.png` — actual target amplitude 1; other amplitudes 0.
- `04_lock_success.png` — successful measurement and collapse.
- `05_reset.png` — uniform restart.
- `06_overshoot.png` — second iteration returns the target probability to 25%.
- `07_lock_miss.png` — an actual non-target CDF sample remains; the target collapses.
- `08_debug_hidden.png` — raw numbers hidden with F3.
- `validation.txt` — final Play Mode verification result and limits.
- `scene-path.txt` — generated prototype scene path.

## Complete created-file manifest

**37 deliverable files created. No original source/assets/documents/settings modified.** Unity also regenerated its derived compilation/import caches. All deliverables are inside the permitted Unity project:

```text
D:/Game/UNITY/NullSignal/Assets/Scripts/Gameplay/QuantumEncounter.cs
D:/Game/UNITY/NullSignal/Assets/Scripts/Gameplay/QuantumEncounter.cs.meta
D:/Game/UNITY/NullSignal/Assets/Scripts/Gameplay/AnveshController.cs
D:/Game/UNITY/NullSignal/Assets/Scripts/Gameplay/AnveshController.cs.meta
D:/Game/UNITY/NullSignal/Assets/Scripts/Presentation/CandidateVisualizer.cs
D:/Game/UNITY/NullSignal/Assets/Scripts/Presentation/CandidateVisualizer.cs.meta
D:/Game/UNITY/NullSignal/Assets/Scripts/Presentation/HUDController.cs
D:/Game/UNITY/NullSignal/Assets/Scripts/Presentation/HUDController.cs.meta
D:/Game/UNITY/NullSignal/Assets/Scripts/Editor/NullSignalPhase2Builder.cs
D:/Game/UNITY/NullSignal/Assets/Scripts/Editor/NullSignalPhase2Builder.cs.meta
D:/Game/UNITY/NullSignal/Assets/Scripts/Editor/NullSignalPhase2Validation.cs
D:/Game/UNITY/NullSignal/Assets/Scripts/Editor/NullSignalPhase2Validation.cs.meta
D:/Game/UNITY/NullSignal/Assets/Materials/AmplitudePrototype.meta
D:/Game/UNITY/NullSignal/Assets/Materials/AmplitudePrototype/Room.mat
D:/Game/UNITY/NullSignal/Assets/Materials/AmplitudePrototype/Room.mat.meta
D:/Game/UNITY/NullSignal/Assets/Materials/AmplitudePrototype/Player.mat
D:/Game/UNITY/NullSignal/Assets/Materials/AmplitudePrototype/Player.mat.meta
D:/Game/UNITY/NullSignal/Assets/Materials/AmplitudePrototype/Signal.mat
D:/Game/UNITY/NullSignal/Assets/Materials/AmplitudePrototype/Signal.mat.meta
D:/Game/UNITY/NullSignal/Assets/Materials/AmplitudePrototype/Wave.mat
D:/Game/UNITY/NullSignal/Assets/Materials/AmplitudePrototype/Wave.mat.meta
D:/Game/UNITY/NullSignal/Assets/Materials/AmplitudePrototype/Flow.mat
D:/Game/UNITY/NullSignal/Assets/Materials/AmplitudePrototype/Flow.mat.meta
D:/Game/UNITY/NullSignal/Assets/Scenes/AmplitudeAmplificationPrototype.unity
D:/Game/UNITY/NullSignal/Assets/Scenes/AmplitudeAmplificationPrototype.unity.meta
D:/Game/UNITY/NullSignal/Docs/PHASE2_PLAYABLE.md
D:/Game/UNITY/NullSignal/Docs/Phase2Evidence/00_equal.png
D:/Game/UNITY/NullSignal/Docs/Phase2Evidence/01_scan.png
D:/Game/UNITY/NullSignal/Docs/Phase2Evidence/02_mark.png
D:/Game/UNITY/NullSignal/Docs/Phase2Evidence/03_amplify.png
D:/Game/UNITY/NullSignal/Docs/Phase2Evidence/04_lock_success.png
D:/Game/UNITY/NullSignal/Docs/Phase2Evidence/05_reset.png
D:/Game/UNITY/NullSignal/Docs/Phase2Evidence/06_overshoot.png
D:/Game/UNITY/NullSignal/Docs/Phase2Evidence/07_lock_miss.png
D:/Game/UNITY/NullSignal/Docs/Phase2Evidence/08_debug_hidden.png
D:/Game/UNITY/NullSignal/Docs/Phase2Evidence/scene-path.txt
D:/Game/UNITY/NullSignal/Docs/Phase2Evidence/validation.txt
```

Temporary verification files were created under `D:/Game/UNITY/NullSignal/Temp/Phase2Verification/` and removed after checks: `preservation.json`, `quantum.rsp`, `runtime.rsp`, `editor.rsp`, `NullSignal.Quantum.dll`, `Assembly-CSharp.dll`, `Assembly-CSharp-Editor.dll`, and `unity-phase2.log`.

Work stops at Phase 2.
