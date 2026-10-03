using UnityEngine;

namespace NullSignal.Presentation
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class StationFeedbackAudio : MonoBehaviour
    {
        private AudioSource source, ambience, storySource, gameplaySource;
        private AudioClip servo, pulse, charge, release, warning, scan, mark, collapse, dodge, attack, hit, alarm, shutdown, success, roomTone, storyMusic, gameplayMusic;
        [SerializeField] private DialogueAudioPlayback voice;
        private bool muted, paused, effectivePause, cinematic, ending, silent;
        private float lastThreat = -10f;
        public void ConfigureVoice(DialogueAudioPlayback playback) => voice = playback;
        private void Awake()
        {
            source = GetComponent<AudioSource>(); source.playOnAwake = false; source.volume = .28f; source.spatialBlend = 0f; source.priority = 90;
            servo = MakeClip("Door actuator", 0.42f, 105f, true);
            pulse = MakeClip("ANVESH activation", 0.24f, 440f, false);
            charge = MakeClip("Quantum energy build", .30f, 210f, false);
            release = MakeClip("Quantum release", .30f, 640f, false);
            warning = MakeClip("Overshoot warning", .32f, 155f, false);
            scan = Effect("SCAN / search sweep", .65f, 320f, 1240f, .06f);
            mark = Effect("MARK / phase crossing", .33f, 880f, 330f, .015f, true);
            collapse = Effect("LOCK / convergence", .54f, 860f, 85f, .09f);
            dodge = Effect("Dodge / air displacement", .20f, 250f, 125f, .55f);
            attack = Effect("Wrist discharge", .13f, 1040f, 230f, .15f);
            hit = Effect("Suit impact", .15f, 130f, 48f, .65f);
            alarm = Effect("Emergency double tone", .65f, 390f, 520f, .02f, true);
            shutdown = Effect("Core power down", 1.8f, 440f, 36f, .10f);
            success = Effect("Access granted", .33f, 550f, 825f, .01f);
            roomTone = RoomTone();
            storyMusic = StoryMusic();
            gameplayMusic = GameplayMusic();

            var emitter = new GameObject("Station ambience / dialogue ducking"); emitter.transform.SetParent(transform, false);
            ambience = emitter.AddComponent<AudioSource>(); ambience.playOnAwake = false; ambience.loop = true;
            ambience.spatialBlend = 0f; ambience.priority = 220; ambience.clip = roomTone; ambience.volume = .10f;

            var mStory = new GameObject("Story music layer"); mStory.transform.SetParent(transform, false);
            storySource = mStory.AddComponent<AudioSource>(); storySource.playOnAwake = false; storySource.loop = true;
            storySource.spatialBlend = 0f; storySource.priority = 200; storySource.clip = storyMusic; storySource.volume = 0f;

            var mGame = new GameObject("Gameplay music layer"); mGame.transform.SetParent(transform, false);
            gameplaySource = mGame.AddComponent<AudioSource>(); gameplaySource.playOnAwake = false; gameplaySource.loop = true;
            gameplaySource.spatialBlend = 0f; gameplaySource.priority = 205; gameplaySource.clip = gameplayMusic; gameplaySource.volume = 0f;

            muted = PlayerPrefs.GetInt("NullSignal.AudioMuted", 0) != 0; ApplyMute();
            ambience.Play(); storySource.Play(); gameplaySource.Play();
        }
        public void Door() => Play(servo, .75f);
        public void Pulse() => Play(pulse, .55f);
        public void Charge() => Play(charge, .45f);
        public void Release() => Play(release, .65f);
        public void Warning() => Play(warning, .65f);
        public void Scan() => Play(scan, .50f);
        public void Mark() => Play(mark, .60f);
        public void Amplify() => Play(release, .72f);
        public void Lock() => Play(collapse, .80f);
        public void Dodge() => Play(dodge, .65f);
        public void Attack() => Play(attack, .55f);
        public void Hit() => Play(hit, .75f);
        public void Alarm() => Play(alarm, .55f);
        public void Shutdown() => Play(shutdown, .75f);
        public void Success() => Play(success, .60f);
        public void Threat()
        {
            // Dense N=16 encounters must not become sixteen competing alarm tones.
            if (Time.time - lastThreat < .55f) return;
            lastThreat = Time.time; Play(warning, .34f);
        }
        public void Cinematic(bool active, bool isEnding = false) { cinematic = active; ending = isEnding; silent = false; }
        public void Silence() { silent = true; if (source != null) source.Stop(); }
        public void SetMuted(bool value)
        { muted = value; PlayerPrefs.SetInt("NullSignal.AudioMuted", value ? 1 : 0); ApplyMute(); }
        public void SetPaused(bool value) { paused = value; RefreshPause(); }
        private void ApplyMute()
        {
            if (source != null) source.mute = muted;
            if (ambience != null) ambience.mute = muted;
            if (storySource != null) storySource.mute = muted;
            if (gameplaySource != null) gameplaySource.mute = muted;
        }
        private void Play(AudioClip clip, float gain)
        { if (source != null && clip != null && !effectivePause && Time.timeScale > 0f) source.PlayOneShot(clip, gain); }
        private void Update()
        {
            RefreshPause();
            if (effectivePause) return;
            if (ambience != null)
            {
                float level = silent ? 0f : cinematic ? ending ? .075f : .15f : .10f;
                if (voice != null && voice.IsSpeaking) level *= .35f;
                ambience.volume = Mathf.MoveTowards(ambience.volume, level, Time.unscaledDeltaTime * .12f);
                ambience.pitch = Mathf.MoveTowards(ambience.pitch, cinematic && ending ? .82f : 1f, Time.unscaledDeltaTime * .1f);
            }
            // Dynamic music crossfade: story music for cinematic/menu, gameplay music during exploration
            float targetStory = silent ? 0f : cinematic ? 0.22f : 0f;
            float targetGameplay = silent ? 0f : cinematic ? 0f : 0.12f;
            if (voice != null && voice.IsSpeaking)
            {
                targetStory *= 0.40f;
                targetGameplay *= 0.40f;
            }
            float dt = Time.unscaledDeltaTime * 0.6f;
            if (storySource != null) storySource.volume = Mathf.MoveTowards(storySource.volume, targetStory, dt);
            if (gameplaySource != null) gameplaySource.volume = Mathf.MoveTowards(gameplaySource.volume, targetGameplay, dt);
        }
        private void RefreshPause()
        {
            bool next = paused || Time.timeScale <= 0f;
            if (next == effectivePause || source == null) return;
            effectivePause = next;
            if (next) { source.Pause(); ambience?.Pause(); storySource?.Pause(); gameplaySource?.Pause(); }
            else { source.UnPause(); ambience?.UnPause(); storySource?.UnPause(); gameplaySource?.UnPause(); }
        }
        private static AudioClip MakeClip(string name, float duration, float frequency, bool mechanical)
        {
            const int rate = 22050;
            float[] samples = new float[Mathf.CeilToInt(duration * rate)];
            var random = new System.Random(13);
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)rate, u = t / duration;
                float tone = Mathf.Sin(2f * Mathf.PI * frequency * t * (1f - 0.22f * u));
                float noise = mechanical ? ((float)random.NextDouble() * 2f - 1f) * 0.2f : 0f;
                samples[i] = (tone * 0.4f + noise) * Mathf.Sin(Mathf.PI * u) * (1f - u);
            }
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, rate, false); clip.SetData(samples, 0); return clip;
        }
        private static AudioClip Effect(string name, float duration, float startFrequency, float endFrequency, float noiseAmount, bool doublePulse = false)
        {
            const int rate = 22050;
            float[] data = new float[Mathf.CeilToInt(duration * rate)]; var random = new System.Random(31);
            double phase = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)rate, u = t / duration;
                phase += 2.0 * System.Math.PI * Mathf.Lerp(startFrequency, endFrequency, u) / rate;
                float envelope = Mathf.Min(1f, t / .012f) * Mathf.Pow(1f - u, 1.8f);
                if (doublePulse) envelope *= .22f + .78f * Mathf.Abs(Mathf.Sin(u * Mathf.PI * 2f));
                float noise = ((float)random.NextDouble() * 2f - 1f) * noiseAmount;
                data[i] = ((float)System.Math.Sin(phase) * .46f + (float)System.Math.Sin(phase * 2) * .07f + noise) * envelope;
            }
            AudioClip clip = AudioClip.Create(name, data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        private static AudioClip RoomTone()
        {
            const int rate = 22050, length = rate * 4;
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)rate;
                // Exact whole cycles keep the loop seam clean. No streamed asset or audio-thread callback.
                data[i] = (Mathf.Sin(t * Mathf.PI * 110f) * .15f + Mathf.Sin(t * Mathf.PI * 165f) * .05f
                    + Mathf.Sin(t * Mathf.PI * 220f) * .025f) * (.7f + .3f * Mathf.Sin(t * Mathf.PI * .5f));
            }
            AudioClip clip = AudioClip.Create("Astra-7 / restrained machinery bed", length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        private static AudioClip StoryMusic()
        {
            const int rate = 22050, length = rate * 4;
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)rate;
                // Whole cycles in 4s: 33Hz (132c), 66Hz (264c), 99Hz (396c)
                float drone = Mathf.Sin(2f * Mathf.PI * 33f * t) * 0.35f
                            + Mathf.Sin(2f * Mathf.PI * 66f * t) * 0.18f
                            + Mathf.Sin(2f * Mathf.PI * 99f * t) * 0.08f;
                // Tension pulse: 1 pulse per sec
                float pulse = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(2f * Mathf.PI * 1f * t)), 8f) * 0.22f;
                // High resonant harmonic: 528Hz (2112c in 4s)
                float eerie = Mathf.Sin(2f * Mathf.PI * 528f * t + Mathf.Sin(t * 1.57f) * 1.2f) * 0.05f;
                float edge = Mathf.Min(1f, t / 0.02f) * Mathf.Min(1f, (4f - t) / 0.02f);
                data[i] = (drone + pulse + eerie) * 0.75f * edge;
            }
            AudioClip clip = AudioClip.Create("NULL SIGNAL / Story Tension", length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        private static AudioClip GameplayMusic()
        {
            const int rate = 22050, length = rate * 4;
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)rate;
                // Ambient pad: A3 (220Hz = 880c), C#4 (277Hz = 1108c), E4 (330Hz = 1320c)
                float pad = Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.15f
                          + Mathf.Sin(2f * Mathf.PI * 277f * t) * 0.11f
                          + Mathf.Sin(2f * Mathf.PI * 330f * t) * 0.09f;
                float lfo = 0.65f + 0.35f * Mathf.Sin(2f * Mathf.PI * 0.25f * t);
                float shimmer = Mathf.Sin(2f * Mathf.PI * 1320f * t) * 0.015f * Mathf.Max(0f, Mathf.Sin(2f * Mathf.PI * 0.5f * t));
                float edge = Mathf.Min(1f, t / 0.02f) * Mathf.Min(1f, (4f - t) / 0.02f);
                data[i] = (pad * lfo + shimmer) * 0.65f * edge;
            }
            AudioClip clip = AudioClip.Create("NULL SIGNAL / Gameplay Ambient", length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        private void OnEnable() { if (ambience != null && !ambience.isPlaying) ambience.Play(); }
        private void OnDisable() { if (source != null) source.Stop(); if (ambience != null) ambience.Stop(); storySource?.Stop(); gameplaySource?.Stop(); }
        private void OnDestroy()
        {
            foreach (AudioClip clip in new[] { servo, pulse, charge, release, warning, scan, mark, collapse, dodge, attack, hit, alarm, shutdown, success, roomTone, storyMusic, gameplayMusic })
                if (clip != null) Destroy(clip);
        }
    }
}
