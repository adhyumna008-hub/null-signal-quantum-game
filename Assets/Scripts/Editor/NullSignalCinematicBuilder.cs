using System.Collections.Generic;
using NullSignal.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NullSignal.Editor
{
    /// <summary>Builds four inexpensive sets and articulated actors; no runtime asset downloads or Timeline dependency.</summary>
    internal static class NullSignalCinematicBuilder
    {
        internal static StationCinematicDirector Build(Astra7VisualKit kit, Transform root,
            SubtitleController subtitles, Camera gameplayCamera)
        {
            Transform owner = Group(root, "Cinematics / continuous 3D opening and ending");
            var director = owner.gameObject.AddComponent<StationCinematicDirector>();
            var references = new StationCinematicDirector.StageReferences();
            TMP_FontAsset font = NullSignalSharpUI.Font();
            Mesh sphere = PrimitiveMesh(PrimitiveType.Sphere);
            references.exterior = Group(owner, "Set A / Astra-7 exterior");
            references.exterior.localPosition = new Vector3(0f, -160f, 0f);
            references.control = Group(owner, "Set B / Anveshak control room");
            references.control.localPosition = new Vector3(240f, -160f, 0f);
            references.docking = Group(owner, "Set C / damaged retrieval dock");
            references.docking.localPosition = new Vector3(480f, -160f, 0f);
            references.cabin = Group(owner, "Set D / emergency craft cabin");
            references.cabin.localPosition = new Vector3(720f, -160f, 0f);
            Exterior(kit, references, sphere, font);
            ControlRoom(kit, references, sphere, font);
            Dock(kit, references, sphere, font);
            Cabin(kit, references, sphere, font);
            Transform cameraObject = Group(owner, "Cinematic camera / perspective shots only");
            var camera = cameraObject.gameObject.AddComponent<Camera>();
            camera.enabled = false; camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.009f, .012f, .02f); camera.nearClipPlane = .08f;
            camera.farClipPlane = 210f; camera.fieldOfView = 47f; camera.allowHDR = true;
            camera.allowMSAA = true; camera.allowDynamicResolution = false;
            var cameraData = cameraObject.gameObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true; cameraData.antialiasing = AntialiasingMode.None;
            StationFeedbackAudio audio = root.GetComponentInChildren<StationFeedbackAudio>(true);
            director.Configure(references, camera, gameplayCamera, subtitles, audio);
            references.exterior.gameObject.SetActive(false);
            references.control.gameObject.SetActive(false);
            references.docking.gameObject.SetActive(false);
            references.cabin.gameObject.SetActive(false);
            return director;
        }

        private static void Exterior(Astra7VisualKit kit, StationCinematicDirector.StageReferences r, Mesh sphere, TMP_FontAsset font)
        {
            Transform root = r.exterior;
            r.stationRing = Group(root, "Rotating habitat assembly");
            kit.Shape(r.stationRing, "Habitat graphite rim", kit.Ring, Vector3.zero, new Vector3(20f, 8f, 20f), kit.Graphite);
            kit.Shape(r.stationRing, "Titanium upper lip", kit.Ring, Vector3.up * .25f, new Vector3(20.2f, 3f, 20.2f), kit.Titanium);
            kit.Shape(r.stationRing, "Copper pressure seal", kit.Ring, Vector3.up * -.3f, new Vector3(19.8f, 1f, 19.8f), kit.Copper);
            for (int i = 0; i < 16; i++)
            {
                float a = i * Mathf.PI / 8f;
                var module = Group(r.stationRing, "Habitat module " + i);
                module.localPosition = new Vector3(Mathf.Sin(a) * 9f, 0f, Mathf.Cos(a) * 9f);
                module.localRotation = Quaternion.Euler(0f, i * 22.5f, 0f);
                kit.Box(module, "Pressure hull", Vector3.zero, new Vector3(1.2f, .7f, .8f), i % 2 == 0 ? kit.Ivory : kit.Titanium);
                kit.Box(module, "Window strip", new Vector3(0f, .12f, -.43f), new Vector3(.83f, .08f, .03f), i % 3 == 0 ? kit.Amber : kit.Cyan);
                kit.Box(module, "Service rib", new Vector3(0f, .48f, 0f), new Vector3(.12f, .28f, .9f), kit.Copper);
            }
            for (int i = 0; i < 6; i++)
            {
                Transform arm = Group(root, "Radial scientific truss " + i);
                arm.localRotation = Quaternion.Euler(0f, i * 60f, 0f);
                kit.Box(arm, "Structural spine", new Vector3(0f, -.15f, 5f), new Vector3(.46f, .38f, 9f), kit.Graphite);
                kit.Box(arm, "Copper data race", new Vector3(.19f, .1f, 5f), new Vector3(.06f, .045f, 8.8f), kit.Amber);
                for (int p = 0; p < 4; p++)
                {
                    Transform brace = kit.Box(arm, "Open truss diagonal", new Vector3(0f, .02f, 2f + p * 1.8f), new Vector3(.8f, .08f, .12f), kit.Titanium);
                    brace.localRotation = Quaternion.Euler(0f, p % 2 == 0 ? 35f : -35f, 0f);
                }
                if (i % 2 == 0)
                {
                    kit.Box(arm, "Solar-array mast", new Vector3(0f, 0f, 12f), new Vector3(.18f, .12f, 6f), kit.Titanium);
                    for (int side = -1; side <= 1; side += 2)
                    {
                        kit.Box(arm, "Photovoltaic frame", new Vector3(side * 1.7f, 0f, 13f), new Vector3(3f, .13f, 5.2f), kit.Copper);
                        kit.Box(arm, "Photovoltaic cells", new Vector3(side * 1.7f, .09f, 13f), new Vector3(2.8f, .025f, 5f), kit.Panel);
                        for (int p = 0; p < 7; p++)
                            kit.Box(arm, "Cell division", new Vector3(side * 1.7f, .11f, 10.8f + p * .73f), new Vector3(2.8f, .015f, .018f), kit.Titanium);
                    }
                }
            }
            kit.Shape(root, "Central pressure tower", kit.Cylinder, new Vector3(0f, 1.4f, 0f), new Vector3(3.5f, 5.2f, 3.5f), kit.Titanium);
            kit.Shape(root, "Central tower inset", kit.Cylinder, new Vector3(0f, 1.65f, 0f), new Vector3(3.57f, 3.4f, 3.57f), kit.Graphite);
            r.stationCore = Group(root, "Anveshak core assembly"); r.stationCore.localPosition = new Vector3(0f, 3.2f, 0f);
            var energy = new List<Transform>();
            for (int i = 0; i < 4; i++)
            {
                kit.Shape(r.stationCore, "Containment flange", kit.Ring, new Vector3(0f, -.8f + i * .5f, 0f), new Vector3(4f - i * .4f, 2f, 4f - i * .4f), kit.Copper);
                Transform glow = Group(r.stationCore, "Contracting core energy " + i);
                glow.localPosition = new Vector3(0f, -.65f + i * .5f, 0f);
                kit.Shape(glow, "Quantum loop", kit.Ring, Vector3.zero, new Vector3(3.5f - i * .4f, .5f, 3.5f - i * .4f), i < 3 ? kit.Cyan : kit.Violet);
                energy.Add(glow);
            }
            r.exteriorEnergy = energy.ToArray();
            kit.Box(root, "Antenna", new Vector3(0f, 6f, 0f), new Vector3(.09f, 4f, .09f), kit.Titanium);
            kit.Shape(root, "Emergency navigation beacon", sphere, new Vector3(0f, 8.1f, 0f), Vector3.one * .17f, kit.Danger);
            r.exteriorCraft = Group(root, "Emergency retrieval craft");
            Craft(kit, r.exteriorCraft);
            Material planet = Material("Cinematic Earth", new Color(.13f, .3f, .40f), .04f);
            r.earth = kit.Shape(root, "Earth / geometric planet", sphere, new Vector3(-28f, -30f, 100f), Vector3.one * 113f, planet);
            // A thin geometric limb, not a bitmap background.
            Transform limb = kit.Shape(root, "Earth atmospheric rim", kit.Ring, new Vector3(-28f, -30f, 98f), new Vector3(115f, 1f, 115f), kit.Cyan);
            limb.localRotation = Quaternion.Euler(74f, 0f, 0f);
            for (int i = 0; i < 82; i++)
            {
                float x = Mathf.Sin(i * 2.399963f) * (20f + i * .4f);
                float y = Mathf.Cos(i * 1.718f) * 29f + 8f;
                kit.Shape(root, "Distant star", kit.Crystal, new Vector3(x, y, 91f + i % 7), Vector3.one * (.04f + i % 3 * .025f), kit.Ivory);
            }
            r.distantSignals = Group(root, "Distant unknown quantum signatures");
            for (int i = 0; i < 28; i++)
            {
                var signature = Group(r.distantSignals, "Unknown signature " + i);
                signature.localPosition = new Vector3(Mathf.Sin(i * 2.3f) * (18f + i), 4f + Mathf.Abs(Mathf.Cos(i * 1.71f)) * 20f, 63f + i % 9 * 1.2f);
                kit.Shape(signature, "Quantum aperture", kit.Ring, Vector3.zero, new Vector3(4f, 1f, 4f), kit.Violet);
                kit.Shape(signature, "Core point", kit.Crystal, Vector3.zero, Vector3.one * 1.4f, kit.Cyan);
            }
            r.distantSignals.gameObject.SetActive(false);
            r.locationTitle = Label(root, font, "Station identification", new Vector3(-.5f, 7.2f, -3f), new Vector2(23f, 2.8f), 8f);
            Light(root, "Warm orbital sun", new Vector3(-12f, 17f, -12f), new Color(1f, .81f, .62f), 25f, 50f);
            Light(root, "Earth bounce", new Vector3(4f, -7f, 4f), new Color(.29f, .59f, .7f), 16f, 36f);
        }

        private static void ControlRoom(Astra7VisualKit kit, StationCinematicDirector.StageReferences r, Mesh sphere, TMP_FontAsset font)
        {
            Transform root = r.control;
            Room(kit, root, 6, 5);
            for (int side = -1; side <= 1; side += 2)
            {
                kit.Instance("Console", root, new Vector3(side * 1.5f, 0f, .6f), 180f);
                kit.Instance("Pillar", root, new Vector3(side * 4.7f, 0f, 4.5f));
                kit.Box(root, "Ivory acoustic wall", new Vector3(side * 4f, 2.2f, 5.7f), new Vector3(2.2f, 2.8f, .16f), kit.Ivory);
                for (int k = 0; k < 5; k++)
                    kit.Box(root, "Copper acoustic blade", new Vector3(side * 4f - .7f + k * .35f, 2.2f, 5.55f), new Vector3(.05f, 2.3f, .14f), kit.Copper);
            }
            r.ananya = Actor(kit, root, "Dr. Ananya Rao / physical researcher", sphere, false, false);
            r.lubna = Actor(kit, root, "Dr. Lubna / physical researcher", sphere, false, true);
            kit.Shape(root, "Anveshak foundation", kit.Cylinder, new Vector3(0f, .18f, 3.8f), new Vector3(4.7f, .36f, 4.7f), kit.Graphite);
            kit.Shape(root, "Containment hardware", kit.Ring, new Vector3(0f, .41f, 3.8f), new Vector3(4.3f, 1.4f, 4.3f), kit.Copper);
            r.arrayRings = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                r.arrayRings[i] = Group(root, "Animated array gimbal " + i);
                r.arrayRings[i].localPosition = new Vector3(0f, 2.1f, 4f);
                kit.Shape(r.arrayRings[i], "Machined ring", kit.Ring, Vector3.zero, new Vector3(2.8f, .8f, 2.8f), i == 1 ? kit.Copper : kit.Titanium);
                kit.Shape(r.arrayRings[i], "Light conductor", kit.Ring, Vector3.up * .025f, new Vector3(2.65f, .22f, 2.65f), kit.Cyan);
            }
            r.signalBars = new Transform[16]; r.signalWaves = new LineRenderer[16];
            for (int i = 0; i < 16; i++)
            {
                r.signalBars[i] = kit.Box(root, "Real amplitude column " + i, new Vector3(-1.65f + i * .22f, 1.3f, 1.4f), new Vector3(.13f, .55f, .13f), kit.Hologram);
                r.signalWaves[i] = Line(kit, root, "Directional phase filament " + i, 17, .012f, new Color(.39f, .87f, .9f, .6f));
            }
            kit.Box(root, "Holographic instrument desk", new Vector3(0f, 1.02f, 1.4f), new Vector3(4f, .09f, 1.1f), kit.Graphite);
            r.researchDisplay = Label(root, font, "Anveshak central display", new Vector3(0f, 3.05f, 3.9f), new Vector2(5.8f, .9f), 5f);
            r.anomaly = Group(root, "Chhaya / assembling three-dimensional fragments"); r.anomaly.localPosition = new Vector3(0f, .65f, 4.7f);
            var fragments = new List<Transform>();
            Vector3[] positions = { new Vector3(0,1.7f,0), new Vector3(0,1.05f,0), new Vector3(-.5f,1.1f,0),new Vector3(.5f,1.1f,0),
                new Vector3(-.65f,.45f,.1f),new Vector3(.65f,.45f,.1f),new Vector3(-.2f,.25f,0),new Vector3(.2f,.25f,0),new Vector3(-.23f,-.35f,.1f),new Vector3(.23f,-.35f,.1f) };
            for (int i = 0; i < positions.Length; i++)
            {
                fragments.Add(kit.Shape(r.anomaly, "Dark body fragment " + i, kit.Crystal, positions[i], i == 1 ? new Vector3(.75f, .9f, .35f) : new Vector3(.33f, .59f, .32f), kit.Graphite));
                fragments.Add(kit.Shape(r.anomaly, "Offset probability fracture " + i, kit.Crystal, positions[i] + new Vector3(.11f, .03f, .07f), new Vector3(.10f, .36f, .10f), kit.Violet));
            }
            r.anomalyFragments = fragments.ToArray();
            r.driftingMotes = new Transform[22];
            for (int i = 0; i < r.driftingMotes.Length; i++)
                r.driftingMotes[i] = kit.Shape(root, "Drifting research particle " + i, kit.Crystal, Vector3.up * 2f, Vector3.one * .035f, i % 4 == 0 ? kit.Amber : kit.Cyan);
            r.researchKey = Light(root, "Warm neutral researchers key", new Vector3(-2.4f, 3.8f, -1.5f), new Color(1f, .85f, .72f), 5.5f, 12f);
            Light(root, "Scientific equipment fill", new Vector3(1.5f, 3.1f, 3f), new Color(.4f, .8f, .9f), 3.5f, 10f);
            r.researchAlarm = Light(root, "Emergency red alarm", new Vector3(2.8f, 3f, .5f), new Color(1f, .17f, .10f), 0f, 12f);
        }

        private static void Dock(Astra7VisualKit kit, StationCinematicDirector.StageReferences r, Mesh sphere, TMP_FontAsset font)
        {
            Transform root = r.docking; Room(kit, root, 4, 3);
            Craft(kit, Group(root, "Docked retrieval pod"), new Vector3(0f, 1.05f, 4.7f));
            for (int side = -1; side <= 1; side += 2)
            {
                kit.Box(root, "Docking bulkhead frame", new Vector3(side * 1.95f, 1.5f, 2.85f), new Vector3(.5f, 3f, .5f), kit.Titanium);
                kit.Box(root, "Emergency jamb light", new Vector3(side * 1.65f, 1.5f, 2.56f), new Vector3(.05f, 2.4f, .05f), kit.Amber);
            }
            r.dockDoorL = kit.Box(root, "Moving port hatch", new Vector3(-.61f, 1.35f, 2.8f), new Vector3(1.2f, 2.7f, .18f), kit.Graphite);
            r.dockDoorR = kit.Box(root, "Moving starboard hatch", new Vector3(.61f, 1.35f, 2.8f), new Vector3(1.2f, 2.7f, .18f), kit.Graphite);
            for (int i = 0; i < 6; i++)
            {
                var plate = kit.Box(root, "Impact debris / fixed deck fragment", new Vector3(-2.1f + i * .8f, .13f, .5f + Mathf.Sin(i * 2f)), new Vector3(.4f, .09f, .7f), kit.Titanium);
                plate.localRotation = Quaternion.Euler(4f, i * 53f, 10f);
            }
            r.arrivingAnirudh = Actor(kit, root, "Anirudh / arriving operative", sphere, true, false);
            TMP_Text arrival = Label(root, font, "Retrieval mission time", new Vector3(0f, 3.45f, 2.65f), new Vector2(5.5f, .65f), 3f);
            arrival.text = "ASTRA–7  /  72 HOURS LATER";
            r.dockAlarm = Light(root, "Retrieval amber beacon", new Vector3(0f, 3f, 2f), new Color(1f, .45f, .15f), 3f, 10f);
            Light(root, "Soft dock key", new Vector3(-2f, 3f, -2f), new Color(.73f, .82f, .84f), 3.5f, 10f);
        }

        private static void Cabin(Astra7VisualKit kit, StationCinematicDirector.StageReferences r, Mesh sphere, TMP_FontAsset font)
        {
            Transform root = r.cabin;
            kit.Box(root, "Cabin deck", new Vector3(0f, -.1f, 0f), new Vector3(6f, .2f, 6.3f), kit.Graphite);
            kit.Box(root, "Warm cabin rear", new Vector3(0f, 1.4f, 2.9f), new Vector3(6f, 2.8f, .2f), kit.Ivory);
            for (int side = -1; side <= 1; side += 2)
            {
                kit.Box(root, "Cabin side rib", new Vector3(side * 2.7f, 1.4f, .4f), new Vector3(.16f, 2.8f, 4.8f), kit.Titanium);
                kit.Box(root, "Emergency interior strip", new Vector3(side * 2.55f, 1.9f, .2f), new Vector3(.04f, .07f, 4.4f), kit.Amber);
                kit.Box(root, "Flight chair", new Vector3(side * 1.5f, .5f, .6f), new Vector3(.78f, .15f, .7f), kit.Suit);
                kit.Box(root, "Chair back", new Vector3(side * 1.5f, 1f, .95f), new Vector3(.78f, 1f, .16f), kit.Suit);
            }
            r.craftDoorL = kit.Box(root, "Escape craft port hatch", new Vector3(-1.6f, 1.35f, 2.7f), new Vector3(1.2f, 2.7f, .2f), kit.Panel);
            r.craftDoorR = kit.Box(root, "Escape craft starboard hatch", new Vector3(1.6f, 1.35f, 2.7f), new Vector3(1.2f, 2.7f, .2f), kit.Panel);
            r.cabinAnirudh = Actor(kit, root, "Anirudh / escape craft performance", sphere, true, false);
            kit.Box(root, "Recovery desk", new Vector3(.25f, .9f, -1.8f), new Vector3(2.5f, .15f, .95f), kit.Panel);
            kit.Box(root, "Recovered ANVESH housing", new Vector3(.15f, 1.04f, -1.65f), new Vector3(.56f, .18f, .46f), kit.Copper);
            r.wristSignal = Group(root, "ANVESH reactivation / holographic lens"); r.wristSignal.localPosition = new Vector3(.15f, 1.36f, -1.65f);
            kit.Shape(r.wristSignal, "Reactivated interface", kit.Ring, Vector3.zero, new Vector3(2f, 1f, 2f), kit.Cyan);
            kit.Shape(r.wristSignal, "Signal kernel", kit.Crystal, Vector3.zero, Vector3.one * .45f, kit.Cyan);
            r.wristWave = Line(kit, root, "Returned signal waveform", 33, .012f, new Color(.4f, .9f, .95f));
            r.wristWave.transform.localPosition = new Vector3(.15f, 1.4f, -1.78f);
            r.cabinDisplay = Label(root, font, "ANVESH impossible search display", new Vector3(.2f, 1.85f, -1.55f), new Vector2(2.5f, .8f), 1.5f);
            r.cabinKey = Light(root, "Quiet cabin lamp", new Vector3(-1.4f, 2.8f, -2f), new Color(.82f, .84f, .79f), 1f, 8f);
            Light(root, "Warm cabin practical", new Vector3(1.6f, 2.4f, 1.4f), new Color(1f, .61f, .3f), 2.5f, 8f);
        }

        private static CinematicActor Actor(Astra7VisualKit kit, Transform parent, string name, Mesh sphere, bool operative, bool bun)
        {
            Transform root = Group(parent, name), torso = Group(root, "Breathing upper-body rig");
            Material clothes = operative ? kit.Suit : kit.Ivory;
            kit.Box(torso, "Tailored chest", new Vector3(0f, 1.27f, 0f), new Vector3(.54f, .63f, .32f), clothes);
            kit.Shape(torso, "Neck", sphere, new Vector3(0f, 1.65f, .01f), new Vector3(.18f, .22f, .17f), kit.Skin);
            kit.Box(torso, "Dark inner shirt", new Vector3(0f, 1.44f, .175f), new Vector3(.20f, .34f, .025f), kit.Hair);
            kit.Box(root, "Utility belt", new Vector3(0f, .94f, 0f), new Vector3(.50f, .11f, .34f), kit.Hair);
            kit.Box(root, "Copper clasp", new Vector3(.02f, .94f, .19f), new Vector3(.11f, .085f, .03f), kit.Copper);
            for (int side = -1; side <= 1; side += 2)
            {
                if (operative)
                {
                    kit.Box(torso, "Copper suit harness", new Vector3(side * .18f, 1.32f, .19f), new Vector3(.045f, .51f, .035f), kit.Copper);
                    kit.Box(torso, "Technical shoulder plate", new Vector3(side * .3f, 1.52f, -.015f), new Vector3(.21f, .16f, .32f), kit.Titanium);
                }
                else
                {
                    Transform lapel = kit.Box(torso, "Coat lapel", new Vector3(side * .13f, 1.42f, .19f), new Vector3(.105f, .4f, .035f), kit.Ivory);
                    lapel.localRotation = Quaternion.Euler(0f, 0f, side * -15f);
                    kit.Box(torso, "Coat tail", new Vector3(side * .14f, .87f, 0f), new Vector3(.25f, .4f, .36f), kit.Ivory);
                    kit.Box(torso, "Copper research lanyard", new Vector3(side * .08f, 1.34f, .21f), new Vector3(.016f, .43f, .016f), kit.Copper);
                }
            }
            if (!operative)
            {
                kit.Box(torso, "Research identity badge", new Vector3(0f, 1.12f, .23f), new Vector3(.12f, .16f, .03f), kit.Titanium);
                kit.Box(torso, "Badge photonic stripe", new Vector3(0f, 1.08f, .25f), new Vector3(.095f, .025f, .01f), kit.Cyan);
            }
            else kit.Box(torso, "Retrieval backpack", new Vector3(0f, 1.32f, -.28f), new Vector3(.4f, .5f, .22f), kit.Graphite);
            Transform head = Group(torso, "Head and gaze pivot"); head.localPosition = new Vector3(0f, 1.78f, .015f);
            kit.Shape(head, "Warm faceted face", sphere, Vector3.zero, new Vector3(.32f, .41f, .32f), kit.Skin);
            kit.Shape(head, "Hair silhouette", sphere, new Vector3(0f, .11f, -.04f), new Vector3(.35f, .27f, .34f), kit.Hair);
            kit.Shape(head, "Nose", sphere, new Vector3(0f, -.008f, .158f), new Vector3(.065f, .09f, .065f), kit.Skin);
            kit.Box(head, "Mouth line", new Vector3(0f, -.09f, .146f), new Vector3(.085f, .014f, .015f), kit.Copper);
            if (bun) kit.Shape(head, "Pinned hair bun", sphere, new Vector3(0f, .025f, -.22f), Vector3.one * .20f, kit.Hair);
            else if (!operative) kit.Shape(head, "Short swept hair", sphere, new Vector3(-.12f, .04f, -.045f), new Vector3(.16f, .29f, .29f), kit.Hair);
            var eyes = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -.072f : .072f;
                eyes[i] = kit.Shape(head, "Eye " + i, sphere, new Vector3(x, .022f, .153f), new Vector3(.048f, .028f, .016f), kit.Hair);
                kit.Box(head, "Eyebrow", new Vector3(x, .067f, .152f), new Vector3(.075f, .02f, .02f), kit.Hair);
                if (!operative)
                {
                    kit.Box(head, "Glasses upper rim", new Vector3(x, .058f, .176f), new Vector3(.125f, .014f, .013f), kit.Hair);
                    kit.Box(head, "Glasses lower rim", new Vector3(x, -.015f, .176f), new Vector3(.125f, .014f, .013f), kit.Hair);
                    kit.Box(head, "Glasses outer rim", new Vector3(x + Mathf.Sign(x) * .061f, .022f, .176f), new Vector3(.012f, .072f, .013f), kit.Hair);
                }
            }
            if (!operative) kit.Box(head, "Glasses bridge", new Vector3(0f, .044f, .176f), new Vector3(.035f, .015f, .018f), kit.Hair);
            var arms = new Transform[2]; var elbows = new Transform[2]; var legs = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                arms[i] = Group(torso, "Articulated shoulder " + i); arms[i].localPosition = new Vector3(side * .33f, 1.49f, 0f);
                kit.Shape(arms[i], "Sleeved upper arm", sphere, new Vector3(0f, -.17f, 0f), new Vector3(.2f, .4f, .23f), clothes);
                elbows[i] = Group(arms[i], "Elbow " + i); elbows[i].localPosition = new Vector3(0f, -.36f, 0f);
                kit.Shape(elbows[i], "Forearm", sphere, new Vector3(0f, -.13f, 0f), new Vector3(.14f, .30f, .17f), kit.Skin);
                kit.Shape(elbows[i], "Hand", sphere, new Vector3(0f, -.32f, .015f), new Vector3(.15f, .17f, .095f), operative ? kit.Hair : kit.Skin);
                legs[i] = Group(root, "Articulated hip " + i); legs[i].localPosition = new Vector3(side * .16f, .91f, 0f);
                kit.Shape(legs[i], "Trouser silhouette", sphere, new Vector3(0f, -.35f, 0f), new Vector3(.23f, .75f, .27f), kit.Suit);
                kit.Box(legs[i], "Boot", new Vector3(0f, -.78f, .07f), new Vector3(.24f, .22f, .38f), kit.Hair);
            }
            if (operative)
            {
                kit.Box(elbows[0], "ANVESH cuff", new Vector3(0f, -.2f, 0f), new Vector3(.20f, .14f, .22f), kit.Copper);
                kit.Box(elbows[0], "ANVESH status lens", new Vector3(-.1f, -.2f, 0f), new Vector3(.025f, .09f, .12f), kit.Cyan);
            }
            var actor = root.gameObject.AddComponent<CinematicActor>();
            actor.Configure(torso, head, arms[0], arms[1], elbows[0], elbows[1], legs[0], legs[1], eyes);
            return actor;
        }

        private static void Room(Astra7VisualKit kit, Transform root, int width, int depth)
        {
            for (int x = 0; x < width; x++)
                for (int z = 0; z < depth; z++)
                    kit.Instance("Floor panel", root, new Vector3((x - (width - 1) * .5f) * 2.4f, 0f, (z - (depth - 1) * .5f) * 2.4f));
            for (int x = 0; x < width; x++)
                kit.Instance("Wall panel", root, new Vector3((x - (width - 1) * .5f) * 2.4f, 0f, depth * 1.2f));
            foreach (Collider collider in root.GetComponentsInChildren<Collider>()) collider.enabled = false;
        }

        private static void Craft(Astra7VisualKit kit, Transform root, Vector3 offset = default)
        {
            root.localPosition = offset;
            kit.Box(root, "Graphite retrieval hull", Vector3.zero, new Vector3(1.5f, .85f, 3.1f), kit.Graphite);
            kit.Box(root, "Titanium cockpit armor", new Vector3(0f, .2f, -.9f), new Vector3(1.2f, .6f, 1.15f), kit.Titanium);
            kit.Box(root, "Cockpit dark canopy", new Vector3(0f, .52f, -.7f), new Vector3(.96f, .12f, .95f), kit.Panel);
            for (int side = -1; side <= 1; side += 2)
            {
                kit.Box(root, "Copper thruster pod", new Vector3(side * 1.08f, -.07f, .72f), new Vector3(.51f, .52f, 1.3f), kit.Copper);
                kit.Shape(root, "Ion plume", kit.Crystal, new Vector3(side * 1.08f, -.07f, 1.7f), new Vector3(.30f, .28f, 1.15f), kit.Cyan);
                kit.Box(root, "Retrieval fin", new Vector3(side * 1.14f, .16f, -.3f), new Vector3(1.03f, .12f, 1.7f), kit.Titanium);
            }
        }

        private static Transform Group(Transform parent, string name)
        { var item = new GameObject(name); item.transform.SetParent(parent, false); return item.transform; }
        private static LineRenderer Line(Astra7VisualKit kit, Transform parent, string name, int points, float width, Color color)
        {
            var line = Group(parent, name).gameObject.AddComponent<LineRenderer>();
            line.sharedMaterial = kit.Line; line.positionCount = points; line.useWorldSpace = false;
            line.widthMultiplier = width; line.startColor = line.endColor = color;
            line.shadowCastingMode = ShadowCastingMode.Off; line.receiveShadows = false; return line;
        }
        private static TMP_Text Label(Transform parent, TMP_FontAsset font, string name, Vector3 position, Vector2 size, float fontSize)
        {
            var item = new GameObject(name, typeof(RectTransform)); item.transform.SetParent(parent, false); item.transform.localPosition = position;
            var text = item.AddComponent<TextMeshPro>(); text.font = font; text.fontSize = fontSize;
            text.rectTransform.sizeDelta = size; text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.Normal; text.richText = true;
            text.color = new Color(.87f, .94f, .95f); text.enableAutoSizing = false; return text;
        }
        private static Light Light(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
        {
            Transform root = Group(parent, name); root.localPosition = position;
            var light = root.gameObject.AddComponent<Light>(); light.type = LightType.Point;
            light.color = color; light.intensity = intensity; light.range = range; light.shadows = LightShadows.None; return light;
        }
        private static Mesh PrimitiveMesh(PrimitiveType primitive)
        {
            GameObject temporary = GameObject.CreatePrimitive(primitive);
            Mesh mesh = temporary.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temporary); return mesh;
        }
        private static Material Material(string name, Color color, float glow)
        {
            string path = Astra7VisualKit.Root + "/Materials/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
            material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", .2f);
            material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", color * glow);
            EditorUtility.SetDirty(material); return material;
        }
    }
}
