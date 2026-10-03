# NULL SIGNAL — Phase 1 foundation

Phase 1 implements the independent quantum simulation, minimal keyboard movement/dodge/interaction, a guided isometric camera, and a Unity editor prototype builder. It stops before combat, enemies, Chhaya, story implementation, cinematics, audio, final art, levels, and final UI.

## Unity next action

Choose **Tools > NULL SIGNAL > Build Foundation Prototype** after Unity finishes importing the scripts.

The builder validates the quantum math, creates and opens a separate primitive scene, and wires the player, camera and four candidate references. It saves a unique `FoundationPrototype` scene in the existing scene folder, adding a numeric suffix on repeated runs. It also creates three shared URP materials in `Assets/Materials/Foundation` if they are absent. It leaves existing material assets intact. None of these scene/material assets have been generated during this implementation.

The normal Unity prompt protects unsaved work before switching scenes. The builder does not modify Bootstrap, build settings, render-pipeline configuration, packages or the active build target. It does not add the prototype to the build's scene list.

## Prototype controls

After the scene is generated, Play Mode uses the project's existing new Input System. Focus the Game view for keyboard input; quantum diagnostics appear in the Unity Console. No input-action asset or Inspector reference dragging is required.

| Input | Foundation behavior |
| --- | --- |
| WASD / arrows | Camera-relative movement with acceleration, turning, gravity and CharacterController collision |
| Space | Short directional dodge with cooldown; idle dodges use the facing direction |
| E | Inspect the closest nearby candidate's actual amplitude and probability; this does not measure it |
| Q | SCAN: print the current calculated state |
| 1 | MARK: invert the target's amplitude sign without changing its probability |
| 2 | AMPLIFY: reflect all amplitudes about their original mean |
| 3 | LOCK: sample the actual distribution and collapse the state to the sampled candidate |
| R | Reset to the uniform initial state |

Each Grover iteration is **MARK then AMPLIFY**. Repeat both operations to see overshooting. Applying AMPLIFY repeatedly without another MARK is not repeated Grover search. The adapter rejects an incomplete sequence. LOCK may also measure the initial or marked state; it never chooses the largest weight deterministically. After LOCK, use R before another iteration.

The generated scene has N=4 and uses the third candidate as its developer-configured target. All four placeholders have the same appearance. The adapter reads simulation snapshots; it does not invent probabilities or change the candidate visuals. N=8 and N=16 are supported and validated in the independent core; this phase generates only four world placeholders.

## Independent quantum foundation

`NullSignal.Quantum` is a separate assembly with `noEngineReferences: true` and no assembly references. Its seven classes contain no GameObjects, MonoBehaviours, Unity APIs, UI or VFX.

- Initialization: each amplitude is `1 / sqrt(N)`.
- Oracle: only the marked amplitude becomes its negative.
- Diffusion: compute the mean of the original state once, then replace each amplitude with `2 * mean - amplitude`.
- Probability: each weight is the square of its amplitude.
- Measurement: inverse-CDF sampling with an injectable `System.Random`, using those calculated weights. Floating-point summation drift is handled without changing their relative weights. Invalid, non-normalized states are rejected.
- Collapse: measurement leaves probability 1 on the sampled candidate. Repeated measurement returns that candidate; reset starts a new search.
- Isolation: amplitude/probability arrays returned by the system are copies; candidate snapshots and scenario configuration are immutable.

## Verification performed

All checks below passed on the installed Unity **6000.6.4f1** toolchain:

1. Compile the quantum assembly using only standard-library references.
2. Execute `QuantumValidation.RunAll()` outside Unity, including all **28 target positions** for N=4, N=8 and N=16, with **64 iterations per target** checked against `sin²((2k+1) asin(1/sqrt(N)))`.
3. Check uniform initialization, Oracle sign inversion and unchanged probabilities, diffusion's original-mean reflection, both operators' involution, normalization, probability sums, amplification, overshooting, immutable snapshots, measurement collapse, reset, invalid operation order and invalid inputs.
4. Check inverse-CDF intervals, signed asymmetric states, zero-mass endpoints, and **120,000 seeded measurements** across uniform and amplified distributions.
5. Compile the quantum, runtime and editor sources against Unity's actual .NET Standard 2.1 and Unity/Input System reference assemblies using C# 9. Runtime compilation used new Input System and WebGL defines. Warnings were treated as errors.
6. Check assembly isolation, valid unique Unity asset GUIDs, and metadata for every new asset/folder.
7. Compare SHA-256 hashes of all **48 original files** in the scene, Unity-setting, URP-setting and package groups: every file remained byte-for-byte unchanged, including Bootstrap and its metadata.

Observed target probabilities, calculated by the simulation:

| N | Initial | After 1 iteration | After 2 | After 3 | After 4 |
| --- | ---: | ---: | ---: | ---: | ---: |
| 4 | 25% | 100% | 25% | 25% | 100% |
| 8 | 12.5% | 78.125% | 94.53125% | 33.0078125% | 1.220703125% |
| 16 | 6.25% | 47.265625% | 90.84472656% | 96.13189697% | 58.17041397% |

These values are verification output, not probability tables used by the implementation.

**Verification limit:** the Unity editor menu has not been executed, Play Mode has not been exercised, and a WebGL player build has not been run. The compilation checks do not establish runtime scene/camera/input behavior. The next Unity action above generates the reviewable prototype without touching Bootstrap. The separate **Tools > NULL SIGNAL > Validate Quantum Foundation** menu can rerun the math checks in Unity.

No packages were installed. No Python, filesystem access, threading or native plugins are required by the runtime scripts. Temporary compiler/runner files were used only inside this project's Temp folder and removed after verification.

## Complete created-file manifest

All paths below are relative to `D:/Game/UNITY/NullSignal/`. **38 deliverable files were created. No existing files were modified.** Folder metadata is listed explicitly; the Story folder is intentionally empty. No future-phase placeholder scripts were added.

```text
Assets/Scripts.meta
Assets/Scripts/Quantum.meta
Assets/Scripts/Player.meta
Assets/Scripts/Gameplay.meta
Assets/Scripts/Presentation.meta
Assets/Scripts/Story.meta
Assets/Scripts/Editor.meta
Assets/Scripts/Quantum/AmplitudeAmplificationSystem.cs
Assets/Scripts/Quantum/AmplitudeAmplificationSystem.cs.meta
Assets/Scripts/Quantum/QuantumCandidate.cs
Assets/Scripts/Quantum/QuantumCandidate.cs.meta
Assets/Scripts/Quantum/OracleOperator.cs
Assets/Scripts/Quantum/OracleOperator.cs.meta
Assets/Scripts/Quantum/DiffusionOperator.cs
Assets/Scripts/Quantum/DiffusionOperator.cs.meta
Assets/Scripts/Quantum/MeasurementSystem.cs
Assets/Scripts/Quantum/MeasurementSystem.cs.meta
Assets/Scripts/Quantum/GroverSearchScenario.cs
Assets/Scripts/Quantum/GroverSearchScenario.cs.meta
Assets/Scripts/Quantum/QuantumValidation.cs
Assets/Scripts/Quantum/QuantumValidation.cs.meta
Assets/Scripts/Quantum/NullSignal.Quantum.asmdef
Assets/Scripts/Quantum/NullSignal.Quantum.asmdef.meta
Assets/Scripts/Player/PlayerController.cs
Assets/Scripts/Player/PlayerController.cs.meta
Assets/Scripts/Player/PlayerDodge.cs
Assets/Scripts/Player/PlayerDodge.cs.meta
Assets/Scripts/Player/PlayerInteraction.cs
Assets/Scripts/Player/PlayerInteraction.cs.meta
Assets/Scripts/Presentation/CameraController.cs
Assets/Scripts/Presentation/CameraController.cs.meta
Assets/Scripts/Gameplay/FoundationQuantumPrototype.cs
Assets/Scripts/Gameplay/FoundationQuantumPrototype.cs.meta
Assets/Scripts/Gameplay/FoundationCandidate.cs
Assets/Scripts/Gameplay/FoundationCandidate.cs.meta
Assets/Scripts/Editor/NullSignalGameBuilder.cs
Assets/Scripts/Editor/NullSignalGameBuilder.cs.meta
Docs/PHASE1_FOUNDATION.md
```

The following ten temporary verification files were also created and then removed. They are not Unity assets or runtime dependencies:

```text
Temp/Phase1Verification/NullSignal.Quantum.dll
Temp/Phase1Verification/Assembly-CSharp.dll
Temp/Phase1Verification/Assembly-CSharp-Editor.dll
Temp/Phase1Verification/QuantumValidationRunner.cs
Temp/Phase1Verification/QuantumValidationRunner.exe
Temp/Phase1Verification/quantum.rsp
Temp/Phase1Verification/runner.rsp
Temp/Phase1Verification/quantum-unity.rsp
Temp/Phase1Verification/runtime.rsp
Temp/Phase1Verification/editor.rsp
```
