using System;
using NullSignal.Gameplay;
using NullSignal.Player;
using NullSignal.Presentation;
using NullSignal.Quantum;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullSignal.Editor
{
    /// <summary>Creates a separate, wired Phase 1 scene without changing Bootstrap or settings.</summary>
    public static class NullSignalGameBuilder
    {
        private const string SceneFolder = "Assets/Scenes";
        private const string MaterialFolder = "Assets/Materials/Foundation";

        [MenuItem("Tools/NULL SIGNAL/Build Foundation Prototype")]
        public static void BuildFoundationPrototype()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Exit Play Mode before building the foundation prototype.");
                return;
            }

            // Fail before touching scenes or assets if the independent math is invalid.
            ValidateQuantumFoundation();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogError("The existing URP Lit shader was not found. Check project import before building.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EnsureFolder(SceneFolder);
            EnsureFolder(MaterialFolder);
            Material roomMaterial = GetOrCreateMaterial("Room", new Color(0.22f, 0.26f, 0.3f), shader);
            Material playerMaterial = GetOrCreateMaterial("Player", new Color(0.15f, 0.8f, 0.86f), shader);
            Material candidateMaterial = GetOrCreateMaterial("Candidate", new Color(0.6f, 0.42f, 0.85f), shader);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("NULL SIGNAL - Phase 1 Foundation");
            var quantum = root.AddComponent<FoundationQuantumPrototype>();
            quantum.Configure(4, 2);

            CreatePrimitive("Floor", PrimitiveType.Cube, root.transform,
                new Vector3(0f, -0.25f, 0f), new Vector3(16f, 0.5f, 12f), roomMaterial);
            CreatePrimitive("North wall", PrimitiveType.Cube, root.transform,
                new Vector3(0f, 1f, 6f), new Vector3(16.5f, 2f, 0.5f), roomMaterial);
            CreatePrimitive("South boundary", PrimitiveType.Cube, root.transform,
                new Vector3(0f, 0.2f, -6f), new Vector3(16.5f, 0.4f, 0.5f), roomMaterial);
            CreatePrimitive("West wall", PrimitiveType.Cube, root.transform,
                new Vector3(-8f, 1f, 0f), new Vector3(0.5f, 2f, 12f), roomMaterial);
            CreatePrimitive("East wall", PrimitiveType.Cube, root.transform,
                new Vector3(8f, 1f, 0f), new Vector3(0.5f, 2f, 12f), roomMaterial);

            var player = new GameObject("Player - movement foundation");
            player.transform.SetParent(root.transform, false);
            player.transform.position = new Vector3(0f, 0.05f, -2.5f);
            CharacterController motor = player.AddComponent<CharacterController>();
            motor.height = 1.8f;
            motor.radius = 0.35f;
            motor.center = new Vector3(0f, 0.9f, 0f);
            motor.stepOffset = 0.2f;
            motor.skinWidth = 0.035f;
            PlayerController controller = player.AddComponent<PlayerController>();
            player.AddComponent<PlayerInteraction>();
            GameObject body = CreatePrimitive("Placeholder body", PrimitiveType.Capsule, player.transform,
                new Vector3(0f, 0.9f, 0f), new Vector3(0.7f, 0.9f, 0.7f), playerMaterial);
            // CharacterController is the single player collider/movement owner.
            UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());
            GameObject facing = CreatePrimitive("Facing indicator", PrimitiveType.Cube, player.transform,
                new Vector3(0f, 1.1f, 0.38f), new Vector3(0.18f, 0.18f, 0.22f), candidateMaterial);
            UnityEngine.Object.DestroyImmediate(facing.GetComponent<Collider>());

            for (int i = 0; i < 4; i++)
            {
                GameObject candidate = CreatePrimitive($"Quantum candidate {i + 1}", PrimitiveType.Cylinder,
                    root.transform, new Vector3(-4.5f + i * 3f, 0.55f, 2f),
                    new Vector3(0.85f, 0.55f, 0.85f), candidateMaterial);
                candidate.AddComponent<FoundationCandidate>().Configure(quantum, i);
            }

            var cameraObject = new GameObject("Isometric Camera");
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.tag = "MainCamera";
            Camera view = cameraObject.AddComponent<Camera>();
            view.clearFlags = CameraClearFlags.SolidColor;
            view.backgroundColor = new Color(0.055f, 0.07f, 0.095f);
            view.nearClipPlane = 0.1f;
            view.farClipPlane = 100f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<CameraController>().Configure(player.transform, new Vector3(0f, 0.8f, 0f));
            controller.SetCamera(cameraObject.transform);

            var lightObject = new GameObject("Foundation Directional Light");
            lightObject.transform.SetParent(root.transform, false);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.6f;
            light.color = new Color(1f, 0.96f, 0.88f);
            light.shadows = LightShadows.Soft;

            string scenePath = AssetDatabase.GenerateUniqueAssetPath(SceneFolder + "/FoundationPrototype.unity");
            if (!EditorSceneManager.SaveScene(scene, scenePath))
                throw new InvalidOperationException("Could not save the foundation scene: " + scenePath);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = root;
            Debug.Log("Foundation created at " + scenePath + ". Bootstrap and build settings were not changed. " +
                "Press Play; focus the Game view. WASD/arrows: move; Space: dodge; E: inspect; " +
                "Q: SCAN; 1: MARK; 2: AMPLIFY; 3: LOCK; R: reset. Repeat 1 then 2 to observe overshooting.", root);
        }

        [MenuItem("Tools/NULL SIGNAL/Validate Quantum Foundation")]
        public static void ValidateQuantumFoundation()
        {
            foreach (string result in QuantumValidation.RunAll())
                Debug.Log("NULL SIGNAL quantum validation: " + result);
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType type, Transform parent,
            Vector3 position, Vector3 scale, Material material)
        {
            GameObject item = GameObject.CreatePrimitive(type);
            item.name = name;
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            item.transform.localScale = scale;
            item.GetComponent<Renderer>().sharedMaterial = material;
            return item;
        }

        private static Material GetOrCreateMaterial(string name, Color color, Shader shader)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;
            material = new Material(shader) { name = "Foundation " + name };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.15f);
            // A unique path also protects an unexpected non-material asset at this location.
            AssetDatabase.CreateAsset(material, AssetDatabase.GenerateUniqueAssetPath(path));
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            int separator = path.LastIndexOf('/');
            string parent = path.Substring(0, separator);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(separator + 1));
        }
    }
}
