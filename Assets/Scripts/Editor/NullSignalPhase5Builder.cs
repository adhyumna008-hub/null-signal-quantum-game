using System.Collections.Generic;
using NullSignal.Gameplay;
using NullSignal.Player;
using NullSignal.Presentation;
using NullSignal.Story;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NullSignal.Editor
{
    public static class NullSignalPhase5Builder
    {
        [MenuItem("Tools/NULL SIGNAL/Build Phase 5 Combat")]
        public static void BuildCombat() => NullSignalPhase4Builder.BuildMainGame(true);

        internal static void BuildPlayerSystems(Astra7VisualKit kit, GameObject player, Camera camera)
        {
            player.AddComponent<AnveshOperationController>();
            LineRenderer beam = Line(kit, player.transform, "ANVESH defensive pulse", 2, .065f, true);
            beam.startColor = beam.endColor = new Color(.4f, .95f, 1f); beam.enabled = false;
            Renderer wrist = null;
            foreach (Renderer part in player.GetComponentsInChildren<Renderer>())
                if (part.name == "ANVESH luminous face") { wrist = part; break; }
            player.AddComponent<PlayerCombat>().Configure(camera, beam, wrist);
            LineRenderer pulse = Line(kit, player.transform, "Quantum operation wavefront", 65, .025f, true);
            pulse.enabled = false;
            player.AddComponent<AnveshOperationPulse>().Configure(player.GetComponent<AnveshOperationController>(), pulse, wrist);
        }

        internal static void BuildEncounter(Astra7VisualKit kit, Transform section, StoryQuantumRoom room,
            QuantumEncounter search, MainGameDirector director, Transform player, StationCamera camera, Light[] lights, bool chhaya, bool prime = false)
        {
            QuantumCombatEncounter combat = section.gameObject.AddComponent<QuantumCombatEncounter>();
            var actors = new CombatManifestation[prime ? 16 : chhaya ? 8 : 4];
            for (int i = 0; i < actors.Length; i++) actors[i] = BuildManifestation(kit, section, combat, i, chhaya);
            combat.Configure(search, actors, room, director, player, camera, lights, chhaya, prime); room.ConfigureCombat(combat);
            kit.Shape(section, "Radial arena boundary", kit.Ring, new Vector3(0, .025f, 0), new Vector3(13f, .03f, 13f), kit.Copper);
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI / 6f;
                Transform marker = kit.Box(section, "Arena circuit interval", new Vector3(Mathf.Cos(a) * 7.2f, .035f, Mathf.Sin(a) * 7.2f), new Vector3(.65f, .025f, .06f), chhaya ? kit.Violet : kit.Cyan);
                marker.localRotation = Quaternion.Euler(0, -i * 30f, 0);
            }
            if (chhaya)
            {
                for (int i = 0; i < 7; i++)
                {
                    Transform shard = kit.Box(section, "Corrupted wall fragment", new Vector3(-8f + i * 2.5f, 1.6f + (i % 3) * .4f, 10.9f), new Vector3(.13f, 1.8f, .24f), kit.Violet);
                    shard.localRotation = Quaternion.Euler(0, 0, i % 2 == 0 ? 28 : -28);
                }
                Transform structure = section.Find("Astra-7 modular structure");
                structure.gameObject.AddComponent<StationPowerState>().Configure(false);
            }
        }

        private static CombatManifestation BuildManifestation(Astra7VisualKit kit, Transform parent, QuantumCombatEncounter owner, int index, bool chhaya)
        {
            Transform root = Group(parent, (chhaya ? "Chhaya possibility " : "Security drone state ") + (index + 1).ToString("00"));
            float angle = index * Mathf.PI * .5f;
            root.localPosition = new Vector3(Mathf.Cos(angle) * 4.9f, 0, Mathf.Sin(angle) * 4.9f);
            Transform body = Group(root, chhaya ? "Fragmented humanoid" : "Floating security chassis"); body.localPosition = Vector3.up * 1.4f;
            var fragments = new List<Transform>(); var emissives = new List<Renderer>();
            Transform sensor = Group(body, "Phase sensor pivot");
            if (!chhaya)
            {
                kit.Shape(body, "Graphite disc chassis", kit.Cylinder, Vector3.zero, new Vector3(1.3f, .32f, 1f), kit.Graphite);
                kit.Shape(body, "Titanium upper cowl", kit.Cylinder, new Vector3(0, .2f, -.04f), new Vector3(1.08f, .16f, .85f), kit.Titanium);
                kit.Shape(body, "Copper stabilizer ring", kit.Ring, new Vector3(0, -.19f, 0), new Vector3(1.1f, .1f, .85f), kit.Copper);
                sensor.localPosition = new Vector3(0, .02f, .52f);
                Transform eye = kit.Shape(sensor, "Circular central sensor", kit.Ring, Vector3.zero, Vector3.one * .39f, kit.Cyan);
                eye.localRotation = Quaternion.Euler(90, 0, 0); emissives.Add(eye.GetComponent<Renderer>());
                kit.Shape(sensor, "Sensor aperture", kit.Crystal, Vector3.zero, Vector3.one * .17f, kit.Hologram);
                for (int side = -1; side <= 1; side += 2)
                {
                    Transform vane = Group(body, "Articulated stabilizer"); vane.localPosition = new Vector3(side * .72f, -.05f, -.12f);
                    kit.Box(vane, "Vane armor", Vector3.zero, new Vector3(.5f, .13f, .52f), kit.Graphite);
                    emissives.Add(kit.Box(vane, "Engine strip", new Vector3(0, -.09f, 0), new Vector3(.34f, .035f, .13f), kit.Cyan).GetComponent<Renderer>());
                    fragments.Add(vane);
                }
            }
            else
            {
                fragments.Add(kit.Box(body, "Broken torso", new Vector3(0, .15f, 0), new Vector3(.65f, .92f, .35f), kit.Graphite));
                fragments.Add(kit.Shape(body, "Unresolved head", kit.Crystal, new Vector3(0, .95f, .02f), new Vector3(.35f, .56f, .33f), kit.Hair));
                for (int side = -1; side <= 1; side += 2)
                {
                    fragments.Add(kit.Box(body, "Suspended forearm", new Vector3(side * .55f, -.1f, .06f), new Vector3(.18f, .9f, .24f), kit.Graphite));
                    fragments.Add(kit.Box(body, "Separated shin", new Vector3(side * .21f, -.83f, 0), new Vector3(.23f, .71f, .27f), kit.Hair));
                    fragments.Add(kit.Box(body, "Overlapping possible contour", new Vector3(side * .37f, .24f, -.22f), new Vector3(.25f, 1.14f, .18f), kit.Panel));
                    emissives.Add(kit.Box(body, "Violet fracture", new Vector3(side * .21f, .21f, .2f), new Vector3(.035f, .73f, .035f), kit.Violet).GetComponent<Renderer>());
                }
                sensor.localPosition = new Vector3(0, .6f, .25f);
                emissives.Add(kit.Shape(sensor, "Anomaly core", kit.Crystal, Vector3.zero, new Vector3(.16f, .32f, .13f), kit.Violet).GetComponent<Renderer>());
            }
            Transform shield = kit.Shape(root, "Quantum shield / exposed outline", kit.Ring, Vector3.up * .08f, Vector3.one * 1.3f, kit.Hologram);
            emissives.Add(shield.GetComponent<Renderer>());
            var motes = new Transform[12];
            for (int m = 0; m < motes.Length; m++) motes[m] = kit.Shape(body, "Phase flow mote", kit.Crystal, Vector3.zero, Vector3.one * .045f, chhaya ? kit.Violet : kit.Cyan);
            LineRenderer wave = Line(kit, root, "Signed amplitude", 33, .025f, false);
            LineRenderer warning = Line(kit, root, "Committed strike footprint", 49, .04f, true); warning.enabled = false;
            LineRenderer beam = Line(kit, root, "Telegraphed energy shot", 2, .08f, true); beam.enabled = false;
            var hitbox = root.gameObject.AddComponent<CapsuleCollider>(); hitbox.isTrigger = true; hitbox.center = Vector3.up * 1.6f; hitbox.radius = .72f; hitbox.height = chhaya ? 2.7f : 1.65f;
            var actor = root.gameObject.AddComponent<CombatManifestation>();
            actor.Configure(owner, index, body, sensor, shield, fragments.ToArray(), motes, wave, warning, beam, emissives.ToArray());
            return actor;
        }

        internal static void BuildCorruption(Astra7VisualKit kit, Transform section)
        {
            for (int i = 0; i < 6; i++)
            {
                Transform fracture = kit.Box(section, "Corrupted transition / violet conduit", new Vector3(12.8f + i * .6f, .04f, (i % 2 == 0 ? -1 : 1) * 1.25f), new Vector3(.7f, .03f, .045f), kit.Violet);
                fracture.localRotation = Quaternion.Euler(0, i * 29, 0);
            }
        }

        internal static void BuildAnalysis(AnveshHUD hud, AnveshOperationController source, AnveshController controller)
        {
            TMPro.TMP_FontAsset font = NullSignalSharpUI.Font();
            RectTransform panel = Rect(hud.transform, "ANVESH ANALYSIS", new Vector2(1, 1), new Vector2(420, -150), new Vector2(396, 618));
            Image background = panel.gameObject.AddComponent<Image>(); background.color = new Color(.025f, .045f, .07f, .96f); background.raycastTarget = false;
            CanvasGroup alpha = panel.gameObject.AddComponent<CanvasGroup>(); alpha.alpha = 0; alpha.blocksRaycasts = false;
            var title = NullSignalSharpUI.Label(panel, "OPERATION", 21, new Vector2(18, -18), new Vector2(360, 56), font); title.color = new Color(.4f, .9f, 1f);
            var formula = NullSignalSharpUI.Label(panel, "EQUATION", 20, new Vector2(18, -85), new Vector2(360, 96), font);
            var values = NullSignalSharpUI.Label(panel, "STATE CHANGE", 23, new Vector2(18, -190), new Vector2(360, 167), font);
            var result = NullSignalSharpUI.Label(panel, "RESULT", 19, new Vector2(18, -365), new Vector2(360, 54), font); result.color = new Color(.9f, .72f, .46f);
            RectTransform graph = Rect(panel, "Actual signed waves", new Vector2(0, 1), new Vector2(18, -432), new Vector2(360, 96));
            graph.gameObject.AddComponent<AnalysisWaveGraphic>().Configure(source);
            var teaching = NullSignalSharpUI.Label(panel, "First-use explanation", 17, new Vector2(18, -544), new Vector2(360, 62), font);
            teaching.color = new Color(.92f, .69f, .44f);
            RectTransform bar = Rect(panel, "Operation progress", new Vector2(0, 1), new Vector2(0, -614), new Vector2(396, 3));
            Image progress = bar.gameObject.AddComponent<Image>(); progress.color = new Color(.4f, .9f, 1f); progress.raycastTarget = false;
            progress.type = Image.Type.Filled; progress.fillMethod = Image.FillMethod.Horizontal; progress.fillOrigin = 0;
            panel.gameObject.AddComponent<AnveshAnalysisPanel>().ConfigureSharp(source, controller, panel, alpha, title, formula, values, result, teaching, progress);
            foreach (Text label in hud.GetComponentsInChildren<Text>(true))
                if (label.text.StartsWith("WASD  MOVE")) label.text = "WASD MOVE    SPACE DODGE    LMB AIM / FIRE    E INTERACT    F3 DEBUG";
            // Keep the independent F3 panel outside the gameplay analysis panel's slot.
            Transform debug = hud.transform.Find("Developer values / F3");
            if (debug != null) ((RectTransform)debug).anchoredPosition = new Vector2(-28, -170);
            NullSignalSharpUI.UpgradeHUD(hud, font);
        }
        private static Text Label(Transform parent, string name, int size, Vector2 position, Vector2 dimensions)
        {
            RectTransform rect = Rect(parent, name, new Vector2(0, 1), position, dimensions);
            Text text = rect.gameObject.AddComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size;
            text.color = new Color(.87f, .94f, 1); text.raycastTarget = false; text.supportRichText = false; text.text = ""; return text;
        }
        private static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var item = new GameObject(name, typeof(RectTransform)); item.transform.SetParent(parent, false);
            var rect = (RectTransform)item.transform; rect.anchorMin = rect.anchorMax = rect.pivot = anchor; rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        private static Transform Group(Transform parent, string name)
        { var item = new GameObject(name); item.transform.SetParent(parent, false); return item.transform; }
        private static LineRenderer Line(Astra7VisualKit kit, Transform parent, string name, int count, float width, bool world)
        {
            LineRenderer line = Group(parent, name).gameObject.AddComponent<LineRenderer>();
            line.sharedMaterial = kit.Line; line.positionCount = count; line.widthMultiplier = width; line.useWorldSpace = world;
            line.shadowCastingMode = ShadowCastingMode.Off; line.receiveShadows = false; line.numCapVertices = 2; return line;
        }
    }
}
