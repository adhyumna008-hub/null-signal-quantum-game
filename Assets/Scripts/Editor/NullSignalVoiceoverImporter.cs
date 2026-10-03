using System;
using System.Collections.Generic;
using System.IO;
using NullSignal.Presentation;
using UnityEditor;
using UnityEngine;

namespace NullSignal.Editor
{
    public static class NullSignalVoiceoverImporter
    {
        private const string LibraryPath = "Assets/Audio/Dialogue/NULL SIGNAL Voiceover.asset";
        private const string ManifestPath = "Assets/Audio/Voice/manifest.json";

        [Serializable]
        public class ManifestEntry
        {
            public int index;
            public string speaker;
            public string text;
            public string cleanText;
            public string fileName;
            public string fullPath;
            public string unityRelativePath;
        }

        [MenuItem("NULL SIGNAL/Audio/Import & Connect Voiceovers")]
        public static void ImportAndConnect()
        {
            Debug.Log("[NullSignalVoiceoverImporter] Starting voiceover import & connection...");

            if (!File.Exists(ManifestPath))
            {
                Debug.LogError($"[NullSignalVoiceoverImporter] Manifest not found at {ManifestPath}");
                return;
            }

            string manifestJson = File.ReadAllText(ManifestPath);
            // Wrap in array object for JsonHelper or parse simply
            // Since Unity's JsonUtility does not parse top-level arrays natively, let's wrap it
            string wrappedJson = "{\"items\":" + manifestJson + "}";
            ManifestWrapper wrapper = JsonUtility.FromJson<ManifestWrapper>(wrappedJson);

            if (wrapper == null || wrapper.items == null || wrapper.items.Length == 0)
            {
                Debug.LogError("[NullSignalVoiceoverImporter] Failed to parse manifest items.");
                return;
            }

            VoiceoverLibrary library = AssetDatabase.LoadAssetAtPath<VoiceoverLibrary>(LibraryPath);
            if (library == null)
            {
                Debug.LogError($"[NullSignalVoiceoverImporter] VoiceoverLibrary asset not found at {LibraryPath}");
                return;
            }

            // Step 1: Ensure all audio clips are imported and configured for WebGL
            foreach (ManifestEntry entry in wrapper.items)
            {
                if (File.Exists(entry.unityRelativePath))
                {
                    AssetDatabase.ImportAsset(entry.unityRelativePath, ImportAssetOptions.ForceUpdate);
                    AudioImporter importer = AssetImporter.GetAtPath(entry.unityRelativePath) as AudioImporter;
                    if (importer != null)
                    {
                        importer.forceToMono = true;
                        importer.loadInBackground = true;

                        // Configure WebGL sample settings
                        AudioImporterSampleSettings settings = importer.GetOverrideSampleSettings("WebGL");
                        settings.loadType = AudioClipLoadType.CompressedInMemory;
                        settings.compressionFormat = AudioCompressionFormat.Vorbis;
                        settings.quality = 0.5f;
                        importer.SetOverrideSampleSettings("WebGL", settings);

                        // Also configure default settings
                        AudioImporterSampleSettings defaultSettings = importer.defaultSampleSettings;
                        defaultSettings.loadType = AudioClipLoadType.CompressedInMemory;
                        defaultSettings.compressionFormat = AudioCompressionFormat.Vorbis;
                        defaultSettings.quality = 0.5f;
                        importer.defaultSampleSettings = defaultSettings;

                        importer.SaveAndReimport();
                    }
                }
            }

            AssetDatabase.Refresh();

            // Step 2: Connect audio clips to library lines
            int connectedCount = 0;
            Dictionary<string, AudioClip> clipByNormalizedKey = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);

            foreach (ManifestEntry entry in wrapper.items)
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(entry.unityRelativePath);
                if (clip != null)
                {
                    string key = VoiceoverLibrary.Key(entry.speaker, entry.text);
                    clipByNormalizedKey[key] = clip;
                }
                else
                {
                    Debug.LogWarning($"[NullSignalVoiceoverImporter] Could not load AudioClip at {entry.unityRelativePath}");
                }
            }

            for (int i = 0; i < library.Lines.Count; i++)
            {
                VoiceoverLibrary.Line line = library.Lines[i];
                if (line == null) continue;

                string key = VoiceoverLibrary.Key(line.speaker, line.text);
                if (clipByNormalizedKey.TryGetValue(key, out AudioClip clip))
                {
                    line.clip = clip;
                    connectedCount++;
                }
                else if (i < wrapper.items.Length)
                {
                    // Fallback by index if key normalization had slight whitespace variance
                    AudioClip indexClip = AssetDatabase.LoadAssetAtPath<AudioClip>(wrapper.items[i].unityRelativePath);
                    if (indexClip != null)
                    {
                        line.clip = indexClip;
                        connectedCount++;
                        Debug.Log($"[NullSignalVoiceoverImporter] Assigned by index fallback [{i}]: {wrapper.items[i].fileName}");
                    }
                }
            }

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();

            Debug.Log($"[NullSignalVoiceoverImporter] Successfully connected {connectedCount}/{library.Lines.Count} voice clips to {LibraryPath}!");
        }

        [Serializable]
        public class ManifestWrapper
        {
            public ManifestEntry[] items;
        }
    }
}
