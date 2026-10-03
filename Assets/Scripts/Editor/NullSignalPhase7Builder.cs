using System;
using System.IO;
using NullSignal.Presentation;
using NullSignal.Story;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NullSignal.Editor
{
    public static class NullSignalPhase7Builder
    {
        private const string MainScene = "Assets/Scenes/MainGame.unity";
        [MenuItem("Tools/NULL SIGNAL/Build Phase 7 Final Game")]
        public static void BuildFinalGame() => BuildFinalGame(false);
        internal static void BuildFinalGame(bool unattended)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode before building the final game.");
            if (unattended)
            {
                Astra7VisualKit.Folder("Assets/Scenes/Archive");
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.isDirty && scene.IsValid())
                    {
                        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/Archive/Unsaved scene before Phase7.unity");
                        if (!EditorSceneManager.SaveScene(scene, path, true)) throw new IOException("Could not preserve the unsaved scene.");
                    }
                }
            }
            NullSignalPhase4Builder.BuildMainGame(true, true, true, unattended);
            if (SceneManager.GetActiveScene().path != MainScene || UnityEngine.Object.FindAnyObjectByType<FinalGameFlow>() == null)
                throw new InvalidOperationException("Final scene generation was cancelled or did not finish.");
            ConfigureWebSettings(); AssetDatabase.SaveAssets();
        }

        internal static void ConfigurePresentation(Astra7VisualKit kit, Transform root, MainGameDirector game, AnveshHUD hud,
            StationCamera camera, SubtitleController subtitles, StationFeedbackAudio sound)
        {
            game.ConfigureStartFlow();
            hud.GetComponent<FinaleHUD>().UseCinematicEnding();
            StationCinematicDirector cinematic = NullSignalCinematicBuilder.Build(kit, root, subtitles, camera.GetComponent<Camera>());
            NullSignalFinalVisuals.ApplyScene(kit, root);
            NullSignalAudioBuilder.Configure(root, subtitles, sound);
            TMP_FontAsset font = NullSignalSharpUI.Font();
            var canvasObject = new GameObject("NULL SIGNAL / Menus and cinematic subtitles", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(root, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 300;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            var flow = root.gameObject.AddComponent<FinalGameFlow>();
            CanvasGroup hudVisibility = hud.gameObject.AddComponent<CanvasGroup>();
            RectTransform menu = Panel(canvas.transform, "Start menu", new Vector2(620, 750));
            CanvasGroup menuAlpha = menu.gameObject.AddComponent<CanvasGroup>();
            Label(menu, "ASTRA-7 / RECOVERED TRANSMISSION", 17, 30, 30, 560, 28, font, new Color(.92f, .69f, .4f));
            Label(menu, "NULL SIGNAL", 56, 30, 83, 560, 82, font, new Color(.88f, .92f, .9f));
            Label(menu, "They amplified a signal.\nSomething answered.", 25, 30, 193, 530, 84, font, new Color(.65f, .82f, .84f));
            TMP_Text play = Button(menu, "WATCH BACKSTORY", 30, 325, 560, font, flow.StartOrPlay);
            Button(menu, "CONTROLS", 30, 407, 560, font, flow.ShowControls);
            Button(menu, "CREDITS", 30, 489, 560, font, flow.ShowCredits);
            TMP_Text mute = Button(menu, "AUDIO: ON", 30, 582, 265, font, flow.ToggleMute, 18);
            TMP_Text shake = Button(menu, "CAMERA SHAKE: NORMAL", 312, 582, 278, font, flow.ToggleShake, 15);
            Label(menu, "Watch or skip the backstory, then press PLAY.\nHeadphones recommended. Subtitles always available.", 17, 30, 668, 560, 60, font, new Color(.67f, .7f, .68f));

            RectTransform pause = Panel(canvas.transform, "Pause menu", new Vector2(580, 560));
            Label(pause, "PAUSED", 43, 30, 35, 520, 70, font, Color.white);
            Button(pause, "RESUME", 30, 145, 520, font, flow.Resume);
            Button(pause, "RESTART CHECKPOINT", 30, 227, 520, font, flow.RestartCheckpoint);
            Button(pause, "CONTROLS", 30, 309, 520, font, flow.ShowControls);
            Button(pause, "RETURN TO MENU", 30, 391, 520, font, flow.ReturnToMenu);
            Label(pause, "ESC — RESUME", 17, 30, 493, 520, 35, font, new Color(.67f, .78f, .8f));

            RectTransform controls = Panel(canvas.transform, "Controls", new Vector2(750, 800));
            Label(controls, "CONTROLS", 40, 35, 30, 680, 65, font, Color.white);
            Label(controls, "WASD / ARROWS   MOVE\nSPACE   DODGE\nLEFT MOUSE   AIM / ATTACK\n\nQ   SCAN\n1   MARK\n2   AMPLIFY\n3   LOCK\n\nE   INTERACT\nR   RESET ENCOUNTER\nF3   DEBUG VALUES\nESC   PAUSE", 23, 35, 122, 680, 540, font, new Color(.84f, .91f, .9f));
            Button(controls, "BACK", 35, 690, 680, font, flow.Back);
            RectTransform credits = Panel(canvas.transform, "Credits", new Vector2(750, 600));
            Label(credits, "CREDITS", 40, 35, 30, 680, 65, font, Color.white);
            Label(credits, "TEAM NAME: [EDIT IN README]\nTEAM MEMBERS: [ADD NAMES]\nEVENT: [ADD HACKATHON]\n\nUnity 6 / Universal Render Pipeline / WebGL\nOriginal procedural geometry and animation\nInter typeface — SIL Open Font License\nAI-assisted development with Codex\nVoice credits: add when recordings are supplied", 21, 35, 125, 680, 335, font, new Color(.84f, .91f, .9f));
            Button(credits, "BACK", 35, 497, 680, font, flow.Back);
            RectTransform complete = Panel(canvas.transform, "End title / return", new Vector2(930, 460));
            Label(complete, "NULL SIGNAL", 57, 45, 65, 840, 90, font, Color.white);
            Label(complete, "THE SEARCH HAS ONLY BEGUN", 25, 45, 190, 840, 65, font, new Color(.62f, .89f, .92f));
            Button(complete, "RETURN TO MENU", 45, 323, 840, font, flow.ReturnToMenu);
            RectTransform skip = Rect(canvas.transform, "Skip cinematic", new Vector2(1, 1), new Vector2(-28, -28), new Vector2(320, 45));
            Label(skip, "SPACE / ESC — SKIP", 19, 8, 7, 300, 32, font, Color.white);
            RectTransform subtitlePanel = (RectTransform)hud.transform.Find("Subtitles");
            subtitlePanel.SetParent(canvas.transform, false); // Existing TMP proxies and subtitle references stay intact.
            subtitlePanel.SetAsLastSibling();
            flow.Configure(game, cinematic, camera, sound, subtitles, hudVisibility, menu.gameObject, menuAlpha, controls.gameObject,
                credits.gameObject, pause.gameObject, complete.gameObject, skip.gameObject, play, mute, shake, subtitlePanel);
            controls.gameObject.SetActive(false); credits.gameObject.SetActive(false); pause.gameObject.SetActive(false);
            complete.gameObject.SetActive(false); skip.gameObject.SetActive(false);
        }

        private static void ConfigureWebSettings()
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MainScene, true) };
            PlayerSettings.productName = "NULL SIGNAL";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.template = "PROJECT:NullSignal";
            PlayerSettings.runInBackground = false;
        }
        [MenuItem("Tools/NULL SIGNAL/Build Final WebGL")]
        public static void BuildFinalWebGL()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new InvalidOperationException("Unity WebGL Build Support is missing.");
            if (!File.Exists(MainScene)) throw new FileNotFoundException("Build Phase 7 Final Game first.");
            ConfigureWebSettings(); AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Builds/WebGL");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { MainScene }, locationPathName = "Builds/WebGL", target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
            File.WriteAllText("Builds/WebGL/build-status.txt", report.summary.result + "\nBytes: " + report.summary.totalSize + "\nErrors: " + report.summary.totalErrors);
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("WebGL build failed: " + report.summary.result);
            Debug.Log("NULL SIGNAL WebGL ready: " + Path.GetFullPath("Builds/WebGL"));
        }
        public static void BuildFromCommandLine() { BuildFinalGame(true); BuildFinalWebGL(); }

        private static RectTransform Panel(Transform parent, string name, Vector2 size)
        {
            var panel = Rect(parent, name, new Vector2(.5f, .5f), Vector2.zero, size);
            Image image = panel.gameObject.AddComponent<Image>(); image.color = new Color(.025f, .035f, .045f, .97f);
            var rule = Rect(panel, "Copper accent", new Vector2(0, 1), Vector2.zero, new Vector2(size.x, 3));
            rule.gameObject.AddComponent<Image>().color = new Color(.75f, .49f, .25f);
            return panel;
        }
        private static TMP_Text Label(Transform parent, string text, int size, float x, float y, float width, float height, TMP_FontAsset font, Color color)
        {
            TMP_Text label = NullSignalSharpUI.Label(parent, text, size, new Vector2(x, -y), new Vector2(width, height), font);
            label.text = text; label.color = color; return label;
        }
        private static TMP_Text Button(Transform parent, string title, float x, float y, float width, TMP_FontAsset font, UnityAction action, int size = 23)
        {
            var rect = Rect(parent, title, new Vector2(0, 1), new Vector2(x, -y), new Vector2(width, 66));
            Image image = rect.gameObject.AddComponent<Image>(); image.color = Color.white;
            Button button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            ColorBlock colors = button.colors; colors.normalColor = new Color(.095f, .17f, .18f); colors.highlightedColor = new Color(.18f, .32f, .31f);
            colors.pressedColor = new Color(.5f, .33f, .17f); colors.selectedColor = colors.normalColor; colors.fadeDuration = .09f; button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None }; UnityEventTools.AddPersistentListener(button.onClick, action);
            TMP_Text label = Label(rect, title, size, 16, 12, width - 32, 42, font, new Color(.91f, .95f, .91f)); label.alignment = TextAlignmentOptions.Center; return label;
        }
        private static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var item = new GameObject(name, typeof(RectTransform)); item.transform.SetParent(parent, false);
            var rect = (RectTransform)item.transform; rect.anchorMin = rect.anchorMax = rect.pivot = anchor; rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
    }
}
