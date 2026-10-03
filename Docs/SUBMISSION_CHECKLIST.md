# NULL SIGNAL submission

- [ ] Hosted WebGL link loads over HTTPS
- [ ] Opening plays and Space / Esc skips to PLAY
- [ ] Controls visible; pause / resume / checkpoint restart work
- [ ] First quantum mechanic understandable without prior quantum knowledge
- [ ] Overshoot demonstrated
- [ ] Drone / Chhaya combat demonstrated
- [ ] Vault N=16 demonstrated
- [ ] Chhaya Prime 4 → 8 → 16 demonstrated
- [ ] Core shutdown, extraction and ending work
- [ ] README team/event/link placeholders filled
- [ ] Repository public if event requires
- [ ] Gameplay video uploaded if event requires

Suggested 60–90 second recording: title/station (6s), SCAN (5s), MARK direction/sign flip (5s), AMPLIFY growth/chance (7s), overshoot (7s), LOCK collapse (6s), drone/Chhaya exposure and attack (8s), Vault (7s), Prime (10s), ending (8s). Use footage from a manual playthrough; do not imply a skipped sequence has been verified.

Deployment folder: `Builds/WebGL/`. Upload the entire folder, including `Build/` and any `StreamingAssets/`. Build uses uncompressed files so no gzip/Brotli response-header configuration is required. Serve through HTTP(S), never `file://`. No backend, accounts or runtime API keys are needed. Vercel/static hosting can use this directory directly with no build command and no SPA rewrite.

Voice performance is optional for this submission: the shipped game uses complete subtitles, differentiated speakers and procedural audio; replace empty per-character slots in `Assets/Audio/Dialogue/NULL SIGNAL Voiceover.asset` with licensed recordings later.
