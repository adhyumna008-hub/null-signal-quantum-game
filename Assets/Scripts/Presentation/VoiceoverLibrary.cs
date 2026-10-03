using System;
using System.Collections.Generic;
using UnityEngine;

namespace NullSignal.Presentation
{
    /// <summary>Baked, licensed voice performances. No network or operating-system speech dependency.</summary>
    [CreateAssetMenu(menuName = "NULL SIGNAL/Voiceover Library")]
    public sealed class VoiceoverLibrary : ScriptableObject
    {
        [Serializable]
        public sealed class Line
        {
            public string speaker;
            [TextArea(1, 3)] public string text;
            [Tooltip("Import a recorded or approved generated performance for this exact line.")]
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = .85f;
            [Tooltip("Performance direction for recording; never changes the actor's pitch at runtime.")]
            public string direction;
        }

        [SerializeField] private List<Line> lines = new List<Line>();
        public List<Line> Lines => lines;
        private Dictionary<string, Line> index;
        public Line Find(string speaker, string text)
        {
            if (index == null)
            {
                index = new Dictionary<string, Line>(StringComparer.Ordinal);
                foreach (Line line in lines)
                    if (line != null && line.clip != null) index[Key(line.speaker, line.text)] = line;
            }
            index.TryGetValue(Key(speaker, text), out Line result);
            return result;
        }

        public static string Key(string speaker, string text) => Normalize(speaker).ToUpperInvariant() + "|" + Normalize(text);
        private static string Normalize(string value) => string.Join(" ", (value ?? string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
        private void OnEnable() => index = null;
        private void OnValidate() => index = null;
    }
}
