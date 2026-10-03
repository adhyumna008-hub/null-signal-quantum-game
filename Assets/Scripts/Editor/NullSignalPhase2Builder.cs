using System;
using NullSignal.Gameplay;
using NullSignal.Player;
using NullSignal.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NullSignal.Editor
{
    public static class NullSignalPhase2Builder
    {
        private const string Materials = "Assets/Materials/AmplitudePrototype";

        [MenuItem("Tools/NULL SIGNAL/Build Amplitude Amplification Prototype")]
        public static void BuildAmplitudeAmplificationPrototype()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Exit Play Mode before building the Phase 2 prototype.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CreateAndSavePrototype();
        }

        public static string CreateAndSavePrototype()
        {
            NullSignalGameBuilder.ValidateQuantumFoundation();
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            Shader waveShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (lit == null || waveShader == null) throw new InvalidOperationException("Required existing URP shaders are unavailable.");
            EnsureFolder(Materials);
            EnsureFolder("Assets/Scenes");
            Material room = MaterialAsset("Room", lit, new Color(0.2f, 0.24f, 0.29f), false);
            Material playerMaterial = MaterialAsset("Player", lit, new Color(0.65f, 0.7f, 0.75f), false);
            Material signal = MaterialAsset("Signal", lit, new Color(0.1f, 0.85f, 0.95f, 0.5f), true);
            Material wave = MaterialAsset("Wave", waveShader, Color.white, true);
            Material particle = MaterialAsset("Flow", lit, new Color(0.1f, 0.85f, 0.95f), false);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("NULL SIGNAL - Phase 2 Amplitude Encounter");
            QuantumEncounter encounter = root.AddComponent<QuantumEncounter>();
            encounter.Configure(4, 0, true);
            encounter.RestartEncounter(); // Equal preview; runtime Awake initializes a fresh encounter.
            Primitive("Floor", PrimitiveType.Cube, root.transform, new Vector3(0f, -0.25f, 0f), new Vector3(16f, 0.5f, 12f), room);
            Primitive("North wall", PrimitiveType.Cube, root.transform, new Vector3(0f, 0.7f, 6f), new Vector3(16f, 1.4f, 0.4f), room);
            Primitive("South boundary", PrimitiveType.Cube, root.transform, new Vector3(0f, 0.17f, -6f), new Vector3(16f, 0.34f, 0.4f), room);
            Primitive("West boundary", PrimitiveType.Cube, root.transform, new Vector3(-8f, 0.17f, 0f), new Vector3(0.4f, 0.34f, 12f), room);
            Primitive("East boundary", PrimitiveType.Cube, root.transform, new Vector3(8f, 0.7f, 0f), new Vector3(0.4f, 1.4f, 12f), room);

            var player = new GameObject("Player - ANVESH");
            player.transform.SetParent(root.transform, false);
            player.transform.position = new Vector3(0f, 0.05f, -2.5f);
            CharacterController motor = player.AddComponent<CharacterController>();
            motor.height = 1.8f; motor.radius = 0.35f; motor.center = new Vector3(0f, 0.9f, 0f);
            motor.stepOffset = 0.2f; motor.skinWidth = 0.035f;
            PlayerController movement = player.AddComponent<PlayerController>();
            player.AddComponent<PlayerInteraction>();
            AnveshController anvesh = player.AddComponent<AnveshController>();
            anvesh.Configure(encounter);
            GameObject body = Primitive("Placeholder body", PrimitiveType.Capsule, player.transform,
                new Vector3(0f, 0.9f, 0f), new Vector3(0.7f, 0.9f, 0.7f), playerMaterial);
            UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());
            GameObject facing = Primitive("Facing indicator", PrimitiveType.Cube, player.transform,
                new Vector3(0f, 1.1f, 0.38f), new Vector3(0.18f, 0.18f, 0.22f), particle);
            UnityEngine.Object.DestroyImmediate(facing.GetComponent<Collider>());

            var visualizers = new CandidateVisualizer[4];
            Vector3 cameraRight = Quaternion.Euler(0f, 45f, 0f) * Vector3.right;
            for (int i = 0; i < visualizers.Length; i++)
            {
                var node = new GameObject($"Candidate {i + 1}");
                node.transform.SetParent(root.transform, false);
                node.transform.position = cameraRight * (-4.5f + i * 3f) + Vector3.forward;
                node.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
                Primitive("Socket", PrimitiveType.Cylinder, node.transform, new Vector3(0f, 0.1f, 0f), new Vector3(1.1f, 0.1f, 1.1f), room);
                GameObject manifestation = Primitive("Amplitude manifestation", PrimitiveType.Cylinder, node.transform,
                    Vector3.zero, Vector3.one, signal);
                UnityEngine.Object.DestroyImmediate(manifestation.GetComponent<Collider>());
                var waveObject = new GameObject("Signed amplitude waveform");
                waveObject.transform.SetParent(node.transform, false);
                LineRenderer line = waveObject.AddComponent<LineRenderer>();
                line.useWorldSpace = false; line.positionCount = 40;
                line.sharedMaterial = wave; line.numCornerVertices = 2;
                line.shadowCastingMode = ShadowCastingMode.Off;
                var dots = new Transform[3];
                for (int dot = 0; dot < dots.Length; dot++)
                {
                    GameObject flow = Primitive("Phase flow " + (dot + 1), PrimitiveType.Sphere, node.transform,
                        Vector3.zero, Vector3.one * 0.1f, particle);
                    UnityEngine.Object.DestroyImmediate(flow.GetComponent<Collider>());
                    dots[dot] = flow.transform;
                }
                visualizers[i] = node.AddComponent<CandidateVisualizer>();
                visualizers[i].Configure(encounter, i, manifestation.transform, line, dots);
            }

            var cameraObject = new GameObject("Isometric Camera");
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.tag = "MainCamera";
            Camera view = cameraObject.AddComponent<Camera>();
            view.clearFlags = CameraClearFlags.SolidColor; view.backgroundColor = new Color(0.035f, 0.045f, 0.065f);
            view.nearClipPlane = 0.1f; view.farClipPlane = 100f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<CameraController>().Configure(player.transform, new Vector3(0f, 0.8f, 0f));
            movement.SetCamera(cameraObject.transform);
            var lightObject = new GameObject("Prototype Directional Light");
            lightObject.transform.SetParent(root.transform, false);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.6f; light.shadows = LightShadows.Soft;
            CreateHud(root.transform, encounter, anvesh, view, visualizers);

            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/AmplitudeAmplificationPrototype.unity");
            if (!EditorSceneManager.SaveScene(scene, path)) throw new InvalidOperationException("Could not save Phase 2 scene.");
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = root;
            Debug.Log("Phase 2 ready at " + path + ". Q SCAN, 1 MARK, 2 AMPLIFY, 3 LOCK, R restart, F3 debug. " +
                "Each iteration is MARK then AMPLIFY. Bootstrap, Phase 1 and build settings are unchanged.");
            return path;
        }

        private static void CreateHud(Transform root, QuantumEncounter encounter, AnveshController anvesh,
            Camera view, CandidateVisualizer[] visualizers)
        {
            var ui = new GameObject("Prototype HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            ui.transform.SetParent(root, false);
            Canvas canvas = ui.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = ui.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f); scaler.matchWidthOrHeight = 0.5f;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Text title = TextElement("Title", ui.transform, font, "NULL SIGNAL  /  ANVESH", 23, TextAnchor.UpperLeft);
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(-48f, 35f));
            Text iterations = TextElement("Encounter state", ui.transform, font, "", 16, TextAnchor.UpperLeft);
            Place(iterations.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(-48f, 30f));
            Text debug = TextElement("Development diagnostics", ui.transform, font, "", 16, TextAnchor.UpperRight);
            Place(debug.rectTransform, Vector2.one, Vector2.one, Vector2.one, new Vector2(-24f, -20f), new Vector2(390f, 105f));
            var panel = new GameObject("Action panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(ui.transform, false);
            panel.GetComponent<Image>().color = new Color(0.035f, 0.055f, 0.08f, 0.96f);
            Place((RectTransform)panel.transform, Vector2.zero, Vector2.right, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 168f));
            Text status = TextElement("Action feedback", panel.transform, font, "", 17, TextAnchor.MiddleCenter);
            Place(status.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 115f), new Vector2(1180f, 45f));
            var buttons = new Button[5];
            string[] names = { "Q  SCAN", "1  MARK", "2  AMPLIFY", "3  LOCK", "R  RESTART" };
            for (int i = 0; i < buttons.Length; i++)
            {
                var button = new GameObject(names[i], typeof(RectTransform), typeof(Image), typeof(Button));
                button.transform.SetParent(panel.transform, false);
                Image image = button.GetComponent<Image>(); image.color = new Color(0.12f, 0.27f, 0.32f);
                buttons[i] = button.GetComponent<Button>(); buttons[i].targetGraphic = image;
                buttons[i].navigation = new Navigation { mode = Navigation.Mode.None };
                ColorBlock colors = buttons[i].colors; colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.65f);
                colors.highlightedColor = new Color(0.65f, 0.95f, 1f); buttons[i].colors = colors;
                Place((RectTransform)button.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-360f + i * 180f, 58f), new Vector2(166f, 46f));
                Text label = TextElement("Label", button.transform, font, names[i], 18, TextAnchor.MiddleCenter);
                Place(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            }
            Text controls = TextElement("Movement controls", panel.transform, font, "WASD / ARROWS MOVE     SPACE DODGE     E INSPECT     F3 TOGGLE DEVELOPMENT VALUES", 13, TextAnchor.MiddleCenter);
            Place(controls.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(1180f, 28f));
            var labels = new Text[visualizers.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = TextElement("Candidate debug " + (i + 1), ui.transform, font, "", 16, TextAnchor.MiddleCenter);
                Place(labels[i].rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(170f, 50f));
            }
            ui.AddComponent<HUDController>().Configure(encounter, anvesh, view, canvas, status, iterations, debug, buttons, labels, visualizers, true);
            var events = new GameObject("Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(root, false);
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static Text TextElement(string name, Transform parent, Font font, string value, int size, TextAnchor alignment)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(Text)); item.transform.SetParent(parent, false);
            Text text = item.GetComponent<Text>(); text.font = font; text.text = value; text.fontSize = size;
            text.alignment = alignment; text.color = Color.white; text.raycastTarget = false;
            return text;
        }
        private static void Place(RectTransform rect, Vector2 min, Vector2 max, Vector2 pivot, Vector2 position, Vector2 size)
        { rect.anchorMin = min; rect.anchorMax = max; rect.pivot = pivot; rect.anchoredPosition = position; rect.sizeDelta = size; }
        private static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject item = GameObject.CreatePrimitive(type); item.name = name; item.transform.SetParent(parent, false);
            item.transform.localPosition = position; item.transform.localScale = scale; item.GetComponent<Renderer>().sharedMaterial = material;
            return item;
        }
        private static Material MaterialAsset(string name, Shader shader, Color color, bool transparent)
        {
            string path = Materials + "/" + name + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var material = new Material(shader) { name = "Amplitude prototype " + name };
            material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.15f);
            if (material.HasProperty("_EmissionColor"))
            { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", color * 0.35f); }
            if (transparent)
            {
                material.SetFloat("_Surface", 1f); material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); material.SetFloat("_ZWrite", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); material.renderQueue = (int)RenderQueue.Transparent;
                material.SetOverrideTag("RenderType", "Transparent"); material.SetShaderPassEnabled("ShadowCaster", false);
            }
            AssetDatabase.CreateAsset(material, AssetDatabase.GenerateUniqueAssetPath(path));
            return material;
        }
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/'); string parent = path.Substring(0, slash);
            EnsureFolder(parent); AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
    }
}
