using System;
using NullSignal.Gameplay;
using NullSignal.Player;
using NullSignal.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace NullSignal.Editor
{
    public static class NullSignalPhase3Builder
    {
        [MenuItem("Tools/NULL SIGNAL/Build Phase 3 Visual Prototype")]
        public static void BuildVisualPrototype()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { Debug.LogWarning("Exit Play Mode before building the visual prototype."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CreateScene();
        }

        private static void CreateScene()
        {
            var kit = new Astra7VisualKit();
            kit.BuildModules();
            Astra7VisualKit.Folder("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("NULL SIGNAL / Astra-7 Quantum Laboratory");
            QuantumEncounter encounter = root.AddComponent<QuantumEncounter>();
            encounter.Configure(4, 0, true); encounter.RestartEncounter();
            Transform architecture = Group(root.transform, "Astra-7 / Modular architecture");
            BuildRoom(kit, architecture);
            Vector3 right = Quaternion.Euler(0f, 45f, 0f) * Vector3.right;
            Vector3 forward = Quaternion.Euler(0f, 45f, 0f) * Vector3.forward;
            Vector3 apparatusPosition = forward * 2.5f;
            BuildApparatus(kit, root.transform, apparatusPosition);

            Transform candidateRoot = Group(root.transform, "Search array / Four equivalent states");
            for (int i = 0; i < 4; i++)
            {
                float horizontal = i == 0 ? -4.6f : i == 1 ? -1.65f : i == 2 ? 1.65f : 4.6f;
                float depth = i == 0 || i == 3 ? 0.15f : -2.0f;
                Vector3 position = right * horizontal + forward * depth;
                BuildNode(kit, candidateRoot, encounter, i, position);
                LineRenderer circuit = Line(kit, architecture, "Floor circuit " + (i + 1), 0.018f);
                circuit.positionCount = 3;
                Vector3 midpoint = Vector3.Lerp(apparatusPosition, position, 0.45f);
                circuit.SetPositions(new[] { apparatusPosition + Vector3.up * 0.028f, midpoint + Vector3.up * 0.028f, position + Vector3.up * 0.028f });
                circuit.startColor = circuit.endColor = new Color(0.39f, 0.52f, 0.69f, 0.32f);
            }

            GameObject player = BuildPlayer(kit, root.transform, forward * -4.8f);
            AnveshController anvesh = player.AddComponent<AnveshController>(); anvesh.Configure(encounter);
            var cameraObject = new GameObject("Guided isometric camera", typeof(Camera), typeof(AudioListener), typeof(UniversalAdditionalCameraData));
            cameraObject.transform.SetParent(root.transform, false); cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.015f, 0.026f, 0.045f); camera.nearClipPlane = 0.1f; camera.farClipPlane = 100f;
            camera.allowHDR = true; camera.allowMSAA = true;
            var cameraData = cameraObject.GetComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true; cameraData.requiresDepthOption = CameraOverrideOption.Off;
            cameraData.requiresColorOption = CameraOverrideOption.Off;
            cameraObject.AddComponent<VisualCameraController>().Configure(player.transform, encounter);
            player.GetComponent<PlayerController>().SetCamera(cameraObject.transform);
            BuildLighting(root.transform, apparatusPosition);
            NullSignalPhase3HUDBuilder.Build(root.transform, encounter, anvesh);

            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/VisualPrototype.unity");
            if (!EditorSceneManager.SaveScene(scene, path)) throw new InvalidOperationException("Could not save visual prototype scene.");
            AssetDatabase.SaveAssets(); Selection.activeGameObject = root;
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.LookAt(new Vector3(0f, 0.8f, 0f), Quaternion.Euler(42f, 45f, 0f), 14f, true);
            Debug.Log("NULL SIGNAL Phase 3 created: " + path + ". Press Play and click Game view. Q / 1 / 2 / 3, R reset, F3 debug. Modular prefabs: " + Astra7VisualKit.Root + "/Prefabs. No project input or render settings changed.");
        }

        private static void BuildRoom(Astra7VisualKit kit, Transform root)
        {
            for (int x = 0; x < 7; x++)
                for (int z = 0; z < 6; z++)
                    kit.Instance("Floor panel", root, new Vector3((x - 3) * 2.4f, 0f, (z - 2.5f) * 2.4f));
            for (int x = 0; x < 7; x++)
            {
                kit.Instance(x == 3 ? "Door" : "Wall panel", root, new Vector3((x - 3) * 2.4f, 0f, 7.2f));
                kit.Instance("Wall trim", root, new Vector3((x - 3) * 2.4f, 3.34f, 7.15f));
            }
            for (int z = 0; z < 6; z++)
            {
                kit.Instance(z == 2 ? "Door" : "Wall panel", root, new Vector3(8.4f, 0f, (z - 2.5f) * 2.4f), 90f);
                kit.Instance("Wall trim", root, new Vector3(8.4f, 3.34f, (z - 2.5f) * 2.4f), 90f);
            }
            foreach (Vector3 position in new[] { new Vector3(-8.25f,0f,7.05f), new Vector3(-3.6f,0f,7.05f), new Vector3(3.6f,0f,7.05f), new Vector3(8.25f,0f,7.05f), new Vector3(8.25f,0f,-3.6f) })
                kit.Instance("Pillar", root, position);
            // Low front edges keep the room legible from the fixed camera.
            for (int z = 0; z < 3; z++) kit.Instance("Railing", root, new Vector3(-8.28f, 0f, 1.2f + z * 2.4f), 90f);
            for (int x = 0; x < 3; x++) kit.Instance("Railing", root, new Vector3(2.4f + x * 2.4f, 0f, -7.08f));
            kit.Box(root, "Front cutaway sill", new Vector3(0f, 0.06f, -7.12f), new Vector3(16.8f, 0.12f, 0.18f), kit.Copper);
            kit.Box(root, "Left cutaway sill", new Vector3(-8.32f, 0.06f, 0f), new Vector3(0.18f, 0.12f, 14.4f), kit.Copper);
            Transform boundary = Group(root, "Safety bounds / cutaway walls");
            Astra7VisualKit.Collider(boundary, new Vector3(-8.5f, 1f, 0f), new Vector3(0.2f, 2f, 14.4f));
            Astra7VisualKit.Collider(boundary, new Vector3(0f, 1f, -7.3f), new Vector3(16.8f, 2f, 0.2f));
            for (int i = 0; i < 3; i++) kit.Instance("Console", root, new Vector3(-5.8f + i * 1.6f, 0f, 5.95f));
            for (int i = 0; i < 2; i++) kit.Instance("Console", root, new Vector3(7.1f, 0f, 2.0f + i * 1.7f), 90f);
            kit.Instance("Light strip", root, new Vector3(-4.6f, 3.3f, 6.4f));
            kit.Instance("Light strip", root, new Vector3(5.2f, 3.3f, 6.4f));
            var plinth = kit.Instance("Hologram pedestal", root, new Vector3(-6.9f, 0f, 3.9f));
            var equipment = Group(plinth.transform, "Holographic instrument");
            equipment.localPosition = new Vector3(0f, 1.4f, 0f);
            LineRenderer globe = Circle(kit, equipment, "Scientific orbit", 0.65f, 0.015f);
            globe.transform.localRotation = Quaternion.Euler(55f, 0f, 25f);
            LineRenderer globe2 = Circle(kit, equipment, "Scientific orbit 2", 0.65f, 0.015f);
            globe2.transform.localRotation = Quaternion.Euler(35f, 90f, -30f);
            equipment.gameObject.AddComponent<ApparatusMotion>().Configure(new[] { globe.transform, globe2.transform });
            WorldLabel(root, "ASTRA–7", new Vector3(-4.7f, 3.46f, 7.05f), 0.17f, kit.Cyan.color);
        }
        internal static void BuildApparatus(Astra7VisualKit kit, Transform parent, Vector3 position)
        {
            Transform root = Group(parent, "Central apparatus / Anveshak interface"); root.localPosition = position;
            kit.Shape(root, "Lower foundation", kit.Cylinder, new Vector3(0f, 0.14f, 0f), new Vector3(3.6f, 0.28f, 3.6f), kit.Graphite);
            kit.Shape(root, "Copper perimeter", kit.Ring, new Vector3(0f, 0.29f, 0f), new Vector3(3.45f, 1f, 3.45f), kit.Copper);
            kit.Shape(root, "Upper deck", kit.Cylinder, new Vector3(0f, 0.39f, 0f), new Vector3(2.7f, 0.24f, 2.7f), kit.Panel);
            kit.Shape(root, "Emitter ring", kit.Ring, new Vector3(0f, 0.54f, 0f), new Vector3(2.05f, 0.7f, 2.05f), kit.Cyan);
            kit.Shape(root, "Containment aperture", kit.Cylinder, new Vector3(0f, 0.64f, 0f), new Vector3(1.25f, 0.22f, 1.25f), kit.Graphite);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4f;
                Transform rib = kit.Box(root, "Radial equipment rib", new Vector3(Mathf.Sin(a) * 1.45f, 0.45f, Mathf.Cos(a) * 1.45f), new Vector3(0.22f, 0.32f, 0.64f), kit.Titanium);
                rib.localRotation = Quaternion.Euler(0f, i * 45f, 0f);
            }
            LineRenderer floorRing = Circle(kit, root, "Scientific floor perimeter", 2.45f, 0.018f);
            floorRing.transform.localPosition = Vector3.up * 0.03f;
            floorRing.startColor = floorRing.endColor = new Color(0.61f, 0.43f, 0.27f, 0.5f);
            Transform field = Group(root, "Contained research hologram"); field.localPosition = new Vector3(0f, 1.9f, 0f);
            kit.Shape(field, "Wireframe focus", kit.Crystal, Vector3.zero, Vector3.one * 0.42f, kit.Violet);
            var moving = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                LineRenderer ring = Circle(kit, field, "Containment orbit " + i, 1.03f, 0.013f);
                ring.transform.localRotation = Quaternion.Euler(i * 50f + 15f, i * 60f, i == 2 ? 45f : 0f);
                ring.startColor = ring.endColor = i == 2 ? new Color(0.64f, 0.52f, 0.95f, 0.5f) : new Color(0.4f, 0.87f, 0.95f, 0.55f);
                moving[i] = ring.transform;
            }
            field.gameObject.AddComponent<ApparatusMotion>().Configure(moving);
            Astra7VisualKit.Collider(root, new Vector3(0f, 0.5f, 0f), new Vector3(2.6f, 1f, 2.6f));
        }

        internal static void BuildNode(Astra7VisualKit kit, Transform parent, QuantumEncounter encounter, int index, Vector3 position)
        {
            Transform node = Group(parent, "Candidate " + (index + 1).ToString("00")); node.localPosition = position;
            node.localRotation = Quaternion.Euler(0f, 45f, 0f);
            kit.Instance("Candidate platform", node, Vector3.zero);
            Transform core = kit.Shape(node, "Floating amplitude core", kit.Crystal, new Vector3(0f, 1.6f, 0f), Vector3.one * 0.48f, kit.Hologram);
            var rings = new LineRenderer[3];
            for (int i = 0; i < rings.Length; i++)
            {
                rings[i] = Circle(kit, node, "Holographic orbit " + (i + 1), 0.86f, 0.018f);
                rings[i].transform.localPosition = new Vector3(0f, 1.6f, 0f);
                rings[i].transform.localRotation = Quaternion.Euler(i == 0 ? 0f : 64f, i * 30f, i == 2 ? 55f : 0f);
            }
            LineRenderer wave = Line(kit, node, "Signed phase waveform", 0.025f); wave.positionCount = 56;
            for (int i = 0; i < 56; i++)
            { float x = i / 55f * 2f - 1f; wave.SetPosition(i, new Vector3(x, 1.35f + 0.36f * Mathf.Sin(x * 6.28f), -0.45f)); }
            LineRenderer probability = Circle(kit, node, "Actual probability arc", 1.05f, 0.036f); probability.loop = false;
            probability.transform.localPosition = Vector3.zero; probability.positionCount = 49;
            for (int i = 0; i < 49; i++)
            { float a = i / 48f * Mathf.PI * 0.5f; probability.SetPosition(i, new Vector3(Mathf.Sin(a) * 1.05f, 0.38f, Mathf.Cos(a) * 1.05f)); }
            LineRenderer halo = Circle(kit, node, "Measurement confirmation halo", 0.65f, 0.022f);
            halo.transform.localPosition = Vector3.up * 0.4f; halo.enabled = false;
            var motes = new Transform[24];
            for (int i = 0; i < motes.Length; i++)
            {
                float t = i / 24f, a = t * Mathf.PI * 4f;
                motes[i] = kit.Shape(node, "Phase mote " + i, kit.Crystal, new Vector3(Mathf.Cos(a) * 0.64f, 0.45f + t * 1.6f, Mathf.Sin(a) * 0.64f), Vector3.one * 0.06f, kit.Cyan);
                motes[i].gameObject.SetActive(i < 12);
            }
            node.gameObject.AddComponent<QuantumNodePresentation>().Configure(encounter, index, core, wave, rings, probability, halo, motes);
            WorldLabel(node, (index + 1).ToString("00"), new Vector3(0f, 0.47f, -0.92f), 0.085f, new Color(0.6f, 0.83f, 0.9f));
        }

        internal static GameObject BuildPlayer(Astra7VisualKit kit, Transform parent, Vector3 position)
        {
            Transform root = Group(parent, "Anirudh / procedural operative"); root.position = position + Vector3.up * 0.05f;
            CharacterController motor = root.gameObject.AddComponent<CharacterController>(); motor.height = 1.9f; motor.radius = 0.33f;
            motor.center = new Vector3(0f, 0.95f, 0f); motor.skinWidth = 0.035f; motor.stepOffset = 0.18f;
            root.gameObject.AddComponent<PlayerController>(); root.gameObject.AddComponent<PlayerInteraction>();
            Transform body = Group(root, "Humanoid silhouette");
            kit.Box(body, "Armored torso", new Vector3(0f, 1.28f, 0f), new Vector3(0.62f, 0.67f, 0.35f), kit.Suit);
            kit.Box(body, "Chest plate", new Vector3(0f, 1.33f, 0.19f), new Vector3(0.40f, 0.37f, 0.07f), kit.Panel);
            kit.Box(body, "Belt", new Vector3(0f, 0.95f, 0f), new Vector3(0.53f, 0.12f, 0.38f), kit.Hair);
            kit.Box(body, "Belt fastener", new Vector3(0f, 0.95f, 0.22f), new Vector3(0.12f, 0.10f, 0.06f), kit.Copper);
            kit.Box(body, "Compact backpack", new Vector3(0f, 1.32f, -0.28f), new Vector3(0.43f, 0.53f, 0.23f), kit.Graphite);
            kit.Box(body, "Backpack status strip", new Vector3(0f, 1.44f, -0.41f), new Vector3(0.065f, 0.22f, 0.025f), kit.Cyan);
            for (int side = -1; side <= 1; side += 2)
                kit.Box(body, "Copper harness", new Vector3(side * 0.21f, 1.30f, 0.20f), new Vector3(0.045f, 0.53f, 0.035f), kit.Copper);
            kit.Shape(body, "Neck", kit.Cylinder, new Vector3(0f, 1.65f, 0f), new Vector3(0.18f, 0.17f, 0.18f), kit.Skin);
            kit.Box(body, "Head", new Vector3(0f, 1.81f, 0.015f), new Vector3(0.30f, 0.35f, 0.28f), kit.Skin);
            kit.Box(body, "Hair silhouette", new Vector3(0f, 1.97f, -0.015f), new Vector3(0.34f, 0.16f, 0.32f), kit.Hair);
            kit.Box(body, "Hair sweep", new Vector3(-0.07f, 2.02f, 0.04f), new Vector3(0.22f, 0.1f, 0.26f), kit.Hair).localRotation = Quaternion.Euler(0f, 0f, -12f);
            kit.Box(body, "Face direction", new Vector3(0f, 1.79f, 0.17f), new Vector3(0.085f, 0.11f, 0.065f), kit.Skin);
            var arms = new Transform[2]; var legs = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                arms[i] = Group(body, i == 0 ? "Left shoulder pivot" : "Right shoulder pivot"); arms[i].localPosition = new Vector3(side * 0.39f, 1.48f, 0f);
                kit.Box(arms[i], "Shoulder armor", new Vector3(0f, -0.08f, 0f), new Vector3(0.22f, 0.24f, 0.32f), kit.Panel);
                kit.Box(arms[i], "Upper sleeve", new Vector3(0f, -0.29f, 0f), new Vector3(0.18f, 0.3f, 0.23f), kit.Suit);
                kit.Box(arms[i], "Forearm", new Vector3(0f, -0.52f, 0.04f), new Vector3(0.16f, 0.23f, 0.18f), kit.Skin);
                kit.Box(arms[i], "Glove", new Vector3(0f, -0.68f, 0.06f), new Vector3(0.17f, 0.16f, 0.19f), kit.Hair);
                legs[i] = Group(body, i == 0 ? "Left hip pivot" : "Right hip pivot"); legs[i].localPosition = new Vector3(side * 0.17f, 0.92f, 0f);
                kit.Box(legs[i], "Trouser leg", new Vector3(0f, -0.35f, 0f), new Vector3(0.23f, 0.69f, 0.27f), kit.Suit);
                kit.Box(legs[i], "Knee guard", new Vector3(0f, -0.43f, 0.14f), new Vector3(0.20f, 0.19f, 0.08f), kit.Panel);
                kit.Box(legs[i], "Boot", new Vector3(0f, -0.78f, 0.06f), new Vector3(0.25f, 0.27f, 0.42f), kit.Hair);
            }
            kit.Box(arms[0], "ANVESH cuff", new Vector3(0f, -0.54f, 0.04f), new Vector3(0.22f, 0.18f, 0.25f), kit.Graphite);
            kit.Box(arms[0], "ANVESH luminous face", new Vector3(-0.12f, -0.54f, 0.04f), new Vector3(0.025f, 0.12f, 0.15f), kit.Cyan);
            root.gameObject.AddComponent<PrototypeHumanoid>().Configure(body, arms[0], arms[1], legs[0], legs[1]);
            return root.gameObject;
        }

        private static void BuildLighting(Transform parent, Vector3 apparatus)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.29f, 0.39f, 0.50f);
            RenderSettings.ambientEquatorColor = new Color(0.13f, 0.20f, 0.27f);
            RenderSettings.ambientGroundColor = new Color(0.055f, 0.075f, 0.105f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.018f, 0.031f, 0.05f); RenderSettings.fogStartDistance = 31f; RenderSettings.fogEndDistance = 57f;
            Transform lighting = Group(parent, "Lighting / limited realtime rig");
            Light key = Light(lighting, "Cool key", LightType.Directional, Vector3.zero, new Color(0.66f, 0.81f, 1f), 1.7f, 0f);
            key.transform.rotation = Quaternion.Euler(48f, -28f, 0f); key.shadows = LightShadows.Soft; key.shadowStrength = 0.7f;
            Light(lighting, "Warm practical", LightType.Point, new Vector3(-4.4f, 2.7f, 5.2f), new Color(1f, 0.53f, 0.25f), 4f, 8f);
            Light(lighting, "Cyan laboratory fill", LightType.Point, new Vector3(5.5f, 2.8f, 1.4f), new Color(0.27f, 0.76f, 1f), 4f, 9f);
            Light(lighting, "Violet apparatus bounce", LightType.Point, apparatus + Vector3.up * 2f, new Color(0.57f, 0.39f, 1f), 1.7f, 5f);
            var volumeObject = new GameObject("Restrained scene atmosphere", typeof(Volume)); volumeObject.transform.SetParent(lighting, false);
            var volume = volumeObject.GetComponent<Volume>(); volume.isGlobal = true;
            string profilePath = Astra7VisualKit.Root + "/Astra7 Atmosphere.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(profile, profilePath);
                Bloom bloom = profile.Add<Bloom>(true); bloom.intensity.Override(0.25f); bloom.threshold.Override(1.15f); bloom.scatter.Override(0.45f); bloom.highQualityFiltering.Override(false);
                Vignette vignette = profile.Add<Vignette>(true); vignette.intensity.Override(0.17f); vignette.smoothness.Override(0.55f);
                Tonemapping tone = profile.Add<Tonemapping>(true); tone.mode.Override(TonemappingMode.ACES);
                foreach (VolumeComponent component in profile.components) AssetDatabase.AddObjectToAsset(component, profile);
                EditorUtility.SetDirty(profile);
            }
            volume.sharedProfile = profile;
        }
        private static Light Light(Transform parent, string name, LightType type, Vector3 position, Color color, float intensity, float range)
        {
            Transform item = Group(parent, name); item.localPosition = position;
            var light = item.gameObject.AddComponent<Light>(); light.type = type; light.color = color;
            light.intensity = intensity; light.range = range; light.shadows = LightShadows.None; return light;
        }
        private static Transform Group(Transform parent, string name)
        { var item = new GameObject(name); item.transform.SetParent(parent, false); return item.transform; }
        private static LineRenderer Line(Astra7VisualKit kit, Transform parent, string name, float width)
        {
            Transform item = Group(parent, name); var line = item.gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false; line.sharedMaterial = kit.Line; line.widthMultiplier = width;
            line.startColor = line.endColor = new Color(0.4f, 0.87f, 0.95f, 0.8f);
            line.shadowCastingMode = ShadowCastingMode.Off; line.receiveShadows = false; line.numCapVertices = 2; return line;
        }
        private static LineRenderer Circle(Astra7VisualKit kit, Transform parent, string name, float radius, float width)
        {
            LineRenderer line = Line(kit, parent, name, width); line.loop = true; line.positionCount = 64;
            for (int i = 0; i < 64; i++)
            { float angle = i / 64f * Mathf.PI * 2f; line.SetPosition(i, new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius)); }
            return line;
        }
        private static void WorldLabel(Transform parent, string text, Vector3 position, float size, Color color)
        {
            Transform label = Group(parent, text); label.localPosition = position;
            TextMesh mesh = label.gameObject.AddComponent<TextMesh>(); mesh.text = text; mesh.fontSize = 64; mesh.characterSize = size;
            mesh.anchor = TextAnchor.MiddleCenter; mesh.alignment = TextAlignment.Center; mesh.color = color;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); mesh.font = font;
            label.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }
    }
}
