# NULL SIGNAL audio handoff

The shipped audio is locally synthesized station machinery and short action effects. This is sound design, not recorded music or human voice acting. No audio assets, authorized neural speech provider, or connected speech tool were available during this pass. The installed Windows desktop voices were deliberately not presented as emotional character performances. Dialogue remains fully subtitled.

`Assets/Audio/Dialogue/NULL SIGNAL Voiceover.asset` is created by the final builder. It contains exact dialogue text, speaker, performance direction, optional AudioClip, and per-line gain. Assign licensed recordings or approved generated clips there; rebuilding preserves assignments. Imported WAV/OGG clips are baked into the Unity build. There is no Windows speech, Python, network service, or API-key dependency at runtime.

Ananya, Lubna, Anirudh, Tara, ANVESH, Chhaya and the unknown signal each have a separate serialized AudioSource slot. Only the speaking character's channel plays; skip, scene exit and Clear stop every channel, including paused voices. Name colors distinguish speakers while dialogue remains neutral and readable: Ananya ivory, Lubna copper, Anirudh silver, Tara cyan, ANVESH green, anomalous voices violet. A quick panel fade introduces the entire readable line without a slow typewriter effect.

## Timing and mix

| Event | Sound / source | Timing | Mix and cleanup |
| --- | --- | --- | --- |
| Station active | Low periodic machinery bed / nonspatial | Continuous after audio context unlock | Quiet loop, 0.35× level during recorded speech; fades between cinematic/gameplay states |
| SCAN | Rising search sweep / ANVESH | Action begins | Short, quieter than dialogue |
| MARK | Two-lobed descending phase cue / ANVESH | Phase action begins | Distinct from amplitude charge |
| AMPLIFY | Charge then release / ANVESH | Anticipation then actual operation commit | Overshoot substitutes a low warning |
| LOCK | Charge then converging tone / ANVESH | Anticipation then real measurement | No audio-driven gameplay changes |
| Dodge / attack / hit | Air displacement / discharge / suit impact | Successful dodge start, actual shot, actual health loss | Input attempts that fail do not play a cue |
| Enemy danger | Short restrained threat tone | Visible committed ground warning | Global 0.55 s cooldown limits overlapping N=16 tones |
| Door | Mechanical actuator | Door starts opening | One short cue |
| Opening failure | Emergency double tone | Cinematic alarm beat | Explicit trigger, not a permanently accumulating loop |
| Core shutdown | Falling electrical tone | Shutdown beat | 1.8 s tail |
| Calm ending | Machinery fades to silence | Cinematic silence beat | Immediate one-shot stop, short ambience fade |
| Dialogue | Optional exact speaker/text recording / nonspatial | Subtitle starts | One voice at a time; subtitle duration extends to the clip; Clear/skip stops it |

Pause freezes subtitle time and pauses voices/SFX/ambience. Voice mute and all-audio mute have separate public hooks; subtitles stay visible. Scene exit destroys generated clips and stops owned playback. Browser audio requires the player's normal initial interaction; Unity handles the WebGL audio context. No autoplay audio is promised before that gesture.

## Performance direction

Ananya is controlled and authoritative; Lubna warm and analytical; Anirudh grounded and urgent; Tara precise and reassuring; ANVESH neutral and brief; the unknown signal quietly ominous. Use distinct cast performances, not one voice shifted in pitch. Suggested emotional delivery is stored with each library entry.

Dialogue synchronization, mix level, and emotional delivery have not been listened to in a running game in this pass. Human review should check that the first action cue, alarms, subtitles, pause/resume, skip, and scene exit remain clear. There are no final voice recordings to evaluate yet.
