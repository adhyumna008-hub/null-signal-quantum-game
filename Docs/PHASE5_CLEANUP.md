# Phase 5 interaction and quantum presentation cleanup

Normal quantum candidates no longer implement the player interaction interface, including the earlier foundation/candidate visual components. Approaching them does not produce an E prompt or consume interaction. Manual release and ANVESH pickup retain their existing visible outcomes.

- SCAN: 0.8 seconds. Cyan device pulse expands across the room. Candidate pedestal emission, rising rings, waveforms and orbiting motes activate in a short stagger. The analysis panel acquires opacity progressively. Combat bodies remain visible and threatening before SCAN.
- MARK: 0.7 seconds. Actual sign changes reverse waveform, ring and particle motion. Only changed-phase nodes emit a brief violet ripple; their amplitude magnitude still comes from the simulation.
- AMPLIFY: 1.0 second. Apparatus rings build energy, followed by a radial release from the apparatus/arena center. Wave height, density, emission and stability follow actual amplitudes. A small camera response and placeholder charge/release sounds reinforce timing.
- Overshoot: actual declining target probability triggers temporary ring instability, violet feedback, a HUD pulse, `TARGET PROBABILITY DECLINING` in the analysis panel, and a short warning tone. Real values naturally shrink the target and restore other candidates.
- LOCK: 1.1 seconds. Short anticipation, slowed candidate motion and a probability-ring pulse precede the original weighted measurement. False states collapse inward and their motes disappear; the actual measured state emits a short vertical beam. Motion resumes without changing global time scale or player controls.

Effects use existing shared materials, bounded mote counts, transform/property-block changes and one small reusable beam renderer per candidate. Operation input remains gated against spam. Audio clips are created once and released on destruction. No quantum operator or probability sampler was changed.

The existing **Tools > NULL SIGNAL > Build Phase 5 Combat** menu picks up all cleanup automatically. Runtime and editor compilation passed with warnings treated as errors. Static review covered reset/cancellation, measured-state selection, phase changes, effect lifetime and interaction routing. No automated Play Mode, screenshot, input simulation or WebGL checks were run; appearance and game feel await human inspection.

Modified scripts: `FoundationCandidate`, `CandidateVisualizer`, `QuantumNodePresentation`, `PlayerInteraction`, `QuantumEncounter`, `AnveshOperationController`, `AnveshOperationPulse`, `StationFeedbackAudio`, `StationCamera`, `AnveshAnalysisPanel`, `AnveshHUD`, `CombatManifestation`, and `ApparatusMotion`. Added `Presentation/QuantumPresentationEffects.cs` and metadata. Updated `PHASE5_COMBAT.md`. Scope ends at Phase 5 cleanup.
