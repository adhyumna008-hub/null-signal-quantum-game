# NULL SIGNAL — GAME IMPLEMENTATION SPECIFICATION

## 1. PROJECT OVERVIEW

NULL SIGNAL is a short, polished 2.5D story-driven sci-fi action/puzzle game.

The game is being created for a quantum-game hackathon.

### Engine
Unity 6

### Rendering
Universal Render Pipeline (URP)

### Target Platform
Web / WebGL

### Presentation
Real 3D environments with a fixed / guided isometric 2.5D camera.

### Target Playtime
Approximately 15–20 minutes.

### Genre
Sci-fi mystery + action + puzzle.

### Main Quantum Topic
**Quantum Amplitude Amplification**

### Primary Gameplay Application
**Grover-style unstructured search**

Grover-style search is used as the main gameplay scenario because it provides a clear and mathematically correct way to demonstrate amplitude amplification.

The game must teach amplitude amplification through gameplay rather than through traditional lessons.

The player should experience the phenomenon first, notice a pattern, hear a short explanation of what they just experienced, and then immediately use that understanding in harder gameplay.

The game must NEVER become:

cutscene
→ long explanation
→ quiz

The preferred structure is:

gameplay experience
→ discovery
→ short contextual voice line
→ harder gameplay application

---

# 2. LEARNING GOAL

By the end of the game, a beginner should understand the basic concepts of quantum amplitude amplification without requiring a separate lecture.

The player should intuitively understand:

1. A quantum search can begin with many possible candidate states.

2. In the game's standard search scenario, candidates begin with equal amplitudes.

3. A specially chosen target can be marked using an Oracle operation.

4. The Oracle does not simply reveal the answer.

5. The Oracle marks the target by changing its phase.

6. In the game's implementation, phase inversion is represented as a sign change of the target amplitude.

7. An amplitude-redistribution / diffusion operation follows the Oracle.

8. This increases the magnitude of the desired state's amplitude while reducing others.

9. Measurement probability is related to the squared magnitude of amplitude.

10. Increasing the desired state's amplitude makes it more likely to be obtained when measured.

11. Oracle + amplification can be repeated.

12. Repeating the process does NOT improve the probability forever.

13. There is an optimal region in which measurement should occur.

14. Too many iterations cause overshooting and reduce success probability again.

15. The success probability therefore oscillates rather than simply filling from 0% to 100%.

16. Grover search is an important application of amplitude amplification for unstructured search.

Do not attempt to teach advanced quantum linear algebra, Hilbert spaces or full proofs.

The game should give strong intuitive understanding of the basic mechanism.

---

# 3. PLAYER-FACING TERMINOLOGY

Do not immediately give the player buttons named:

- Oracle Operator
- Diffusion Operator
- Measurement Operator
- Grover Iteration

That would make the game feel educational.

Instead, ANVESH initially exposes four player-friendly abilities:

## SCAN

Identify the active search space and candidate objects.

## MARK

Apply the Oracle marking operation.

The desired state's phase is inverted.

## AMPLIFY

Perform the amplitude redistribution step.

This should internally execute the diffusion operation.

## LOCK

Perform measurement and commit to one result.

Later in the game, the scientific terminology is attached to these mechanics.

The intended realization is:

> "I've already been doing amplitude amplification. Now I know what it is called."

---

# 4. CORE GAMEPLAY LOOP

The main gameplay loop is:

SCAN
→ MARK
→ AMPLIFY
→ DECIDE
→ LOCK
→ CONSEQUENCE

The player must decide how many amplitude-amplification iterations to perform before measurement.

Too few iterations:
the desired state may still have a low probability.

Near the optimum:
the desired state has a high probability.

Too many iterations:
the player overshoots and the desired state's probability begins decreasing.

This loop must be reused throughout:

- doors
- security systems
- environmental puzzles
- moving targets
- enemy encounters
- archive search
- Chhaya fight
- Chhaya Prime final boss

Do not create unrelated quantum mechanics for each level.

The entire game should deepen understanding of the same central concept.

---

# 5. ACTUAL QUANTUM MODEL

Do not fake the quantum probability system using scripted values.

The game's VFX and gameplay must be driven by actual calculated amplitudes.

The game teaches amplitude amplification using the canonical Grover-style search case.

For the hackathon build, support search spaces with:

N = 4
N = 8
N = 16

Exactly one target state is marked in the standard encounters.

Real-valued amplitudes are sufficient for this implementation because the standard Oracle and diffusion used here can be represented through sign changes and real reflections.

---

## 5.1 INITIALIZATION

For N candidates, initialize every amplitude equally:

a_i = 1 / sqrt(N)

Example with four states:

a = [0.5, 0.5, 0.5, 0.5]

Probabilities are:

P(i) = |a_i|²

Therefore initially:

25%
25%
25%
25%

The probabilities must always approximately sum to 1.

Use floating-point tolerance when validating normalization.

---

# 6. ORACLE / MARK OPERATION

The Oracle must NOT reveal the target directly.

It performs a phase inversion on the marked state.

For target t:

a_t = -a_t

All other amplitudes remain unchanged.

Example:

Before:

[+0.5, +0.5, +0.5, +0.5]

If candidate 3 is marked:

[+0.5, +0.5, -0.5, +0.5]

This sign change represents the phase inversion.

The visual system should show the phase change using:

- reversed particle flow
- inverted wave direction
- reversed oscillation
- mirrored waveform
- brief phase ripple

Do NOT simply turn the correct object green.

The magnitude should remain approximately unchanged immediately after the Oracle.

The player should understand:

**Marked does not mean revealed.**

---

# 7. DIFFUSION / AMPLIFICATION OPERATION

After Oracle marking, perform inversion about the mean.

Calculate:

mean = sum(a_i) / N

Then for every candidate:

a_i = 2 × mean - a_i

This reflects each amplitude about the average amplitude.

Because the desired state was phase-inverted by the Oracle, the reflection causes its amplitude magnitude to increase while others decrease.

This operation must be implemented mathematically.

Do not hard-code probability increases.

---

# 8. GROVER-STYLE ITERATION

One standard search iteration consists of:

Oracle
→ Diffusion

The player-facing action can simplify this depending on progression.

Early game:

MARK may apply Oracle.

AMPLIFY may apply Diffusion.

Later encounters may treat MARK + AMPLIFY together as one completed search iteration.

Internally preserve the mathematical distinction.

Track:

- iteration count
- amplitudes
- probabilities
- marked state
- normalization
- highest-probability candidate

---

# 9. MEASUREMENT / LOCK

LOCK performs measurement.

For every candidate:

P(i) = |a_i|²

Randomly sample one candidate using the calculated probability distribution.

Measurement must not simply return the target automatically.

If the target probability is 80%, it should succeed approximately 80% of the time over repeated trials.

After measurement:

- one state becomes the measured result
- other manifestations collapse visually
- the encounter resolves according to the result

Different levels may apply different failure consequences.

Level 0:
very forgiving.

Later:
wrong measurement may:

- spawn an enemy
- cost time
- damage the player
- reset part of a puzzle
- restore boss shield

Never restart the entire game because of one incorrect quantum measurement.

---

# 10. OVERSHOOT

Overshooting is a required mechanic.

Repeated amplitude-amplification iterations must naturally produce oscillating success probability.

The desired probability should behave conceptually like:

low
→ higher
→ near maximum
→ lower
→ lower
→ rise again

Do NOT create a simple progress bar that permanently increases.

When the player continues after the optimal region, the target probability must decline based on the actual quantum simulation.

This is one of the most important teaching moments in the game.

---

# 11. VISUAL TEACHING SYSTEM

The game must teach mostly through animation.

Use three distinct visual concepts:

## PHASE

Represented by:

- waveform orientation
- directional particle flow
- oscillation direction

Oracle changes phase.

## AMPLITUDE

Represented by:

- waveform height
- particle density
- solidity
- hologram strength
- sound intensity

Amplitude redistribution changes these values.

## PROBABILITY

Derived from:

P = |a|²

Represent probability using:

- subtle ground ring
- small meter
- later numeric indicator
- probability arc

Early levels should avoid raw numbers.

Later levels may show:

TARGET PROBABILITY: 78%

The intended progression is:

player sees phase
→ player sees amplitude
→ player understands probability

---

# 12. ART DIRECTION

The game uses a futuristic Indian quantum-science aesthetic.

The setting is an Indian-led orbital research station.

The Indian influence should be subtle and contemporary.

Use:

- dark graphite
- titanium
- matte black metal
- copper accents
- restrained saffron accents
- cyan quantum energy
- violet quantum distortion
- jaali-inspired ventilation patterns
- radial scientific geometry
- mandala-like symmetry in architecture
- yantra-inspired technical circuit layouts

Do NOT use:

- religious symbols
- temple architecture
- palace aesthetics
- mythological costumes
- excessive neon
- stereotypical Indian decoration

The environment must remain believable science-fiction research architecture.

Use the images inside:

Docs/References/

as visual references.

They are concept references, not necessarily runtime game assets.

---

# 13. CAMERA

The game should use a 2.5D fixed / guided isometric camera.

Requirements:

- real 3D world
- smooth player follow
- camera should keep important candidates visible
- limited rotation where needed
- room-based framing
- zoom out in large quantum encounters
- cinematic framing during story sequences
- subtle screen shake for impacts
- reduced-camera-shake accessibility setting

Avoid unrestricted third-person camera control.

One major reason for using 2.5D is that players should be able to visually compare multiple candidate states simultaneously.

---

# 14. PLAYER CONTROLS

Recommended controls:

WASD
Movement

Mouse
Selection / aiming where useful

E
Interact

Space
Dodge

Left Mouse
Basic attack

Q
SCAN / quantum mode

1
MARK

2
AMPLIFY

3
LOCK

Controls may be changed if a better scheme is found.

All controls must appear in:

- pause menu
- optional contextual hints

Do not flood the screen with tutorials.

---

# 15. PLAYER MOVEMENT

Movement should feel responsive.

Use:

- smooth acceleration
- smooth deceleration
- clear collision
- dodge with short cooldown
- readable movement speed
- simple hit reaction

The player does not need complicated platforming.

Dodge is more important than jumping.

If jumping complicates development, keep vertical traversal minimal.

---

# 16. BASIC COMBAT

Combat must remain deliberately simple.

Core combat actions:

- movement
- basic attack
- dodge
- quantum abilities

Do not attempt a deep combo system.

The game's main skill is understanding and applying amplitude amplification under pressure.

Enemies should:

- telegraph attacks clearly
- deal moderate damage
- allow recovery
- have simple patterns

Quantum enemies become vulnerable after successful measurement.

Typical loop:

enemy creates candidates
→ player searches
→ player locks
→ true target becomes vulnerable
→ attack window
→ enemy splits again

---

# 17. LEVEL STRUCTURE

The game should feel continuous.

Do not create seven completely unrelated worlds.

Reuse a modular Astra-7 station kit.

Change:

- lighting
- room layout
- props
- VFX
- damage state
- quantum effects

to give each section a distinct identity.

Recommended sections:

0. Opening cinematic
1. Level 0 — Awaken
2. Level 1 — The Four
3. Level 2 — Oracle Lab
4. Level 3 — Amplification Lab
5. Level 4 — Overshoot Reactor
6. Level 5 — Chhaya
7. Level 6 — The Vault
8. Chhaya Prime
9. Anveshak Core
10. Ending cinematic

---

# 18. OPENING CINEMATIC

Target duration:

30–45 seconds.

Black screen.

Radio static.

ANANYA:

"Anveshak Control, this is Rao. Search iteration twenty-seven complete."

LUBNA:

"Signal amplitude increased again."

ANANYA:

"That's impossible. There shouldn't be a marked state."

Silence.

UNKNOWN VOICE:

"YOU FOUND ME."

Then rapidly show:

- Astra-7 exterior
- alarms
- scientists running
- quantum monitors distorting
- overlapping versions of corridors
- security lockdown
- Chhaya silhouette behind observation glass

Cut to black.

Display:

ASTRA-7 — 72 HOURS LATER

Anirudh's transport crashes.

Immediately begin gameplay.

Do not keep the player watching a long cinematic.

---

# 19. LEVEL 0 — AWAKEN

## Purpose

Teach:

- movement
- camera
- interaction
- dodge
- environmental awareness
- ANVESH
- first candidate system

WITHOUT feeling like a tutorial.

Anirudh wakes inside a damaged transport pod.

Smoke.

Emergency lighting.

Metal damage.

TARA:

"Emergency impact detected."

"Anirudh? If you're conscious, move. That compartment is losing pressure."

No giant tutorial window.

A tiny contextual movement hint may appear briefly.

---

## Interaction

Door is jammed.

Nearby console sparks.

TARA:

"Manual release still has power."

Player approaches console.

Interaction prompt appears.

Player uses it.

Door opens.

---

## Dodge

Corridor collapses.

Player gets a visible warning.

Player must dodge.

Failing the first dodge should not kill them.

Anirudh gets knocked down and recovers.

The player naturally learns dodge.

---

# 20. DISCOVERING ANVESH

Power fails.

Room becomes dark.

One cyan glow remains.

Player follows it.

ANVESH is found.

ANVESH:

"Quantum search interface restored."

TARA:

"Good. You'll need that."

Unlock:

SCAN

Four damaged power modules appear.

One can restore power.

The first encounter may automatically perform most of the quantum operations.

The four candidates begin equally.

ANVESH helps increase confidence in one candidate.

Player chooses it.

Power returns.

The player learns subconsciously:

multiple candidates exist
→ one matters

Do not explain amplitude amplification yet.

---

# 21. FIRST TRUE AMPLITUDE AMPLIFICATION PUZZLE

A sealed bulkhead blocks the player.

Display:

AUTHORIZATION FRAGMENTED

CANDIDATES: 4

Four nodes appear.

They have equal amplitudes.

ANVESH unlocks:

MARK

Player activates MARK.

Target phase flips.

Particle flow reverses.

ANVESH:

"Target signature inverted."

TARA:

"You haven't found it."

"You've only made the desired state different from the others."

Then unlock:

AMPLIFY

Player activates AMPLIFY.

Run real diffusion operation.

Target amplitude increases.

Others weaken.

TARA:

"There. The difference is growing."

Then:

LOCK

Measurement occurs.

Quantum collapse VFX.

One result remains.

Correct result opens the door.

ANIRUDH:

"So we didn't check all four."

TARA:

"No."

"We changed the odds."

No extra explanation.

---

# 22. LEVEL 1 — THE FOUR

## Goal

Teach:

**Oracle marking does not equal revealing the answer.**

Environment:

Astra-7 Security Sector.

Four security drones activate.

Only one contains the actual security authorization core.

Other drones are quantum decoys.

Player uses:

SCAN
→ MARK
→ AMPLIFY
→ LOCK

Enemies continue moving during quantum search mode.

Optionally slow time slightly.

Do not completely pause gameplay.

After MARK:

ANIRUDH:

"Which one's the real one?"

TARA:

"You still can't know."

"Marking changes its phase. It doesn't reveal its position."

After successful measurement:

real drone becomes vulnerable.

Player attacks.

After incorrect measurement:

wrong drone becomes hostile or player loses time.

---

# 23. LEVEL 2 — ORACLE LAB

## Goal

Teach:

- target marking
- phase inversion
- Oracle does not directly provide answer

Anirudh enters the Oracle research laboratory.

Dr. Ananya Rao holographic recording plays while the player walks.

ANANYA:

"People misunderstand the Oracle."

"It doesn't tell us the answer."

Nearby holographic candidates appear.

One waveform phase-flips.

ANANYA:

"It marks the state we're interested in..."

"...by changing its phase."

Immediately follow this recording with gameplay requiring MARK.

The player has already experienced the phenomenon.

The recording simply gives it a scientific name.

---

# 24. LEVEL 3 — AMPLIFICATION LAB

## Goal

Teach actual amplitude redistribution.

Search space:

8 candidates.

All begin equal.

Player MARKS desired target.

Phase changes.

Player AMPLIFIES.

Radial field expands.

Target amplitude increases.

Others decrease.

Dr. Lubna recording begins while player remains active.

LUBNA:

"The clever part isn't marking the answer."

"The clever part is what happens next."

Player amplifies.

LUBNA:

"We redistribute amplitude."

Target becomes stronger.

LUBNA:

"The state we want becomes more likely to survive measurement..."

Other states become weaker.

LUBNA:

"...while the alternatives become less likely."

No quiz.

No full-screen lesson.

---

# 25. LEVEL 4 — OVERSHOOT REACTOR

## Goal

Teach:

- optimal stopping region
- overshooting
- oscillation

Environment:

damaged Astra-7 reactor.

8 possible reactor-control states.

One target.

Player:

MARK
AMPLIFY
AMPLIFY

Target becomes increasingly dominant.

Design the encounter so the player is tempted to AMPLIFY again.

They do.

The target begins weakening.

Other states regain probability.

ANVESH:

"Target probability declining."

ANIRUDH:

"Why? I amplified it again."

Lubna diagnostic triggers:

"Amplitude amplification isn't a charging bar."

"Think of it as rotating toward the state you want."

"Go too far and you rotate past it."

Show subtle circular visual representation.

Do NOT reset immediately.

Allow player to keep iterating.

They should observe probability oscillating.

---

# 26. LEVEL 5 — CHHAYA

## Goal

Turn amplitude amplification into combat skill.

Lights fail.

Chhaya appears.

Chhaya creates:

4 manifestations.

Only one corresponds to the target state.

All copies move and threaten player.

CHHAYA:

"SEARCH."

Player:

- dodges
- MARKS
- AMPLIFIES
- chooses when to LOCK

Correct measurement:

false manifestations collapse.

True Chhaya becomes vulnerable.

Player attacks.

Later stage:

8 manifestations.

Now the player must understand overshooting under pressure.

Do not introduce unrelated mechanics.

---

# 27. LEVEL 6 — THE VAULT

## Goal

Teach why amplitude amplification is useful in search.

Astra-7 central quantum archive.

16 memory sectors.

One contains the original disaster transmission.

TARA:

"Classical diagnostics can test the sectors individually."

Timer appears:

SYSTEM COLLAPSE: 01:00

ANIRUDH:

"No time."

TARA:

"Then don't search them one by one."

No further explanation.

Player knows what to do.

MARK.

AMPLIFY.

AMPLIFY.

Continue if needed.

Choose correct time.

LOCK.

The player succeeds because they understand amplitude amplification.

Optional background hologram may show:

CLASSICAL

candidate
→ candidate
→ candidate
→ candidate

versus

ANVESHAK

equal candidates
→ mark
→ amplify
→ measure

Do not force player to read it.

---

# 28. STORY REVEAL

After Vault completion, recover Ananya's recording.

ANANYA:

"Anveshak wasn't searching for extraterrestrial signals."

"We were testing how far amplitude amplification could push an almost undetectable marked state."

LUBNA:

"There was one."

ANANYA:

"Its initial amplitude was almost zero."

Show signal.

ANANYA:

"We amplified it."

Signal strengthens.

"Again."

Chhaya silhouette develops.

"And again."

Chhaya stabilizes.

ANANYA:

"Until it could observe us back."

Story implication:

The researchers may not have simply discovered Chhaya.

They may have amplified an extremely weak signal until it became stable enough to manifest.

The quantum concept becomes the reason the story happened.

---

# 29. FINAL BOSS — CHHAYA PRIME

The final boss must test existing mechanics.

Do not introduce a new quantum mechanic.

---

## PHASE 1

4 manifestations.

Simple familiar loop:

MARK
→ AMPLIFY
→ LOCK
→ ATTACK

---

## PHASE 2

8 manifestations.

Chhaya attacks continuously.

Arena hazards activate.

Player must dodge while monitoring amplitudes.

Overshooting restores part of Chhaya's shield.

---

## PHASE 3

16 manifestations.

Music intensifies.

HUD becomes minimal.

TARA:

"Anirudh..."

Pause.

"You know what to do."

No tutorial hints.

Player must:

- understand equal candidates
- identify Oracle phase marking
- amplify
- observe target amplitude
- avoid overshoot
- choose measurement time
- LOCK
- expose true Chhaya
- attack

This is the final demonstration of mastery.

It should feel like a boss fight, not an exam.

---

# 30. SCIENTIFIC PAYOFF

After defeating Chhaya Prime, player walks toward Anveshak Core.

TARA reconstructs what the player has already learned.

TARA:

"Equal starting amplitudes."

Visual:
all candidates equal.

TARA:

"Oracle phase marking."

One phase flips.

TARA:

"Amplitude amplification."

Target grows.

TARA:

"Measurement."

Other candidates collapse.

TARA:

"Quantum amplitude amplification."

Pause.

"Grover search is one of the procedures built from the same principle."

ANIRUDH:

"That's what Anveshak was doing."

TARA:

"Yes."

This is where terminology is attached to gameplay experience.

---

# 31. ENDING

Anirudh reaches Anveshak Core.

TARA:

"Target amplitude collapsing."

Anirudh destroys Array.

Chhaya disappears.

Quantum distortions vanish.

Astra-7 destabilizes.

Anirudh escapes using emergency craft.

Station collapses behind him.

Everything becomes quiet.

No duplicate objects.

No distorted probability fields.

No voices.

Mission complete.

---

# 32. CLIFFHANGER

Inside escape craft.

ANVESH is powered off.

Suddenly:

BEEP.

ANVESH activates.

Display:

SEARCH SPACE: UNKNOWN

TARGETS: 1

POSSIBILITIES: 7,942,381,625

Probability rises:

0.00000001%

0.0001%

0.01%

1%

ANIRUDH:

"I destroyed the Array."

UNKNOWN VOICE:

"You destroyed one."

Anirudh freezes.

TARA is offline.

Camera exits spacecraft.

Earth appears.

Beyond Earth:

one artificial light activates.

Then another.

Hundreds.

Thousands.

Huge quantum structures become visible.

UNKNOWN VOICE:

"Ananya Rao taught your world how to search."

Pause.

"Lubna taught it how to amplify."

Pause.

"Now we know how to search back."

Every structure activates.

Cut to black.

NULL SIGNAL

THE SEARCH HAS ONLY BEGUN

---

# 33. VOICEOVER DESIGN RULES

Every voiceover must follow these rules.

## Rule 1

Maximum approximately 1–3 sentences at once.

## Rule 2

Do not stop gameplay unless it is a story-critical cinematic.

## Rule 3

Whenever possible, voiceovers explain something the player JUST experienced.

Bad:

"Amplitude amplification works using repeated Oracle and diffusion operators."

Good:

Player just watches target shrink after one extra iteration.

LUBNA:

"You're past the peak. Another iteration moved you away from the target."

## Rule 4

Voice lines should not solve puzzles before the player has a chance to think.

## Rule 5

Use subtitles for all important dialogue.

---

# 34. CHARACTER VOICEOVER ROLES

## TARA

Role:

practical gameplay interpretation.

Voice style:

calm
precise
short

Examples:

"Marked, not found."

"Amplitude increasing."

"Target probability declining."

"You're past the peak."

---

## DR. ANANYA RAO

Role:

Oracle
phase
big conceptual ideas
story

Example:

"An Oracle doesn't reveal the answer. It changes the target's phase."

---

## DR. LUBNA

Role:

amplitude redistribution
overshoot
iteration behavior

Example:

"Amplitude isn't a battery. Push beyond the peak and you rotate away from the state you want."

---

## ANIRUDH

Role:

express the questions the player is likely thinking.

Examples:

"Which one's real?"

"Why did it get weaker?"

"So we didn't check all four?"

This makes explanations feel conversational rather than instructional.

---

# 35. VOICEOVER IMPLEMENTATION

Dialogue must not be hard-coded throughout unrelated gameplay scripts.

Create a reusable voice/dialogue system.

Each voice event should support:

- Speaker
- AudioClip
- SubtitleText
- TriggerType
- TriggerCondition
- Priority
- CanInterrupt
- PlayOnce
- Delay
- OptionalCooldown

Example:

Speaker:
Lubna

Trigger:
playerOvershotTarget

Subtitle:

"Amplitude doesn't rise forever. Push beyond the peak and you rotate away from the state you want."

PlayOnce:
true

If final voice files are unavailable:

- subtitle system must still work
- use temporary placeholder audio or TTS only if appropriate
- allow final WAV/OGG files to be inserted later without code changes

---

# 36. SUBTITLES

Subtitles must be ON by default.

Place subtitles near bottom of screen.

Format:

LUBNA

"You're past the peak. Another iteration moved you away from the target."

Maximum approximately two subtitle lines at a time.

Keep readable contrast.

---

# 37. GAME ARCHITECTURE

Keep quantum simulation independent from Unity presentation and gameplay.

Recommended structure:

Assets/
    Scripts/

        Quantum/
            AmplitudeAmplificationSystem.cs
            QuantumCandidate.cs
            OracleOperator.cs
            DiffusionOperator.cs
            MeasurementSystem.cs
            GroverSearchScenario.cs
            QuantumValidation.cs

        Player/
            PlayerController.cs
            PlayerHealth.cs
            PlayerCombat.cs
            PlayerDodge.cs
            PlayerInteraction.cs

        Gameplay/
            AnveshController.cs
            QuantumEncounter.cs
            SecurityDrone.cs
            ChhayaController.cs
            ChhayaPrimeController.cs
            DoorController.cs
            ReactorPuzzle.cs
            VaultController.cs

        Presentation/
            CandidateVisualizer.cs
            QuantumVFXController.cs
            CameraController.cs
            HUDController.cs
            SubtitleController.cs
            DialogueSystem.cs
            AudioManager.cs

        Story/
            LevelDirector.cs
            VoiceTrigger.cs
            ObjectiveSystem.cs
            CutsceneController.cs
            CheckpointSystem.cs

        Editor/
            NullSignalGameBuilder.cs

---

# 38. IMPORTANT DATA FLOW

Quantum system calculates:

amplitudes
↓
probabilities

Presentation system reads calculated data.

CandidateVisualizer uses actual amplitude/probability to drive:

- wave height
- opacity
- particle density
- solidity
- probability ring

The presentation layer must NEVER independently invent quantum values.

Gameplay systems should ask the quantum system for results.

Example:

Chhaya boss
does NOT calculate fake probabilities.

Instead:

Chhaya boss creates candidate encounter
→ AmplitudeAmplificationSystem calculates amplitudes
→ CandidateVisualizer renders them
→ MeasurementSystem samples result
→ boss reacts to result

---

# 39. UNITY EDITOR AUTOMATION

The user is a beginner with Unity.

Reduce manual editor configuration wherever reasonable.

Create editor tooling where useful.

Preferred approach:

Tools
→ NULL SIGNAL
→ Build Prototype

The editor tool may automatically create:

- player
- camera
- lights
- rooms
- candidate pedestals
- test quantum encounter
- UI
- managers

Do not require the user to manually drag dozens of references in the Inspector if automation can reasonably do it.

---

# 40. SCENES

Recommended scenes:

Bootstrap

MainMenu

Game

Credits

For the hackathon build, most levels may exist as connected sections inside one main gameplay scene to reduce scene-loading complexity.

If necessary, split into:

Level0_Awaken
Station_Main
FinalBoss

But avoid creating excessive scenes.

---

# 41. BOOTSTRAP SCENE

Bootstrap should initialize persistent systems:

- GameManager
- AudioManager
- Settings
- Save/checkpoint system

Then load MainMenu or Game.

Do not place level-specific gameplay inside Bootstrap.

---

# 42. UI

Minimal sci-fi HUD.

Required:

- health
- objective
- candidate count
- quantum ability availability
- amplitude indicator
- later probability indicator
- later iteration count
- overshoot warning
- boss health
- subtitles

Do not show advanced quantum numbers from the first minute.

Unlock information gradually.

---

# 43. DEBUG OVERLAY

Create optional developer overlay.

Toggle:

F3

Display:

- N candidate count
- marked candidate index
- current amplitudes
- current probabilities
- sum of probabilities
- iteration count
- measurement result
- current encounter state

Disabled by default.

This will be extremely useful during judging and debugging.

---

# 44. QUANTUM VALIDATION

Create tests/debug validation for:

N = 4
N = 8
N = 16

Validate:

- initialization normalized
- Oracle changes only marked state's sign
- diffusion formula correct
- probabilities sum approximately to 1
- repeated iterations produce amplification
- overshoot occurs
- measurement sampling uses calculated probabilities

Do not rely only on visual inspection.

---

# 45. PLACEHOLDER ASSETS

Do not wait for perfect final assets.

If final 3D models are unavailable, use stylized procedural placeholders.

Examples:

Anirudh:
capsule / simple humanoid

Security drone:
procedural sci-fi drone using primitive meshes

Chhaya:
dark humanoid silhouette built from geometry + particles

Station:
modular walls, floors, pillars and panels

Quantum nodes:
holographic geometry

Replace later when final assets are available.

Gameplay must remain functional without external art.

---

# 46. VFX

Essential VFX:

## Equal state

all candidates visually equal

## Oracle

phase reversal
wave flip
particle direction inversion

## Amplification

target waveform increases
particle density increases
target solidity increases
others weaken

## Overshoot

target visual strength decreases again

## Measurement

dramatic but fast collapse
unmeasured candidates dissolve

## Chhaya

violet/dark probability distortion
fractured geometric fragments

Avoid excessive particles that hurt Web performance.

---

# 47. AUDIO

Required categories:

- station ambience
- emergency alarms
- footsteps
- interaction
- Oracle mark sound
- amplification sound
- probability peak cue
- overshoot cue
- measurement collapse
- drone sound
- attacks
- hit impacts
- Chhaya distortion
- boss music
- cinematic ambience

Use placeholders if necessary.

Final audio can replace placeholders later.

---

# 48. ACCESSIBILITY / SETTINGS

Required:

- subtitles enabled by default
- master volume
- music volume
- SFX volume
- reduced camera shake
- pause
- restart checkpoint
- controls display

---

# 49. CHECKPOINTS

Create lightweight checkpoints.

Recommended checkpoints:

- beginning of Level 0
- security sector
- Oracle Lab
- Amplification Lab
- Overshoot Reactor
- Chhaya
- Vault
- Chhaya Prime

Death should reload nearest checkpoint quickly.

Do not restart the entire game.

---

# 50. WEB PERFORMANCE

The final build targets browser/WebGL.

Prioritize stable performance over ultra-realistic graphics.

Target approximately:

60 FPS on a normal desktop where practical.

Optimize:

- lighting
- particle counts
- shadows
- materials
- repeated geometry
- physics
- object pooling

Prefer:

- baked or limited lighting
- reusable materials
- moderate texture sizes
- simple shaders
- GPU instancing where useful

Avoid:

- HDRP
- huge texture files
- excessive transparent particles
- expensive realtime reflections
- large numbers of realtime lights

---

# 51. GAME FEEL

The game must not feel like a science simulator.

Use:

- responsive controls
- camera movement
- sound feedback
- hit feedback
- brief screen shake
- clear attack telegraphs
- smooth transitions
- satisfying measurement collapse
- strong quantum VFX
- cinematic environment framing

The player should enjoy the experience even before realizing they are learning quantum mechanics.

---

# 52. 16-HOUR IMPLEMENTATION PRIORITY

The game must be developed incrementally.

## HOURS 0–2

- Unity project setup
- Git repository
- URP configuration
- Web target
- project folders
- player movement
- camera
- basic modular room

## HOURS 2–4

Implement actual quantum system:

- initialization
- Oracle
- diffusion
- measurement
- validation
- N = 4 / 8 / 16

## HOURS 4–6

- ANVESH
- candidate visuals
- phase visualization
- Level 0
- first real amplitude-amplification puzzle

At this point a playable quantum mechanic must exist.

## HOURS 6–8

- security drone
- simple combat
- Level 1
- Oracle Lab

## HOURS 8–10

- Amplification Lab
- amplitude VFX
- Overshoot Reactor
- overshoot detection
- Lubna voice trigger

## HOURS 10–12

- Chhaya encounter
- 4 / 8 candidate combat phases

## HOURS 12–13

- Vault
- 16 candidates
- story reveal

## HOURS 13–14

- Chhaya Prime
- reuse existing candidate system

## HOURS 14–15

- voiceovers/subtitles
- opening cinematic
- ending cinematic
- lighting
- VFX
- audio polish

## HOURS 15–16

- Web build
- bug fixing
- performance
- README
- gameplay recording

---

# 53. IF TIME RUNS OUT

Protect these features:

1. actual amplitude-amplification math

2. Level 0

3. Oracle / phase marking

4. amplitude redistribution

5. overshooting

6. at least one combat encounter

7. Chhaya / Chhaya Prime payoff

8. ending

Cut environment size before cutting quantum mechanics.

Levels 2–4 may be merged into one larger laboratory if required.

Optional supporting characters may remain voice logs only.

---

# 54. PLAYER EXPERIENCE TARGET

Ideal progression:

0–3 min:

"This station is interesting. What happened?"

3–5 min:

"ANVESH is searching among possibilities."

5–8 min:

"MARK changes something about the target, but doesn't reveal it."

8–11 min:

"AMPLIFY is redistributing probability."

11–13 min:

"More isn't always better. I can overshoot."

13–16 min:

"I understand how to beat Chhaya."

Ending:

"So this was amplitude amplification."

The player should be capable of explaining:

> Quantum amplitude amplification increases the measurement probability of desired states. A target is marked through a phase change, then an amplification operation redistributes amplitude toward it. Repeating the process helps until an optimal region, but too many iterations can overshoot and reduce the success probability again. Grover search is an important search application of this principle.

---

# 55. ACCEPTANCE TESTS

Before considering the project complete:

## PROJECT

[ ] Unity project opens without critical errors

[ ] URP works

[ ] Web build works

[ ] Bootstrap loads correctly

## PLAYER

[ ] movement works

[ ] interaction works

[ ] dodge works

[ ] attack works

[ ] health works

[ ] checkpoints work

## QUANTUM

[ ] uniform amplitudes initialized correctly

[ ] Oracle phase inversion works

[ ] Oracle does not alter non-target states

[ ] diffusion operation works

[ ] probabilities derived from amplitude squared

[ ] probabilities remain normalized

[ ] N=4 works

[ ] N=8 works

[ ] N=16 works

[ ] repeated iterations produce amplification

[ ] overshooting naturally occurs

[ ] LOCK performs real probabilistic measurement

## VISUAL TEACHING

[ ] equal amplitudes appear equal

[ ] phase inversion is visually readable

[ ] target amplitude increase is visible

[ ] other amplitudes decrease visibly

[ ] overshoot is visible

[ ] measurement collapse is clear

## LEVELS

[ ] opening cinematic

[ ] Level 0 Awaken

[ ] first amplitude-amplification puzzle

[ ] Security / The Four

[ ] Oracle Lab

[ ] Amplification Lab

[ ] Overshoot Reactor

[ ] Chhaya

[ ] Vault

[ ] Chhaya Prime

[ ] Anveshak Core

[ ] cliffhanger ending

## VOICE / STORY

[ ] subtitles work

[ ] dialogue triggers only when appropriate

[ ] Ananya recordings work

[ ] Lubna recordings work

[ ] TARA gameplay dialogue works

[ ] final terminology reveal works

## PERFORMANCE

[ ] no major console errors

[ ] no obvious memory leak

[ ] stable desktop browser performance

[ ] particle effects are reasonable

[ ] restart/checkpoint works

---

# 56. DEVELOPMENT PRINCIPLES

When implementing:

1. Make the smallest working system first.

2. Test it.

3. Do not rewrite working systems without a strong reason.

4. Keep quantum math independent.

5. Reuse the same quantum system for puzzles and bosses.

6. Automate Unity setup where practical.

7. Prefer working gameplay over excessive visual complexity.

8. Keep Web compatibility in mind from the start.

9. Use placeholder assets until mechanics work.

10. Commit regularly.

11. Never fake quantum behavior simply for visual convenience.

12. Never sacrifice player enjoyment for unnecessary explanation.

13. Never sacrifice scientific accuracy for an easier scripted solution when the correct math is simple enough to implement.

---

# 57. FINAL DESIGN RULE

NULL SIGNAL should feel like:

**a sci-fi game that happens to teach amplitude amplification**

NOT:

**an amplitude-amplification lesson disguised as a game.**

The player's emotional progression should be:

curiosity
→ discovery
→ understanding
→ mastery
→ boss victory
→ story revelation
→ cliffhanger

The player's scientific progression should be:

equal candidates
→ phase marking
→ amplitude redistribution
→ measurement
→ repeated iterations
→ optimal stopping
→ overshoot
→ practical search application

Both progressions should happen at the same time.