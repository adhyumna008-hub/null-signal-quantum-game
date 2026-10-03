using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NullSignal.Player;
using NullSignal.Presentation;
using UnityEditor;
using UnityEngine;

namespace NullSignal.Editor
{
    internal static class NullSignalAudioBuilder
    {
        private const string LibraryPath = "Assets/Audio/Dialogue/NULL SIGNAL Voiceover.asset";
        internal static DialogueAudioPlayback Configure(Transform root, SubtitleController subtitles, StationFeedbackAudio feedback)
        {
            VoiceoverLibrary library = Library();
            DialogueAudioPlayback voice = subtitles != null ? subtitles.GetComponentInChildren<DialogueAudioPlayback>(true) : root.GetComponentInChildren<DialogueAudioPlayback>(true);
            if (voice == null)
            {
                var item = new GameObject("Dialogue voice / optional baked performances");
                item.transform.SetParent(subtitles != null ? subtitles.transform : root, false);
                voice = item.AddComponent<DialogueAudioPlayback>();
            }
            string[] speakers = { "ANANYA", "LUBNA", "ANIRUDH", "TARA", "ANVESH", "CHHAYA", "UNKNOWN SIGNAL" };
            var channels = new DialogueAudioPlayback.CharacterVoice[speakers.Length];
            for (int i = 0; i < speakers.Length; i++)
            {
                Transform channel = voice.transform.Find("Voice - " + speakers[i]);
                if (channel == null)
                { channel = new GameObject("Voice - " + speakers[i]).transform; channel.SetParent(voice.transform, false); }
                AudioSource output = channel.GetComponent<AudioSource>();
                if (output == null) output = channel.gameObject.AddComponent<AudioSource>();
                output.playOnAwake = false; output.spatialBlend = 0f; output.priority = 24;
                channels[i] = new DialogueAudioPlayback.CharacterVoice { speaker = speakers[i], source = output };
            }
            voice.Configure(library, channels);
            if (subtitles != null) subtitles.ConfigureVoice(voice);
            if (feedback != null) feedback.ConfigureVoice(voice);
            foreach (PlayerDodge player in root.GetComponentsInChildren<PlayerDodge>(true))
            {
                PlayerAudioFeedback binding = player.GetComponent<PlayerAudioFeedback>();
                if (binding == null) binding = player.gameObject.AddComponent<PlayerAudioFeedback>();
                binding.Configure(feedback);
            }
            foreach (LineRenderer line in root.GetComponentsInChildren<LineRenderer>(true))
            {
                if (line.name != "Committed strike footprint") continue;
                ThreatAudioFeedback binding = line.GetComponent<ThreatAudioFeedback>();
                if (binding == null) binding = line.gameObject.AddComponent<ThreatAudioFeedback>();
                binding.Configure(feedback, line);
            }
            return voice;
        }

        private static VoiceoverLibrary Library()
        {
            Astra7VisualKit.Folder("Assets/Audio/Dialogue");
            VoiceoverLibrary library = AssetDatabase.LoadAssetAtPath<VoiceoverLibrary>(LibraryPath);
            if (library == null)
            { library = ScriptableObject.CreateInstance<VoiceoverLibrary>(); AssetDatabase.CreateAsset(library, LibraryPath); }
            var keys = new HashSet<string>();
            foreach (VoiceoverLibrary.Line line in library.Lines)
                if (line != null) keys.Add(VoiceoverLibrary.Key(line.speaker, line.text));
            // Retain every assigned recording when the scene is rebuilt; append only new exact lines.
            var pattern = new Regex(@"(?:new\s+SubtitleCue|Cue)\(\s*""(?<speaker>(?:\\.|[^""\\])*)""\s*,\s*""(?<text>(?:\\.|[^""\\])*)""");
            foreach (string file in Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories))
                foreach (Match match in pattern.Matches(File.ReadAllText(file)))
                    Add(library, keys, Regex.Unescape(match.Groups["speaker"].Value), Regex.Unescape(match.Groups["text"].Value));
            Add(library, keys, "LUBNA", "Background search complete.");
            Add(library, keys, "ANANYA", "Again.");
            Add(library, keys, "LUBNA", "That signal was barely there.");
            Add(library, keys, "ANANYA", "One more iteration.");
            Add(library, keys, "UNKNOWN SIGNAL", "You found me.");
            Add(library, keys, "TARA", "Emergency retrieval protocol active.");
            Add(library, keys, "ANIRUDH", "Find the Core. Shut it down.");
            Add(library, keys, "ANIRUDH", "I destroyed the Array.");
            Add(library, keys, "UNKNOWN SIGNAL", "You destroyed one.");
            Add(library, keys, "UNKNOWN SIGNAL", "Now we know how to search back.");
            EditorUtility.SetDirty(library);
            return library;
        }
        private static void Add(VoiceoverLibrary library, HashSet<string> keys, string speaker, string text)
        {
            if (!keys.Add(VoiceoverLibrary.Key(speaker, text))) return;
            library.Lines.Add(new VoiceoverLibrary.Line { speaker = speaker, text = text, direction = Direction(speaker) });
        }
        private static string Direction(string speaker)
        {
            switch (speaker.ToUpperInvariant())
            {
                case "ANANYA": return "Mature Indian English scientist; controlled authority, curiosity giving way to restrained alarm. Distinct from Lubna.";
                case "LUBNA": return "Warm, lower female Indian English register; reflective and observant, then quietly unsettled.";
                case "ANIRUDH": return "Male Indian English retrieval specialist; grounded, breath-aware urgency and relief, never theatrical shouting.";
                case "TARA": return "Clear, calm female assistant; empathetic human-like phrasing, concise and practical under pressure.";
                case "ANVESH": return "Neutral, concise interface voice; precise but intelligible and unhurried. Distinct from Tara.";
                case "CHHAYA": return "Very quiet, unfamiliar resonant voice; one-word challenge, minimal processing, readable consonants.";
                case "UNKNOWN SIGNAL": return "Low, intimate and controlled; ominous through calm delivery and pauses, not shouting or heavy distortion.";
                default: return "Natural intelligible performance matched to this character and the scene.";
            }
        }
    }
}
