using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace NullSignal.Editor
{
    /// <summary>Shared, locally generated geometry/materials and reusable station prefabs.</summary>
    internal sealed class Astra7VisualKit
    {
        internal const string Root = "Assets/Art/Astra7";
        internal Material Graphite, Panel, Titanium, Copper, Cyan, Amber, Violet, Hologram, Line, Suit, Skin, Hair;
        internal Material Ivory, Success, Danger, ResearcherProjection, ResearcherFace, ResearcherEcho;
        internal Mesh Bevel, Cylinder, Ring, Crystal;
        internal readonly Dictionary<string, GameObject> Modules = new Dictionary<string, GameObject>();

        internal Astra7VisualKit()
        {
            Folder(Root + "/Materials"); Folder(Root + "/Meshes"); Folder(Root + "/Prefabs");
            Graphite = Material("Graphite", "20262B", 0.3f, 0.30f);
            Panel = Material("Panel", "455154", 0.35f, 0.26f);
            Titanium = Material("Titanium", "9DA5A5", 0.55f, 0.40f);
            Copper = Material("Copper", "BB8057", 0.55f, 0.35f);
            Ivory = Material("Research ivory", "D3CEC1", 0.12f, 0.30f);
            Cyan = Material("Cyan practical", "63DCE8", 0f, 0.2f, 2.2f);
            Amber = Material("Saffron practical", "E5AD60", 0f, 0.2f, 1.8f);
            Violet = Material("Violet quantum", "9380ED", 0f, 0.2f, 1.6f);
            Success = Material("Access granted", "86E9B2", 0f, 0.2f, 1.4f);
            Danger = Material("Emergency red", "E76156", 0f, 0.2f, 1.5f);
            Suit = Material("Technical suit", "273039", 0.12f, 0.22f);
            Skin = Material("Warm skin", "A77A5D", 0f, 0.2f);
            Hair = Material("Hair and boots", "161B22", 0f, 0.15f);
            Hologram = Material("Holographic core", "63DCE8", 0.15f, 0.45f, 1.2f, true);
            Line = Material("Holographic lines", "FFFFFF", 0f, 0f, 0f, true, true);
            ResearcherProjection = CreateResearcherMaterial("Researcher ivory projection", new Color(.80f, .82f, .77f), new Color(.32f, .77f, .82f), .70f);
            ResearcherFace = CreateResearcherMaterial("Researcher warm projection", new Color(.76f, .52f, .36f), new Color(.55f, .72f, .77f), .81f);
            ResearcherEcho = CreateResearcherMaterial("Researcher dark projection", new Color(.12f, .16f, .22f), new Color(.57f, .44f, .79f), .76f);
            Bevel = MeshAsset("Beveled plate", BeveledPrism);
            Cylinder = MeshAsset("Twelve sided drum", () => Drum(12));
            Ring = MeshAsset("Radial hardware ring", () => Annulus(48));
            Crystal = MeshAsset("Octahedral core", Octahedron);
        }

        internal void BuildModules()
        {
            Module("Floor panel", root =>
            {
                Box(root, "Structural pan", new Vector3(0f, -0.16f, 0f), new Vector3(2.4f, 0.28f, 2.4f), Graphite);
                Box(root, "Recessed walking plate", new Vector3(0f, -0.03f, 0f), new Vector3(2.29f, 0.06f, 2.29f), Panel);
                for (int i = -1; i <= 1; i += 2)
                {
                    Box(root, "Copper seam", new Vector3(i * 1.10f, 0.008f, 0f), new Vector3(0.025f, 0.018f, 1.68f), Copper);
                    for (int j = -1; j <= 1; j += 2)
                        Box(root, "Corner latch", new Vector3(i * 0.98f, 0.025f, j * 0.98f), new Vector3(0.15f, 0.035f, 0.15f), Titanium);
                }
                Collider(root, new Vector3(0f, -0.12f, 0f), new Vector3(2.4f, 0.24f, 2.4f));
            });
            Module("Wall panel", root =>
            {
                Box(root, "Wall frame", new Vector3(0f, 1.65f, 0f), new Vector3(2.4f, 3.3f, 0.35f), Graphite);
                Box(root, "Titanium inset", new Vector3(0f, 1.65f, -0.21f), new Vector3(2.16f, 2.82f, 0.15f), Titanium);
                Box(root, "Graphite inset", new Vector3(0f, 1.65f, -0.31f), new Vector3(1.94f, 2.62f, 0.08f), Panel);
                Box(root, "Vent bed", new Vector3(0f, 1.80f, -0.38f), new Vector3(1.42f, 1.64f, 0.05f), Graphite);
                // Diamond lattice is technical ventilation, with no symbolic motif.
                for (int row = 0; row < 4; row++)
                    for (int col = 0; col < 3; col++)
                    {
                        float x = (col - 1) * 0.42f, y = 1.17f + row * 0.42f;
                        for (int side = 0; side < 4; side++)
                        {
                            float angle = side * 90f;
                            Vector3 offset = Quaternion.Euler(0f, 0f, angle) * new Vector3(0.1f, 0.1f, 0f);
                            Transform beam = Box(root, "Jaali vent", new Vector3(x, y, -0.42f) + offset, new Vector3(0.027f, 0.29f, 0.025f), Copper);
                            beam.localRotation = Quaternion.Euler(0f, 0f, angle - 45f);
                        }
                    }
                Box(root, "Upper practical", new Vector3(0f, 2.97f, -0.33f), new Vector3(1.62f, 0.055f, 0.07f), Amber);
                Box(root, "Ivory service header", new Vector3(0f, 2.76f, -0.37f), new Vector3(1.64f, 0.20f, 0.055f), Ivory);
                Box(root, "Lower guide", new Vector3(0f, 0.20f, -0.33f), new Vector3(1.9f, 0.035f, 0.07f), Cyan);
                Collider(root, new Vector3(0f, 1.65f, 0f), new Vector3(2.4f, 3.3f, 0.6f));
            });
            Module("Wall trim", root =>
            {
                Box(root, "Rail", Vector3.zero, new Vector3(2.4f, 0.12f, 0.16f), Titanium);
                Box(root, "Copper inlay", new Vector3(0f, 0.065f, -0.06f), new Vector3(2.15f, 0.025f, 0.035f), Copper);
            });
            Module("Pillar", root =>
            {
                Box(root, "Column", new Vector3(0f, 1.85f, 0f), new Vector3(0.48f, 3.7f, 0.48f), Titanium);
                Box(root, "Column inset", new Vector3(0f, 1.85f, -0.26f), new Vector3(0.3f, 3.2f, 0.13f), Graphite);
                Box(root, "Data conduit", new Vector3(0f, 1.95f, -0.34f), new Vector3(0.045f, 2.5f, 0.045f), Cyan);
                for (int i = 0; i < 2; i++)
                    Box(root, "Copper collar", new Vector3(0f, 0.35f + i * 2.9f, 0f), new Vector3(0.66f, 0.22f, 0.66f), Copper);
                Collider(root, new Vector3(0f, 1.8f, 0f), new Vector3(0.55f, 3.6f, 0.55f));
            });
            Module("Door", root =>
            {
                Box(root, "Bulkhead", new Vector3(0f, 1.55f, 0f), new Vector3(2.4f, 3.1f, 0.3f), Graphite);
                for (int side = -1; side <= 1; side += 2)
                {
                    Box(root, "Door leaf", new Vector3(side * 0.46f, 1.5f, -0.22f), new Vector3(0.88f, 2.83f, 0.14f), Panel);
                    Box(root, "Frame", new Vector3(side * 1.05f, 1.6f, -0.2f), new Vector3(0.23f, 3.2f, 0.5f), Titanium);
                    Box(root, "Warm frame inset", new Vector3(side * 0.94f, 1.6f, -0.47f), new Vector3(0.045f, 2.42f, 0.04f), Amber);
                    Box(root, "Latch", new Vector3(side * 0.16f, 1.55f, -0.35f), new Vector3(0.16f, 0.34f, 0.12f), Copper);
                }
                Box(root, "Lintel", new Vector3(0f, 3.12f, -0.18f), new Vector3(2.4f, 0.28f, 0.5f), Titanium);
                Box(root, "Authorization line", new Vector3(0f, 2.93f, -0.5f), new Vector3(0.66f, 0.04f, 0.025f), Cyan);
                Collider(root, new Vector3(0f, 1.6f, 0f), new Vector3(2.4f, 3.2f, 0.65f));
            });
            Module("Console", root =>
            {
                Box(root, "Foot", new Vector3(0f, 0.12f, 0f), new Vector3(1.35f, 0.24f, 0.8f), Graphite);
                Box(root, "Cabinet", new Vector3(0f, 0.62f, 0.06f), new Vector3(1.13f, 1.05f, 0.62f), Panel);
                Box(root, "Ivory service cover", new Vector3(0f, 0.44f, -0.265f), new Vector3(0.94f, 0.42f, 0.035f), Ivory);
                Box(root, "Copper belt", new Vector3(0f, 0.75f, -0.30f), new Vector3(1.2f, 0.075f, 0.04f), Copper);
                Transform display = new GameObject("Sloped instrument head").transform;
                display.SetParent(root, false); display.localPosition = new Vector3(0f, 1.25f, 0f); display.localRotation = Quaternion.Euler(22f, 0f, 0f);
                Box(display, "Monitor frame", Vector3.zero, new Vector3(1.30f, 0.65f, 0.20f), Titanium);
                Box(display, "Dark display", new Vector3(0f, 0f, -0.115f), new Vector3(1.13f, 0.48f, 0.035f), Graphite);
                for (int i = 0; i < 5; i++)
                    Box(display, "Circuit segment", new Vector3(-0.36f + i * 0.18f, -0.04f, -0.14f), new Vector3(0.025f, 0.14f + (i % 3) * 0.06f, 0.016f), Cyan);
                Box(display, "Readout rule", new Vector3(0f, -0.19f, -0.14f), new Vector3(0.8f, 0.016f, 0.016f), Cyan);
                Collider(root, new Vector3(0f, 0.7f, 0f), new Vector3(1.35f, 1.4f, 0.8f));
            });
            Module("Railing", root =>
            {
                for (int side = -1; side <= 1; side += 2)
                    Box(root, "Upright", new Vector3(side * 1.06f, 0.51f, 0f), new Vector3(0.07f, 1.02f, 0.1f), Titanium);
                Box(root, "Handrail", new Vector3(0f, 1.02f, 0f), new Vector3(2.3f, 0.07f, 0.12f), Titanium);
                Box(root, "Lower rail", new Vector3(0f, 0.37f, 0f), new Vector3(2.3f, 0.055f, 0.075f), Copper);
                Box(root, "Guide light", new Vector3(0f, 0.10f, 0f), new Vector3(1.9f, 0.025f, 0.045f), Amber);
                Collider(root, new Vector3(0f, 0.5f, 0f), new Vector3(2.4f, 1f, 0.16f));
            });
            Module("Light strip", root =>
            {
                Box(root, "Housing", Vector3.zero, new Vector3(1.8f, 0.14f, 0.14f), Graphite);
                Box(root, "Diffuser", new Vector3(0f, 0f, -0.085f), new Vector3(1.6f, 0.045f, 0.03f), Cyan);
                Box(root, "End cap L", new Vector3(-0.86f, 0f, -0.02f), new Vector3(0.07f, 0.17f, 0.16f), Copper);
                Box(root, "End cap R", new Vector3(0.86f, 0f, -0.02f), new Vector3(0.07f, 0.17f, 0.16f), Copper);
            });
            Module("Hologram pedestal", root => Pedestal(root, 0.65f));
            Module("Candidate platform", root => Pedestal(root, 1f));
        }

        private void Pedestal(Transform root, float factor)
        {
            Shape(root, "Lower socket", Cylinder, new Vector3(0f, 0.12f, 0f), new Vector3(2.15f, 0.24f, 2.15f) * factor, Graphite);
            Shape(root, "Machined lip", Ring, new Vector3(0f, 0.25f, 0f), new Vector3(2.0f, 1.7f, 2.0f) * factor, Copper);
            Shape(root, "Emitter bed", Cylinder, new Vector3(0f, 0.28f, 0f), new Vector3(1.7f, 0.16f, 1.7f) * factor, Panel);
            Shape(root, "Emitter aperture", Ring, new Vector3(0f, 0.38f, 0f), new Vector3(1.15f, 0.6f, 1.15f) * factor, Cyan);
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f;
                Transform clamp = Box(root, "Radial clamp", new Vector3(Mathf.Sin(a) * 0.86f, 0.33f, Mathf.Cos(a) * 0.86f) * factor,
                    new Vector3(0.18f, 0.24f, 0.35f) * factor, Titanium);
                clamp.localRotation = Quaternion.Euler(0f, i * 90f, 0f);
            }
            Collider(root, new Vector3(0f, 0.2f, 0f), new Vector3(1.65f, 0.4f, 1.65f) * factor);
        }

        internal GameObject Instance(string module, Transform parent, Vector3 position, float yaw = 0f)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(Modules[module], parent);
            instance.transform.localPosition = position; instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            return instance;
        }

        internal Transform Box(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
            => Shape(parent, name, Bevel, position, scale, material);
        internal Transform Shape(Transform parent, string name, Mesh mesh, Vector3 position, Vector3 scale, Material material)
        {
            var item = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            item.transform.SetParent(parent, false); item.transform.localPosition = position; item.transform.localScale = scale;
            item.GetComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = item.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = material == Cyan || material == Amber || material == Violet || material == Hologram || material == Success || material == Danger
                || material == ResearcherProjection || material == ResearcherFace || material == ResearcherEcho
                ? ShadowCastingMode.Off : ShadowCastingMode.On;
            return item.transform;
        }
        internal static void Collider(Transform parent, Vector3 center, Vector3 size)
        { BoxCollider collider = parent.gameObject.AddComponent<BoxCollider>(); collider.center = center; collider.size = size; }

        private void Module(string name, Action<Transform> build)
        {
            var root = new GameObject(name);
            try
            {
                build(root.transform);
                // Collapse static detail to one renderer per material in each reusable prefab.
                var groups = new Dictionary<Material, List<CombineInstance>>();
                MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter filter in filters)
                {
                    Material material = filter.GetComponent<Renderer>().sharedMaterial;
                    if (!groups.TryGetValue(material, out List<CombineInstance> group))
                    { group = new List<CombineInstance>(); groups.Add(material, group); }
                    group.Add(new CombineInstance { mesh = filter.sharedMesh, transform = root.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix });
                }
                foreach (MeshFilter filter in filters) UnityEngine.Object.DestroyImmediate(filter.gameObject);
                foreach (var pair in groups)
                {
                    Mesh mesh = MeshAsset(name + " " + pair.Key.name, () =>
                    {
                        var combined = new Mesh { name = name + " " + pair.Key.name };
                        combined.CombineMeshes(pair.Value.ToArray(), true, true); return combined;
                    }, true);
                    Shape(root.transform, pair.Key.name, mesh, Vector3.zero, Vector3.one, pair.Key);
                }
                Modules[name] = PrefabUtility.SaveAsPrefabAsset(root, Root + "/Prefabs/" + name + ".prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private Material Material(string name, string hex, float metallic, float smooth, float glow = 0f, bool transparent = false, bool line = false)
        {
            string path = Root + "/Materials/" + name + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find(line ? "Universal Render Pipeline/Particles/Unlit" : "Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("Existing URP shader unavailable: " + name);
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            var material = existing != null ? existing : new Material(shader) { name = name, enableInstancing = true };
            color.a = transparent && !line ? 0.5f : 1f;
            material.SetColor("_BaseColor", color); material.SetFloat("_Metallic", metallic); material.SetFloat("_Smoothness", smooth);
            if (glow > 0f) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", color * glow); }
            if (transparent)
            {
                material.SetFloat("_Surface", 1f); material.SetFloat("_Blend", 0f);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f); material.SetFloat("_Cull", (float)CullMode.Off);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); material.SetOverrideTag("RenderType", "Transparent");
                material.renderQueue = (int)RenderQueue.Transparent; material.SetShaderPassEnabled("ShadowCaster", false);
            }
            if (existing == null) AssetDatabase.CreateAsset(material, path); else EditorUtility.SetDirty(material);
            return material;
        }

        internal Material CreateResearcherMaterial(string name, Color baseColor, Color tint, float opacity)
        {
            string path = Root + "/Materials/" + name + ".mat";
            Shader shader = Shader.Find("NullSignal/Researcher Hologram");
            if (shader == null) throw new InvalidOperationException("Researcher hologram shader was not imported.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool created = material == null;
            if (created) material = new Material(shader) { name = name, enableInstancing = true };
            material.SetColor("_BaseColor", baseColor); material.SetColor("_TintColor", tint);
            material.SetFloat("_Opacity", opacity); material.SetFloat("_Distortion", .012f);
            material.SetFloat("_SignalStrength", 1f);
            if (created) AssetDatabase.CreateAsset(material, path); else EditorUtility.SetDirty(material);
            return material;
        }

        private static Mesh MeshAsset(string name, Func<Mesh> generate, bool refresh = false)
        {
            string path = Root + "/Meshes/" + name + ".asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null)
            {
                if (refresh)
                {
                    Mesh updated = generate(); EditorUtility.CopySerialized(updated, mesh);
                    mesh.name = name; EditorUtility.SetDirty(mesh); UnityEngine.Object.DestroyImmediate(updated);
                }
                return mesh;
            }
            mesh = generate(); mesh.name = name; AssetDatabase.CreateAsset(mesh, path); return mesh;
        }
        internal static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/'); Folder(path.Substring(0, slash));
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }

        private static Mesh BeveledPrism()
        {
            Vector2[] outline = { new Vector2(-0.4f,-0.5f), new Vector2(0.4f,-0.5f), new Vector2(0.5f,-0.4f), new Vector2(0.5f,0.4f),
                new Vector2(0.4f,0.5f), new Vector2(-0.4f,0.5f), new Vector2(-0.5f,0.4f), new Vector2(-0.5f,-0.4f) };
            var vertices = new List<Vector3>();
            for (int ring = 0; ring < 4; ring++)
                for (int i = 0; i < 8; i++)
                {
                    float y = ring == 0 ? -0.5f : ring == 1 ? -0.42f : ring == 2 ? 0.42f : 0.5f;
                    float factor = ring == 0 || ring == 3 ? 0.91f : 1f;
                    vertices.Add(new Vector3(outline[i].x * factor, y, outline[i].y * factor));
                }
            var tris = new List<int>();
            for (int ring = 0; ring < 3; ring++)
                for (int i = 0; i < 8; i++) Quad(tris, ring * 8 + i, (ring + 1) * 8 + i, (ring + 1) * 8 + (i + 1) % 8, ring * 8 + (i + 1) % 8);
            for (int i = 1; i < 7; i++) { tris.AddRange(new[] { 0, i, i + 1, 24, 24 + i + 1, 24 + i }); }
            return FlatMesh(vertices, tris);
        }
        private static Mesh Drum(int sides)
        {
            var vertices = new List<Vector3>(); var tris = new List<int>();
            for (int ring = 0; ring < 2; ring++)
                for (int i = 0; i < sides; i++)
                { float a = i * Mathf.PI * 2f / sides; vertices.Add(new Vector3(Mathf.Sin(a) * 0.5f, ring - 0.5f, Mathf.Cos(a) * 0.5f)); }
            for (int i = 0; i < sides; i++) Quad(tris, i, (i + 1) % sides, (i + 1) % sides + sides, i + sides);
            for (int i = 1; i < sides - 1; i++) tris.AddRange(new[] { 0, i + 1, i, sides, sides + i, sides + i + 1 });
            return FlatMesh(vertices, tris);
        }
        private static Mesh Annulus(int sides)
        {
            var vertices = new List<Vector3>(); var tris = new List<int>();
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2f / sides;
                vertices.Add(new Vector3(Mathf.Sin(a) * 0.5f, -0.04f, Mathf.Cos(a) * 0.5f));
                vertices.Add(new Vector3(Mathf.Sin(a) * 0.5f, 0.04f, Mathf.Cos(a) * 0.5f));
                vertices.Add(new Vector3(Mathf.Sin(a) * 0.41f, 0.04f, Mathf.Cos(a) * 0.41f));
                vertices.Add(new Vector3(Mathf.Sin(a) * 0.41f, -0.04f, Mathf.Cos(a) * 0.41f));
            }
            for (int i = 0; i < sides; i++)
                for (int side = 0; side < 4; side++) Quad(tris, i * 4 + side, ((i + 1) % sides) * 4 + side,
                    ((i + 1) % sides) * 4 + (side + 1) % 4, i * 4 + (side + 1) % 4);
            return FlatMesh(vertices, tris);
        }
        private static Mesh Octahedron()
        {
            var vertices = new List<Vector3> { Vector3.up * 0.7f, Vector3.down * 0.7f, Vector3.forward * 0.5f, Vector3.right * 0.5f, Vector3.back * 0.5f, Vector3.left * 0.5f };
            var tris = new List<int>();
            for (int i = 0; i < 4; i++) tris.AddRange(new[] { 0, 2 + i, 2 + (i + 1) % 4, 1, 2 + (i + 1) % 4, 2 + i });
            return FlatMesh(vertices, tris);
        }
        private static void Quad(List<int> tris, int a, int b, int c, int d) => tris.AddRange(new[] { a, b, c, a, c, d });
        private static Mesh FlatMesh(List<Vector3> points, List<int> triangles)
        {
            var vertices = new Vector3[triangles.Count]; var indices = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) { vertices[i] = points[triangles[i]]; indices[i] = i; }
            var mesh = new Mesh(); mesh.vertices = vertices; mesh.triangles = indices; mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
    }
}
