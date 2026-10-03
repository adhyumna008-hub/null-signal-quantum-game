using System.Collections.Generic;
using NullSignal.Gameplay;
using NullSignal.Presentation;
using NullSignal.Story;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NullSignal.Editor
{
    public static class NullSignalPhase6Builder
    {
        [MenuItem("Tools/NULL SIGNAL/Build Phase 6 Complete Game")]
        public static void BuildCompleteGame() => NullSignalPhase4Builder.BuildMainGame(true, true);

        internal static void BuildFinalRoom(Astra7VisualKit kit, Transform section, StoryQuantumRoom room,
            MainGameDirector director, Transform player, StationCamera camera, Light[] lights, SubtitleController dialogue)
        {
            var finale = section.gameObject.AddComponent<FinaleRoomController>(); room.ConfigureFinale(finale);
            bool vault = room.Kind == StoryRoomKind.Vault, prime = room.Kind == StoryRoomKind.Prime;
            ArenaSurgeHazard hazard = null;
            GameObject ananya = null, lubna = null, reconstruction = null;
            Transform signal = null, silhouette = null;
            QuantumEncounter demo = null;
            AnveshakCorePresentation core = null;
            if (vault || prime)
            {
                BuildRadialArchitecture(kit, section, prime);
                LineRenderer danger = Line(kit, section, "Amber surge / 1.4 second warning", 65, .045f, true);
                danger.enabled = false;
                hazard = section.gameObject.AddComponent<ArenaSurgeHazard>(); hazard.Configure(player, camera, danger);
            }
            if (vault)
            {
                Transform array = section.Find("Candidate array");
                for (int i = 0; i < 16; i++)
                {
                    float angle = (i + .5f) * Mathf.PI / 8f;
                    NullSignalPhase3Builder.BuildNode(kit, array, room.Encounter, i, new Vector3(Mathf.Cos(angle) * 7.7f, 0, Mathf.Sin(angle) * 6.7f));
                    array.GetChild(array.childCount - 1).localScale = Vector3.one * .85f;
                }
                NullSignalPhase3Builder.BuildApparatus(kit, section, new Vector3(0, 0, 2f));
                ananya = NullSignalPhase4Builder.BuildHologram(kit, section, dialogue, "ANANYA"); ananya.transform.localPosition = new Vector3(-3.2f, 0, 1.5f);
                lubna = NullSignalPhase4Builder.BuildHologram(kit, section, dialogue, "LUBNA"); lubna.transform.localPosition = new Vector3(3.2f, 0, 1.5f);
                signal = Group(section, "Archived signal / narrative reconstruction"); signal.localPosition = new Vector3(0, 2.9f, 1.7f);
                kit.Shape(signal, "Almost undetectable signature", kit.Crystal, Vector3.zero, new Vector3(.45f, 1f, .45f), kit.Hologram);
                for (int i = 0; i < 3; i++)
                    kit.Shape(signal, "Structured signal orbit", kit.Ring, Vector3.zero, Vector3.one * (1f + i * .28f), kit.Hologram).localRotation = Quaternion.Euler(i * 43f, i * 54f, 23f);
                silhouette = BuildSilhouette(kit, section); silhouette.localPosition = new Vector3(0, .7f, -1.3f);
                signal.gameObject.SetActive(false); silhouette.gameObject.SetActive(false);
            }
            else if (prime)
            {
                NullSignalPhase5Builder.BuildEncounter(kit, section, room, room.Encounter, director, player, camera, lights, true, true);
                NullSignalPhase3Builder.BuildApparatus(kit, section, new Vector3(0, 0, 3.2f));
                // This object's display name contains '/', which Transform.Find treats as a hierarchy path.
                Transform apparatus = section.GetChild(section.childCount - 1);
                apparatus.localScale = new Vector3(1.1f, 1.4f, 1.1f);
                Transform crown = kit.Shape(apparatus, "Suspended titanium containment crown", kit.Ring, new Vector3(0, 3.6f, 0), new Vector3(3.6f, .28f, 3.6f), kit.Titanium);
                var fragments = new List<Transform>();
                for (int i = 0; i < 12; i++)
                {
                    float a = i * Mathf.PI / 6f;
                    Transform shard = kit.Shape(apparatus, "Suspended quantum fracture", kit.Crystal,
                        new Vector3(Mathf.Cos(a) * 1.8f, 2.2f + (i % 3) * .25f, Mathf.Sin(a) * 1.8f), new Vector3(.11f, .6f, .12f), kit.Violet);
                    shard.localRotation = Quaternion.Euler(12f, i * 30f, 23f); fragments.Add(shard);
                }
                // The crown is structural and stays after shutdown; emissive fragments collapse without physics.
                crown.name = "Suspended containment structure";
                Transform emergency = Group(section, "Emergency amber route / powers on at shutdown");
                for (int i = 0; i < 10; i++) kit.Box(emergency, "Emergency floor strip", new Vector3(1f + i, .06f, -.8f), new Vector3(.65f, .03f, .08f), kit.Amber);
                emergency.gameObject.SetActive(false);
                var emissives = new List<Renderer>();
                foreach (Renderer renderer in apparatus.GetComponentsInChildren<Renderer>())
                    if (renderer.sharedMaterial == kit.Cyan || renderer.sharedMaterial == kit.Violet || renderer.sharedMaterial == kit.Hologram || renderer is LineRenderer) emissives.Add(renderer);
                foreach (Transform child in section)
                    if (child.name == "Corrupted wall fragment") { fragments.Add(child); emissives.Add(child.GetComponent<Renderer>()); }
                core = apparatus.gameObject.AddComponent<AnveshakCorePresentation>();
                core.Configure(apparatus.GetComponentInChildren<ApparatusMotion>(), emissives.ToArray(), fragments.ToArray(), lights, emergency.gameObject);
                BuildDevice(kit, section, finale, false, new Vector3(0, 0, .9f));
                demo = Group(section, "Scientific reconstruction / original N4 simulation").gameObject.AddComponent<QuantumEncounter>();
                demo.Configure(4, 0); demo.RestartEncounter();
                Transform projection = Group(section, "Scientific payoff / four reconstructed states"); reconstruction = projection.gameObject;
                Vector3 right = new Vector3(.7071f, 0, -.7071f);
                for (int i = 0; i < 4; i++)
                {
                    NullSignalPhase3Builder.BuildNode(kit, projection, demo, i, right * ((i - 1.5f) * 2.3f) + new Vector3(-1.4f, 0, -1.4f));
                    projection.GetChild(projection.childCount - 1).localScale = Vector3.one * .7f;
                }
                reconstruction.SetActive(false);
            }
            else BuildExtraction(kit, section, finale);
            finale.Configure(room, director, camera, hazard, ananya, lubna, signal, silhouette, demo, reconstruction, core);
        }

        private static void BuildRadialArchitecture(Astra7VisualKit kit, Transform section, bool prime)
        {
            kit.Shape(section, "Circular archive inlay", kit.Ring, new Vector3(0, .035f, 0), new Vector3(20f, .025f, 20f), kit.Copper);
            kit.Shape(section, "Inner instrument circle", kit.Ring, new Vector3(0, .041f, 0), new Vector3(8.8f, .018f, 8.8f), kit.Titanium);
            for (int i = 0; i < 24; i++)
            {
                float a = i * Mathf.PI / 12f;
                Transform line = kit.Box(section, "Scientific radial floor interval", new Vector3(Mathf.Cos(a) * 8.8f, .04f, Mathf.Sin(a) * 8.8f), new Vector3(1.9f, .02f, .035f), kit.Copper);
                line.localRotation = Quaternion.Euler(0, -i * 15f, 0);
            }
            // Vertical detail sits against the rear wall, leaving the camera-facing field open.
            for (int i = 0; i < 9; i++)
            {
                float x = -9.2f + i * 2.3f;
                Transform column = Group(section, prime ? "Core support rib" : "Transmission archive column"); column.localPosition = new Vector3(x, 0, 10.4f);
                kit.Box(column, "Graphite archive casing", new Vector3(0, 2.15f, 0), new Vector3(.85f, 4.3f, .65f), kit.Graphite);
                kit.Box(column, "Contained memory light", new Vector3(0, 2.2f, -.35f), new Vector3(.13f, 3.6f, .06f), prime && i % 3 == 0 ? kit.Violet : kit.Cyan);
                for (int level = 0; level < 5; level++)
                    kit.Box(column, "Copper memory bus", new Vector3(0, .7f + level * .7f, -.4f), new Vector3(.7f, .065f, .04f), kit.Copper);
            }
        }
        private static Transform BuildSilhouette(Astra7VisualKit kit, Transform section)
        {
            Transform figure = Group(section, "Archived Chhaya / emerging 3D silhouette");
            kit.Box(figure, "Unstable torso", new Vector3(0, 1.5f, 0), new Vector3(.8f, 1.2f, .38f), kit.Hologram);
            kit.Shape(figure, "Emerging observer", kit.Crystal, new Vector3(0, 2.45f, 0), new Vector3(.4f, .64f, .36f), kit.Violet);
            for (int side = -1; side <= 1; side += 2)
            {
                kit.Box(figure, "Possible arm", new Vector3(side * .66f, 1.35f, 0), new Vector3(.2f, 1.18f, .25f), kit.Hologram);
                kit.Box(figure, "Possible leg", new Vector3(side * .25f, .48f, 0), new Vector3(.24f, .9f, .25f), kit.Hologram);
            }
            return figure;
        }
        private static void BuildDevice(Astra7VisualKit kit, Transform section, FinaleRoomController finale, bool extraction, Vector3 position)
        {
            Transform device = Group(section, extraction ? "Emergency craft boarding control / E" : "Anveshak shutdown control / E"); device.localPosition = position;
            kit.Box(device, "Device pedestal", new Vector3(0, .48f, 0), new Vector3(.75f, .96f, .6f), kit.Graphite);
            kit.Box(device, "Cyan interaction surface", new Vector3(0, 1.05f, 0), new Vector3(.8f, .13f, .65f), kit.Cyan);
            kit.Box(device, "Copper control rim", new Vector3(0, .92f, 0), new Vector3(.88f, .09f, .74f), kit.Copper);
            Astra7VisualKit.Collider(device, new Vector3(0, .55f, 0), new Vector3(.8f, 1.1f, .65f));
            device.gameObject.AddComponent<FinaleInteractable>().Configure(finale, extraction);
        }
        private static void BuildExtraction(Astra7VisualKit kit, Transform section, FinaleRoomController finale)
        {
            Transform craft = Group(section, "Emergency craft / extraction"); craft.localPosition = new Vector3(4, 0, 1.5f);
            kit.Box(craft, "Compact graphite hull", new Vector3(0, 1f, 0), new Vector3(4.2f, 1.8f, 2.5f), kit.Graphite);
            kit.Box(craft, "Titanium canopy", new Vector3(.1f, 1.95f, 0), new Vector3(2.8f, .4f, 2.1f), kit.Titanium);
            kit.Box(craft, "Cyan cockpit", new Vector3(-2.13f, 1.32f, 0), new Vector3(.08f, .55f, 1.5f), kit.Hologram);
            for (int side = -1; side <= 1; side += 2)
            {
                kit.Box(craft, "Boarding stabilizer", new Vector3(.4f, .8f, side * 1.7f), new Vector3(2.8f, .3f, 1.25f), kit.Panel);
                kit.Shape(craft, "Engine readiness ring", kit.Ring, new Vector3(1.3f, .85f, side * 1.9f), new Vector3(.85f, .2f, .85f), kit.Cyan);
            }
            Astra7VisualKit.Collider(craft, new Vector3(0, 1f, 0), new Vector3(4.2f, 2f, 2.5f));
            BuildDevice(kit, section, finale, true, new Vector3(.8f, 0, .5f));
            for (int i = 0; i < 12; i++) kit.Box(section, "Escape direction strip", new Vector3(-10 + i, .05f, -.6f), new Vector3(.6f, .025f, .1f), kit.Amber);
        }

        internal static void BuildFinalHUD(AnveshHUD hud, MainGameDirector director)
        {
            TMP_FontAsset font = NullSignalSharpUI.Font();
            foreach (Transform child in hud.transform)
                if (child.name == "Developer values / F3")
                {
                    var debug = (RectTransform)child; debug.anchoredPosition = new Vector2(-28, -170); debug.sizeDelta = new Vector2(400, 560);
                    Text data = child.GetComponentInChildren<Text>(true);
                    if (data != null) data.rectTransform.sizeDelta = new Vector2(372, 530);
                }
            RectTransform panel = Rect(hud.transform, "Vault timer / Prime vitality", new Vector2(.5f, 1), new Vector2(0, -58), new Vector2(530, 70));
            Image background = panel.gameObject.AddComponent<Image>(); background.color = new Color(.025f, .045f, .07f, .92f); background.raycastTarget = false;
            TMP_Text heading = NullSignalSharpUI.Label(panel, "Encounter heading", 20, new Vector2(12, -6), new Vector2(506, 27), font);
            TMP_Text status = NullSignalSharpUI.Label(panel, "Shield or timer status", 16, new Vector2(12, -34), new Vector2(506, 26), font);
            RectTransform bar = Rect(panel, "Live health / time remaining", new Vector2(0, 1), new Vector2(0, -65), new Vector2(530, 5));
            Image meter = bar.gameObject.AddComponent<Image>(); meter.type = Image.Type.Filled; meter.fillMethod = Image.FillMethod.Horizontal; meter.raycastTarget = false;
            // A white built-in UI sprite ensures Filled mode has geometry on all uGUI versions.
            meter.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            RectTransform ending = Rect(hud.transform, "Temporary Phase 6 ending", Vector2.zero, Vector2.zero, Vector2.zero);
            ending.anchorMax = Vector2.one; ending.offsetMin = ending.offsetMax = Vector2.zero;
            Canvas overlay = ending.gameObject.AddComponent<Canvas>(); overlay.overrideSorting = true; overlay.sortingOrder = 200;
            ending.gameObject.AddComponent<GraphicRaycaster>();
            Image black = ending.gameObject.AddComponent<Image>(); black.color = Color.black;
            CanvasGroup fade = ending.gameObject.AddComponent<CanvasGroup>(); fade.alpha = 0; fade.blocksRaycasts = false;
            RectTransform card = Rect(ending, "Ending title card", new Vector2(.5f, .5f), Vector2.zero, new Vector2(1200, 420));
            TMP_Text endTitle = NullSignalSharpUI.Label(card, "Mission complete", 48, new Vector2(0, 0), new Vector2(1200, 90), font);
            TMP_Text endWords = NullSignalSharpUI.Label(card, "Cliffhanger", 25, new Vector2(0, -145), new Vector2(1200, 275), font);
            endTitle.alignment = endWords.alignment = TextAlignmentOptions.Center;
            endTitle.color = new Color(.45f, .88f, .98f);
            hud.gameObject.AddComponent<FinaleHUD>().Configure(director, panel.gameObject, heading, status, meter, fade, endTitle, endWords);
            panel.gameObject.SetActive(false);
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
            var line = Group(parent, name).gameObject.AddComponent<LineRenderer>(); line.sharedMaterial = kit.Line;
            line.positionCount = count; line.widthMultiplier = width; line.useWorldSpace = world; line.shadowCastingMode = ShadowCastingMode.Off; line.receiveShadows = false; return line;
        }
    }
}
