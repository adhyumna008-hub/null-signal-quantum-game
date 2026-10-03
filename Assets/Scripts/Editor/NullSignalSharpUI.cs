using System;
using NullSignal.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace NullSignal.Editor
{
    internal static class NullSignalSharpUI
    {
        private const string FontPath = Astra7VisualKit.Root + "/Fonts/Inter SDF.asset";
        internal static TMP_FontAsset Font()
        {
            // TMP is already part of the installed uGUI package. No package import or manual setup.
            AssetDatabase.ImportAsset(Astra7VisualKit.Root + "/Fonts/Resources/TMP Settings.asset", ImportAssetOptions.ForceSynchronousImport);
            TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
            if (settings == null) throw new InvalidOperationException("NULL SIGNAL TMP Settings are missing.");
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null)
            {
                try
                {
                    const string sourcePath = Astra7VisualKit.Root + "/Fonts/Inter-Regular.otf";
                    AssetDatabase.ImportAsset(sourcePath, ImportAssetOptions.ForceSynchronousImport);
                    Font source = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
                    if (source != null)
                    {
                        font = TMP_FontAsset.CreateFontAsset(source, 64, 8, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
                        if (font != null)
                        {
                            font.name = "NULL SIGNAL Inter SDF";
                            AssetDatabase.CreateAsset(font, FontPath);
                            font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,:;!?+-=/()[]_%'|\nΣ√²→↓−×—·", out string _);
                            foreach (Texture2D atlas in font.atlasTextures)
                            { atlas.name = "Inter SDF atlas"; atlas.filterMode = FilterMode.Bilinear; if (!AssetDatabase.Contains(atlas)) AssetDatabase.AddObjectToAsset(atlas, font); }
                            font.material.name = "Inter SDF / sharp, no glow";
                            AssetDatabase.AddObjectToAsset(font.material, font);
                            font.material.SetFloat(ShaderUtilities.ID_FaceDilate, 0f);
                            font.material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0f);
                            EditorUtility.SetDirty(font);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Could not create Inter SDF dynamically in this mode: " + ex.Message + ". Falling back to default TMP font.");
                    font = null;
                }
            }
            if (font == null)
            {
                // Fallback to LiberationSans SDF from imported TMP resources
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
                if (font == null)
                    font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (font == null)
                {
                    string[] existingFonts = AssetDatabase.FindAssets("t:TMP_FontAsset");
                    foreach (string guid in existingFonts)
                    {
                        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                        if (font != null) break;
                    }
                }
            }
            if (font == null) throw new InvalidOperationException("No TMP Font Asset could be found or created.");
            TMP_Settings.defaultFontAsset = font; EditorUtility.SetDirty(settings);
            return font;
        }
        internal static TMP_Text Label(Transform parent, string name, int size, Vector2 position, Vector2 dimensions, TMP_FontAsset font)
        {
            var item = new GameObject(name, typeof(RectTransform)); item.transform.SetParent(parent, false);
            var rect = (RectTransform)item.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position; rect.sizeDelta = dimensions;
            var text = item.AddComponent<TextMeshProUGUI>(); text.font = font; text.fontSize = size; text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.Normal; text.overflowMode = TextOverflowModes.Truncate;
            text.color = new Color(.87f, .94f, 1f); text.raycastTarget = false; text.richText = false; text.text = "";
            text.extraPadding = true; return text;
        }
        internal static void UpgradeHUD(AnveshHUD hud, TMP_FontAsset font)
        {
            CanvasScaler scaler = hud.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            hud.GetComponent<Canvas>().pixelPerfect = true;
            foreach (Text original in hud.GetComponentsInChildren<Text>(true))
            {
                TMP_Text output = Label(original.transform, "Crisp SDF display", Mathf.Max(18, original.fontSize), Vector2.zero, Vector2.zero, font);
                RectTransform rect = output.rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one;
                output.alignment = Alignment(original.alignment);
                original.gameObject.AddComponent<SharpHudText>().Configure(original, output);
            }
        }
        private static TextAlignmentOptions Alignment(TextAnchor alignment)
        {
            switch (alignment)
            {
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.TopLeft;
            }
        }
    }
}
