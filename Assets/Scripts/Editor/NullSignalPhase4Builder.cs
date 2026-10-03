using System;
using NullSignal.Gameplay;
using NullSignal.Player;
using NullSignal.Presentation;
using NullSignal.Story;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NullSignal.Editor
{
    public static class NullSignalPhase4Builder
    {
        private const float Spacing = 28.8f;
        private static readonly Vector3 Right = new Vector3(0.7071068f, 0f, -0.7071068f);
        private static readonly Vector3 Forward = new Vector3(0.7071068f, 0f, 0.7071068f);

        [MenuItem("Tools/NULL SIGNAL/Build Phase 4 Main Game")]
        public static void BuildMainGame() => BuildMainGame(false);
        internal static void BuildMainGame(bool withCombat, bool completeGame = false, bool finalPresentation = false, bool unattended = false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogWarning("Exit Play Mode before building MainGame."); return; }
            if (!unattended && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (withCombat && AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/MainGame.unity") != null)
            {
                Astra7VisualKit.Folder("Assets/Scenes/Archive");
                string backup = AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/Archive/MainGame before Phase" + (finalPresentation ? "7" : completeGame ? "6" : "5 hotfix") + ".unity");
                if (!AssetDatabase.CopyAsset("Assets/Scenes/MainGame.unity", backup))
                    throw new InvalidOperationException("Could not preserve the previous MainGame before rebuilding.");
            }
            var kit = new Astra7VisualKit(); kit.BuildModules();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Transform root = Group(null, "NULL SIGNAL / Playable Astra-7 route");
            var director = root.gameObject.AddComponent<MainGameDirector>();
            var objectives = root.gameObject.AddComponent<ObjectiveSystem>();
            var subtitles = root.gameObject.AddComponent<SubtitleController>();
            var audio = root.gameObject.AddComponent<StationFeedbackAudio>();

            // Broad structural floor continues beyond all normal camera bounds, including transitions.
            int roomCount = completeGame ? 11 : withCombat ? 8 : 6;
            kit.Box(root, "Continuous station underdeck / no exterior void", new Vector3(Spacing * (roomCount - 1) * .5f, -0.55f, 0f), new Vector3(Spacing * roomCount + 96f, 0.5f, 100f), kit.Panel);
            for (int i = -2; i < roomCount + 3; i++)
                kit.Box(root, "Distant structural rib", new Vector3(i * 24f, -0.20f, 0f), new Vector3(0.3f, 0.08f, 96f), kit.Graphite);

            GameObject player = NullSignalPhase3Builder.BuildPlayer(kit, root, new Vector3(-8f, 0f, -3f));
            var health = player.AddComponent<PlayerHealth>();
            var anvesh = player.AddComponent<AnveshController>(); anvesh.SetUnlocked(AnveshAbility.None);
            var cameraObject = new GameObject("Station isometric camera", typeof(Camera), typeof(AudioListener), typeof(UniversalAdditionalCameraData), typeof(StationCamera));
            cameraObject.transform.SetParent(root, false); cameraObject.tag = "MainCamera";
            Camera view = cameraObject.GetComponent<Camera>(); view.backgroundColor = new Color(0.045f, 0.065f, 0.09f);
            view.clearFlags = CameraClearFlags.SolidColor; view.nearClipPlane = 0.1f; view.farClipPlane = 100f;
            view.allowHDR = true; view.allowMSAA = true; view.allowDynamicResolution = false;
            var cameraData = cameraObject.GetComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true; cameraData.antialiasing = AntialiasingMode.None;
            cameraData.requiresDepthOption = CameraOverrideOption.Off; cameraData.requiresColorOption = CameraOverrideOption.Off;
            StationCamera cameraRig = cameraObject.GetComponent<StationCamera>(); cameraRig.Configure(player.transform);
            player.GetComponent<PlayerController>().SetCamera(cameraObject.transform);
            if (withCombat) NullSignalPhase5Builder.BuildPlayerSystems(kit, player, view);
            var checkpoints = root.gameObject.AddComponent<CheckpointSystem>(); checkpoints.Configure(player.GetComponent<PlayerController>(), health, director);
            ConfigureRendering(root);

            var rooms = new StoryQuantumRoom[roomCount];
            for (int i = 0; i < rooms.Length; i++)
            {
                StoryRoomKind kind = (StoryRoomKind)i;
                Transform section = Group(root, i.ToString("00") + " / " + kind); section.localPosition = new Vector3(i * Spacing, 0f, 0f);
                rooms[i] = section.gameObject.AddComponent<StoryQuantumRoom>();
                BuildArchitecture(kit, section, i, i < rooms.Length - 1);
                DoorController door = i < rooms.Length - 1 ? BuildDoor(kit, section, audio) : null;
                if (i < rooms.Length - 1) BuildCorridor(kit, section);
                Light[] lights = BuildPracticals(section, kind);
                Transform checkpoint = Group(section, "Checkpoint / " + kind); checkpoint.localPosition = new Vector3(-9f, 0.05f, i == 0 ? -3f : 0f);
                QuantumEncounter encounter = null; GameObject nodes = null;
                if (i > 0 && i < 10)
                {
                    encounter = Group(section, "Quantum encounter / original simulation").gameObject.AddComponent<QuantumEncounter>();
                    encounter.Configure(i == 8 ? 16 : i >= 4 && i < 6 ? 8 : 4, 0, true); encounter.RestartEncounter();
                    Transform array = Group(section, "Candidate array"); nodes = array.gameObject;
                    int count = encounter.CandidateCount;
                    for (int c = 0; c < (i >= 6 ? 0 : count); c++)
                    {
                        Vector3 position;
                        if (count == 8)
                        {
                            float angle = c * Mathf.PI * 0.25f + Mathf.PI * 0.125f;
                            position = Right * (Mathf.Cos(angle) * 5.2f) + Forward * (Mathf.Sin(angle) * 4.8f);
                        }
                        else position = Right * (c == 0 ? -4.6f : c == 1 ? -1.65f : c == 2 ? 1.65f : 4.6f) + Forward * (c == 0 || c == 3 ? 0.15f : -2f);
                        NullSignalPhase3Builder.BuildNode(kit, array, encounter, c, position);
                    }
                    if (i > 1 && i < 6) NullSignalPhase3Builder.BuildApparatus(kit, section, count == 8 ? Vector3.zero : Forward * 2.8f);
                }
                GameObject hologram = null;
                if (i >= 3 && i < 8)
                    hologram = BuildHologram(kit, section, subtitles, i == 3 || i == 6 ? "ANANYA" : "LUBNA");
                rooms[i].Configure(i, kind, encounter, door, director, checkpoint, hologram, lights,
                    i < 8 ? Phase4StoryContent.Entrance(kind) : Array.Empty<SubtitleCue>(), Phase4StoryContent.Mark(kind), Phase4StoryContent.Amplify(kind),
                    withCombat && i == 5 ? new[] { new SubtitleCue("LUBNA", "You saw the signal rise, fall, and return. More amplification is not always better.", 5f), new SubtitleCue("TARA", "Reactor stable. Security lockdown ahead. Stay ready.", 3f) }
                        : i < 8 ? Phase4StoryContent.Success(kind) : Array.Empty<SubtitleCue>(), Phase4StoryContent.Overshoot(kind));
                rooms[i].ConfigureVisuals(nodes);
                if (i >= 6 && i < 8) NullSignalPhase5Builder.BuildEncounter(kit, section, rooms[i], encounter, director, player.transform, cameraRig, lights, i == 7);
                if (i >= 8) NullSignalPhase6Builder.BuildFinalRoom(kit, section, rooms[i], director, player.transform, cameraRig, lights, subtitles);
                if (withCombat && i == 6) NullSignalPhase5Builder.BuildCorruption(kit, section);
                if (i == 0)
                {
                    BuildInteractable(kit, section, director, StationInteraction.ManualRelease, new Vector3(8.2f, 0f, -2.4f));
                    BuildDamage(kit, section);
                    BuildHazard(kit, section, player.transform, subtitles, cameraRig, new Vector3(14.4f, 0f, 0f), false);
                }
                if (i == 1)
                {
                    BuildInteractable(kit, section, director, StationInteraction.AnveshPickup, new Vector3(-6f, 0f, -2.8f));
                    nodes.SetActive(false);
                }
                if (i == 5)
                {
                    BuildDamage(kit, section);
                }
            }
            director.Configure(rooms, anvesh, health, player.transform, objectives, subtitles, checkpoints, cameraRig, audio);
            for (int i = 0; i < rooms.Length - 1; i++)
                if (rooms[i].Exit == null) throw new InvalidOperationException("Room " + rooms[i].Kind + " is missing its generated exit.");
            anvesh.Configure(null);
            AnveshHUD hud = NullSignalPhase3HUDBuilder.Build(root, rooms[1].Encounter, anvesh, completeGame ? 16 : 8); hud.ConfigureStory(objectives);
            BuildStoryHud(hud, director, health, subtitles);
            if (withCombat) NullSignalPhase5Builder.BuildAnalysis(hud, player.GetComponent<AnveshOperationController>(), anvesh);
            if (completeGame) NullSignalPhase6Builder.BuildFinalHUD(hud, director);
            if (finalPresentation) NullSignalPhase7Builder.ConfigurePresentation(kit, root, director, hud, cameraRig, subtitles, audio);
            cameraRig.SetRoom(Vector3.zero, 0); cameraRig.SnapToPlayer();

            Astra7VisualKit.Folder("Assets/Scenes");
            string path = withCombat ? "Assets/Scenes/MainGame.unity" : AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/MainGame.unity");
            if (!EditorSceneManager.SaveScene(scene, path)) throw new InvalidOperationException("Could not save MainGame.");
            AssetDatabase.SaveAssets(); Selection.activeGameObject = root.gameObject;
            if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.LookAt(player.transform.position, Quaternion.Euler(45f, 45f, 0f), 12f, true);
            Debug.Log("NULL SIGNAL Phase " + (finalPresentation ? "7" : completeGame ? "6" : withCombat ? "5" : "4") + " ready: " + path + ". Play, focus Game view, use WASD / Space / E / LMB attack. ANVESH unlocks through the story. Q, top-row or numpad 1/2/3, R, F3. Previous scenes are preserved.");
        }

        private static void BuildArchitecture(Astra7VisualKit kit, Transform room, int index, bool hasExit)
        {
            Transform structure = Group(room, "Astra-7 modular structure");
            if (index <= 1) structure.gameObject.AddComponent<StationPowerState>().Configure(index != 0);
            for (int x = 0; x < 10; x++)
                for (int z = 0; z < 10; z++) kit.Instance("Floor panel", structure, new Vector3((x - 4.5f) * 2.4f, 0f, (z - 4.5f) * 2.4f));
            for (int x = 0; x < 10; x++)
            {
                kit.Instance("Wall panel", structure, new Vector3((x - 4.5f) * 2.4f, 0f, 12f));
                kit.Instance("Wall trim", structure, new Vector3((x - 4.5f) * 2.4f, 3.35f, 12f));
            }
            for (int z = 0; z < 10; z++)
            {
                if (hasExit && (z == 4 || z == 5)) continue;
                kit.Instance("Wall panel", structure, new Vector3(12f, 0f, (z - 4.5f) * 2.4f), 90f);
            }
            // Camera-facing sides remain low while invisible collisions keep the route bounded.
            Transform bounds = Group(structure, "Cutaway boundaries");
            Astra7VisualKit.Collider(bounds, new Vector3(0f, 1f, -12.1f), new Vector3(24f, 2f, 0.2f));
            Astra7VisualKit.Collider(bounds, new Vector3(-12.1f, 1f, -7.2f), new Vector3(0.2f, 2f, 9.6f));
            Astra7VisualKit.Collider(bounds, new Vector3(-12.1f, 1f, 7.2f), new Vector3(0.2f, 2f, 9.6f));
            if (index == 0) Astra7VisualKit.Collider(bounds, new Vector3(-12.1f, 1f, 0f), new Vector3(0.2f, 2f, 4.8f));
            for (int i = 0; i < 4; i++)
            {
                kit.Instance("Pillar", structure, new Vector3(-10.8f + i * 7.2f, 0f, 11.8f));
                kit.Instance("Console", structure, new Vector3(-6f + i * 3.4f, 0f, 9.8f));
                kit.Instance("Light strip", structure, new Vector3(-6f + i * 4f, 3.2f, 11.3f));
            }
            // Copper guides lead to the actual exit, without blocking the central comparison space.
            for (int i = 0; i < 4; i++)
            {
                Transform guide = kit.Box(structure, "Eastern route chevron", new Vector3(7.2f + i * 1.15f, 0.028f, -0.7f), new Vector3(0.45f, 0.025f, 0.06f), kit.Copper);
                guide.localRotation = Quaternion.Euler(0f, -35f, 0f);
                kit.Box(structure, "Eastern route light", new Vector3(7.2f + i * 1.15f, 0.03f, 0.7f), new Vector3(0.35f, 0.02f, 0.035f), index == 0 || index == 5 ? kit.Amber : kit.Cyan);
            }
            if (index > 1)
            {
                kit.Instance("Railing", structure, new Vector3(-5.5f, 0f, 8.5f));
                kit.Instance("Railing", structure, new Vector3(5.5f, 0f, 8.5f));
            }
        }

        private static DoorController BuildDoor(Astra7VisualKit kit, Transform parent, StationFeedbackAudio audio)
        {
            Transform door = Group(parent, "Animated eastern bulkhead"); door.localPosition = new Vector3(12f, 0f, 0f); door.localRotation = Quaternion.Euler(0f, 90f, 0f);
            var leaves = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                float sign = i == 0 ? -1f : 1f;
                leaves[i] = Group(door, i == 0 ? "Left moving leaf" : "Right moving leaf"); leaves[i].localPosition = new Vector3(sign * 0.9f, 1.5f, 0f);
                kit.Box(leaves[i], "Door armor", Vector3.zero, new Vector3(1.78f, 3f, 0.3f), kit.Panel);
                kit.Box(leaves[i], "Titanium inset", new Vector3(0f, 0f, -0.17f), new Vector3(1.36f, 2.60f, 0.07f), kit.Graphite);
                kit.Box(leaves[i], "Amber latch guide", new Vector3(-sign * 0.72f, 0f, -0.23f), new Vector3(0.035f, 2.2f, 0.03f), kit.Amber);
                kit.Box(door, "Frame upright", new Vector3(sign * 2.1f, 1.7f, 0f), new Vector3(0.6f, 3.4f, 0.7f), kit.Titanium);
                Astra7VisualKit.Collider(door, new Vector3(sign * 2.1f, 1.7f, 0f), new Vector3(0.6f, 3.4f, 0.7f));
            }
            kit.Box(door, "Frame lintel", new Vector3(0f, 3.35f, 0f), new Vector3(4.8f, 0.3f, 0.7f), kit.Titanium);
            Transform collision = Group(door, "Closed-door collision"); var blocker = collision.gameObject.AddComponent<BoxCollider>(); blocker.center = Vector3.up * 1.6f; blocker.size = new Vector3(3.6f, 3.2f, 0.45f);
            DoorController controller = door.gameObject.AddComponent<DoorController>(); controller.Configure(leaves[0], leaves[1], blocker, audio); return controller;
        }
        private static void BuildCorridor(Astra7VisualKit kit, Transform room)
        {
            Transform passage = Group(room, "Connected service passage");
            for (int x = 0; x < 2; x++)
                for (int z = 0; z < 2; z++) kit.Instance("Floor panel", passage, new Vector3(13.2f + x * 2.4f, 0f, -1.2f + z * 2.4f));
            for (int x = 0; x < 2; x++)
            {
                kit.Instance("Wall panel", passage, new Vector3(13.2f + x * 2.4f, 0f, 2.4f));
                kit.Instance("Railing", passage, new Vector3(13.2f + x * 2.4f, 0f, -2.4f));
                kit.Box(passage, "Passage guide", new Vector3(13.2f + x * 2.4f, 0.025f, -1.95f), new Vector3(1.8f, 0.02f, 0.03f), kit.Amber);
            }
        }
        private static Light[] BuildPracticals(Transform room, StoryRoomKind kind)
        {
            bool warm = kind == StoryRoomKind.Maintenance || kind == StoryRoomKind.Overshoot;
            var lights = new[] {
                Lamp(room, "Room practical", new Vector3(-4f, 3f, 3f), warm ? new Color(1f, 0.43f, 0.16f) : new Color(0.25f, 0.76f, 1f), kind == StoryRoomKind.AssistedPower ? 0.12f : 3f, 12f),
                Lamp(room, "Scientific fill", new Vector3(4f, 2.6f, 0f), new Color(0.44f, 0.53f, 1f), kind == StoryRoomKind.AssistedPower ? 0.08f : 1.8f, 10f) };
            if (kind == StoryRoomKind.Maintenance) room.gameObject.AddComponent<StationFlicker>().Configure(lights[0], 3f);
            return lights;
        }
        private static void BuildInteractable(Astra7VisualKit kit, Transform room, MainGameDirector director, StationInteraction action, Vector3 position)
        {
            GameObject item = kit.Instance(action == StationInteraction.ManualRelease ? "Console" : "Hologram pedestal", room, position);
            Transform indicator = kit.Box(item.transform, "Interactive cyan indicator", new Vector3(0f, 1.0f, -0.2f), new Vector3(0.35f, 0.20f, 0.10f), kit.Cyan);
            if (action == StationInteraction.AnveshPickup)
            {
                item.name = "ANVESH / quantum wrist interface dock";
                Transform device = Group(item.transform, "Attached ANVESH wrist interface"); device.localPosition = new Vector3(0f, .95f, 0f);
                kit.Box(device, "Device housing", Vector3.zero, new Vector3(.7f, .24f, .44f), kit.Graphite);
                kit.Box(device, "Cyan interface display", new Vector3(0f, .135f, 0f), new Vector3(.48f, .02f, .30f), kit.Cyan);
                kit.Box(device, "Copper cuff left", new Vector3(-.32f, -.08f, 0f), new Vector3(.055f, .26f, .44f), kit.Copper);
                kit.Box(device, "Copper cuff right", new Vector3(.32f, -.08f, 0f), new Vector3(.055f, .26f, .44f), kit.Copper);
                Lamp(item.transform, "ANVESH beacon", Vector3.up * 1.5f, new Color(0.2f, 0.85f, 1f), 2f, 4f);
            }
            else item.name = "MANUAL RELEASE / station access terminal";
            Transform label = Group(item.transform, "Interaction placard"); label.localPosition = new Vector3(0f, 1.5f, 0f); label.localRotation = Quaternion.Euler(0f, 45f, 0f);
            TextMesh text = label.gameObject.AddComponent<TextMesh>(); text.text = action == StationInteraction.ManualRelease ? "RELEASE TERMINAL" : "ANVESH";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 48; text.characterSize = .055f;
            text.anchor = TextAnchor.MiddleCenter; text.color = new Color(.45f, .9f, 1f); label.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
            item.AddComponent<StoryInteractable>().Configure(director, action, indicator.GetComponent<Renderer>(), action == StationInteraction.ManualRelease ? "ACCESS RELEASE TERMINAL" : "RECOVER ANVESH");
        }
        private static void BuildDamage(Astra7VisualKit kit, Transform room)
        {
            for (int i = 0; i < 5; i++)
            {
                Transform debris = kit.Box(room, "Damaged service plate", new Vector3(-5f + i * 2.1f, 0.08f, -6f + (i % 2) * 0.7f), new Vector3(1.1f, 0.14f, 1.4f), kit.Graphite);
                debris.localRotation = Quaternion.Euler(4f + i * 2f, i * 37f, 7f);
                kit.Box(debris, "Exposed warm conduit", new Vector3(0f, 0.6f, 0f), new Vector3(0.65f, 0.05f, 0.05f), kit.Amber);
            }
            Transform pod = Group(room, "Damaged transport shell"); pod.localPosition = new Vector3(-7f, 0f, -6f); pod.localRotation = Quaternion.Euler(0f, -20f, 0f);
            kit.Box(pod, "Transport hull", new Vector3(0f, 0.6f, 0f), new Vector3(3.7f, 1.2f, 2f), kit.Graphite);
            kit.Box(pod, "Buckled titanium panel", new Vector3(0.3f, 1.2f, 0f), new Vector3(2.8f, 0.15f, 1.8f), kit.Titanium).localRotation = Quaternion.Euler(-18f, 0f, 8f);
            Astra7VisualKit.Collider(pod, new Vector3(0f, 0.65f, 0f), new Vector3(3.7f, 1.3f, 2f));
        }
        private static void BuildHazard(Astra7VisualKit kit, Transform room, Transform player, SubtitleController subtitles, StationCamera camera, Vector3 position, bool repeat)
        {
            Transform root = Group(room, repeat ? "Reactor service discharge" : "Telegraphed falling panel"); root.localPosition = position;
            Transform warning = kit.Box(root, "Amber danger footprint", new Vector3(0f, 0.045f, 0f), new Vector3(1.8f, 0.035f, 3.4f), kit.Amber);
            warning.GetComponent<Renderer>().enabled = false;
            Transform panel = kit.Box(root, "Falling service panel", new Vector3(0f, 4f, 0f), new Vector3(1.55f, 0.18f, 3f), kit.Graphite);
            root.gameObject.AddComponent<StationHazard>().Configure(player, panel, warning.GetComponent<Renderer>(), subtitles, camera, repeat);
        }
        internal static GameObject BuildHologram(Astra7VisualKit kit, Transform room, SubtitleController subtitles, string speaker)
        {
            Transform root = Group(room, speaker + " / animated 3D recording"); root.localPosition = new Vector3(-6.7f, 0f, 5.6f); root.localRotation = Quaternion.Euler(0f, -135f, 0f);
            kit.Instance("Hologram pedestal", root, Vector3.zero);
            Transform body = Group(root, "Holographic researcher");
            kit.Box(body, "Research coat", new Vector3(0f, 1.16f, 0f), new Vector3(0.63f, 1.02f, 0.35f), kit.Hologram);
            kit.Box(body, "Left leg", new Vector3(-0.15f, 0.58f, 0f), new Vector3(0.20f, 0.60f, 0.24f), kit.Hologram);
            kit.Box(body, "Right leg", new Vector3(0.15f, 0.58f, 0f), new Vector3(0.20f, 0.60f, 0.24f), kit.Hologram);
            Transform head = Group(body, "Head pivot"); head.localPosition = new Vector3(0f, 1.81f, 0f);
            kit.Box(head, "Head", Vector3.zero, new Vector3(0.34f, 0.39f, 0.31f), kit.Hologram);
            kit.Box(head, "Hair volume", new Vector3(0f, 0.17f, -0.035f), new Vector3(0.38f, 0.18f, 0.35f), kit.Hologram);
            Transform arm = Group(body, "Gesturing shoulder"); arm.localPosition = new Vector3(-0.43f, 1.52f, 0f);
            kit.Box(arm, "Gesturing arm", new Vector3(0f, -0.32f, 0f), new Vector3(0.18f, 0.64f, 0.20f), kit.Hologram);
            kit.Box(body, "Resting arm", new Vector3(0.42f, 1.18f, 0f), new Vector3(0.18f, 0.66f, 0.20f), kit.Hologram);
            root.gameObject.AddComponent<HologramPerformance>().Configure(body, arm, head, subtitles, speaker);
            root.gameObject.SetActive(false); return root.gameObject;
        }
        private static void ConfigureRendering(Transform root)
        {
            string path = Astra7VisualKit.Root + "/Phase4 Full Resolution URP.asset";
            UniversalRenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (pipeline == null)
            {
                pipeline = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset"));
                pipeline.name = "Phase4 Full Resolution URP";
                pipeline.supportsCameraDepthTexture = false; pipeline.supportsCameraOpaqueTexture = false;
                AssetDatabase.CreateAsset(pipeline, path);
            }
            // Apply to cached assets too: rebuilding must not retain the older 2x preset.
            pipeline.renderScale = 1f; pipeline.msaaSampleCount = 4;
            pipeline.supportsCameraDepthTexture = false; pipeline.supportsCameraOpaqueTexture = false;
            EditorUtility.SetDirty(pipeline);
            root.gameObject.AddComponent<GameplayRenderSettings>().Configure(pipeline);
            RenderSettings.ambientMode = AmbientMode.Trilight; RenderSettings.ambientSkyColor = new Color(0.23f, 0.32f, 0.41f);
            RenderSettings.ambientEquatorColor = new Color(0.12f, 0.18f, 0.24f); RenderSettings.ambientGroundColor = new Color(0.07f, 0.10f, 0.14f);
            RenderSettings.fog = false;
            Light key = Lamp(root, "Station key", Vector3.zero, new Color(0.68f, 0.82f, 1f), 1.3f, 1f);
            key.type = LightType.Directional; key.transform.rotation = Quaternion.Euler(52f, -24f, 0f); key.shadows = LightShadows.Soft; key.shadowStrength = 0.62f;
            var volume = Group(root, "Sharp gameplay post processing").gameObject.AddComponent<Volume>(); volume.isGlobal = true; volume.priority = 10f;
            string volumePath = Astra7VisualKit.Root + "/Phase4 Restrained Atmosphere.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(volumePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(profile, volumePath);
                Bloom bloom = profile.Add<Bloom>(true); bloom.intensity.Override(0.10f); bloom.threshold.Override(1.5f); bloom.scatter.Override(0.35f); bloom.highQualityFiltering.Override(false);
                Vignette vignette = profile.Add<Vignette>(true); vignette.intensity.Override(0.10f);
                DepthOfField depth = profile.Add<DepthOfField>(true); depth.mode.Override(DepthOfFieldMode.Off);
                MotionBlur motion = profile.Add<MotionBlur>(true); motion.intensity.Override(0f);
                Tonemapping tone = profile.Add<Tonemapping>(true); tone.mode.Override(TonemappingMode.ACES);
                foreach (VolumeComponent component in profile.components) AssetDatabase.AddObjectToAsset(component, profile);
                EditorUtility.SetDirty(profile);
            }
            Bloom sharpBloom = profile.TryGet(out Bloom existingBloom) ? existingBloom : profile.Add<Bloom>(true);
            sharpBloom.intensity.Override(.06f); sharpBloom.threshold.Override(1.8f); sharpBloom.scatter.Override(.2f); sharpBloom.highQualityFiltering.Override(false);
            DepthOfField sharpDepth = profile.TryGet(out DepthOfField existingDepth) ? existingDepth : profile.Add<DepthOfField>(true);
            sharpDepth.mode.Override(DepthOfFieldMode.Off);
            MotionBlur sharpMotion = profile.TryGet(out MotionBlur existingMotion) ? existingMotion : profile.Add<MotionBlur>(true);
            sharpMotion.intensity.Override(0f);
            foreach (VolumeComponent component in profile.components)
                if (!AssetDatabase.Contains(component)) AssetDatabase.AddObjectToAsset(component, profile);
            EditorUtility.SetDirty(profile);
            volume.sharedProfile = profile;
        }
        private static void BuildStoryHud(AnveshHUD hud, MainGameDirector director, PlayerHealth health, SubtitleController subtitles)
        {
            Transform canvas = hud.transform;
            RectTransform objective = (RectTransform)canvas.Find("Objective"); objective.sizeDelta = new Vector2(600f, 112f);
            Text objectiveText = objective.GetComponentInChildren<Text>(); objectiveText.rectTransform.sizeDelta = new Vector2(565f, 88f); objectiveText.fontSize = 18;
            Text area = null;
            foreach (Text label in canvas.GetComponentsInChildren<Text>()) if (label.text.StartsWith("ASTRA")) { area = label; break; }
            if (area == null) area = UiText(canvas, "Location", 18, new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(550f, 34f));
            Text vitals = UiText(canvas, "Vitals", 18, new Vector2(0f, 1f), new Vector2(31f, -115f), new Vector2(370f, 28f));
            Text prompt = UiText(canvas, "Interaction prompt", 23, new Vector2(0.5f, 0f), new Vector2(0f, 340f), new Vector2(900f, 36f));
            Image subtitlePanel = UiPanel(canvas, "Subtitles", new Vector2(0.5f, 0f), new Vector2(0f, 207f), new Vector2(1120f, 108f));
            Text speaker = UiText(subtitlePanel.transform, "Speaker", 18, new Vector2(0f, 1f), new Vector2(22f, -10f), new Vector2(1076f, 24f));
            speaker.color = new Color(0.95f, 0.69f, 0.39f);
            Text words = UiText(subtitlePanel.transform, "Dialogue", 25, new Vector2(0f, 1f), new Vector2(22f, -38f), new Vector2(1076f, 65f));
            subtitles.Configure(speaker, words, subtitlePanel.gameObject); subtitlePanel.gameObject.SetActive(false);
            Image toast = UiPanel(canvas, "Ability pulse", new Vector2(0.5f, 1f), new Vector2(0f, -107f), new Vector2(450f, 48f));
            Text unlock = UiText(toast.transform, "Unlock", 20, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(440f, 40f));
            canvas.gameObject.AddComponent<StoryHudOverlay>().Configure(director, health, vitals, prompt, area, unlock, toast);
        }
        private static Text UiText(Transform parent, string name, int fontSize, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(Text)); item.transform.SetParent(parent, false);
            var rect = (RectTransform)item.transform; rect.anchorMin = rect.anchorMax = rect.pivot = anchor; rect.anchoredPosition = position; rect.sizeDelta = size;
            Text text = item.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = fontSize;
            text.color = new Color(0.84f, 0.94f, 0.98f); text.raycastTarget = false; text.alignment = anchor.x == 0.5f ? TextAnchor.MiddleCenter : TextAnchor.UpperLeft; return text;
        }
        private static Image UiPanel(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(Image)); item.transform.SetParent(parent, false);
            var rect = (RectTransform)item.transform; rect.anchorMin = rect.anchorMax = rect.pivot = anchor; rect.anchoredPosition = position; rect.sizeDelta = size;
            Image image = item.GetComponent<Image>(); image.color = new Color(0.025f, 0.045f, 0.065f, 0.93f); image.raycastTarget = false; return image;
        }
        private static Light Lamp(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
        {
            Transform item = Group(parent, name); item.localPosition = position; Light light = item.gameObject.AddComponent<Light>();
            light.type = LightType.Point; light.color = color; light.intensity = intensity; light.range = range; light.shadows = LightShadows.None; return light;
        }
        private static Transform Group(Transform parent, string name)
        { var item = new GameObject(name); item.transform.SetParent(parent, false); return item.transform; }
    }
}
