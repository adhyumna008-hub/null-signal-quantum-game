using UnityEngine;

namespace NullSignal.Presentation
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class StationFeedbackAudio : MonoBehaviour
    {
        private AudioSource source, ambience;
        private AudioClip servo, pulse, charge, release, warning, scan, mark, collapse, dodge, attack, hit, alarm, shutdown, success, roomTone;
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
            var emitter = new GameObject("Station ambience / dialogue ducking"); emitter.transform.SetParent(transform, false);
            ambience = emitter.AddComponent<AudioSource>(); ambience.playOnAwake = false; ambience.loop = true;
            ambience.spatialBlend = 0f; ambience.priority = 220; ambience.clip = roomTone; ambience.volume = .10f;
            muted = PlayerPrefs.GetInt("NullSignal.AudioMuted", 0) != 0; ApplyMute(); ambience.Play();
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
        private void ApplyMute() { if (source != null) source.mute = muted; if (ambience != null) ambience.mute = muted; }
        private void Play(AudioClip clip, float gain)
        { if (source != null && clip != null && !effectivePause && Time.timeScale > 0f) source.PlayOneShot(clip, gain); }
        private void Update()
        {
            RefreshPause();
            if (ambience == null || effectivePause) return;
            float level = silent ? 0f : cinematic ? ending ? .075f : .15f : .10f;
            if (voice != null && voice.IsSpeaking) level *= .35f;
            ambience.volume = Mathf.MoveTowards(ambience.volume, level, Time.unscaledDeltaTime * .12f);
            ambience.pitch = Mathf.MoveTowards(ambience.pitch, cinematic && ending ? .82f : 1f, Time.unscaledDeltaTime * .1f);
        }
        private void RefreshPause()
        {
            bool next = paused || Time.timeScale <= 0f;
            if (next == effectivePause || source == null) return;
            effectivePause = next;
            if (next) { source.Pause(); ambience?.Pause(); }
            else { source.UnPause(); ambience?.UnPause(); }
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
        private void OnEnable() { if (ambience != null && !ambience.isPlaying) ambience.Play(); }
        private void OnDisable() { if (source != null) source.Stop(); if (ambience != null) ambience.Stop(); }
        private void OnDestroy()
        {
            foreach (AudioClip clip in new[] { servo, pulse, charge, release, warning, scan, mark, collapse, dodge, attack, hit, alarm, shutdown, success, roomTone })
                if (clip != null) Destroy(clip);
        }
    }
}
