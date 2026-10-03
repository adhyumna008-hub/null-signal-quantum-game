using UnityEngine;

namespace NullSignal.Presentation
{
    /// <summary>One speaking voice at a time; subtitles remain complete when a recording is missing.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
    public sealed class DialogueAudioPlayback : MonoBehaviour
    {
        [System.Serializable]
        public struct CharacterVoice
        {
            public string speaker;
            public AudioSource source;
        }
        [SerializeField] private VoiceoverLibrary library;
        [SerializeField] private CharacterVoice[] characterVoices = new CharacterVoice[0];
        private AudioSource source, fallback;
        private bool paused, muted, effectivePause;
        public bool IsSpeaking => source != null && source.clip != null && source.isPlaying && !muted;
        public void Configure(VoiceoverLibrary recordings) => library = recordings;
        public void Configure(VoiceoverLibrary recordings, CharacterVoice[] actors)
        { library = recordings; characterVoices = actors ?? new CharacterVoice[0]; }
        private void Awake()
        {
            muted = PlayerPrefs.GetInt("NullSignal.VoiceMuted", 0) != 0;
            fallback = source = GetComponent<AudioSource>(); Prepare(fallback);
            foreach (CharacterVoice actor in characterVoices) Prepare(actor.source);
        }
        private void Prepare(AudioSource channel)
        {
            if (channel == null) return;
            channel.playOnAwake = false; channel.loop = false; channel.spatialBlend = 0f;
            channel.pitch = 1f; channel.priority = 24; channel.mute = muted; channel.Stop();
        }
        public float Play(SubtitleCue cue)
        {
            Stop();
            VoiceoverLibrary.Line line = library != null ? library.Find(cue.speaker, cue.text) : null;
            source = fallback;
            foreach (CharacterVoice actor in characterVoices)
                if (actor.source != null && string.Equals(actor.speaker, cue.speaker, System.StringComparison.OrdinalIgnoreCase))
                { source = actor.source; break; }
            if (line == null || source == null) return 0f;
            source.clip = line.clip; source.volume = line.volume; source.Play();
            if (effectivePause) source.Pause();
            return line.clip.length;
        }
        public void Stop()
        {
            StopChannel(fallback);
            foreach (CharacterVoice actor in characterVoices) StopChannel(actor.source);
        }
        private static void StopChannel(AudioSource channel) { if (channel != null) { channel.Stop(); channel.clip = null; } }
        public void SetMuted(bool value)
        {
            muted = value; PlayerPrefs.SetInt("NullSignal.VoiceMuted", value ? 1 : 0);
            if (fallback != null) fallback.mute = value;
            foreach (CharacterVoice actor in characterVoices) if (actor.source != null) actor.source.mute = value;
        }
        public void SetPaused(bool value) { paused = value; RefreshPause(); }
        private void Update() => RefreshPause();
        private void RefreshPause()
        {
            bool next = paused || Time.timeScale <= 0f;
            if (next == effectivePause || source == null) return;
            effectivePause = next;
            if (next) source.Pause(); else source.UnPause();
        }
        private void OnDisable() => Stop();
    }
}
