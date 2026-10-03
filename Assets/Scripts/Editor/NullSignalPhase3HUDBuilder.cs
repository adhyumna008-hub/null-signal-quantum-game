using NullSignal.Gameplay;
using NullSignal.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace NullSignal.Editor
{
    internal static class NullSignalPhase3HUDBuilder
    {
        private static readonly Color Ink = new Color(0.025f, 0.045f, 0.065f, 0.92f);
        private static readonly Color Cyan = new Color(0.39f, 0.86f, 0.91f);
        private static readonly Color Muted = new Color(0.53f, 0.66f, 0.73f);
        private static readonly Color Warm = new Color(0.87f, 0.62f, 0.34f);
        private static Font font;

        internal static AnveshHUD Build(Transform parent, QuantumEncounter encounter, AnveshController controller, int slots = 0)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var ui = new GameObject("ANVESH / Prototype HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            ui.transform.SetParent(parent, false);
            ui.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            ui.GetComponent<Canvas>().pixelPerfect = true;
            var scaler = ui.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f); scaler.matchWidthOrHeight = 0.5f;

            RectTransform identity = Panel("Operative identity", ui.transform, new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(330f, 76f));
            Label(identity, "AV", 25, new Vector2(15f, -13f), new Vector2(48f, 48f), Warm, TextAnchor.MiddleCenter);
            Rule(identity, new Vector2(78f, -14f), new Vector2(1f, 48f), Muted * 0.6f);
            Label(identity, "ANIRUDH VARMA", 18, new Vector2(94f, -14f), new Vector2(224f, 24f), Color.white);
            Label(identity, "ANVESH  /  QUANTUM INTERFACE", 11, new Vector2(94f, -44f), new Vector2(224f, 18f), Cyan);

            RectTransform mission = Panel("Objective", ui.transform, Vector2.one, new Vector2(-24f, -24f), new Vector2(420f, 76f));
            Text objective = Label(mission, "RESOLVE THE AUTHORIZATION\nQ  Scan the four quantum nodes.", 16,
                new Vector2(18f, -13f), new Vector2(394f, 53f), Color.white);
            objective.lineSpacing = 1.25f;
            Text location = Label(ui.transform, "ASTRA–7    /    QUANTUM SYSTEMS", 14, Vector2.zero, new Vector2(520f, 30f), Muted, TextAnchor.MiddleCenter);
            Place(location.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -35f), new Vector2(520f, 30f));
            Text feedback = Label(ui.transform, "ANVESH  /  SEARCH INTERFACE READY", 13, new Vector2(30f, -116f), new Vector2(750f, 26f), Cyan);
            Text warning = Label(ui.transform, "!  OVERSHOOT  /  TARGET STRENGTH DECLINED", 15, new Vector2(30f, -145f), new Vector2(690f, 30f), Warm);
            warning.gameObject.SetActive(false);

            RectTransform dock = Panel("Search controls", ui.transform, new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(1552f, 124f));
            Label(dock, "SEARCH ARRAY", 12, new Vector2(18f, -14f), new Vector2(150f, 22f), Cyan);
            Text counts = Label(dock, "STATES  04\nITERATION  00", 17, new Vector2(18f, -48f), new Vector2(153f, 61f), Color.white);
            counts.lineSpacing = 1.35f;
            Rule(dock, new Vector2(167f, -16f), new Vector2(1f, 90f), Muted * 0.4f);
            string[] names = { "SCAN", "MARK", "AMPLIFY", "LOCK", "RESET" };
            string[] keys = { "Q", "1", "2", "3", "R" };
            string[] hints = { "ACQUIRE", "PHASE", "REDISTRIBUTE", "MEASURE", "RETRY" };
            var buttons = new Button[5];
            for (int i = 0; i < buttons.Length; i++)
            {
                float x = 184f + i * 132f;
                var button = new GameObject(names[i], typeof(RectTransform), typeof(Image), typeof(Button)); button.transform.SetParent(dock, false);
                Place((RectTransform)button.transform, new Vector2(0f, 1f), new Vector2(x, -17f), new Vector2(118f, 68f));
                Image background = button.GetComponent<Image>(); background.color = Color.white;
                Button control = button.GetComponent<Button>(); control.targetGraphic = background;
                ColorBlock colors = control.colors;
                colors.normalColor = new Color(0.065f, 0.14f, 0.19f, 0.95f);
                colors.highlightedColor = new Color(0.13f, 0.33f, 0.40f);
                colors.pressedColor = new Color(0.24f, 0.52f, 0.56f);
                colors.selectedColor = colors.normalColor;
                colors.disabledColor = new Color(0.035f, 0.06f, 0.08f, 0.7f);
                colors.fadeDuration = 0.08f; control.colors = colors;
                control.navigation = new Navigation { mode = Navigation.Mode.None };
                Rule(button.transform, new Vector2(0f, 0f), new Vector2(118f, 1f), i == 3 ? Warm : Cyan * 0.7f);
                Label(button.transform, keys[i], 13, new Vector2(10f, -6f), new Vector2(96f, 18f), i == 3 ? Warm : Cyan);
                Label(button.transform, names[i], 17, new Vector2(10f, -29f), new Vector2(100f, 25f), Color.white);
                Label(dock, hints[i], 10, new Vector2(x, -93f), new Vector2(118f, 16f), Muted, TextAnchor.MiddleCenter);
                buttons[i] = control;
            }
            Rule(dock, new Vector2(850f, -16f), new Vector2(1f, 90f), Muted * 0.4f);
            Label(dock, "AMPLITUDE / PHASE", 11, new Vector2(875f, -9f), new Vector2(250f, 17f), Cyan);
            Label(dock, "MEASUREMENT ODDS", 10, new Vector2(1260f, -9f), new Vector2(260f, 17f), Muted, TextAnchor.UpperRight);
            var graphObject = new GameObject("Live signed amplitudes", typeof(RectTransform), typeof(CanvasRenderer), typeof(AmplitudeScopeGraphic));
            graphObject.transform.SetParent(dock, false);
            Place((RectTransform)graphObject.transform, new Vector2(0f, 1f), new Vector2(868f, -31f), new Vector2(660f, 49f));
            var scope = graphObject.GetComponent<AmplitudeScopeGraphic>(); scope.Configure(encounter); scope.color = Cyan;
            var labels = new Text[slots > 0 ? slots : encounter.CandidateCount];
            for (int i = 0; i < labels.Length; i++)
                labels[i] = Label(dock, $"{i + 1:00} / --", 13, new Vector2(868f + i * 660f / labels.Length, -88f),
                    new Vector2(660f / labels.Length, 23f), Color.white, TextAnchor.MiddleCenter);

            Text controls = Label(ui.transform, "WASD  MOVE     SPACE  DODGE     E  INSPECT     F3  DEVELOPER HUD", 11, Vector2.zero, new Vector2(1000f, 20f), Muted, TextAnchor.MiddleCenter);
            Place(controls.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(1000f, 20f));
            RectTransform debug = Panel("Developer values / F3", ui.transform, Vector2.one, new Vector2(-24f, -113f), new Vector2(350f, 245f));
            Text debugText = Label(debug, "", 15, new Vector2(16f, -13f), new Vector2(322f, 220f), Muted); debugText.lineSpacing = 1.3f;
            debug.gameObject.SetActive(false);
            AnveshHUD hud = ui.AddComponent<AnveshHUD>();
            hud.Configure(encounter, controller, objective, feedback, counts, warning, debugText, debug.gameObject, buttons, labels, scope);

            var eventSystem = new GameObject("UI Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(parent, false);
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            return hud;
        }

        private static RectTransform Panel(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(Image)); item.transform.SetParent(parent, false);
            item.GetComponent<Image>().color = Ink; item.GetComponent<Image>().raycastTarget = false;
            var rect = (RectTransform)item.transform; Place(rect, anchor, position, size);
            Rule(rect, Vector2.zero, new Vector2(size.x, 1f), new Color(0.25f, 0.52f, 0.62f, 0.8f));
            Rule(rect, new Vector2(0f, -size.y + 1f), new Vector2(size.x, 1f), new Color(0.25f, 0.40f, 0.49f, 0.5f));
            return rect;
        }
        private static void Rule(Transform parent, Vector2 position, Vector2 size, Color color)
        {
            var item = new GameObject("Fine border", typeof(RectTransform), typeof(Image)); item.transform.SetParent(parent, false);
            item.GetComponent<Image>().color = color; item.GetComponent<Image>().raycastTarget = false;
            Place((RectTransform)item.transform, new Vector2(0f, 1f), position, size);
        }
        private static Text Label(Transform parent, string value, int size, Vector2 position, Vector2 dimensions, Color color, TextAnchor alignment = TextAnchor.UpperLeft)
        {
            var item = new GameObject(string.IsNullOrEmpty(value) ? "Readout" : value.Replace('\n', ' '), typeof(RectTransform), typeof(Text)); item.transform.SetParent(parent, false);
            Text text = item.GetComponent<Text>(); text.font = font; text.text = value; text.fontSize = Mathf.Max(16, Mathf.RoundToInt(size * 1.2f)); text.color = color;
            text.alignment = alignment; text.raycastTarget = false; text.supportRichText = false;
            Place(text.rectTransform, new Vector2(0f, 1f), position, dimensions); return text;
        }
        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        { rect.anchorMin = anchor; rect.anchorMax = anchor; rect.pivot = anchor; rect.anchoredPosition = position * 1.2f; rect.sizeDelta = size * 1.2f; }
    }
}
