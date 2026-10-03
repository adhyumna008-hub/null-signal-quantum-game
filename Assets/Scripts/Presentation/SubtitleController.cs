using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NullSignal.Presentation
{
    [Serializable]
    public struct SubtitleCue
    {
        public string speaker;
        [TextArea(1, 2)] public string text;
        public float duration;
        public SubtitleCue(string who, string words, float seconds = 3f) { speaker = who; text = words; duration = seconds; }
    }
    public sealed class SubtitleController : MonoBehaviour
    {
        [SerializeField] private Text speakerText, subtitleText;
        [SerializeField] private GameObject panel;
        [SerializeField] private DialogueAudioPlayback voice;
        private readonly Queue<SubtitleCue> queue = new Queue<SubtitleCue>();
        private readonly HashSet<string> played = new HashSet<string>();
        private float remaining;
        private bool paused;
        private CanvasGroup opacity;
        public event Action<SubtitleCue> CueStarted;
        public event Action CueFinished;
        public string Speaker { get; private set; }
        public string CurrentText { get; private set; }
        public void Configure(Text speaker, Text words, GameObject background)
        { speakerText = speaker; subtitleText = words; panel = background; }
        public void ConfigureVoice(DialogueAudioPlayback playback) => voice = playback;
        public void SetVoiceMuted(bool value) => voice?.SetMuted(value);
        public void SetPaused(bool value) { paused = value; voice?.SetPaused(value); }
        private void Awake()
        {
            if (panel != null)
            {
                opacity = panel.GetComponent<CanvasGroup>();
                if (opacity == null) opacity = panel.AddComponent<CanvasGroup>();
                opacity.blocksRaycasts = false; opacity.interactable = false;
            }
        }
        public void Play(string trigger, params SubtitleCue[] lines)
        {
            if (!string.IsNullOrEmpty(trigger) && !played.Add(trigger)) return;
            foreach (SubtitleCue line in lines) queue.Enqueue(line);
        }
        public void Clear()
        {
            queue.Clear(); remaining = 0f; voice?.Stop();
            if (Speaker != null) CueFinished?.Invoke();
            Speaker = CurrentText = null; if (panel != null) panel.SetActive(false);
        }
        private void Update()
        {
            if (paused || Time.timeScale <= 0f) return;
            if (opacity != null) opacity.alpha = Mathf.MoveTowards(opacity.alpha, 1f, Time.unscaledDeltaTime * 7f);
            remaining -= Time.unscaledDeltaTime;
            if (remaining > 0f) return;
            if (Speaker != null) { voice?.Stop(); CueFinished?.Invoke(); Speaker = CurrentText = null; }
            if (queue.Count == 0) { if (panel != null) panel.SetActive(false); return; }
            SubtitleCue line = queue.Dequeue(); Speaker = line.speaker; CurrentText = line.text;
            float audioDuration = voice != null ? voice.Play(line) : 0f;
            remaining = Mathf.Max(1f, line.duration, audioDuration + .15f);
            if (speakerText != null) { speakerText.text = line.speaker; speakerText.color = SpeakerColor(line.speaker); }
            if (subtitleText != null) subtitleText.text = line.text;
            if (panel != null) panel.SetActive(true);
            if (opacity != null) opacity.alpha = 0f;
            CueStarted?.Invoke(line);
        }
        public static Color SpeakerColor(string speaker)
        {
            switch ((speaker ?? string.Empty).ToUpperInvariant())
            {
                case "ANANYA": return new Color(1f, .9f, .71f);
                case "LUBNA": return new Color(1f, .69f, .42f);
                case "ANIRUDH": return new Color(.89f, .93f, .95f);
                case "ANVESH": return new Color(.61f, .94f, .74f);
                case "CHHAYA": return new Color(.88f, .63f, 1f);
                case "UNKNOWN SIGNAL": return new Color(.88f, .63f, 1f);
                default: return new Color(.44f, .89f, .96f);
            }
        }
        private void OnDisable() => Clear();
    }
}
