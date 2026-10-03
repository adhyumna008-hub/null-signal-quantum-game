# NULL SIGNAL — CINEMATIC & ANIMATION REQUIREMENTS

## CORE RULE

All important story cinematics must be REAL 3D ANIMATED SEQUENCES.

Do NOT create:
- slideshow cinematics
- still images with zoom/pan
- concept-art sequences pretending to be animation
- static characters with narration
- text-only story sequences when animation is practical

The cinematic should look like part of the actual 3D game world.

---

# ANIMATION STANDARD

Use continuous 3D motion.

Characters should use:
- skeletal animation
- keyframed body movement
- walking
- running
- turning
- reaching
- interacting with objects
- reacting to danger
- looking toward speakers/events
- head and upper-body movement
- suitable facial animation when practical

Do not let characters stand completely frozen during dialogue.

Animation should blend smoothly between states.

Target cinematic playback:
30–60 FPS.

Unity interpolation between animation keyframes is expected.

"Frame-by-frame animation" means the final cinematic must contain continuous visible motion throughout the sequence, not a collection of still images.

---

# CAMERA

Use actual animated Unity cameras.

Camera motion may include:
- dolly
- tracking
- orbit
- controlled handheld movement
- push-in
- pull-out
- crane movement
- cinematic cuts
- focus changes where practical

Camera motion must be deliberate.

Do not simply move a camera across a static image.

Use Cinemachine or equivalent Unity camera systems when useful.

---

# ENVIRONMENT ANIMATION

The environment should also move.

Examples:
- warning lights flashing
- doors closing
- screens changing
- cables swinging
- smoke moving
- sparks
- alarms
- holograms failing
- objects falling
- quantum distortions
- station sections flickering between states
- particles flowing
- emergency shutters moving

The scene should feel alive even when the main character is not moving.

---

# OPENING BACKSTORY CINEMATIC

Target:
approximately 35–50 seconds.

This must be a real 3D animated sequence.

## SHOT 1 — ASTRA-7

Exterior of Astra-7 in orbit.

Camera slowly approaches the station.

Small maintenance craft and station lights move.

Earth is visible in the distance.

Transition into the research sector.

---

## SHOT 2 — ANVESHAK CONTROL ROOM

Dr. Ananya Rao walks toward a holographic research console.

Researchers move naturally in the background.

Dr. Lubna operates another terminal.

The Anveshak Array is active behind glass.

Quantum candidate visualizations move continuously.

Ananya:

"Anveshak Control, this is Rao. Search iteration twenty-seven complete."

Lubna looks at her display.

Lubna:

"Signal amplitude increased again."

Ananya turns toward Lubna.

Ananya:

"That's impossible. There shouldn't be a marked state."

Their body language should react naturally to the conversation.

---

## SHOT 3 — THE SIGNAL

Camera moves toward the main holographic display.

Thousands of faint candidate states are visible.

One anomalous signal begins becoming stronger.

The waveform changes.

Particle density increases.

Researchers notice it.

The room gradually becomes quiet.

Unknown voice:

"YOU FOUND ME."

Do not show this as text only.

The voice should feel like it is coming from the station's audio system.

---

## SHOT 4 — FAILURE

The Anveshak Array destabilizes.

Real-time animated events:

- holograms distort
- warning lights activate
- researchers turn toward the Array
- emergency shutters begin closing
- sparks fall
- scientists run
- security doors lock
- monitors fail
- quantum distortions spread

The camera follows the chaos rather than showing still images.

---

## SHOT 5 — QUANTUM DISTORTION

A corridor begins existing in several visible configurations.

Use real 3D geometry/VFX to create overlapping possibilities.

Security drones duplicate.

Objects briefly appear in multiple possible positions.

Researchers react physically.

One person runs toward a closing door.

The door shuts before they reach it.

---

## SHOT 6 — FIRST CHHAYA GLIMPSE

Inside an observation laboratory.

A scientist backs away from a glass wall.

Behind the glass, a dark humanoid manifestation begins forming.

It does not simply fade in.

Show particles and unstable geometry assembling into a humanoid structure.

It briefly splits into multiple possible positions.

Camera pushes toward it.

The lights fail before its complete form can be seen.

CUT TO BLACK.

---

# TITLE TRANSITION

Silence.

Display:

ASTRA-7
72 HOURS LATER

Do not remain on static text for long.

Immediately transition back into 3D.

---

# ANIRUDH ARRIVAL

Show Anirudh's transport approaching the damaged station.

The craft loses control.

Thrusters fire.

Debris moves past the windows.

Anirudh physically reacts inside the cockpit.

Warning displays flicker.

The craft impacts Astra-7.

Camera shake.

Metal deformation / sparks / smoke.

Then smoothly transition from cinematic camera to gameplay camera.

Player receives control.

This transition should make the cinematic feel connected directly to the game.

---

# IN-GAME STORY SCENES

Most later dialogue should happen while the player remains inside the real game environment.

Examples:

Ananya hologram:
real animated holographic character.

Lubna recording:
animated 3D hologram / lab reconstruction.

Chhaya introduction:
actual real-time entrance animation.

Chhaya Prime:
actual transformation sequence.

Do not replace these with still illustrations unless development time absolutely requires it.

---

# CHHAYA ANIMATION

Chhaya should feel physically unnatural.

Possible movement:

- delayed limbs
- sudden short movements
- smooth floating transitions
- fragmented body pieces
- multiple overlapping poses
- temporary duplicates
- geometry assembling/disassembling
- limbs briefly appearing in alternative positions

Movement should still be readable enough for gameplay.

---

# CHHAYA PRIME TRANSFORMATION

The final transformation should be a real-time 3D animation.

Sequence:

Chhaya collapses inward.

Quantum fragments orbit the center.

Multiple possible bodies appear.

The bodies overlap.

Amplitude energy gathers.

Fragments lock together.

Chhaya Prime forms.

Camera rotates around the transformation.

Arena geometry reacts.

Then gameplay resumes.

---

# ENDING CINEMATIC

Must also use real 3D animation.

Anirudh enters the escape craft.

He physically sits / interacts with controls.

Astra-7 collapses behind him.

ANVESH is visible beside him.

ANVESH activates by itself.

Anirudh turns toward it.

His posture changes when the unknown voice speaks.

Camera slowly moves outside the craft.

Earth appears.

Then distant structures activate sequentially.

Use animated lights / geometry.

Thousands activate.

Unknown voice:

"Ananya Rao taught your world how to search."

"Lubna taught it how to amplify."

"Now we know how to search back."

Cut to black.

NULL SIGNAL

THE SEARCH HAS ONLY BEGUN

---

# UNITY IMPLEMENTATION

Prefer:

- Unity Timeline
- Animator Controllers
- Animation Clips
- Cinemachine
- Timeline Signals
- particle/VFX systems
- scripted environment animation
- animation events where appropriate

Cinematics should be controllable through a reusable CutsceneController.

Do not tightly couple game logic to one Timeline.

Gameplay should regain control cleanly after each cinematic.

---

# DEVELOPMENT STRATEGY

Do not build final cinematics before core gameplay works.

Order:

1. gameplay prototype
2. quantum mechanic
3. player/enemy interaction
4. level blockout
5. character models
6. animation
7. cinematics
8. polish

During early development, temporary primitives and placeholder animations are acceptable.

Final presentation should replace them with animated 3D sequences.

---

# PERFORMANCE

Remember that the target is WebGL.

Avoid extremely heavy cinematic rendering.

Prefer:
- optimized character models
- reasonable bone counts
- compressed animations
- reusable materials
- controlled particle counts
- limited real-time lights

The goal is strong cinematic direction, not photorealism.

---

# ABSOLUTE CINEMATIC RULE

The viewer should be able to mute the narration and still SEE events happening.

Story must be communicated through:
- character movement
- camera movement
- environment motion
- reactions
- VFX
- staging

Narration supports the cinematic.

Narration must not replace animation.