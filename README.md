# NULL SIGNAL

A signal answered from Astra-7. Enter the damaged research station, turn competing quantum possibilities into a measurable target, and shut down the experiment before it learns to search back.

## Quantum Concept

The game teaches **quantum amplitude amplification**, using a **Grover-style unstructured search** application. Encounters use real calculated amplitudes for 4, 8 and 16 states, with exactly one target:

1. Equal starting amplitudes: `1 / sqrt(N)`.
2. MARK applies the Oracle phase inversion to the target.
3. AMPLIFY redistributes amplitudes by reflecting each around their mean.
4. MARK and AMPLIFY may be repeated.
5. Too many iterations naturally overshoot: success probability falls again.
6. LOCK randomly samples the calculated distribution, where `P(i) = amplitude(i)²`.

Wave direction, height, intensity, chance readouts and collapse reflect these values. The original simulation is independent from Unity objects, UI and effects. A missed measurement is recoverable; success is never substituted with a scripted probability.

## Controls

| Input | Action |
|---|---|
| WASD / arrows | Move |
| Space | Dodge; skip opening/ending during cinematics |
| Left mouse | Aim and fire |
| Q | SCAN |
| 1 / 2 / 3 | MARK / AMPLIFY / LOCK |
| E | Use nearby mission device |
| R | Reset the current encounter |
| F3 | Separate developer values |
| Esc | Pause; skip opening/ending during cinematics |

## How to Play

Choose **WATCH BACKSTORY**, then **PLAY** after the animated opening (or skip it). Recover ANVESH, observe the first assisted search, and apply the same four actions to puzzles and hostile manifestations. Repeat MARK before each AMPLIFY. Watch the target's chance before choosing LOCK. Correct combat measurements expose the real body for a short attack window. Follow open eastern bulkheads through the station; checkpoints keep retries local.

## Learning Design

See → try → notice → short explanation → apply again. Short explanations follow visible changes; repeated actions and late combat emphasize live values. No quizzes or prior quantum-mechanics knowledge. Scientific terminology is attached to actions already learned.

## Technology

Unity **6000.6.4f1**, Universal Render Pipeline, Unity Input System, TextMeshPro and WebGL. Procedural station kit, animated 3D characters, scripted continuous opening/ending cameras, and lightweight action/audio feedback. No additional runtime packages, Python or external API services required.

## Run

Hosted link: **https://null-signal-quantum-game.vercel.app**

In Unity: **Tools → NULL SIGNAL → Build Phase 7 Final Game** generates the connected `Assets/Scenes/MainGame.unity` scene, including its animated menu, backstory, gameplay, pause and ending. Previous scenes are archived/preserved. **Tools → NULL SIGNAL → Build Final WebGL** creates `Builds/WebGL/`.

Deploy that whole folder to a static HTTPS host with no build command. The WebGL template includes loading/error states and fullscreen. Uncompressed output avoids special compression-header requirements. Desktop keyboard and mouse are recommended.

## Presentation and Audio

The opening and ending use moving 3D geometry, character acting, machines and camera shots, rather than still images. Colors distinguish science (cyan), anomaly (violet), danger (red), access/success (green), and architecture (graphite, ivory, titanium and copper).

Subtitles and sound effects ship by default. Ananya, Lubna, Anirudh, Tara, ANVESH, Chhaya and the unknown signal have separate ready-to-fill voice channels and clip slots. Emotional voice performances have **not** been recorded or generated. Add licensed recordings to the generated voiceover library; subtitles remain complete without them. Inter font uses the SIL Open Font License; see its included license under `Assets/Art/Astra7/Fonts/`.

## Team

- Team: **[ADD TEAM NAME]**
- Members: **[ADD MEMBER NAMES]**
- Event: **[ADD HACKATHON / EVENT]**
- Tools: Unity / URP / Codex-assisted development

Final manual smoke-test and recording checklist: [Docs/SUBMISSION_CHECKLIST.md](Docs/SUBMISSION_CHECKLIST.md).
